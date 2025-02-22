using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace NoxusBoss.Core.SoundSystems.Music;

public class MusicVolumeManipulationSystem : ModSystem
{
    public static float MuffleFactor
    {
        get;
        set;
    } = 1f;

    public static bool MusicIsPaused
    {
        get;
        private set;
    }

    public override void OnModLoad()
    {
        On_Main.UpdateAudio += MakeMusicShutUp;
        On_ASoundEffectBasedAudioTrack.ReMapVolumeToMatchXact += RepentYouIsolentFools;
    }

    private float RepentYouIsolentFools(On_ASoundEffectBasedAudioTrack.orig_ReMapVolumeToMatchXact orig, ASoundEffectBasedAudioTrack self, float musicVolume)
    {
        // These magic constants were found by performing a regression algorithm on the original ReMapVolumeToMatchXact method, with the constraint that it actually
        // returns 0 at an input value of 0.
        // This ensures that the volume curve of the game is generally the same, not blasting people's ears out, but also having the capacity for natural crossfading.
        // I hope someday TML will fix this on their own end.
        return Exp(musicVolume * 2.25609f) * musicVolume * 0.0523f;
    }

    private void MakeMusicShutUp(On_Main.orig_UpdateAudio orig, Main self)
    {
        if (Main.gameMenu)
            MuffleFactor = 1f;

        bool monoStereoEnabled = ModLoader.TryGetMod("MonoStereoMod", out _);
        if (monoStereoEnabled || MuffleFactor >= 0.01f)
            orig(self);

        if (MuffleFactor <= 0.9999f)
        {
            for (int i = 0; i < Main.musicFade.Length; i++)
            {
                float volume = Main.musicFade[i] * Main.musicVolume * Saturate(MuffleFactor);
                float tempFade = Main.musicFade[i];

                if (volume <= 0f && tempFade <= 0f)
                    continue;

                for (int j = 0; j < 50; j++)
                {
                    Main.audioSystem.UpdateCommonTrackTowardStopping(i, volume, ref tempFade, Main.musicFade[i] >= 0.5f);
                    Main.musicFade[i] = tempFade;
                }
            }
            Main.audioSystem.UpdateAudioEngine();

            // Make the music muffle factor naturally dissipate.
            if (Main.instance.IsActive && !Main.gamePaused)
                MuffleFactor = Saturate(MuffleFactor * 1.03f + 0.03f);

            if (MusicIsPaused)
            {
                Main.audioSystem.ResumeAll();
                MusicIsPaused = false;
            }
        }

        if (!monoStereoEnabled && MuffleFactor <= 0.11f)
        {
            Main.audioSystem.PauseAll();
            MusicIsPaused = true;
        }
    }
}
