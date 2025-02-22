using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.Buffs;
using NoxusBoss.Content.Items;
using NoxusBoss.Core.Autoloaders;
using NoxusBoss.Core.Data;
using NoxusBoss.Core.Graphics.RenderTargets;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Graphics.Nilk;

public class NilkMusicScene : ModSceneEffect
{
    public override bool IsSceneEffectActive(Player player) => NilkEffectManager.NilkInsanityInterpolant >= 0.5f;

    public override SceneEffectPriority Priority => (SceneEffectPriority)20;

    public override int Music => MusicLoader.GetMusicSlot(Mod, "Assets/Sounds/Music/NilkSongBoss");

    public override void Load()
    {
        string contentPath = GetAssetPath("Content/Items/Placeable/MusicBoxes", "NilkSongMusicBox");
        string musicPath = "Assets/Sounds/Music/NilkSongBoss";
        MusicBoxAutoloader.Create(Mod, contentPath, musicPath, out _, out _);
    }
}

[Autoload(Side = ModSide.Client)]
public class NilkEffectManager : ModSystem
{
    /// <summary>
    /// The Nilk Insanity interpolant.
    /// </summary>
    public static float NilkInsanityInterpolant
    {
        get;
        set;
    }

    /// <summary>
    /// The current palette of the Nilk shader.
    /// </summary>
    public static Vector3[]? CurrentPalette
    {
        get;
        private set;
    }

    /// <summary>
    /// How long it takes until the Nilk shader palette should be shuffled.
    /// </summary>
    public static int CountdownUntilPaletteShuffle
    {
        get;
        private set;
    }

    /// <summary>
    /// The previous render frame of the game.
    /// </summary>
    public static InstancedRequestableTarget PreviousFrame
    {
        get;
        private set;
    }

    public const string PaletteFilePath = "Core/Graphics/Nilk/NilkShaderPalettes.json";

    public override void PostSetupContent()
    {
        On_SoundPlayer.Play_Inner += RandomizePitch;
        Main.OnPreDraw += RenderPreviousFrame;

        PreviousFrame = new InstancedRequestableTarget();
        Main.ContentThatNeedsRenderTargets.Add(PreviousFrame);
    }

    public override void OnModUnload() => Main.OnPreDraw -= RenderPreviousFrame;

    private void RenderPreviousFrame(GameTime obj)
    {
        if (NilkInsanityInterpolant > 0f)
        {
            PreviousFrame.Request((int)ViewportSize.X, (int)ViewportSize.Y, 0, () =>
            {
                Main.spriteBatch.Begin();
                Main.spriteBatch.Draw(ShaderManager.AuxiliaryTarget, ViewportArea, Color.White);
                Main.spriteBatch.End();
            });
        }
    }

    private SlotId RandomizePitch(On_SoundPlayer.orig_Play_Inner orig, SoundPlayer self, ref SoundStyle style, Vector2? position, SoundUpdateCallback updateCallback)
    {
        if (NilkInsanityInterpolant > 0f && !Main.gameMenu)
        {
            SoundStyle copy = style;
            int nilkDebuffID = Main.LocalPlayer.FindBuffIndex(ModContent.BuffType<NilkDebuff>());
            if (nilkDebuffID != -1)
                copy.Pitch += Main.rand.NextFloat(-0.4f, 0.6f) * NilkInsanityInterpolant.Squared();
            return orig(self, ref copy, position, updateCallback);
        }
        return orig(self, ref style, position, updateCallback);
    }

    public override void PreUpdateEntities()
    {
        NilkInsanityInterpolant = 0f;
        if (!ShaderManager.TryGetFilter("NoxusBoss.NilkScreenDistortionShader", out ManagedScreenFilter nilkShader))
            return;

        int nilkDebuffID = Main.LocalPlayer.FindBuffIndex(ModContent.BuffType<NilkDebuff>());
        if (nilkDebuffID == -1)
            return;

        float nilkEffectCompletion = 1f - Saturate(Main.LocalPlayer.buffTime[nilkDebuffID] / (float)NilkCarton.DebuffDuration);
        NilkInsanityInterpolant = InverseLerpBump(0f, 0.3f, 0.9f, 1f, nilkEffectCompletion);
        if (NilkInsanityInterpolant > 0f && PreviousFrame.TryGetTarget(0, out RenderTarget2D? previousFrameTarget) && previousFrameTarget is not null)
        {
            if (CurrentPalette is null)
                ShufflePalette();

            nilkShader.TrySetParameter("intensity", NilkInsanityInterpolant);
            nilkShader.TrySetParameter("palette", CurrentPalette);
            nilkShader.TrySetParameter("datamoshIntensity", 0.51f);
            nilkShader.SetTexture(NilkOverlayVisualsManager.OverlayTarget, 2, SamplerState.LinearClamp);
            nilkShader.SetTexture(previousFrameTarget, 3);
            nilkShader.SetTexture(PerlinNoise, 4, SamplerState.LinearWrap);
            nilkShader.Activate();
        }

        CountdownUntilPaletteShuffle--;
        if (CountdownUntilPaletteShuffle <= 0)
        {
            ShufflePalette();

            CountdownUntilPaletteShuffle = MinutesToFrames(2f);
            if (Main.rand.NextBool(3))
                CountdownUntilPaletteShuffle = MinutesToFrames(1f);
            if (Main.rand.NextBool(6))
                CountdownUntilPaletteShuffle = MinutesToFrames(0.4f);
            if (Main.rand.NextBool(9))
                CountdownUntilPaletteShuffle = MinutesToFrames(0.05f);
            if (Main.rand.NextBool(15))
                CountdownUntilPaletteShuffle = MinutesToFrames(0.01f);
        }
    }

    /// <summary>
    /// Picks a new random palette for the nilk effect to use.
    /// </summary>
    public static void ShufflePalette()
    {
        List<Vector3[]> potentialPalettes = LocalDataManager.Read<Vector3[]>(PaletteFilePath).Values.ToList();
        CurrentPalette = potentialPalettes[Main.rand.Next(potentialPalettes.Count)];
    }
}
