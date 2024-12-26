using BeatSpiderSharp.Core.Models;
using SongDetailsCache.Structs;

namespace BeatSpiderSharp.Core.Utilities.Extensions;

public static class LegacyExtensions
{
    public static MapMods ToMapMods(this LegacyPreset.ModRequirements req)
    {
        var result = req.NoodleExtensions ? MapMods.NoodleExtensions : 0;
        result |= req.MappingExtensions ? MapMods.MappingExtensions : 0;
        result |= req.Chroma ? MapMods.Chroma : 0;
        result |= req.Cinema ? MapMods.Cinema : 0;
        return result;
    }
    
    public static MapCharacteristic ToMapCharacteristic(this LegacyPreset.SongFilterSetting.CharacteristicsFilter.SongCharacteristic characteristic)
    {
        return characteristic switch
        {
            LegacyPreset.SongFilterSetting.CharacteristicsFilter.SongCharacteristic.Standard => MapCharacteristic.Standard,
            LegacyPreset.SongFilterSetting.CharacteristicsFilter.SongCharacteristic.OneSaber => MapCharacteristic.OneSaber,
            LegacyPreset.SongFilterSetting.CharacteristicsFilter.SongCharacteristic.NoArrows => MapCharacteristic.NoArrows,
            LegacyPreset.SongFilterSetting.CharacteristicsFilter.SongCharacteristic.NinetyDegree => MapCharacteristic.NinetyDegree,
            LegacyPreset.SongFilterSetting.CharacteristicsFilter.SongCharacteristic.ThreeSixtyDegree => MapCharacteristic.ThreeSixtyDegree,
            LegacyPreset.SongFilterSetting.CharacteristicsFilter.SongCharacteristic.Lightshow => MapCharacteristic.Lightshow,
            LegacyPreset.SongFilterSetting.CharacteristicsFilter.SongCharacteristic.Lawless => MapCharacteristic.Lawless,
            _ => MapCharacteristic.Custom
        };
    }

    public static MapDifficulty ToMapDifficulty(this LegacyPreset.SongFilterSetting.DifficultyFilter.Difficulty difficulty)
    {
        return difficulty switch
        {
            LegacyPreset.SongFilterSetting.DifficultyFilter.Difficulty.Easy => MapDifficulty.Easy,
            LegacyPreset.SongFilterSetting.DifficultyFilter.Difficulty.Normal => MapDifficulty.Normal,
            LegacyPreset.SongFilterSetting.DifficultyFilter.Difficulty.Hard => MapDifficulty.Hard,
            LegacyPreset.SongFilterSetting.DifficultyFilter.Difficulty.Expert => MapDifficulty.Expert,
            LegacyPreset.SongFilterSetting.DifficultyFilter.Difficulty.ExpertPlus => MapDifficulty.ExpertPlus,
            _ => MapDifficulty.ExpertPlus
        };
    }
}