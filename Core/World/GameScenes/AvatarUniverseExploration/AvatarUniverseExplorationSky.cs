using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.NPCs.Bosses.CeaselessVoid;
using NoxusBoss.Core.CrossCompatibility.Inbound.BaseCalamity;
using NoxusBoss.Core.CrossCompatibility.Inbound.RealisticSky;
using NoxusBoss.Core.DataStructures;
using NoxusBoss.Core.GlobalInstances;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace NoxusBoss.Core.World.GameScenes.AvatarUniverseExploration;

[JITWhenModsEnabled(CalamityCompatibility.ModName)]
[ExtendsFromMod(CalamityCompatibility.ModName)]
public class AvatarUniverseExplorationSky : CustomSky
{
    private bool isActive;

    internal static float intensity;

    public static float PushPlayersOutInterpolant
    {
        get
        {
            int time = 0;
            foreach (Player player in Main.ActivePlayers)
                time = Math.Max(time, player.GetValueRef<int>(TimeInUniverseVariableName));

            return InverseLerp(900f, 1150f, time).Squared();
        }
    }

    public static float ForcefulSmokeRadius
    {
        get
        {
            float reelBack = Convert01To010(InverseLerp(0f, 0.25f, PushPlayersOutInterpolant)) * 1500f;
            return SmoothStep(3200f, -210f, PushPlayersOutInterpolant) + reelBack;
        }
    }

    public const string TimeInUniverseVariableName = "TimeSpentInExplorableAvatarUniverse";

    public const string ScreenShaderKey = "NoxusBoss:AvatarUniverseExplorationSky";

    public override void OnLoad()
    {
        PlayerDataManager.PostUpdateEvent += UpdateInUniverse;
    }

    private void UpdateInUniverse(PlayerDataManager p)
    {
        Referenced<int> timeSpentInUniverse = p.GetValueRef<int>(TimeInUniverseVariableName);

        if (AvatarUniverseExplorationSystem.InAvatarUniverse)
        {
            timeSpentInUniverse.Value++;

            Vector2 smokeCenter = p.Player.Center;
            int riftIndex = NPC.FindFirstNPC(ModContent.NPCType<CeaselessVoidRift>());
            if (riftIndex != -1)
                smokeCenter = Main.npc[riftIndex].Center;

            // Push the player away from the smoke radius.
            float pushForce = Lerp(24f, 48f, PushPlayersOutInterpolant);
            if (!p.Player.WithinRange(smokeCenter, ForcefulSmokeRadius + 150f))
                p.Player.velocity = Vector2.Lerp(p.Player.velocity, p.Player.SafeDirectionTo(smokeCenter) * pushForce, 0.1f);

            if (PushPlayersOutInterpolant >= 1f && !AvatarUniverseExplorationSystem.EnteringRift)
                AvatarUniverseExplorationSystem.Exit();

            ScreenShakeSystem.SetUniversalRumble(PushPlayersOutInterpolant.Squared() * 20f, TwoPi, null, 0.45f);
        }
        else
            timeSpentInUniverse.Value = 0;
    }

    public override void Update(GameTime gameTime)
    {
        // Make the intensity go up or down based on whether the sky is in use.
        if (!Main.gamePaused)
        {
            intensity = Saturate(intensity + isActive.ToDirectionInt() * 0.25f);
            UpdateForegroundFog();
            UpdateForcefulSmoke();
        }
    }

    public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
    {
        RealisticSkyCompatibility.SunBloomOpacity = 0f;
        if (maxDepth < float.MaxValue || minDepth >= float.MaxValue)
            return;

        RenderBackgroundFog();
    }

    private static void RenderBackgroundFog()
    {
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);

        ManagedShader fogShader = ShaderManager.GetShader("NoxusBoss.AvatarUniverseFogBackgroundShader");
        fogShader.TrySetParameter("arcCurvature", 2.2f);
        fogShader.TrySetParameter("fogColor", new Vector4(0.04f, 0.04f, 0.054f, 1f));
        fogShader.SetTexture(PerlinNoise, 1, SamplerState.LinearWrap);
        fogShader.Apply();

        Vector2 screenArea = ViewportSize;
        Vector2 textureArea = screenArea / WhitePixel.Size();
        Main.spriteBatch.Draw(WhitePixel, screenArea * 0.5f, null, Color.Black * intensity, 0f, WhitePixel.Size() * 0.5f, textureArea, 0, 0f);

        Main.spriteBatch.ResetToDefault();
    }

    private static void UpdateForegroundFog()
    {
        ManagedScreenFilter fogShader = ShaderManager.GetFilter("NoxusBoss.AvatarUniverseRedFogShader");
        fogShader.TrySetParameter("intensity", intensity);
        fogShader.TrySetParameter("fogDensityExponent", 5.6f);
        fogShader.TrySetParameter("fogColor", new Vector4(1f, 0f, 0.15f, 0f));
        fogShader.SetTexture(PerlinNoise, 1, SamplerState.LinearWrap);
        fogShader.Activate();
    }

    private static void UpdateForcefulSmoke()
    {
        Vector2 smokeCenter = Main.LocalPlayer.Center;
        int riftIndex = NPC.FindFirstNPC(ModContent.NPCType<CeaselessVoidRift>());
        if (riftIndex != -1)
            smokeCenter = Main.npc[riftIndex].Center;

        ManagedScreenFilter smokeShader = ShaderManager.GetFilter("NoxusBoss.AvatarUniverseSuperSmokeShader");
        smokeShader.TrySetParameter("smokeCenter", Vector2.Transform(smokeCenter - Main.screenPosition, Main.GameViewMatrix.TransformationMatrix));
        smokeShader.TrySetParameter("generalSmokeColor", new Color(62, 34, 62).ToVector3());
        smokeShader.TrySetParameter("zoom", Main.GameViewMatrix.Zoom);
        smokeShader.TrySetParameter("radius", ForcefulSmokeRadius);
        smokeShader.SetTexture(PerlinNoise, 1, SamplerState.LinearWrap);
        smokeShader.Activate();
    }

    #region Boilerplate
    public override void Activate(Vector2 position, params object[] args)
    {
        isActive = true;
    }

    public override void Deactivate(params object[] args)
    {
        isActive = false;
    }

    public override float GetCloudAlpha() => Main.gameMenu ? 0f : 1f - intensity;

    public override bool IsActive() => isActive || intensity > 0f;

    public override void Reset()
    {
        isActive = false;
    }

    #endregion Boilerplate
}
