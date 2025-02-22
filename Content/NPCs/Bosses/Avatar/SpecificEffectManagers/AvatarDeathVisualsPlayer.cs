using Luminance.Common.Easings;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Assets.Fonts;
using NoxusBoss.Content.NPCs.Bosses.Avatar.FirstPhaseForm;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity;
using NoxusBoss.Core.SoundSystems;
using NoxusBoss.Core.SoundSystems.Music;
using ReLogic.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SpecificEffectManagers;

[Autoload(Side = ModSide.Client)]
public class AvatarDeathVisualsPlayer : ModPlayer
{
    /// <summary>
    /// The overriding timer that dictates the duration of the player's death animation.
    /// </summary>
    public int DeathTimerOverride
    {
        get;
        set;
    }

    /// <summary>
    /// Whether the player was killed by the Avatar of Emptiness or its rift.
    /// </summary>
    public bool WasKilledByAvatar
    {
        get;
        set;
    }

    /// <summary>
    /// The music that was played at the time of the player's death.
    /// </summary>
    public int? MusicAtTimeOfDeath
    {
        get;
        set;
    }

    /// <summary>
    /// How long the player's death animation goes on for.
    /// </summary>
    public static int AnimationDuration => SecondsToFrames(6f);

    /// <summary>
    /// The screen contents at the time of the player's death.
    /// </summary>
    public static ManagedRenderTarget ScreenAtTimeOfDeath
    {
        get;
        private set;
    }

    public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource)
    {
        // Determine if the death resulted from the Avatar. Technically there are a handful of niche cases can cause this where it wasn't actually the Avatar who killed the player, such as
        // if two players enabled PVP while the Avatar is killing both of them, but honestly who cares.
        WasKilledByAvatar = NamelessDeityBoss.Myself is null && (AvatarRift.Myself is not null || AvatarOfEmptiness.Myself is not null);

        if (AvatarRift.Myself is not null)
            MusicAtTimeOfDeath = AvatarRift.Myself.As<AvatarRift>().Music;
        if (AvatarOfEmptiness.Myself is not null)
            MusicAtTimeOfDeath = AvatarOfEmptiness.Myself.As<AvatarOfEmptiness>().Music;

        return true;
    }

    public override void Load()
    {
        ScreenAtTimeOfDeath = new(true, ManagedRenderTarget.CreateScreenSizedTarget);
        RenderTargetManager.RenderTargetUpdateLoopEvent += TakeSnapshotIfNecessary;
        Main.OnPostDraw += RenderDeathAnimationEffect;
    }

    public override void Unload() => Main.OnPostDraw -= RenderDeathAnimationEffect;

    private void TakeSnapshotIfNecessary()
    {
        if (!Main.LocalPlayer.TryGetModPlayer(out AvatarDeathVisualsPlayer player))
            return;

        if (player.DeathTimerOverride <= 0 || player.DeathTimerOverride >= 3)
            return;

        ScreenAtTimeOfDeath.CopyContentsFrom(Main.screenTarget);
    }

    private void RenderDeathAnimationEffect(GameTime obj)
    {
        if (!Main.LocalPlayer.TryGetModPlayer(out AvatarDeathVisualsPlayer player))
            return;

        if (player.DeathTimerOverride <= 0)
            return;

        // Prevent the game from being paused.
        Main.gamePaused = false;

        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

        float animationCompletion = player.DeathTimerOverride / (float)AnimationDuration;
        float invertAnimationCompletion = EasingCurves.Cubic.Evaluate(EasingType.InOut, InverseLerp(0.02f, 0.2f, animationCompletion));

        ManagedShader animationShader = ShaderManager.GetShader("NoxusBoss.AvatarPlayerDeathAnimationShader");
        animationShader.TrySetParameter("invertAnimationCompletion", invertAnimationCompletion);
        animationShader.TrySetParameter("blackDissolveInterpolant", EasingCurves.Cubic.Evaluate(EasingType.InOut, InverseLerp(0.2f, 0.6f, animationCompletion)));
        animationShader.SetTexture(WatercolorNoiseB, 1, SamplerState.LinearWrap);
        animationShader.SetTexture(WavyBlotchNoiseDetailed, 2, SamplerState.LinearWrap);
        animationShader.SetTexture(PerlinNoise, 3, SamplerState.LinearWrap);
        animationShader.Apply();

        Main.spriteBatch.Draw(ScreenAtTimeOfDeath, Main.rand.NextVector2Unit() * Convert01To010(invertAnimationCompletion) * 20f, Color.White);

        Main.pixelShader.CurrentTechnique.Passes[0].Apply();

        string text = Language.GetTextValue("Mods.NoxusBoss.Dialog.AvatarPlayerDeathText");
        DynamicSpriteFont font = FontRegistry.Instance.NamelessDeityText;
        float scale = 1.3f;
        float maxHeight = 300f;
        float textOpacity = InverseLerpBump(0.5f, 0.6f, 0.95f, 1f, animationCompletion);
        Vector2 textSize = font.MeasureString(text);
        if (textSize.Y > maxHeight)
            scale = maxHeight / textSize.Y;
        Vector2 textDrawPosition = ViewportSize * 0.5f - textSize * scale * 0.5f;
        ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, text, textDrawPosition, Color.Red * Pow(textOpacity, 1.6f), 0f, Vector2.Zero, new(scale), -1f, 2f);

        Main.spriteBatch.End();
    }

    public override void PreUpdate()
    {
        if (!Player.dead)
        {
            DeathTimerOverride = 0;
            MusicAtTimeOfDeath = null;
        }
        else if (WasKilledByAvatar)
        {
            if (Main.myPlayer == Player.whoAmI)
            {
                if (DeathTimerOverride == 1)
                {
                    SoundEngine.StopTrackedSounds();
                    SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.PlayerKill);
                }

                if (DeathTimerOverride == (int)(AnimationDuration * 0.5f))
                    SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.PlayerPerishTextAppear);
            }

            SoundMufflingSystem.MuffleFactor = 0f;
            MusicVolumeManipulationSystem.MuffleFactor = 1f + DeathTimerOverride / (float)AnimationDuration * 0.6f;

            DeathTimerOverride = Utils.Clamp(DeathTimerOverride + 1, 0, AnimationDuration);
            if (DeathTimerOverride < AnimationDuration)
                Player.respawnTimer = 8;
        }
    }
}
