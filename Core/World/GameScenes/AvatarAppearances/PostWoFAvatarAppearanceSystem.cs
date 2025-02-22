using Luminance.Core.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.GlobalInstances;
using NoxusBoss.Core.SoundSystems;
using NoxusBoss.Core.SoundSystems.Music;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace NoxusBoss.Core.World.GameScenes.AvatarAppearances;

public class PostWoFAvatarAppearanceSystem : BaseAvatarAppearanceSystem
{
    public override void OnModLoad()
    {
        // Load events.
        GlobalNPCEventHandlers.OnKillEvent += InitiateWaitAfterWoF;
    }

    public override void OnModUnload()
    {
        GlobalNPCEventHandlers.OnKillEvent -= InitiateWaitAfterWoF;
    }

    private void InitiateWaitAfterWoF(NPC npc)
    {
        // Start the appearance wait after the Wall of Flesh is defeated.
        if (npc.type == NPCID.WallofFlesh && !Main.hardMode)
            WaitingToStart = true;
    }

    public override void UpdateEvent()
    {
        int earthquakeDelay = 60;
        int earthquakeDuration = 300000;
        int animationDuration = 900;
        float earthquakeInterpolant = InverseLerpBump(0f, earthquakeDuration, earthquakeDuration, earthquakeDuration + 90f, EventTimer - earthquakeDelay);

        // Create the Avatar appear after the earthquake.
        float appearInterpolant = InverseLerp(-90f, 60f, EventTimer - earthquakeDelay - earthquakeDuration);

        // Make the rift appear in the background.
        RiftEclipseSky.IsEnabled = true;

        // Shake the screen if the earthquake is ongoing.
        if (earthquakeInterpolant > 0f)
        {
            float rumblePower = InverseLerp(0f, 0.64f, earthquakeInterpolant) * 11f;
            ScreenShakeSystem.SetUniversalRumble(rumblePower, TwoPi, null, 0.2f);
        }

        // Make the rift appear.
        RiftEclipseSky.RiftScaleFactor = Pow(InverseLerp(0.1f, 0.9f, appearInterpolant), 0.7f);
        RiftEclipseSky.RiftScaleFactor -= InverseLerp(-32f, 0f, EventTimer - animationDuration) * 0.4f;

        // Make sounds dissipate as the fog appears.
        SoundMufflingSystem.MuffleFactor = InverseLerp(0.8f, 0f, appearInterpolant);
        MusicVolumeManipulationSystem.MuffleFactor = Utils.Remap(appearInterpolant, 0f, 0.7f, 1f, 0.34f);

        // Make the Avatar create sounds before the animation ends.
        if (EventTimer == animationDuration - 90)
        {
            SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.Chirp with { Volume = 1.7f });
            SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.Murmur with { Volume = 1.6f });
            ScreenShakeSystem.StartShake(4.7f);
        }

        // Increment the event timer.
        EventTimer++;
        if (EventTimer >= animationDuration)
        {
            EventTimer = 0;
            MusicVolumeManipulationSystem.MuffleFactor = 0f;
            RiftEclipseSky.IsEnabled = false;
            SoundMufflingSystem.MuffleFactor = 1f;
        }
    }
}
