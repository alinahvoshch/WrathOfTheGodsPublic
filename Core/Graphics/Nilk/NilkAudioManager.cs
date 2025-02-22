using Microsoft.Xna.Framework;
using MonoStereo;
using MonoStereo.Filters;
using NoxusBoss.Core.CrossCompatibility.Inbound.MonoStereo;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Graphics.Nilk;

[ExtendsFromMod(MonoStereoSystem.ModName)]
public class NilkAudioManager : ModSystem
{
    public static float PitchOffset
    {
        get;
        private set;
    }

    [JITWhenModsEnabled(MonoStereoSystem.ModName)]
    public static PitchShiftFilter PitchFilter
    {
        get;
        private set;
    }

    public override void PostSetupContent()
    {
        if (MonoStereoSystem.Enabled)
            LoadWrapper();
    }

    [JITWhenModsEnabled(MonoStereoSystem.ModName)]
    private static void LoadWrapper()
    {
        PitchFilter = new(1f);
        AudioManager.SoundMixer?.AddFilter(PitchFilter);
    }

    public override void UpdateUI(GameTime gameTime)
    {
        if (MonoStereoSystem.Enabled)
            UpdateWrapper();
    }

    [JITWhenModsEnabled(MonoStereoSystem.ModName)]
    private static void UpdateWrapper()
    {
        float desiredPitchFactor = Lerp(-0.5f, 0.5f, Cos01(Main.GlobalTimeWrappedHourly * 2f)) * NilkEffectManager.NilkInsanityInterpolant + 1f;
        if (Distance(PitchFilter.PitchFactor, desiredPitchFactor) >= 0.005f)
            PitchFilter.PitchFactor = desiredPitchFactor;
    }
}
