using BeatSpiderSharp.Core.Models.Preset.Enums;
using SongDetailsCache.Structs;

namespace BeatSpiderSharp.Core.Utilities.Extensions;

public static class EnumConversions
{
    public static MapCharacteristic ToMapCharacteristic(this MCharacteristic characteristic)
    {
        return characteristic switch
        {
            MCharacteristic.Standard => MapCharacteristic.Standard,
            MCharacteristic.OneSaber => MapCharacteristic.OneSaber,
            MCharacteristic.NoArrows => MapCharacteristic.NoArrows,
            MCharacteristic.NinetyDegree => MapCharacteristic.NinetyDegree,
            MCharacteristic.ThreeSixtyDegree => MapCharacteristic.ThreeSixtyDegree,
            MCharacteristic.Lightshow => MapCharacteristic.Lightshow,
            MCharacteristic.Lawless => MapCharacteristic.Lawless,
            _ => MapCharacteristic.Custom
        };
    }

    public static MCharacteristic ToMCharacteristic(this MapCharacteristic characteristic)
    {
        return characteristic switch
        {
            MapCharacteristic.Standard => MCharacteristic.Standard,
            MapCharacteristic.OneSaber => MCharacteristic.OneSaber,
            MapCharacteristic.NoArrows => MCharacteristic.NoArrows,
            MapCharacteristic.NinetyDegree => MCharacteristic.NinetyDegree,
            MapCharacteristic.ThreeSixtyDegree => MCharacteristic.ThreeSixtyDegree,
            MapCharacteristic.Lightshow => MCharacteristic.Lightshow,
            MapCharacteristic.Lawless => MCharacteristic.Lawless,
            _ => MCharacteristic.Other
        };
    }

    public static MapDifficulty ToMapDifficulty(this MDifficulty difficulty)
    {
        return difficulty switch
        {
            MDifficulty.Easy => MapDifficulty.Easy,
            MDifficulty.Normal => MapDifficulty.Normal,
            MDifficulty.Hard => MapDifficulty.Hard,
            MDifficulty.Expert => MapDifficulty.Expert,
            MDifficulty.ExpertPlus => MapDifficulty.ExpertPlus,
            _ => MapDifficulty.ExpertPlus
        };
    }

    public static MDifficulty ToMDifficulty(this MapDifficulty difficulty)
    {
        return difficulty switch
        {
            MapDifficulty.Easy => MDifficulty.Easy,
            MapDifficulty.Normal => MDifficulty.Normal,
            MapDifficulty.Hard => MDifficulty.Hard,
            MapDifficulty.Expert => MDifficulty.Expert,
            MapDifficulty.ExpertPlus => MDifficulty.ExpertPlus,
            _ => MDifficulty.ExpertPlus
        };
    }

    public static MapMods ToMapMods(this ICollection<MMod> mods)
    {
        MapMods result = 0;
        foreach (var mMod in mods)
        {
            result |= mMod switch
            {
                MMod.NoodleExtensions => MapMods.NoodleExtensions,
                MMod.MappingExtensions => MapMods.MappingExtensions,
                MMod.Chroma => MapMods.Chroma,
                MMod.Cinema => MapMods.Cinema,
                _ => 0
            };
        }

        return result;
    }

    public static HashSet<MMod> ToMMods(this MapMods mods)
    {
        return Enum.GetValues<MMod>().Where(mMod =>
        {
            var mod = mMod switch
            {
                MMod.NoodleExtensions => MapMods.NoodleExtensions,
                MMod.MappingExtensions => MapMods.MappingExtensions,
                MMod.Chroma => MapMods.Chroma,
                MMod.Cinema => MapMods.Cinema,
                _ => (MapMods)0
            };

            return mod != 0 && mods.HasFlag(mod);
        }).ToHashSet();
    }
}