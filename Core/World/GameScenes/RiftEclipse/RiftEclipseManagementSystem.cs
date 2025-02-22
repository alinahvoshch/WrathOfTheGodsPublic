using Luminance.Core.Hooking;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using NoxusBoss.Content.Items.Fishing;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using NoxusBoss.Content.NPCs.Bosses.Draedon;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity;
using NoxusBoss.Core.CrossCompatibility.Inbound;
using NoxusBoss.Core.CrossCompatibility.Inbound.RealisticSky;
using NoxusBoss.Core.GlobalInstances;
using NoxusBoss.Core.SoundSystems;
using NoxusBoss.Core.World.GameScenes.AvatarAppearances;
using NoxusBoss.Core.World.WorldSaving;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Core.World.GameScenes.RiftEclipse;

public class RiftEclipseManagementSystem : ModSystem
{
    /// <summary>
    /// Whether the Rift Eclipse event is ongoing or not.
    /// </summary>
    public static bool RiftEclipseOngoing
    {
        get
        {
            if (BossDownedSaveSystem.HasDefeated<AvatarOfEmptiness>())
                return false;
            if (AvatarOfEmptiness.Myself is not null)
                return false;
            if (NamelessDeityBoss.Myself is not null)
                return false;

            return PostMLRiftAppearanceSystem.AvatarHasCoveredMoon;
        }
    }

    /// <summary>
    /// How intense the general effect is. This increases throughout progression.
    /// </summary>
    public static float IntensityInterpolant
    {
        get
        {
            if (CommonCalamityVariables.CalamitasDefeated && CommonCalamityVariables.DraedonDefeated)
                return 1f;
            if (CommonCalamityVariables.CalamitasDefeated || CommonCalamityVariables.DraedonDefeated)
                return 0.857f;
            if (CommonCalamityVariables.YharonDefeated)
                return 0.714f;
            if (CommonCalamityVariables.DevourerOfGodsDefeated)
                return 0.571f;
            if (CommonCalamityVariables.StormWeaverDefeated || CommonCalamityVariables.CeaselessVoidDefeated || CommonCalamityVariables.SignusDefeated)
                return 0.429f;
            if (CommonCalamityVariables.ProvidenceDefeated)
                return 0.286f;

            return 0.143f;
        }
    }

    /// <summary>
    /// The scale of the Avatar in the sky.
    /// </summary>
    public static float RiftScale
    {
        get;
        set;
    } = 1f;

    public override void OnModLoad()
    {
        new ManagedILEdit("Limit Angler Quests During Rift Eclipse", Mod, edit =>
        {
            IL_Main.AnglerQuestSwap += edit.SubscriptionWrapper;
        }, edit =>
        {
            IL_Main.AnglerQuestSwap -= edit.SubscriptionWrapper;
        }, LimitQuestFishDuringRiftEclipse).Apply();

        new ManagedILEdit("Disable World Evil Spreading During Rift Eclipse", Mod, edit =>
        {
            IL_WorldGen.UpdateWorld_Inner += edit.SubscriptionWrapper;
        }, edit =>
        {
            IL_WorldGen.UpdateWorld_Inner -= edit.SubscriptionWrapper;
        }, DisableWorldEvilSpreading).Apply();
        On_WorldGen.UpdateWorld_GrassGrowth += DisableAbovegroundGrassGrowth;
        On_WorldGen.GrowMoreVines += DisableAbovegroundVineGrowth;
        On_Main.UpdateAudio += RemoveAmbientMusic;
        GlobalTileEventHandlers.RandomUpdateEvent += RandomlyDestroyVegetation;
    }

    private void RemoveAmbientMusic(On_Main.orig_UpdateAudio orig, Main self)
    {
        orig(self);

        if (RiftEclipseOngoing)
        {
            float rainFade = Main.musicFade[MusicID.RainSoundEffect];
            for (int i = 0; i < 2; i++)
                Main.audioSystem.UpdateCommonTrackTowardStopping(MusicID.RainSoundEffect, Main.ambientVolume, ref rainFade, rainFade >= 0.2f);
            Main.musicFade[MusicID.RainSoundEffect] = rainFade;

            const int windMusicID = 45; // I'm not sure why there isn't a MusicID value for this.
            float windFade = Main.musicFade[windMusicID];
            for (int i = 0; i < 2; i++)
                Main.audioSystem.UpdateCommonTrackTowardStopping(windMusicID, Main.ambientVolume, ref windFade, windFade >= 0.2f);
            Main.musicFade[windMusicID] = windFade;
        }
    }

    public static int DecideRandomQuestFish()
    {
        return ModContent.ItemType<Veilray>();
    }

    public static void LimitQuestFishDuringRiftEclipse(ILContext context, ManagedILEdit edit)
    {
        ILCursor cursor = new ILCursor(context);

        // Go before the NetMessage.SendAnglerQuest update.
        if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchCallOrCallvirt<NetMessage>("SendAnglerQuest")))
        {
            edit.LogFailure("The NetMessage.SendAnglerQuest call could not be found.");
            return;
        }

        // Go before the -1 constant load.
        if (!cursor.TryGotoPrev(MoveType.Before, i => i.MatchLdcI4(-1)))
        {
            edit.LogFailure("The -1 constant load could not be found.");
            return;
        }

        cursor.EmitDelegate(() =>
        {
            if (RiftEclipseOngoing)
                Main.anglerQuest = Main.anglerQuestItemNetIDs.ToList().IndexOf(DecideRandomQuestFish());
        });
    }

    public static void DisableWorldEvilSpreading(ILContext context, ManagedILEdit edit)
    {
        ILCursor cursor = new ILCursor(context);

        // Go before the 'WorldGen.AllowedToSpreadInfections = ' line.
        if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchStsfld<WorldGen>("AllowedToSpreadInfections")))
        {
            edit.LogFailure("The AllowedToSpreadInfections storage could not be found.");
            return;
        }

        // Convert the base 'WorldGen.AllowedToSpreadInfections = true' line into the following:
        // WorldGen.AllowedToSpreadInfections = true & !RiftEclipseOngoing;
        cursor.Emit(OpCodes.Call, typeof(RiftEclipseManagementSystem).GetMethod($"get_{nameof(RiftEclipseOngoing)}")!);
        cursor.Emit(OpCodes.Not);
        cursor.Emit(OpCodes.And);
    }

    private void DisableAbovegroundGrassGrowth(On_WorldGen.orig_UpdateWorld_GrassGrowth orig, int i, int j, int minI, int maxI, int minJ, int maxJ, bool underground)
    {
        if (!underground && RiftEclipseOngoing)
            return;

        orig(i, j, minI, maxI, minJ, maxJ, underground);
    }

    private bool DisableAbovegroundVineGrowth(On_WorldGen.orig_GrowMoreVines orig, int x, int y)
    {
        if (x < Main.worldSurface && RiftEclipseOngoing)
            return false;
        return orig(x, y);
    }

    private void RandomlyDestroyVegetation(int x, int y, int type)
    {
        bool isThorns = type == TileID.CorruptThorns || type == TileID.CrimsonThorns || type == TileID.JungleThorns;
        bool isGrass = type == TileID.Plants || type == TileID.Plants2 || type == TileID.CorruptPlants || type == TileID.CrimsonPlants || type == TileID.HallowedPlants || type == TileID.HallowedPlants2 || type == TileID.JunglePlants || type == TileID.JunglePlants2;
        bool isVines = type == TileID.Vines || type == TileID.VineFlowers || type == TileID.CorruptVines || type == TileID.CrimsonVines || type == TileID.HallowedVines || type == TileID.JungleVines;
        bool isJunglePlant = type == TileID.PlantDetritus;
        if (!isThorns && !isGrass && !isVines && !isJunglePlant)
            return;

        if (y >= Main.worldSurface)
            return;

        if (!RiftEclipseOngoing)
            return;

        float oldMuffleFactor = SoundMufflingSystem.MuffleFactor;
        SoundMufflingSystem.MuffleFactor = 0f;
        WorldGen.KillTile(x, y);
        SoundMufflingSystem.MuffleFactor = oldMuffleFactor;
    }

    public override void PostUpdateNPCs()
    {
        // Keep the Avatar over the moon if necessary.
        if (RiftEclipseOngoing)
        {
            RiftEclipseSky.MoveOverSunInterpolant = 1f;
            RiftEclipseSky.RiftScaleFactor = RiftEclipseSky.ScaleWhenOverSun;
            RiftEclipseSky.IsEnabled = true;
            RiftScale = Lerp(RiftScale, 1f, 0.041f);
        }

        // If the Avatar was over the moon but isn't anymore, get rid of him.
        else if (RiftEclipseSky.MoveOverSunInterpolant != 0f && MarsBody.Myself is null)
        {
            RiftEclipseSky.RiftScaleFactor = 0f;
            RiftEclipseSky.MoveOverSunInterpolant = 0f;
            RiftEclipseSky.IsEnabled = false;
        }
    }

    public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
    {
        // Make the sky darker if the Avatar is covering it.
        if (RiftEclipseSky.IsEnabled)
        {
            RealisticSkyCompatibility.SunBloomOpacity = 0f;

            Color originalBackgroundColor = backgroundColor;
            Color originalTileColor = backgroundColor;

            float backgroundDarkness = InverseLerp(0.4f, 0.15f, backgroundColor.ToVector3().Length()) + IntensityInterpolant * 0.6f;
            Color idealBackgroundColor = Color.Lerp(new(47, 9, 30), Color.Black, backgroundDarkness);
            idealBackgroundColor = Color.Lerp(idealBackgroundColor, new(35, 36, 76), InverseLerp(0.25f, 0.7f, Main.cloudAlpha - backgroundDarkness) * 0.5f);
            backgroundColor = Color.Lerp(backgroundColor, idealBackgroundColor, 0.85f);
            tileColor = Color.Lerp(tileColor, new(3, 3, 3), Lerp(0.23f, 0.6f, IntensityInterpolant));

            // Interpolate between the new and old background color based on the Avatar's scale.
            backgroundColor = Color.Lerp(originalBackgroundColor, backgroundColor, RiftScale * RiftEclipseSky.RiftScaleFactor / RiftEclipseSky.ScaleWhenOverSun);
            tileColor = Color.Lerp(originalTileColor, tileColor, RiftScale * RiftEclipseSky.RiftScaleFactor / RiftEclipseSky.ScaleWhenOverSun);
        }
    }
}
