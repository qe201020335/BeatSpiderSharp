using System.Text.Json;
using System.Text.Json.Serialization;

namespace BeatSpiderSharp.Core.Models;

/// <summary>
/// The preset model for BeatSpider, aka a .brset preset file.
/// </summary>
public class LegacyPreset
{
    [JsonPropertyName("下载方式选择")]
    public DataSource SongSource { get; set; }

    [JsonPropertyName("BeatSaver")]
    public BeatSaverSetting BeatSaver { get; set; } = new();

    [JsonPropertyName("谱师个人页面")]
    public MapperSetting Mapper { get; set; } = new();

    [JsonPropertyName("本地数据缓存")]
    public DataCacheSetting DataCache { get; set; } = new();

    [JsonPropertyName("ScoreSaber")]
    public ScoreSaberSetting ScoreSaber { get; set; } = new();

    [JsonPropertyName("BEASTSABER")]
    public BeastSaberSetting BeastSaber { get; set; } = new();

    [JsonPropertyName("歌曲列表")]
    public PlaylistInputSetting PlaylistInput { get; set; } = new();

    [JsonPropertyName("手动")]
    public ManualSongInputSetting ManualSongInput { get; set; } = new();

    [JsonPropertyName("搜索过滤")]
    public SearchFilterSetting SearchFilter { get; set; } = new();

    [JsonPropertyName("本地歌曲目录")]
    public LocalSongSetting LocalSong { get; set; } = new();

    [JsonPropertyName("保存方式")]
    public OutputSetting Output { get; set; } = new();

    [JsonPropertyName("筛选项目")]
    public SongFilterSetting SongFilter { get; set; } = new();

    [JsonPropertyName("下载限制")]
    public LimitSetting Limits { get; set; } = new();

    /// <summary>
    /// This is included for completeness.
    /// Will not do anything.
    /// </summary>
    [JsonPropertyName("封面标签")]
    public ThumbnailTagSetting ThumbnailTag { get; set; } = new();

    #region Sub Data Class

    public enum DataSource
    {
        BeatSaver = 0,
        Mapper = 1,
        LocalCache = 2,
        ScoreSaber = 3,
        BeastSaber = 4,
        Playlist = 5,
        ManualInput = 6,
    }

    public class BeatSaverSetting
    {
        [JsonPropertyName("搜索词")]
        public string SearchKeyword { get; set; } = "";

        [JsonPropertyName("开始页数")]
        [JsonConverter(typeof(StringIntConverter))]
        public int? StartPage { get; set; }

        [JsonPropertyName("下载方式")]
        public SortType Sort { get; set; }

        [JsonPropertyName("自动生成")]
        public GeneratedSongSetting GeneratedSong { get; set; } = new();

        [JsonPropertyName("排位曲")]
        public RankedSongSetting RankedSong { get; set; } = new();

        [JsonPropertyName("全难度")]
        public DifficultySetting Difficulty { get; set; } = new();

        [JsonPropertyName("BPM")]
        public MinMaxFloatSetting Bpm { get; set; } = new();

        [JsonPropertyName("方块密度")]
        public MinMaxFloatSetting Nps { get; set; } = new();

        [JsonPropertyName("歌曲时长")]
        public MinMaxIntSetting Duration { get; set; } = new();

        [JsonPropertyName("上传时间")]
        public MinMaxTimeSetting UploadTime { get; set; } = new();

        [JsonPropertyName("总评分")]
        public MinMaxFloatSetting Rating { get; set; } = new();

        [JsonPropertyName("需求组件")]
        public RequireModsSetting RequireMods { get; set; } = new();

        [JsonPropertyName("排除组件")]
        public ExcludeModsSetting ExcludeMods { get; set; } = new();

        public enum SortType
        {
            Latest = 0,
            Relevance = 1,
            Rating = 2,
        }

        public class DifficultySetting : DisablableSetting
        {
            [JsonPropertyName("全难度")]
            public bool IsFullSpread { get; set; }
        }

        public class RequireModsSetting : DisablableSetting
        {
            [JsonPropertyName("需求组件")]
            public ModRequirements RequireMods { get; set; } = new();
        }

        public class ExcludeModsSetting : DisablableSetting
        {
            [JsonPropertyName("排除组件")]
            public ModRequirements ExcludeMods { get; set; } = new();
        }
    }

    public class MapperSetting
    {
        [JsonPropertyName("地址")]
        public string MapperAddress { get; set; } = "";
    }

    public class DataCacheSetting
    {
        [JsonPropertyName("选择本地缓存")]
        public bool UseLocalCache { get; set; }

        [JsonPropertyName("选择在线缓存")]
        public bool UseOnlineCache { get; set; }
    }

    public class ScoreSaberSetting
    {
        [JsonPropertyName("难度")]
        public MinMaxFloat StarDifficulty { get; set; } = new();

        public class MinMaxFloat
        {
            [JsonPropertyName("min")]
            [JsonConverter(typeof(StringFloatConverter))]
            public float? Min { get; set; }

            [JsonPropertyName("max")]
            [JsonConverter(typeof(StringFloatConverter))]
            public float? Max { get; set; }
        }
    }

    public class BeastSaberSetting
    {
        [JsonPropertyName("地址")]
        public string Address { get; set; } = "";

        [JsonPropertyName("开始页数")]
        [JsonConverter(typeof(StringIntConverter))]
        public int? StartPage { get; set; }
    }

    public class PlaylistInputSetting
    {
        [JsonPropertyName("路径")]
        public string Path { get; set; } = "";
    }

    public class ManualSongInputSetting
    {
        [JsonPropertyName("手动下载歌曲")]
        [JsonConverter(typeof(StringMultiLineArrayConverter))]
        public IList<string> Songs { get; set; } = new List<string>();
    }

    public class SearchFilterSetting
    {
        [JsonPropertyName("搜索内容")]
        public string SearchContent { get; set; } = "";

        [JsonPropertyName("搜索开关")]
        public bool SearchEnabled { get; set; }

        [JsonPropertyName("搜索标题")]
        public bool SearchTitle { get; set; }

        [JsonPropertyName("搜索歌名")]
        public bool SearchSongName { get; set; }

        [JsonPropertyName("搜索作者")]
        public bool SearchAuthor { get; set; }

        [JsonPropertyName("搜索谱师")]
        public bool SearchMapper { get; set; }

        [JsonPropertyName("搜索介绍")]
        public bool SearchDescription { get; set; }

        [JsonPropertyName("高级搜索")]
        public bool AdvancedSearch { get; set; }

        [JsonPropertyName("正则搜索")]
        public bool RegexSearch { get; set; }
    }

    public class LocalSongSetting
    {
        [JsonPropertyName("目录")]
        [JsonConverter(typeof(StringMultiLineArrayConverter))]
        public IList<string> LocalSongPaths { get; set; } = new List<string>();

        [JsonPropertyName("跳过")]
        [JsonConverter(typeof(StringMultiLineArrayConverter))]
        public IList<string> LocalSongSkipPaths { get; set; } = new List<string>();

        /// <summary>
        /// It was only used to determine which tab to display in the original project.
        /// Does not affect functionality or behaviour.
        /// Both modes are always active.
        /// </summary>
        [JsonPropertyName("现行子夹")]
        public LocalSongMode Mode { get; set; }

        public enum LocalSongMode
        {
            LocalSongCopy = 0,
            LocalSongSkip = 1,
        }
    }

    public class OutputSetting
    {
        [JsonPropertyName("歌曲列表")]
        public PathSetting Playlist { get; set; } = new();

        [JsonPropertyName("下载歌曲")]
        public PathSetting Songs { get; set; } = new();

        [JsonPropertyName("命名方式")]
        public NamingSetting Naming { get; set; } = new();

        public class PathSetting : DisablableSetting
        {
            [JsonPropertyName("路径")]
            public string Path { get; set; } = "";
        }

        public class NamingSetting
        {
            [JsonPropertyName("英文")]
            public bool AllEnglish { get; set; } = true;

            [JsonPropertyName("内容")]
            public string Template { get; set; } = "[bsr] ([歌名] - [谱师])";
        }
    }

    public class SongFilterSetting
    {
        [JsonPropertyName("上传者ID")]
        public ContentSetting UploaderId { get; set; } = new();

        [JsonPropertyName("上传者名称")]
        public ContentSetting UploaderName { get; set; } = new();

        [JsonPropertyName("需求组件")]
        public ModsFilter RequireMods { get; set; } = new();

        [JsonPropertyName("排除组件")]
        public ModsFilter ExcludeMods { get; set; } = new();

        [JsonPropertyName("包含模式")]
        public CharacteristicsFilter IncludeCharacteristics { get; set; } = new();

        [JsonPropertyName("包含难度")]
        public DifficultyFilter IncludeDifficulties { get; set; } = new();

        [JsonPropertyName("下载量")]
        public MinMaxIntSetting DownloadCount { get; set; } = new();

        [JsonPropertyName("游戏次数")]
        public MinMaxIntSetting PlayCount { get; set; } = new();

        [JsonPropertyName("点赞数量")]
        public MinMaxIntSetting UpVotes { get; set; } = new();

        [JsonPropertyName("点赞比例")]
        public MinMaxFloatSetting UpVotePercentage { get; set; } = new();

        [JsonPropertyName("点踩数量")]
        public MinMaxIntSetting DownVotes { get; set; } = new();

        [JsonPropertyName("点踩比例")]
        public MinMaxFloatSetting DownVotePercentage { get; set; } = new();

        [JsonPropertyName("总评分")]
        public MinMaxFloatSetting Rating { get; set; } = new();

        [JsonPropertyName("自动生成")]
        public GeneratedSongSetting GeneratedSong { get; set; } = new();

        [JsonPropertyName("排位曲")]
        public RankedSongSetting RankedSong { get; set; } = new();

        [JsonPropertyName("筛选中文")]
        public DisablableSetting FilterChinese { get; set; } = new();

        [JsonPropertyName("BPM")]
        public MinMaxFloatSetting Bpm { get; set; } = new();

        [JsonPropertyName("歌曲时长")]
        public MinMaxIntSetting Duration { get; set; } = new();

        [JsonPropertyName("谱面时长")]
        public MinMaxFloatSetting Length { get; set; } = new();

        [JsonPropertyName("节拍数量")]
        public MinMaxFloatSetting Beats { get; set; } = new();

        [JsonPropertyName("飞行速度")]
        public MinMaxFloatSetting Njs { get; set; } = new();

        [JsonPropertyName("偏移值")]
        public MinMaxFloatSetting Offset { get; set; } = new();

        [JsonPropertyName("方块数量")]
        public MinMaxIntSetting Notes { get; set; } = new();

        [JsonPropertyName("方块密度")]
        public MinMaxFloatSetting Nps { get; set; } = new();

        [JsonPropertyName("炸弹数量")]
        public MinMaxIntSetting Bombs { get; set; } = new();

        [JsonPropertyName("灯光事件")]
        public MinMaxIntSetting Events { get; set; } = new();

        [JsonPropertyName("墙壁数量")]
        public MinMaxIntSetting Walls { get; set; } = new();

        /// <summary>
        /// This is ScoreSaber ranked stars
        /// </summary>
        [JsonPropertyName("难度星级")]
        public MinMaxFloatSetting Stars { get; set; } = new();

        [JsonPropertyName("校验错误")]
        public MinMaxIntSetting ParityErrors { get; set; } = new();

        [JsonPropertyName("校验警告")]
        public MinMaxIntSetting ParityWarns { get; set; } = new();

        [JsonPropertyName("校验重置")]
        public MinMaxIntSetting ParityResets { get; set; } = new();

        [JsonPropertyName("上传时间")]
        public MinMaxTimeSetting UploadTime { get; set; } = new();

        [JsonPropertyName("标签")]
        public TagsFilterGroup Tags { get; set; } = new();

        [JsonPropertyName("Sage分数")]
        public MinMaxIntSetting SageScore { get; set; } = new();

        [JsonPropertyName("最高分数")]
        public MinMaxIntSetting MaxScore { get; set; } = new();

        public class ModsFilter : DisablableSetting
        {
            [JsonPropertyName("内容")]
            public ModRequirements Mods { get; set; } = new();
        }

        public class CharacteristicsFilter : DisablableSetting
        {
            [JsonPropertyName("内容")]
            [JsonConverter(typeof(CharacteristicsEnumConverter))]
            public IList<SongCharacteristic> Characteristics { get; set; } = new List<SongCharacteristic>();

            public enum SongCharacteristic
            {
                Standard,
                OneSaber,
                NoArrows,
                NinetyDegree,
                ThreeSixtyDegree,
                Lightshow,
                Lawless
            }

            private class CharacteristicsEnumConverter : EnumArrayConverter<SongCharacteristic>
            {
                // Standard,OneSaber,NoArrows,90Degree,360Degree,Lightshow,Lawless
                protected override SongCharacteristic ParseEnumFromString(string value)
                {
                    return value switch
                    {
                        "Standard" => SongCharacteristic.Standard,
                        "OneSaber" => SongCharacteristic.OneSaber,
                        "NoArrows" => SongCharacteristic.NoArrows,
                        "90Degree" => SongCharacteristic.NinetyDegree,
                        "360Degree" => SongCharacteristic.ThreeSixtyDegree,
                        "Lightshow" => SongCharacteristic.Lightshow,
                        "Lawless" => SongCharacteristic.Lawless,
                        _ => throw new JsonException($"Unknown song characteristic {value}")
                    };
                }

                protected override string ConvertEnumToString(SongCharacteristic value)
                {
                    return value switch
                    {
                        SongCharacteristic.Standard => "Standard",
                        SongCharacteristic.OneSaber => "OneSaber",
                        SongCharacteristic.NoArrows => "NoArrows",
                        SongCharacteristic.NinetyDegree => "90Degree",
                        SongCharacteristic.ThreeSixtyDegree => "360Degree",
                        SongCharacteristic.Lightshow => "Lightshow",
                        SongCharacteristic.Lawless => "Lawless",
                        _ => throw new JsonException($"Invalid song characteristic {value}")
                    };
                }
            }
        }

        public class DifficultyFilter : DisablableSetting
        {
            [JsonPropertyName("内容")]
            [JsonConverter(typeof(EnumArrayConverter<Difficulty>))]
            public IList<Difficulty> Difficulties { get; set; } = new List<Difficulty>();
            
            [JsonPropertyName("与")]
            public bool And { get; set; }

            [JsonPropertyName("或")]
            public bool Or { get; set; }

            //Easy,Normal,Hard,Expert,ExpertPlus
            public enum Difficulty
            {
                Easy,
                Normal,
                Hard,
                Expert,
                ExpertPlus
            }
        }

        public class TagsFilterGroup
        {
            [JsonPropertyName("包含")]
            public TagsFilter Include { get; set; } = new();

            [JsonPropertyName("排除")]
            public TagsFilter Exclude { get; set; } = new();

            public class TagsFilter : ContentArraySetting
            {
                [JsonPropertyName("与")]
                public bool And { get; set; }

                [JsonPropertyName("或")]
                public bool Or { get; set; }
            }
        }
    }

    public class LimitSetting
    {
        [JsonPropertyName("限制数量")]
        public ContentIntSetting Count { get; set; } = new();
    }

    public class ThumbnailTagSetting
    {
        [JsonPropertyName("总开关")]
        public DisablableSetting Enable { get; set; } = new();

        [JsonPropertyName("封面ACG预设")]
        public DisablableSetting AcgPreset { get; set; } = new();

        [JsonPropertyName("封面Token等待")]
        public DisablableSetting WaitForToken { get; set; } = new();

        [JsonPropertyName("封面包含")]
        public ContentSetting IncludeTags { get; set; } = new();

        [JsonPropertyName("封面包含与")]
        public DisablableSetting IncludeAnd { get; set; } = new();

        [JsonPropertyName("封面包含或")]
        public DisablableSetting IncludeOr { get; set; } = new();

        [JsonPropertyName("封面排除")]
        public ContentSetting ExcludeTags { get; set; } = new();

        [JsonPropertyName("封面排除与")]
        public DisablableSetting ExcludeAnd { get; set; } = new();

        [JsonPropertyName("封面排除或")]
        public DisablableSetting ExcludeOr { get; set; } = new();

        [JsonPropertyName("封面包含置信度")]
        public ContentInt IncludeConfidence { get; set; } = new();

        [JsonPropertyName("封面排除置信度")]
        public ContentInt ExcludeConfidence { get; set; } = new();

        public class ContentInt
        {
            [JsonPropertyName("内容")]
            [JsonConverter(typeof(StringIntConverter))]
            public int? Content { get; set; }
        }
    }

    #endregion

    #region Common Data Class

    public class DisablableSetting
    {
        [JsonPropertyName("启用")]
        [JsonPropertyOrder(-99)]
        public bool Enable { get; set; }
    }

    public class ContentSetting : DisablableSetting
    {
        [JsonPropertyName("内容")]
        [JsonPropertyOrder(-89)]
        public string Content { get; set; } = "";
    }

    public class ContentArraySetting : DisablableSetting
    {
        [JsonPropertyName("内容")]
        [JsonPropertyOrder(-89)]
        [JsonConverter(typeof(StringArrayConverter))]
        public IList<string> Content { get; set; } = new List<string>();
    }

    public class ContentIntSetting : DisablableSetting
    {
        [JsonPropertyName("内容")]
        [JsonPropertyOrder(-89)]
        [JsonConverter(typeof(StringIntConverter))]
        public int? Content { get; set; }
    }

    public class MinMaxIntSetting : DisablableSetting
    {
        [JsonPropertyName("min")]
        [JsonConverter(typeof(StringIntConverter))]
        public int? Min { get; set; }

        [JsonPropertyName("max")]
        [JsonConverter(typeof(StringIntConverter))]
        public int? Max { get; set; }
    }

    public class MinMaxFloatSetting : DisablableSetting
    {
        [JsonPropertyName("min")]
        [JsonConverter(typeof(StringFloatConverter))]
        public float? Min { get; set; }

        [JsonPropertyName("max")]
        [JsonConverter(typeof(StringFloatConverter))]
        public float? Max { get; set; }
    }

    public class MinMaxTimeSetting : DisablableSetting
    {
        [JsonPropertyName("min")]
        [JsonConverter(typeof(StringTimestampConverter))]
        public DateTimeOffset? Min { get; set; }

        [JsonPropertyName("max")]
        [JsonConverter(typeof(StringTimestampConverter))]
        public DateTimeOffset? Max { get; set; }
    }

    // Original project's settings is weird (but consistent with BeatSaver's API)
    // Disabled: Human Only
    // Enabled && True: All
    // Enabled && False: Generated Only
    public class GeneratedSongSetting : DisablableSetting
    {
        [JsonPropertyName("自动生成")]
        public bool IncludeGenerated { get; set; }
    }

    public class RankedSongSetting : DisablableSetting
    {
        [JsonPropertyName("排位曲")]
        public bool IsRanked { get; set; }
    }

    [JsonConverter(typeof(ModRequirementsConvertor))]
    public class ModRequirements
    {
        public bool NoodleExtensions { get; set; }

        public bool Chroma { get; set; }

        public bool MappingExtensions { get; set; }

        public bool Cinema { get; set; }

        public static ModRequirements FromString(string stringValue)
        {
            var set = new HashSet<string>(stringValue.ToLower().Split(','));
            return new ModRequirements
            {
                NoodleExtensions = set.Contains("ne"),
                Chroma = set.Contains("chroma"),
                MappingExtensions = set.Contains("me"),
                Cinema = set.Contains("cinema"),
            };
        }

        public override string ToString()
        {
            var list = new List<string>(4);
            if (NoodleExtensions)
            {
                list.Add("ne");
            }

            if (Chroma)
            {
                list.Add("chroma");
            }

            if (MappingExtensions)
            {
                list.Add("me");
            }

            if (Cinema)
            {
                list.Add("cinema");
            }

            return string.Join(',', list);
        }

        private class ModRequirementsConvertor : JsonConverter<ModRequirements>
        {
            public override ModRequirements Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var stringValue = reader.GetString();
                if (stringValue == null)
                {
                    return new ModRequirements();
                }

                return FromString(stringValue);
            }

            public override void Write(Utf8JsonWriter writer, ModRequirements? value, JsonSerializerOptions options)
            {
                if (value == null)
                {
                    writer.WriteNullValue();
                    return;
                }

                writer.WriteStringValue(value.ToString());
            }
        }
    }

    #endregion

    #region Common Converters

    private class StringIntConverter : JsonConverter<int?>
    {
        public override bool HandleNull => true;

        public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return string.IsNullOrWhiteSpace(value) || !int.TryParse(value, out var result) ? null : result;
        }

        public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.HasValue ? value.Value.ToString() : string.Empty);
        }
    }

    private class StringFloatConverter : JsonConverter<float?>
    {
        public override bool HandleNull => true;

        public override float? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return string.IsNullOrWhiteSpace(value) || !float.TryParse(value, out var result) ? null : result;
        }

        public override void Write(Utf8JsonWriter writer, float? value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value?.ToString() ?? "");
        }
    }

    private class StringTimestampConverter : JsonConverter<DateTimeOffset?>
    {
        public override bool HandleNull => true;

        public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return string.IsNullOrWhiteSpace(value) || !long.TryParse(value, out var number) ? null : DateTimeOffset.FromUnixTimeSeconds(number);
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value?.ToUnixTimeSeconds().ToString() ?? "");
        }
    }

    private class StringArrayConverter() : JsonConverter<IList<string>>
    {
        private char[] Separators { get; } = [','];

        private string SeparatorForWrite { get; } = ",";


        public StringArrayConverter(char[] separators, string separatorForWrite) : this()
        {
            Separators = separators;
            SeparatorForWrite = separatorForWrite;
        }

        public override IList<string> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return value == null ? [] : value.Split(Separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        public override void Write(Utf8JsonWriter writer, IList<string>? value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            var stringValue = string.Join(SeparatorForWrite, value);
            writer.WriteStringValue(stringValue);
        }
    }

    private class StringMultiLineArrayConverter() : StringArrayConverter(['\r', '\n'], Environment.NewLine);

    private class EnumArrayConverter<T>() : JsonConverter<IList<T>> where T : struct, Enum
    {
        private readonly StringArrayConverter _arrayConverter = new();

        public EnumArrayConverter(StringArrayConverter arrayConverter) : this()
        {
            _arrayConverter = arrayConverter;
        }

        public EnumArrayConverter(char[] separators, string separatorForWrite) : this()
        {
            _arrayConverter = new StringArrayConverter(separators, separatorForWrite);
        }

        public override IList<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var stringList = _arrayConverter.Read(ref reader, typeToConvert, options);
            return stringList.Select(ParseEnumFromString).ToList();
        }

        public override void Write(Utf8JsonWriter writer, IList<T> value, JsonSerializerOptions options)
        {
            var stringList = value.Select(ConvertEnumToString).ToList();
            _arrayConverter.Write(writer, stringList, options);
        }

        protected virtual T ParseEnumFromString(string value)
        {
            return Enum.Parse<T>(value);
        }

        protected virtual string ConvertEnumToString(T value)
        {
            return value.ToString();
        }
    }

    #endregion
}