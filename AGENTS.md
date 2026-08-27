# AGENTS.md

Guidance for AI coding agents working in this repository.

## What this project is

BeatSpiderSharp is a .NET rewrite of [BeatSpider](https://github.com/WGzeyu/BeatSpider), a tool that filters Beat
Saber maps and produces playlists and/or downloaded song folders. Today it is command-line only. A GUI is planned but
no work has started on it — there is no GUI project and no GUI code anywhere in the tree.

That plan shapes one existing decision: `BeatSpider` (in Core) is an abstract base holding the load/filter/output
pipeline, and `BeatSpiderCLI` is a subclass that only supplies logger configuration and CLI-option handling. A GUI
front-end is expected to be a second subclass. Keep console-specific concerns out of Core when you touch it —
`Serilog.Sinks.Console` is referenced by the CLI project alone (`System.CommandLine` by the CLI and the Cacher), and
Core takes no sink dependency of its own: `BeatSpider.ConfigureLogger` is an empty virtual that each front-end
overrides.

Two executables:

- **`BeatSpiderSharp.Cacher`** — crawls the entire BeatSaver map database (`api.beatsaver.com/search/text/{page}`)
  into a single JSON file: a `docs` array followed by a `date` unix-seconds timestamp. Optionally GZipped. It shares
  nothing with the CLI but `BeatSpiderSharp.Shared`; in particular it does not depend on Core or Models. Its
  user-facing strings are in Chinese.
  **The `docs` array is guaranteed to be ordered newest upload first** — the CLI relies on this, see
  `SortType.Latest` below. `ConcurrentRequests` fetches several pages at once but the results are drained from the
  task list in page order, so the output order stays deterministic; keep it that way when touching that loop.
- **`BeatSpiderSharp.CLI`** — reads that cache file, applies a preset's filters, and exports a playlist and/or
  downloads the matching songs. Its user-facing strings are in English.

The Cacher output is the CLI's input. The CLI never calls the BeatSaver search API itself. Its only network traffic is
map zips, fetched from the URLs already present in the cached data, and playlists given to it as `http(s)` URLs.

## Layout

```
BeatSpiderSharp.CLI/          Entry point, System.CommandLine options, BeatSpiderCLI orchestration
BeatSpiderSharp.Cacher/       Standalone BeatSaver crawler
Libs/BeatSpiderSharp.Core/    Filter engine, PlaylistExporter, SongDownloader, preset loading
Libs/BeatSpiderSharp.Models/  BeatSaver DTOs, preset/filter option types, enums, templates
Libs/BeatSpiderSharp.Legacy/  Converts old (Chinese) .brset presets to the new format
Libs/BeatSpiderSharp.Extensions/  JSON streaming, LINQ, string, collection helpers
Libs/BeatSpiderSharp.Shared/  Constants shared by Core and the Cacher — the only project both reference
```

`Directory.Build.props` sets `net10.0`, nullable enabled, implicit usings for every project.
`Directory.Packages.props` holds all package versions — **central package management is on**, so `<PackageReference>`
entries in `.csproj` files must not carry a `Version` attribute. Add new versions to `Directory.Packages.props`.

## Build and run

```powershell
dotnet build BeatSpiderSharp.sln -c Release
```

There is **no test project** in the solution. `dotnet test` does nothing today. Two `//TODO Unit tests` markers exist
(`Options.cs` on `LogicSetOption`, `LegacyPresetLoader.cs` on `CombineRange`) — if you add tests, those are the
intended first targets.

Running the CLI against real data requires a song cache file, which is large and not in the repo. Typical invocation:

```powershell
dotnet run --project BeatSpiderSharp.CLI -- -i preset.json -s cache.json.gz -z -o ./playlists -O ./songs
```

`RunLegacyPresets.ps1` batch-converts and runs every `.brset` in a directory. It builds in Release, writes to
`./run-output/` (gitignored), and passes `-D` to disable downloads unless `-AllowDownload` is given.

## Pipeline

`BeatSpiderCLI.RunAsync` is the whole story, in order:

1. Load preset — either `PresetLoader.LoadPreset` (new JSON format) or `LegacyPresetLoader.LoadAndConvertLegacyPreset`
   when `--legacy` is passed. `--convert-only` exits after conversion.
2. `OverwriteOptions` — CLI flags override values baked into the preset. Note `bool?` options (`--save-song-zips`,
   `--use-local-zips`) intentionally default to `null` so "not specified" is distinguishable from "explicitly false".
3. `VerifyOutput` — output directories must already exist; the tool does not create them.
4. Stream songs from the cache. `JsonExtensions.DeserializeArrayAsync` walks to the `docs` property and yields
   `Song` records one at a time as `IAsyncEnumerable`. The cache is far too large to load whole; keep it streaming.
   A `PipeReader` owns the buffering, and `Utf8JsonReader` consumes its `ReadOnlySequence` directly - no
   `StreamReader` transcoding every byte to UTF-16, and no hand-rolled buffer growth. Two details are load-bearing:
   the pipe is created with a 1 MiB segment size (the 4 KiB default splits nearly every song across segments and
   costs ~40% throughput), and the loop completes the pipe in a `finally` because a count limit routinely
   abandons the enumeration early.
   `JsonSerializer.DeserializeAsyncEnumerable` cannot replace this - it requires the array to be the whole payload
   and rejects the cache's trailing `,"date":N}`.
   Because `Utf8JsonReader` is a `ref struct` that cannot cross an `await`, parsing sits in a sync method while the
   async loop carries the position across reads. An element running past the buffered bytes is rewound and
   re-parsed once more arrive. If you touch that state machine, re-verify it at small chunk sizes: every resume
   point (mid-element, and between the property name and the `[`) has to be restartable.
5. Narrow by input source (`SongInputSource.BeatSaver` = everything, or intersect with playlists / a manual bsr+hash
   list via `SongSourceFactory`, which is `IDisposable` because it lazily opens an `HttpClient`). An entry in
   `InputConfig.Playlists` is either a local path — `.json`/`.bplist` or `.blist`, by extension — or an `http(s)` URL,
   which is downloaded and always parsed as bplist. A playlist that fails to load aborts the run rather than being
   skipped; `RunAsync` catches it and returns exit code 1.
6. Filter (see below).
7. `OutputSongsAsync` — optional sort, optional count limit, then materialize to an array and hand it to
   `PlaylistExporter` and/or `SongDownloader`.
   `SortType.Latest` deliberately sorts nothing: the cache is already newest-upload first, so cache order *is*
   latest order. Only `SortType.Rating` reorders.
   Sorting and limiting together goes through `TakeTopAsync`, a bounded heap, rather than
   `OrderByDescending(...).Take(n)`. It returns the same songs in the same order - LINQ's sort is stable, so the
   heap key carries the source index to break ties the same way. The reason is not the sort itself, which costs a
   few milliseconds on 122k songs either way: `OrderByDescending` has to buffer the whole source before yielding,
   which keeps every parsed song alive at once and promotes them out of gen0 (417 MB peak and ~0.8s of GC, versus
   55 MB and free). Any full-buffering operator added to this pipeline - a sort, `Distinct`, `GroupBy` - pays the
   same cost.

## Filter semantics

These rules are easy to get backwards; check them before touching filter code.

- `Preset.FilterOptions` is a list of `FilterConfig`. Multiple configs are **OR**'d — a song passes if any config
  accepts it (`BeatSpider.FilterSongs`).
- Inside one config, `RootFilter` **AND**s three filters: `SearchFilter`, `SongDetailFilter`, `LevelDetailFilter`.
- Every filter option derives from `Option`, which has an `Enable` flag and an `implicit operator bool`. That is why
  filter code reads `if (filter.Njs && !filter.Njs.InRange(...))` — the first operand is the enable check, not a
  null check. Preserve this idiom.
- An enabled option with an **empty** filter set passes everything (`Filter.Count == 0 => true`). Disabled and
  empty behave the same; do not "optimize" one into the other without checking `LogicSetOption.SatisfiedBy`.
- `LogicSetOption.IsOr` picks `Overlaps` (OR) vs `IsSubsetOf` (AND) for set membership. `LogicExcludeOption` negates
  that result.
- `LevelDetailFilter` operates per-difficulty: characteristics and difficulties narrow the `diffs` list first, then
  the remaining numeric filters must be satisfied by **at least one** surviving difficulty.
- `SearchFilter` builds a haystack from whichever text fields `SearchOptions` enables (title, song name + sub name,
  song author, mapper, description), then passes a song if **any** regex pattern or **any** advance term matches
  **any** haystack field. Regex and advance terms run together, not either/or. An advance term is a lowercased
  substring match whose hits are suppressed when they sit inside one of its exclusion words. An enabled filter with
  no terms, or with no haystack fields enabled, passes everything.
- `SongDetailFilter`'s **constructor throws** if `Downloads` or `Plays` is enabled — BeatSaver no longer reports
  either. `SongDetailOptions.Chinese` is parsed and carried through conversion but not implemented; it filters
  nothing.

## Conventions

- **Formatting**: `.editorconfig` is authoritative — 4 spaces, CRLF, 120-column limit, `var` preferred everywhere,
  private instance/static fields prefixed `_`, private static readonly and constants in `PascalCase` except
  non-private `const` which is `ALL_UPPER`. ReSharper/Rider settings are checked in and enforce these as warnings.
- **JSON**: split by path, and the split is deliberate.
  - The **song cache** (`BeatSaver/*` models) uses `System.Text.Json` with the source generator -
    `[JsonPropertyName("camelCase")]`, registered in `BeatSaverJsonContext`. Add a new model type to that context
    or it falls back to reflection. These models are deserialize-only; nothing writes them.
    **A collection property must coerce null in its `init` accessor** (see `Song.Tags`) - System.Text.Json drops
    the property initializer both when the field is absent and when it is an explicit `null`, where Newtonsoft
    reused the existing instance. BeatSaver omits `tags`, `collaborators` and `diffs` on most maps, so a plain
    `public List<T> X { get; init; } = [];` silently yields `null` for ~half the cache and blows up in the
    filters. `= []` on its own is not enough.
  - **Everything else** - presets, legacy presets, playlists - stays on Newtonsoft.Json with
    `[JsonProperty("camelCase")]`. Preset serialization goes through `PresetLoader`'s configured serializer
    (indented, UTC dates, string enums). Do not migrate these without a reason; they are cold paths.
- **Logging**: Serilog static `Log.X` with structured message templates (`Log.Information("Loading {Path}", path)`).
  Never interpolate into the template. `Log.Verbose` is the level for per-song filter exclusion reasons.
- **DEBUG-only behavior**: several `#if DEBUG` blocks change semantics. A new BeatSaver API field throws in Debug
  builds but is silently ignored in Release; adding a field to the `BeatSaver/*` models is the fix. The mechanism
  differs by library: the `BeatSaver/*` records each carry a `#if DEBUG`-guarded
  `[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]` (System.Text.Json applies this per type, not
  as a global setting - a new model type without the attribute silently loses the check), while the legacy-preset
  deserializer still uses Newtonsoft's global `MissingMemberHandling = MissingMemberHandling.Error`.
- **Filename templates**: `Templates` (new, `{{Bsr}}`) and `LegacyTemplates` (old). `FileUtils.SanitizeFileName`
  strips Windows-invalid characters on every platform so output is portable.
- **Paths**: compare with `FileUtils.PathComparer` (case-insensitive on Windows/macOS), not `StringComparer.Ordinal`.

## Things worth knowing before editing

- `SongDownloader` runs 8-way parallel with a matching `MaxConnectionsPerServer`. Per song it checks, in order:
  `SkipExisting` (folder name already present under `ExistingSongPaths`), `CopyLocalSongs` (copy the folder out of
  `LocalSongPaths` instead of downloading, falling back to download if the copy fails), then `UseLocalZips`
  (`{hash}.zip` on disk). Downloaded zips either stream to a pooled `RecyclableMemoryStream` or are written to
  `LocalZipsPath` as `{hash}.zip` via a `.part` temp file. Extraction goes to a `.tmp` sibling of the target folder,
  then `Directory.Move`s into place, merging via `FileSystem.MoveDirectory` if the target already exists.
- `SpecialFolders` creates `%APPDATA%/BeatSpiderSharp` plus a randomly-named folder under its `Temp/`, and deletes the
  latter on dispose. `BeatSpider` implements `IDisposable` explicitly — the `using var beatSpider = ...` in
  `BeatSpiderRunner` is what triggers cleanup. Nothing currently *reads* `TempFolder` or `DataFolder`; the download
  path builds its own scratch directories. Wire new temp usage through `SpecialFolders` rather than adding another.
- `PlaylistExporter` embeds `Assets/cover.png` and `Assets/font.ttf` as manifest resources and renders the playlist
  name onto the cover, auto-scaling the font. The resource names are hardcoded strings; renaming the assets breaks it.
- Legacy conversion is deliberately strict: it throws `LegacyConversionException` rather than silently dropping a
  setting it cannot represent (BeastSaber source, thumbnail tags, BeatSaver search keywords/start page, multi-mod
  exclusions, unparseable mapper URLs) or one it cannot reconcile (a merged min/max range that inverts, an input
  source whose implied filter contradicts the preset's own filter). Keep that posture — a wrong playlist is worse
  than a failed conversion.
- Build every `HttpClient` with `HttpClientCreator.Create(handler, productName)`; it is the only thing that attaches
  the `User-Agent`. There are three: `SongDownloader` and `SongSourceFactory` send `BeatSpiderSharp/{version}`,
  `BeatSaverCrawler` sends `BeatSpiderSharp.Cacher/{version}` so BeatSaver can tell the crawler from the downloader.
- `Constants.Version` reads the **entry** assembly's `AssemblyInformationalVersion`, so it reports the running exe's
  version, not Shared's. The SDK appends `+{commit sha}` to that attribute inside a git repo, which is why it is
  parsed through `SemanticVersion` and emitted with `ToNormalizedString()` — that drops the sha but keeps a `-beta`
  prerelease tag. Note `SemanticVersion.Parse` rejects four-part versions, so the `GetName().Version?.ToString(3)`
  fallback must stay three-part.
- `BeatSpiderSong.LatestVersion` is `Versions.First()` and will throw on an empty list. `ValidateBeatSaverSong` gates
  this in the pipeline; call it before constructing a `BeatSpiderSong` from raw API data.

## Git

Default branch is `master`; development happens on `dev`.
