using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.Dusts;
using NoxusBoss.Content.Projectiles;
using NoxusBoss.Content.Projectiles.Visuals;
using NoxusBoss.Core.Data;
using NoxusBoss.Core.Fixes;
using NoxusBoss.Core.Graphics.SpecificEffectManagers;
using NoxusBoss.Core.SoundSystems.Music;
using NoxusBoss.Core.World.Subworlds;
using NoxusBoss.Core.World.TileDisabling;
using SubworldLibrary;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Core.World.GameScenes.TerminusStairway;

public class TerminusStairwaySystem : ModSystem
{
    /// <summary>
    /// Whether the stairway effect is enabled.
    /// </summary>
    public static bool Enabled
    {
        get;
        private set;
    }

    /// <summary>
    /// The countdown used before the next step sound can be played.
    /// </summary>
    public static int StepSoundCountdown
    {
        get;
        set;
    }

    /// <summary>
    /// How long it's been since the effect was enabled.
    /// </summary>
    public static int Time
    {
        get;
        set;
    }

    /// <summary>
    /// How long it takes until a new column rise sound can be played.
    /// </summary>
    public static int ColumnRiseSoundReplayDelay
    {
        get;
        set;
    }

    /// <summary>
    /// How far the player has walked up the stairs.
    /// </summary>
    public static float WalkDistance
    {
        get;
        set;
    }

    /// <summary>
    /// 
    /// </summary>
    public static float StairAscentStartX
    {
        get;
        set;
    }

    /// <summary>
    /// The amount by which the player leave animation has progressed.
    /// </summary>
    public static float LeaveInterpolant
    {
        get;
        set;
    }

    /// <summary>
    /// The amount by which the clouds should appear at the top of the world, irrespective of whether <see cref="Enabled"/> is <see langword="true"/>.
    /// </summary>
    public static float CloudTopOfWorldAppearInterpolant
    {
        get;
        set;
    }

    /// <summary>
    /// The starting point of the stairs.
    /// </summary>
    public static Vector2 StairsStart => new(Main.maxTilesX * 8f, (float)Main.worldSurface * 16f - 1050f);

    /// <summary>
    /// The lowest point at which stairs can appear.
    /// </summary>
    public static float StairsBottomY => (float)Main.worldSurface * 16f - 300f;

    /// <summary>
    /// How far up the player has to walk up the stairs to reach the garden.
    /// </summary>
    public static float NecessaryWalkDistance => 12000f;

    /// <summary>
    /// How far along the player is with walking up the stairs.
    /// </summary>
    public static float WalkAnimationInterpolant => Saturate(WalkDistance / NecessaryWalkDistance);

    /// <summary>
    /// How steep individual steps should be.
    /// </summary>
    public static float StairSteepnessFactor => 0.38f;

    /// <summary>
    /// The file path for the color palettes used by this effect.
    /// </summary>
    public const string PaletteFilePath = "Core/World/GameScenes/TerminusStairway/TerminusStairwayPalettes.json";

    /// <summary>
    /// Reads a palette of a given key from the <see cref="PaletteFilePath"/>.
    /// </summary>
    /// <param name="key">The palette's identifying key.</param>
    public static Vector3[] ReadPalette(string key)
    {
        Vector3[] palette = LocalDataManager.Read<Vector3[]>(PaletteFilePath)[key];
        return palette;
    }

    /// <summary>
    /// Starts the stairway effect.
    /// </summary>
    public static void Start()
    {
        Main.musicFade[Main.curMusic] = 0f;

        Enabled = true;
        TileDisablingSystem.TilesAreUninteractable = true;

        // Get all players to a consistent location.
        foreach (Player player in Main.ActivePlayers)
        {
            player.Center = StairsStart + new Vector2(1f, -StairSteepnessFactor) * 1000f;

            if (Main.myPlayer == player.whoAmI)
            {
                Vector2 beamSpawnPosition = player.Center + Vector2.UnitY * 1150f;
                NewProjectileBetter(new EntitySource_WorldEvent(), beamSpawnPosition, Vector2.Zero, ModContent.ProjectileType<TerminusStairAppearanceBeam>(), 0, 0f, player.whoAmI);
            }
        }

        foreach (Gore gore in Main.gore)
            gore.active = false;
        foreach (Dust dust in Main.dust)
            dust.active = false;

        StairAscentStartX = Main.LocalPlayer.Center.X + 50f;
    }

    /// <summary>
    /// Prompts the stairway effect to end, making the player leave.
    /// </summary>
    public static void Leave()
    {
        if (LeaveInterpolant > 0f)
            return;

        LeaveInterpolant = 0.01f;
        BlockerSystem.Start(true, false, () => Enabled);
    }

    public override void OnModLoad()
    {
        On_Player.DryCollision += WalkUpStairs;
        On_Main.DrawNPCs += RenderStairsWrapper;
        On_Player.SpawnFastRunParticles += DisableHermesBootsSounds;
        Main.OnPostDraw += RenderLight;
        Main.OnPostDraw += RenderBottomFog;

        MapStyleLockingSystem.RegisterConditionSet(1, true, () => Enabled);
    }

    public override void OnWorldLoad() => Enabled = false;

    public override void OnWorldUnload() => Enabled = false;

    public override void PostUpdateEverything()
    {
        if (Enabled)
        {
            for (int i = 0; i < 8; i++)
                CreateGodRayParticles();

            // Update timers.
            StepSoundCountdown--;
            ColumnRiseSoundReplayDelay--;
            if (!AnyProjectiles(ModContent.ProjectileType<TerminusStairAppearanceBeam>()))
                Time++;

            CloudTopOfWorldAppearInterpolant = 0f;

            if (Main.SceneMetrics.ActiveMusicBox >= 0)
                MusicVolumeManipulationSystem.MuffleFactor = Pow(InverseLerp(-0.01f, 0.7f, WalkAnimationInterpolant), 0.51f);

            foreach (Player player in Main.ActivePlayers)
            {
                // Disable fall damage.
                player.fallStart = player.fallStart2 = (int)(player.TopLeft.Y / 16f);

                SendPlayerToGardenIfNecessary(player);
                if (player.position.Y >= StairsBottomY - 180f)
                    Leave();
            }
            UpdateStairEnd(Main.rand.NextFloat(0.02f, 0.025f));

            // Create wind.
            if (Main.rand.NextBool(7))
            {
                Vector2 windVelocity = Vector2.UnitX * -Main.rand.NextFloat(10f, 14f);
                Vector2 spawnPosition = Main.LocalPlayer.Center + new Vector2(Sign(windVelocity.X) * -Main.rand.NextFloat(1050f, 1250f), Main.rand.NextFloatDirection() * 1250f);
                Projectile.NewProjectile(Main.LocalPlayer.GetSource_FromThis(), spawnPosition, windVelocity, ModContent.ProjectileType<WindStreakVisual>(), 0, 0f, Main.myPlayer);
            }

            if (LeaveInterpolant > 0f)
                UpdateLeaveEffects();
        }
        else
        {
            Time = 0;
            WalkDistance = 0f;
            LeaveInterpolant = 0f;
            CloudTopOfWorldAppearInterpolant = Saturate(CloudTopOfWorldAppearInterpolant - 0.007f);
        }
    }

    private void WalkUpStairs(On_Player.orig_DryCollision orig, Player self, bool fallThrough, bool ignorePlats)
    {
        if (Enabled)
        {
            bool onGround = EnsurePlayerStaysAboveStairs(self);
            HandlePlayerWalkMovement(self, onGround);
            return;
        }

        orig(self, fallThrough, ignorePlats);
    }

    /// <summary>
    /// Makes the leave animation progress.
    /// </summary>
    private static void UpdateLeaveEffects()
    {
        LeaveInterpolant = Saturate(LeaveInterpolant + 0.027f);
        TotalScreenOverlaySystem.OverlayInterpolant = SmoothStep(0f, 1f, LeaveInterpolant);
        TotalScreenOverlaySystem.OverlayColor = Color.White;

        Main.LocalPlayer.velocity = Vector2.Zero;

        if (LeaveInterpolant >= 1f)
        {
            LeaveInterpolant = 0f;
            CloudTopOfWorldAppearInterpolant = 1f;
            Main.LocalPlayer.Center = new(StairsStart.X, 500f);
            if (Collision.SolidCollision(Main.LocalPlayer.TopLeft, Main.LocalPlayer.width, Main.LocalPlayer.height))
                Main.LocalPlayer.position.Y += 8f;

            Enabled = false;
        }
    }

    /// <summary>
    /// Updates the horizontal stair end position.
    /// </summary>
    /// <param name="updateInterpolant">How quickly, as a 0-1 interpolant, the stair's horizontal position should move.</param>
    private static void UpdateStairEnd(float updateInterpolant)
    {
        float desiredEnd = 0f;
        foreach (Player player in Main.ActivePlayers)
        {
            float endForPlayer = player.Center.X + 1036f;
            desiredEnd = MathF.Max(desiredEnd, endForPlayer);
        }

        StairAscentStartX = Lerp(StairAscentStartX, desiredEnd, updateInterpolant);
    }

    /// <summary>
    /// Checks if a given player should be sent to the garden or not.
    /// </summary>
    /// <param name="player">The player to check.</param>
    private static void SendPlayerToGardenIfNecessary(Player player)
    {
        if (player.position.Y <= 700f && Main.myPlayer == player.whoAmI)
        {
            Enabled = false;

            EternalGardenNew.ClientWorldDataTag = EternalGardenNew.SafeWorldDataToTag("Client", false);
            EternalGardenIntroBackgroundFix.ShouldDrawWhite = true;
            SubworldSystem.Enter<EternalGardenNew>();
        }
    }

    /// <summary>
    /// Clamps a given player's position to be on the stairs.
    /// </summary>
    /// <param name="player">The player whose position should be clamped.</param>
    private static bool EnsurePlayerStaysAboveStairs(Player player)
    {
        // Calculate the bottom Y position of the stairs.
        Vector2 stairsStart = StairsStart;
        float horizontalOffsetFromCenter = player.Center.X - stairsStart.X;
        float bottom = stairsStart.Y + horizontalOffsetFromCenter * -StairSteepnessFactor;
        if (bottom > StairsBottomY)
            bottom = StairsBottomY;

        // Ensure that the player does not go below the stairs.
        bool onGround = false;
        if (player.Bottom.Y >= bottom)
        {
            onGround = true;
            player.Bottom = new(player.Bottom.X, bottom);
            if (player.velocity.Y > 0f)
                player.velocity.Y = 0f;
        }

        return onGround;
    }

    /// <summary>
    /// Handles the movement part of player/stair interactions.
    /// </summary>
    /// <param name="player">The player to perform interactions for.</param>
    /// <param name="onGround">Whether the player is on the ground or not.</param>
    private static void HandlePlayerWalkMovement(Player player, bool onGround)
    {
        // Update how much the player has walked, assuming they haven't reached the top yet.
        if (WalkDistance < NecessaryWalkDistance)
            WalkDistance = Clamp(WalkDistance + player.velocity.X, 0f, NecessaryWalkDistance);

        // Update the player's position.
        player.position += player.velocity;

        // Play step sounds as the player moves on the stairs. They are higher pitched the faster the player is moving.
        // Furthermore, it takes less time for the next potential sound to occur the faster the player is moving.
        float horizontalSpeed = Abs(player.velocity.X);
        if (horizontalSpeed >= 2f && onGround)
            ApplyStepVisuals(player);
    }

    /// <summary>
    /// Applies step visual and sound effects as they move up the stairs.
    /// </summary>
    /// <param name="player">The player to create effects relative to.</param>
    private static void ApplyStepVisuals(Player player)
    {
        int dustCount = 1;
        float dustLingerance = 1.1f;
        float horizontalSpeed = Abs(player.velocity.X);
        if (StepSoundCountdown <= 0)
        {
            float volume = InverseLerp(1900f, 3850f, player.Bottom.Y) * 0.4f;
            float stepSoundPitch = Utils.Remap(horizontalSpeed, 4f, 11f, 0f, 0.55f);
            SoundEngine.PlaySound(GennedAssets.Sounds.Environment.DivineStairwayStep with { Pitch = stepSoundPitch, Volume = volume, PitchVariance = 0.15f, MaxInstances = 15 }, player.Bottom);

            // Define the delay for the next step sound.
            StepSoundCountdown = (int)Utils.Remap(horizontalSpeed, 4f, 11f, 30f, 2f);
            dustCount += 5;
            dustLingerance += 0.9f;

            // Add a very tiny screen shake.
            ScreenShakeSystem.StartShakeAtPoint(player.Bottom, 1f);
        }

        // Create a little bit of fancy dust at the player's feet.
        for (int i = 0; i < dustCount; i++)
        {
            Color dustColor = Main.hslToRgb(Main.rand.NextFloat(0.94f, 1.25f) % 1f, 1f, 0.84f);
            dustColor.A /= 8;

            Vector2 footSpawnPosition = Vector2.Lerp(player.BottomLeft, player.BottomRight, Main.rand.NextFloat());
            Dust light = Dust.NewDustPerfect(footSpawnPosition + Main.rand.NextVector2Circular(3f, 3f), 267, -Vector2.UnitY.RotatedByRandom(0.6f) * Main.rand.NextFloat(0.5f, 4f) * dustLingerance, 0, dustColor);
            light.scale = 0.1f;
            light.fadeIn = Main.rand.NextFloat(dustLingerance);
            light.noGravity = true;
            light.noLight = true;
        }
    }

    private void RenderStairsWrapper(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles)
    {
        orig(self, behindTiles);

        if (Enabled)
            RenderStairs();
    }

    private void DisableHermesBootsSounds(On_Player.orig_SpawnFastRunParticles orig, Player self)
    {
        if (Enabled)
            self.runSoundDelay = self.hermesStepSound.IntendedCooldown;

        orig(self);
    }

    /// <summary>
    /// Renders the glowing light, with maximum layering priority, at the bottom of the stairs, or at the top of the world if necessary.
    /// </summary>
    private static void RenderBottomFog(GameTime obj)
    {
        if (Main.gameMenu)
            return;
        if (!Enabled && CloudTopOfWorldAppearInterpolant <= 0f)
            return;

        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

        float startingY = StairsBottomY - 300f;
        float endingY = StairsBottomY - 750f;
        if (CloudTopOfWorldAppearInterpolant > 0f)
        {
            startingY = 0f;
            endingY = SmoothStep(150f, 1200f, Sqrt(CloudTopOfWorldAppearInterpolant));
        }

        Vector2 screenArea = ViewportSize;
        ManagedShader lightShader = ShaderManager.GetShader("NoxusBoss.TerminusBottomOfStairsCloudShader");
        lightShader.TrySetParameter("screenPosition", Main.screenPosition);
        lightShader.TrySetParameter("startingY", startingY);
        lightShader.TrySetParameter("endingY", endingY);
        lightShader.TrySetParameter("parallaxOffset", Main.screenPosition / Main.ScreenSize.ToVector2());
        lightShader.TrySetParameter("screenSize", screenArea);
        lightShader.SetTexture(PerlinNoise, 1, SamplerState.LinearWrap);
        lightShader.Apply();

        Vector2 textureArea = screenArea / WhitePixel.Size();
        Main.spriteBatch.Draw(WhitePixel, screenArea * 0.5f, null, new Color(255, 255, 253), 0f, WhitePixel.Size() * 0.5f, textureArea, 0, 0f);

        Main.spriteBatch.End();
    }

    /// <summary>
    /// Renders the glowing light, with maximum layering priority, at the top of the world.
    /// </summary>
    private static void RenderLight(GameTime obj)
    {
        if (!Enabled)
            return;

        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

        ManagedShader lightShader = ShaderManager.GetShader("NoxusBoss.TerminusLightOverlayShader");
        lightShader.TrySetParameter("lightAngle", -0.5f);
        lightShader.TrySetParameter("screenPosition", Main.screenPosition);
        lightShader.TrySetParameter("startingY", Main.instance.GraphicsDevice.Viewport.Height * 1.5f);
        lightShader.TrySetParameter("endingY", Main.instance.GraphicsDevice.Viewport.Height * 1.5f + 1000f);
        lightShader.TrySetParameter("screenWidth", Main.instance.GraphicsDevice.Viewport.Width);
        lightShader.TrySetParameter("lightDirection", Vector2.UnitY.RotatedBy(0.2f));
        lightShader.TrySetParameter("weakRayColor", new Vector3(1f, 0.6f, 0.67f));
        lightShader.TrySetParameter("strongRayColor", new Vector3(1f, 0.9f, 0.67f));
        lightShader.TrySetParameter("rayColorAdditiveBias", new Vector3(0f, 0f, 0.4f));
        lightShader.TrySetParameter("rayColorExponent", 2.75f);
        lightShader.SetTexture(PerlinNoise, 1, SamplerState.LinearWrap);
        lightShader.SetTexture(BubblyNoise, 2, SamplerState.LinearWrap);
        lightShader.SetTexture(MulticoloredNoise, 3, SamplerState.LinearWrap);
        lightShader.Apply();

        Vector2 screenArea = ViewportSize;
        Vector2 textureArea = screenArea / WhitePixel.Size();
        Main.spriteBatch.Draw(WhitePixel, screenArea * 0.5f, null, Color.White, 0f, WhitePixel.Size() * 0.5f, textureArea, 0, 0f);

        Main.spriteBatch.End();
    }

    /// <summary>
    /// Creates particles within the god rays in the sky.
    /// </summary>
    private static void CreateGodRayParticles()
    {
        if (Main.netMode == NetmodeID.Server)
            return;

        int tries;
        Vector2 particleSpawnPosition = Vector2.Zero;
        for (tries = 0; tries < 20; tries++)
        {
            particleSpawnPosition = Main.screenPosition + new Vector2(Main.rand.NextFloat() * Main.instance.GraphicsDevice.Viewport.Width, Main.rand.NextFloat() * Main.instance.GraphicsDevice.Viewport.Height);
            if (particleSpawnPosition.Y < Main.screenHeight * 1.5f + 500f)
                break;
        }

        if (tries >= 19)
            return;

        Dust particle = Dust.NewDustPerfect(particleSpawnPosition, ModContent.DustType<TwinkleDust>());
        particle.velocity = Main.rand.NextVector2Circular(2f, 2f);
        particle.noGravity = true;
        particle.color = Color.Lerp(Color.Coral, Color.Yellow, Main.rand.NextFloat());
        particle.scale = 0.3f;
    }

    /// <summary>
    /// Renders the stairs that appear during the Terminus animation.
    /// </summary>
    private static void RenderStairs()
    {
        int stepsPerPillar = 3;
        float scale = 1f;
        float spacingPerPillar = scale * 16f;
        Texture2D stair = GennedAssets.Textures.TerminusStairway.Stair;
        Texture2D pillar = GennedAssets.Textures.TerminusStairway.Pillar;
        Texture2D pillarSteps = GennedAssets.Textures.TerminusStairway.PillarSteps;

        for (float dx = -1500f; dx < 1500f; dx += spacingPerPillar)
        {
            // Calculate various stair render data.
            CalculateStairDrawInfo(dx, spacingPerPillar, stepsPerPillar, out Vector2 stairWorldPosition, out float ascentInterpolant, out float descentInterpolant, out bool renderPillar);

            float opacity = (1f - ascentInterpolant).Cubed() * (1f - descentInterpolant).Cubed();
            Vector2 verticalOffset = Vector2.UnitY * (SmoothStep(0f, -800f, ascentInterpolant) + SmoothStep(0f, 800f, descentInterpolant));
            Vector2 stairDrawPosition = stairWorldPosition - Main.screenPosition + verticalOffset;

            // Calculate the step color, iterating between pastel rainbows in accordance with the world position.
            Color stepColor = Main.hslToRgb((stairWorldPosition.X / 1200f + Main.GlobalTimeWrappedHourly * 0.05f).Modulo(1f), 1f, 0.9f) * opacity;

            // Calculate the bottom glow light color, decreasing strongly in opacity if the step is in the middle of ascending or descending into place.
            Color bottomLightColor = Color.White * (1f - ascentInterpolant).Cubed() * (1f - descentInterpolant).Cubed() * opacity * 0.5f;
            bottomLightColor.A = 0;

            // Perform rendering.
            if (renderPillar)
            {
                dx += spacingPerPillar * (stepsPerPillar - 1);
                stairDrawPosition.X += spacingPerPillar;
                stairDrawPosition.Y -= spacingPerPillar * StairSteepnessFactor * 2f;

                RenderStairPillar(pillar, pillarSteps, stairDrawPosition, bottomLightColor, stepColor, scale);

                bool closeToBottomOfStairs = Main.LocalPlayer.WithinRange(stairWorldPosition, 900f);
                if (descentInterpolant >= 0.95f && descentInterpolant < 1f && ColumnRiseSoundReplayDelay <= 0 && Time >= 120 && Main.LocalPlayer.velocity.Length() >= 6f && closeToBottomOfStairs)
                {
                    SoundEngine.PlaySound(GennedAssets.Sounds.Environment.DivineStairwayColumnRise with { Volume = 0.76f });
                    ColumnRiseSoundReplayDelay = 10;
                }
            }
            else
                RenderStairStep(stair, stairDrawPosition, bottomLightColor, stepColor, scale);
        }
    }

    /// <summary>
    /// Calculates various data for an individual stair instance.
    /// </summary>
    /// <param name="dx">The relative horizontal offset for the stair step.</param>
    /// <param name="spacingPerPillar">How much stairs should be spaced between each other.</param>
    /// <param name="stepsPerPillar">How many stair steps are included on pillars.</param>
    /// <param name="stairWorldPosition">The world position of the stair step.</param>
    /// <param name="ascentInterpolant">The 0-1 interpolant dictating how much the stair should be moved upward.</param>
    /// <param name="descentInterpolant">The 0-1 interpolant dictating how much the stair should be moved downward.</param>
    /// <param name="renderPillar">Whether a pillar should be rendered instead of a regular stair step.</param>
    private static void CalculateStairDrawInfo(float dx, float spacingPerPillar, int stepsPerPillar, out Vector2 stairWorldPosition, out float ascentInterpolant, out float descentInterpolant, out bool renderPillar)
    {
        Vector2 stairsStart = StairsStart;
        float horizontalOffsetFromCenter = Main.LocalPlayer.Center.X - stairsStart.X + dx;

        stairWorldPosition = new(Main.LocalPlayer.Center.X + dx, stairsStart.Y + horizontalOffsetFromCenter * -StairSteepnessFactor);

        bool reachedBottom = stairWorldPosition.Y > StairsBottomY;
        if (reachedBottom)
            stairWorldPosition.Y = StairsBottomY;
        else
            stairWorldPosition.Y += stairWorldPosition.X % spacingPerPillar * StairSteepnessFactor;

        // Snap the world position into place, so that the offsets are visually relative to the world instead of the player.
        stairWorldPosition.X -= stairWorldPosition.X % spacingPerPillar;

        int pillarIndex = (int)(stairWorldPosition.X / stepsPerPillar);

        // Calculate the base ascent and descent interpolants.
        // By default, stair steps rise down from above.
        ascentInterpolant = Pow(InverseLerp(0f, 270f, stairWorldPosition.X - StairAscentStartX), 3.2f);
        descentInterpolant = 0f;

        // Every 160th step counts as a pillar, to add variety to the stairs.
        renderPillar = pillarIndex % 160 == 0 && !reachedBottom;

        // Make pillars rise from below instead of falling from above.
        if (renderPillar)
        {
            Utils.Swap(ref ascentInterpolant, ref descentInterpolant);
            descentInterpolant = SmoothStep(0f, 1f, Sqrt(descentInterpolant));
        }
    }

    private static void RenderStairStep(Texture2D stair, Vector2 stairDrawPosition, Color bottomLightColor, Color stepColor, float scale)
    {
        Rectangle lightFrame = new Rectangle(20, 0, 20, 64);
        Main.spriteBatch.Draw(stair, stairDrawPosition, lightFrame, bottomLightColor, 0f, lightFrame.Size() * new Vector2(0.5f, 0f), scale, 0, 0f);

        Rectangle stepFrame = new Rectangle(0, 0, 20, 64);
        Main.spriteBatch.Draw(stair, stairDrawPosition, stepFrame, stepColor, 0f, stepFrame.Size() * new Vector2(0.5f, 0f), scale, 0, 0f);
    }

    private static void RenderStairPillar(Texture2D pillar, Texture2D pillarSteps, Vector2 stairDrawPosition, Color bottomLightColor, Color stepColor, float scale)
    {
        Rectangle lightFrame = new Rectangle(56, 0, 56, 1096);
        Main.spriteBatch.Draw(pillar, stairDrawPosition, lightFrame, bottomLightColor, 0f, lightFrame.Size() * new Vector2(0.5f, 0f), scale, 0, 0f);

        Rectangle stepFrame = new Rectangle(0, 0, 56, 1096);
        Main.spriteBatch.Draw(pillar, stairDrawPosition, stepFrame, stepColor, 0f, stepFrame.Size() * new Vector2(0.5f, 0f), scale, 0, 0f);

        Main.spriteBatch.Draw(pillarSteps, stairDrawPosition, null, stepColor, 0f, pillarSteps.Size() * new Vector2(0.5f, 0f), scale, 0, 0f);
    }
}
