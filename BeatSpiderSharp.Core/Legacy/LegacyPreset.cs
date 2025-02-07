using System.Globalization;
using Newtonsoft.Json;

namespace BeatSpiderSharp.Core.Legacy;

/// <summary>
/// The preset model for BeatSpider, aka a .brset preset file.
/// </summary>
public class LegacyPreset
{
    [JsonProperty("下载方式选择")]
    public DataSource SongSource { get; private set; }

    [JsonProperty("BeatSaver")]
    public BeatSaverSetting BeatSaver { get; private set; } = new();

    [JsonProperty("谱师个人页面")]
    public MapperSetting Mapper { get; private set; } = new();

    [JsonProperty("本地数据缓存")]
    public DataCacheSetting DataCache { get; private set; } = new();

    [JsonProperty("ScoreSaber")]
    public ScoreSaberSetting ScoreSaber { get; private set; } = new();

    [JsonProperty("BEASTSABER")]
    public BeastSaberSetting BeastSaber { get; private set; } = new();

    [JsonProperty("歌曲列表")]
    public PlaylistInputSetting PlaylistInput { get; private set; } = new();

    [JsonProperty("手动")]
    public ManualSongInputSetting ManualSongInput { get; private set; } = new();

    [JsonProperty("搜索过滤")]
    public SearchFilterSetting SearchFilter { get; private set; } = new();

    [JsonProperty("本地歌曲目录")]
    public LocalSongSetting LocalSong { get; private set; } = new();

    [JsonProperty("保存方式")]
    public OutputSetting Output { get; private set; } = new();

    [JsonProperty("筛选项目")]
    public SongFilterSetting SongFilter { get; private set; } = new();

    [JsonProperty("下载限制")]
    public LimitSetting Limits { get; private set; } = new();

    /// <summary>
    /// This is included for completeness.
    /// Will not do anything.
    /// </summary>
    [JsonProperty("封面标签")]
    public ThumbnailTagSetting ThumbnailTag { get; private set; } = new();

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
        [JsonProperty("搜索词")]
        public string SearchKeyword { get; private set; } = "";

        [JsonProperty("开始页数")]
        [JsonConverter(typeof(StringIntConverter))]
        public int? StartPage { get; private set; }

        [JsonProperty("下载方式")]
        public SortType Sort { get; private set; }

        [JsonProperty("自动生成")]
        public GeneratedSongSetting GeneratedSong { get; private set; } = new();

        [JsonProperty("排位曲")]
        public RankedSongSetting RankedSong { get; private set; } = new();

        [JsonProperty("全难度")]
        public DifficultySetting Difficulty { get; private set; } = new();

        [JsonProperty("BPM")]
        public MinMaxFloatSetting Bpm { get; private set; } = new();

        [JsonProperty("方块密度")]
        public MinMaxFloatSetting Nps { get; private set; } = new();

        [JsonProperty("歌曲时长")]
        public MinMaxIntSetting Duration { get; private set; } = new();

        [JsonProperty("上传时间")]
        public MinMaxTimeSetting UploadTime { get; private set; } = new();

        [JsonProperty("总评分")]
        public MinMaxFloatSetting Rating { get; private set; } = new();

        [JsonProperty("需求组件")]
        public RequireModsSetting RequireMods { get; private set; } = new();

        [JsonProperty("排除组件")]
        public ExcludeModsSetting ExcludeMods { get; private set; } = new();

        public enum SortType
        {
            Latest = 0,
            Relevance = 1,
            Rating = 2,
        }

        public class DifficultySetting : DisablableSetting
        {
            [JsonProperty("全难度")]
            public bool IsFullSpread { get; private set; }
        }

        public class RequireModsSetting : DisablableSetting
        {
            [JsonProperty("需求组件")]
            public ModRequirements RequireMods { get; private set; } = new();
        }

        public class ExcludeModsSetting : DisablableSetting
        {
            [JsonProperty("排除组件")]
            public ModRequirements ExcludeMods { get; private set; } = new();
        }
    }

    public class MapperSetting
    {
        [JsonProperty("地址")]
        public string MapperAddress { get; private set; } = "";
    }

    public class DataCacheSetting
    {
        [JsonProperty("选择本地缓存")]
        public bool UseLocalCache { get; private set; }

        [JsonProperty("选择在线缓存")]
        public bool UseOnlineCache { get; private set; }
    }

    public class ScoreSaberSetting
    {
        [JsonProperty("难度")]
        public MinMaxFloat StarDifficulty { get; private set; } = new();

        public class MinMaxFloat: IMinMaxSetting<float>
        {
            [JsonProperty("min")]
            [JsonConverter(typeof(StringFloatConverter))]
            public float? Min { get; private set; }

            [JsonProperty("max")]
            [JsonConverter(typeof(StringFloatConverter))]
            public float? Max { get; private set; }
        }
    }

    public class BeastSaberSetting
    {
        [JsonProperty("地址")]
        public string Address { get; private set; } = "";

        [JsonProperty("开始页数")]
        [JsonConverter(typeof(StringIntConverter))]
        public int? StartPage { get; private set; }
    }

    public class PlaylistInputSetting
    {
        [JsonProperty("路径")]
        public string Path { get; private set; } = "";
    }

    public class ManualSongInputSetting
    {
        [JsonProperty("手动下载歌曲")]
        [JsonConverter(typeof(StringMultiLineArrayConverter))]
        public IList<string> Songs { get; private set; } = new List<string>();
    }

    public class SearchFilterSetting
    {
        [JsonProperty("搜索内容")]
        public string SearchContent { get; private set; } = "";

        [JsonProperty("搜索开关")]
        public bool SearchEnabled { get; private set; }

        [JsonProperty("搜索标题")]
        public bool SearchTitle { get; private set; }

        [JsonProperty("搜索歌名")]
        public bool SearchSongName { get; private set; }

        [JsonProperty("搜索作者")]
        public bool SearchAuthor { get; private set; }

        [JsonProperty("搜索谱师")]
        public bool SearchMapper { get; private set; }

        [JsonProperty("搜索介绍")]
        public bool SearchDescription { get; private set; }

        [JsonProperty("高级搜索")]
        public bool AdvancedSearch { get; private set; }

        [JsonProperty("正则搜索")]
        public bool RegexSearch { get; private set; }
    }

    public class LocalSongSetting
    {
        [JsonProperty("目录")]
        [JsonConverter(typeof(StringMultiLineArrayConverter))]
        public IList<string> LocalSongPaths { get; private set; } = new List<string>();

        [JsonProperty("跳过")]
        [JsonConverter(typeof(StringMultiLineArrayConverter))]
        public IList<string> LocalSongSkipPaths { get; private set; } = new List<string>();

        /// <summary>
        /// It was only used to determine which tab to display in the original project.
        /// Does not affect functionality or behaviour.
        /// Both modes are always active.
        /// </summary>
        [JsonProperty("现行子夹")]
        public LocalSongMode Mode { get; private set; }

        public enum LocalSongMode
        {
            LocalSongCopy = 0,
            LocalSongSkip = 1,
        }
    }

    public class OutputSetting
    {
        [JsonProperty("歌曲列表")]
        public PathSetting Playlist { get; private set; } = new();

        [JsonProperty("下载歌曲")]
        public PathSetting Songs { get; private set; } = new();

        [JsonProperty("命名方式")]
        public NamingSetting Naming { get; private set; } = new();

        public class PathSetting : DisablableSetting
        {
            [JsonProperty("路径")]
            public string Path { get; private set; } = "";
        }

        public class NamingSetting
        {
            [JsonProperty("英文")]
            public bool AllEnglish { get; private set; } = true;

            [JsonProperty("内容")]
            public string Template { get; private set; } = "[bsr] ([歌名] - [谱师])";
        }
    }

    public class SongFilterSetting
    {
        [JsonProperty("上传者ID")]
        public ContentSetting UploaderId { get; private set; } = new();

        [JsonProperty("上传者名称")]
        public ContentSetting UploaderName { get; private set; } = new();

        [JsonProperty("需求组件")]
        public ModsFilter RequireMods { get; private set; } = new();

        [JsonProperty("排除组件")]
        public ModsFilter ExcludeMods { get; private set; } = new();

        [JsonProperty("包含模式")]
        public CharacteristicsFilter IncludeCharacteristics { get; private set; } = new();

        [JsonProperty("包含难度")]
        public DifficultyFilter IncludeDifficulties { get; private set; } = new();

        [JsonProperty("下载量")]
        public MinMaxIntSetting DownloadCount { get; private set; } = new();

        [JsonProperty("游戏次数")]
        public MinMaxIntSetting PlayCount { get; private set; } = new();

        [JsonProperty("点赞数量")]
        public MinMaxIntSetting UpVotes { get; private set; } = new();

        [JsonProperty("点赞比例")]
        public MinMaxFloatSetting UpVotePercentage { get; private set; } = new();

        [JsonProperty("点踩数量")]
        public MinMaxIntSetting DownVotes { get; private set; } = new();

        [JsonProperty("点踩比例")]
        public MinMaxFloatSetting DownVotePercentage { get; private set; } = new();

        [JsonProperty("总评分")]
        public MinMaxFloatSetting Rating { get; private set; } = new();

        [JsonProperty("自动生成")]
        public GeneratedSongSetting GeneratedSong { get; private set; } = new();

        [JsonProperty("排位曲")]
        public RankedSongSetting RankedSong { get; private set; } = new();

        [JsonProperty("筛选中文")]
        public DisablableSetting FilterChinese { get; private set; } = new();

        [JsonProperty("BPM")]
        public MinMaxFloatSetting Bpm { get; private set; } = new();

        [JsonProperty("歌曲时长")]
        public MinMaxIntSetting Duration { get; private set; } = new();

        [JsonProperty("谱面时长")]
        public MinMaxFloatSetting MapSeconds { get; private set; } = new();

        [JsonProperty("节拍数量")]
        public MinMaxFloatSetting MapLength { get; private set; } = new();

        [JsonProperty("飞行速度")]
        public MinMaxFloatSetting Njs { get; private set; } = new();

        [JsonProperty("偏移值")]
        public MinMaxFloatSetting Offset { get; private set; } = new();

        [JsonProperty("方块数量")]
        public MinMaxIntSetting Notes { get; private set; } = new();

        [JsonProperty("方块密度")]
        public MinMaxFloatSetting Nps { get; private set; } = new();

        [JsonProperty("炸弹数量")]
        public MinMaxIntSetting Bombs { get; private set; } = new();

        [JsonProperty("灯光事件")]
        public MinMaxIntSetting Events { get; private set; } = new();

        [JsonProperty("墙壁数量")]
        public MinMaxIntSetting Walls { get; private set; } = new();

        /// <summary>
        /// This is ScoreSaber ranked stars
        /// </summary>
        [JsonProperty("难度星级")]
        public MinMaxFloatSetting Stars { get; private set; } = new();

        [JsonProperty("校验错误")]
        public MinMaxIntSetting ParityErrors { get; private set; } = new();

        [JsonProperty("校验警告")]
        public MinMaxIntSetting ParityWarns { get; private set; } = new();

        [JsonProperty("校验重置")]
        public MinMaxIntSetting ParityResets { get; private set; } = new();

        [JsonProperty("上传时间")]
        public MinMaxTimeSetting UploadTime { get; private set; } = new();

        [JsonProperty("标签")]
        public TagsFilterGroup Tags { get; private set; } = new();

        [JsonProperty("Sage分数")]
        public MinMaxIntSetting SageScore { get; private set; } = new();

        [JsonProperty("最高分数")]
        public MinMaxIntSetting MaxScore { get; private set; } = new();

        public class ModsFilter : DisablableSetting
        {
            [JsonProperty("内容")]
            public ModRequirements Mods { get; private set; } = new();
        }

        public class CharacteristicsFilter : DisablableSetting
        {
            [JsonProperty("内容")]
            [JsonConverter(typeof(CharacteristicsEnumConverter))]
            public IList<SongCharacteristic> Characteristics { get; private set; } = new List<SongCharacteristic>();

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
            [JsonProperty("内容")]
            [JsonConverter(typeof(EnumArrayConverter<Difficulty>))]
            public IList<Difficulty> Difficulties { get; private set; } = new List<Difficulty>();
            
            [JsonProperty("与")]
            public bool And { get; private set; }

            [JsonProperty("或")]
            public bool Or { get; private set; }

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
            [JsonProperty("包含")]
            public TagsFilter Include { get; private set; } = new();

            [JsonProperty("排除")]
            public TagsFilter Exclude { get; private set; } = new();

            public class TagsFilter : ContentArraySetting
            {
                [JsonProperty("与")]
                public bool And { get; private set; }

                [JsonProperty("或")]
                public bool Or { get; private set; }
            }
        }
    }

    public class LimitSetting
    {
        [JsonProperty("限制数量")]
        public ContentIntSetting Count { get; private set; } = new();
    }

    public class ThumbnailTagSetting
    {
        [JsonProperty("总开关")]
        public DisablableSetting Enable { get; private set; } = new();

        [JsonProperty("封面ACG预设")]
        public DisablableSetting AcgPreset { get; private set; } = new();

        [JsonProperty("封面Token等待")]
        public DisablableSetting WaitForToken { get; private set; } = new();

        [JsonProperty("封面包含")]
        public ContentSetting IncludeTags { get; private set; } = new();

        [JsonProperty("封面包含与")]
        public DisablableSetting IncludeAnd { get; private set; } = new();

        [JsonProperty("封面包含或")]
        public DisablableSetting IncludeOr { get; private set; } = new();

        [JsonProperty("封面排除")]
        public ContentSetting ExcludeTags { get; private set; } = new();

        [JsonProperty("封面排除与")]
        public DisablableSetting ExcludeAnd { get; private set; } = new();

        [JsonProperty("封面排除或")]
        public DisablableSetting ExcludeOr { get; private set; } = new();

        [JsonProperty("封面包含置信度")]
        public ContentInt IncludeConfidence { get; private set; } = new();

        [JsonProperty("封面排除置信度")]
        public ContentInt ExcludeConfidence { get; private set; } = new();

        public class ContentInt
        {
            [JsonProperty("内容")]
            [JsonConverter(typeof(StringIntConverter))]
            public int? Content { get; private set; }
        }
    }

    #endregion

    #region Common Data Class

    public class DisablableSetting
    {
        [JsonProperty("启用", Order = -99)]
        public bool Enable { get; private set; }
    }

    public class ContentSetting : DisablableSetting
    {
        [JsonProperty("内容", Order = -89)]
        public string Content { get; private set; } = "";
    }

    public class ContentArraySetting : DisablableSetting
    {
        [JsonProperty("内容", Order = -89)]
        [JsonConverter(typeof(StringArrayConverter))]
        public IList<string> Content { get; private set; } = new List<string>();
    }

    public class ContentIntSetting : DisablableSetting
    {
        [JsonProperty("内容", Order = -89)]
        [JsonConverter(typeof(StringIntConverter))]
        public int? Content { get; private set; }
    }

    public interface IMinMaxSetting<T> where T: struct, IComparable<T>
    {
        T? Min { get; }
        T? Max { get; }
    }

    public class MinMaxIntSetting : DisablableSetting, IMinMaxSetting<int>
    {
        [JsonProperty("min")]
        [JsonConverter(typeof(StringIntConverter))]
        public int? Min { get; private set; }

        [JsonProperty("max")]
        [JsonConverter(typeof(StringIntConverter))]
        public int? Max { get; private set; }
        
        public bool InRange(int value)
        {
            return (!Min.HasValue || value >= Min.Value) && (!Max.HasValue || value <= Max.Value);
        }
    }

    public class MinMaxFloatSetting : DisablableSetting, IMinMaxSetting<float>
    {
        [JsonProperty("min")]
        [JsonConverter(typeof(StringFloatConverter))]
        public float? Min { get; private set; }

        [JsonProperty("max")]
        [JsonConverter(typeof(StringFloatConverter))]
        public float? Max { get; private set; }
        
        public bool InRange(float value)
        {
            return (!Min.HasValue || value >= Min.Value) && (!Max.HasValue || value <= Max.Value);
        }
    }

    public class MinMaxTimeSetting : DisablableSetting, IMinMaxSetting<DateTimeOffset>
    {
        [JsonProperty("min")]
        [JsonConverter(typeof(StringTimestampConverter))]
        public DateTimeOffset? Min { get; private set; }

        [JsonProperty("max")]
        [JsonConverter(typeof(StringTimestampConverter))]
        public DateTimeOffset? Max { get; private set; }
        
        public bool InRange(DateTimeOffset value)
        {
            return (!Min.HasValue || value >= Min.Value) && (!Max.HasValue || value <= Max.Value);
        }
    }

    // Original project's settings is weird (but consistent with BeatSaver's API)
    // Disabled: Human Only
    // Enabled && True: All
    // Enabled && False: Generated Only
    public class GeneratedSongSetting : DisablableSetting
    {
        [JsonProperty("自动生成")]
        public bool IncludeGenerated { get; private set; }
    }

    public class RankedSongSetting : DisablableSetting
    {
        [JsonProperty("排位曲")]
        public bool IsRanked { get; private set; }
    }

    [JsonConverter(typeof(ModRequirementsConvertor))]
    public class ModRequirements
    {
        public bool NoodleExtensions { get; private set; }

        public bool Chroma { get; private set; }

        public bool MappingExtensions { get; private set; }

        public bool Cinema { get; private set; }

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
            public override ModRequirements ReadJson(JsonReader reader, Type objectType, ModRequirements? existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                var value = reader.Value as string;
                return string.IsNullOrWhiteSpace(value) ? new ModRequirements() : FromString(value);
            }

            public override void WriteJson(JsonWriter writer, ModRequirements? value, JsonSerializer serializer)
            {
                if (value == null)
                {
                    writer.WriteNull();
                    return;
                }

                writer.WriteValue(value.ToString());
            }
        }
    }

    #endregion

    #region Common Converters

    private class StringIntConverter : JsonConverter<int?>
    {
        public override int? ReadJson(JsonReader reader, Type objectType, int? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var value = reader.Value as string;
            return string.IsNullOrWhiteSpace(value) || !int.TryParse(value, out var result) ? null : result;
        }

        public override void WriteJson(JsonWriter writer, int? value, JsonSerializer serializer)
        {
            writer.WriteValue(value.HasValue ? value.Value.ToString() : string.Empty);
        }
    }

    private class StringFloatConverter : JsonConverter<float?>
    {
        public override float? ReadJson(JsonReader reader, Type objectType, float? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var value = reader.Value as string;
            return string.IsNullOrWhiteSpace(value) || !float.TryParse(value, out var result) ? null : result;
        }

        public override void WriteJson(JsonWriter writer, float? value, JsonSerializer serializer)
        {
            writer.WriteValue(value.HasValue ? value.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);
        }
    }

    private class StringTimestampConverter : JsonConverter<DateTimeOffset?>
    {
        public override DateTimeOffset? ReadJson(JsonReader reader, Type objectType, DateTimeOffset? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var value = reader.Value as string;
            return string.IsNullOrWhiteSpace(value) || !long.TryParse(value, out var number) ? null : DateTimeOffset.FromUnixTimeSeconds(number);
        }

        public override void WriteJson(JsonWriter writer, DateTimeOffset? value, JsonSerializer serializer)
        {
            writer.WriteValue(value?.ToUnixTimeSeconds().ToString() ?? "");
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

        public override IList<string> ReadJson(JsonReader reader, Type objectType, IList<string>? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var value = reader.Value as string;
            return string.IsNullOrWhiteSpace(value) ? [] : value.Split(Separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        public override void WriteJson(JsonWriter writer, IList<string>? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var stringValue = string.Join(SeparatorForWrite, value);
            writer.WriteValue(stringValue);
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

        public override IList<T> ReadJson(JsonReader reader, Type objectType, IList<T>? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var existingList = existingValue?.Select(ConvertEnumToString).ToList();
            var stringList = _arrayConverter.ReadJson(reader, objectType, existingList, hasExistingValue, serializer);
            return stringList.Select(ParseEnumFromString).ToList();
        }

        public override void WriteJson(JsonWriter writer, IList<T>? value, JsonSerializer serializer)
        {
            var stringList = value?.Select(ConvertEnumToString).ToList();
            _arrayConverter.WriteJson(writer, stringList, serializer);
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