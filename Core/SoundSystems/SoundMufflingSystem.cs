using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using NoxusBoss.Core.CrossCompatibility.Inbound.BaseCalamity;
using ReLogic.Utilities;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Core.SoundSystems;

public class SoundMufflingSystem : ModSystem
{
    public static float MuffleFactor
    {
        get;
        set;
    } = 1f;

    public static List<SoundStyle> ExemptedSoundStyles
    {
        get;
        private set;
    } = [];

    public override void OnModLoad()
    {
        On_SoundPlayer.Play_Inner += ReduceVolume;

        ExemptedSoundStyles = [
            GennedAssets.Sounds.Common.Glitch,
            GennedAssets.Sounds.Common.MediumBloodSpill,
            GennedAssets.Sounds.NamelessDeity.PlayerSliceFemale,
            GennedAssets.Sounds.NamelessDeity.PlayerSliceMale,
            GennedAssets.Sounds.Common.EarRinging,
            GennedAssets.Sounds.Avatar.PlayerKill,
            GennedAssets.Sounds.Avatar.PlayerPerishTextAppear,

            GennedAssets.Sounds.NamelessDeity.ChantLoop,
            GennedAssets.Sounds.NamelessDeity.CosmicLaserStart,
            GennedAssets.Sounds.NamelessDeity.CosmicLaserLoop,
            GennedAssets.Sounds.NamelessDeity.CosmicLaserObliteration,
            GennedAssets.Sounds.NamelessDeity.JermaImKillingYou,
            GennedAssets.Sounds.NamelessDeity.Phase3Transition,

            GennedAssets.Sounds.Avatar.NamelessDispelsStatic,
            GennedAssets.Sounds.Avatar.NamelessVortexLoopEndBuildup,
            GennedAssets.Sounds.Avatar.NamelessVortexLoop,
            GennedAssets.Sounds.Avatar.NamelessOpensVortex,

            SoundID.DSTFemaleHurt,
            SoundID.DSTMaleHurt,
            SoundID.FemaleHit,
            SoundID.PlayerHit];

        if (CalamityCompatibility.Enabled)
        {
            ExemptedSoundStyles.Add(new SoundStyle("CalamityMod/Sounds/Custom/AbilitySounds/FullRage"));
            ExemptedSoundStyles.Add(new SoundStyle("CalamityMod/Sounds/Custom/AbilitySounds/RageActivate"));
            ExemptedSoundStyles.Add(new SoundStyle("CalamityMod/Sounds/Custom/AbilitySounds/RageEnd"));

            ExemptedSoundStyles.Add(new SoundStyle("CalamityMod/Sounds/Custom/AbilitySounds/FullAdrenaline"));
            ExemptedSoundStyles.Add(new SoundStyle("CalamityMod/Sounds/Custom/AbilitySounds/AdrenalineActivate"));
            ExemptedSoundStyles.Add(new SoundStyle("CalamityMod/Sounds/Custom/AbilitySounds/AdrenalineMajorLoss"));
            ExemptedSoundStyles.Add(new SoundStyle("CalamityMod/Sounds/Custom/AbilitySounds/NanomachinesActivate"));
        }
    }

    public override void PreUpdateEntities()
    {
        MuffleFactor = Lerp(MuffleFactor, 1f, 0.013f);
        if (MuffleFactor >= 0.999f)
            MuffleFactor = 1f;
    }

    private SlotId ReduceVolume(On_SoundPlayer.orig_Play_Inner orig, SoundPlayer self, ref SoundStyle style, Vector2? position, SoundUpdateCallback updateCallback)
    {
        SoundStyle copy = style;

        if (MuffleFactor < 0.999f && !ExemptedSoundStyles.Any(s => s.IsTheSameAs(copy)))
            style.Volume *= MuffleFactor;

        SlotId result = orig(self, ref style, position, updateCallback);
        style.Volume = copy.Volume;

        return result;
    }
}
