using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.Data;
using NoxusBoss.Core.DataStructures;
using NoxusBoss.Core.GlobalInstances;
using NoxusBoss.Core.Netcode;
using NoxusBoss.Core.Netcode.Packets;
using NoxusBoss.Core.World.TileDisabling;
using NoxusBoss.Core.World.WorldGeneration;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Graphics.SpecificEffectManagers;

[Autoload(Side = ModSide.Client)]
public class PermafrostTileProtectionVisualSystem : ModSystem
{
    public struct TileImpact
    {
        public Vector2 Position;

        public float LifetimeRatio;

        public float MaxRadius;
    }

    private static bool ignoreStandardImpactHitHook;

    /// <summary>
    /// The set of all tile impact effects that have been performed at a given position within the keep.
    /// </summary>
    public static TileImpact[] Impacts
    {
        get;
        private set;
    } = new TileImpact[10];

    /// <summary>
    /// The render target that contains all tile render data within Permafrost's keep.
    /// </summary>
    public static ManagedRenderTarget KeepTilesMaskTarget
    {
        get;
        private set;
    }

    /// <summary>
    /// The standard maximum impact radius of tile break effects.
    /// </summary>
    public const float StandardMaxImpactRadius = 60f;

    /// <summary>
    /// The palette that's cycled through for the glow overlay visual.
    /// </summary>
    public static Palette GlowPalette
    {
        get;
        private set;
    }

    public override void OnModLoad()
    {
        KeepTilesMaskTarget = new(true, ManagedRenderTarget.CreateScreenSizedTarget);
        RenderTargetManager.RenderTargetUpdateLoopEvent += PrepareTilesTarget;

        GlobalWallEventHandlers.IsWallUnbreakableEvent += MakeKeepWallsUnbreakable;
        GlobalTileEventHandlers.IsTileUnbreakableEvent += MakeKeepUnbreakable;
        GlobalTileEventHandlers.ModifyKillSoundEvent += UseCustomUnbreakableHitSounds;
        GlobalTileEventHandlers.KillTileEvent += UseCustomUnbreakableVfx;
        GlobalProjectileEventHandlers.PreKillEvent += MakeExplosivesCreateImpacts;
        On_Player.PickTile += CreateImpactAtMousePosition;
        On_Main.DoDraw_WallsTilesNPCs += RenderMagicOverlay;

        GlowPalette = new Palette(LocalDataManager.Read<Vector3[]>("Core/Graphics/SpecificEffectManagers/PermafrostKeepShaderPalettes.json")["GlowColors"]);
    }

    private bool MakeExplosivesCreateImpacts(Projectile projectile)
    {
        bool isBomb = projectile.type == ProjectileID.Bomb || projectile.type == ProjectileID.BombFish || projectile.type == ProjectileID.StickyBomb ||
                      projectile.type == ProjectileID.BouncyBomb || projectile.type == ProjectileID.ScarabBomb;
        bool isDynamite = projectile.type == ProjectileID.Dynamite || projectile.type == ProjectileID.BouncyDynamite || projectile.type == ProjectileID.StickyDynamite;

        if (isBomb)
            CreateImpact(projectile.Center, 100f);
        if (isDynamite)
            CreateImpact(projectile.Center, 200f);

        return true;
    }

    private bool MakeKeepWallsUnbreakable(int x, int y, int type) => PermafrostKeepWorldGen.IsProtected(x, y);

    private bool MakeKeepUnbreakable(int x, int y, int type) => PermafrostKeepWorldGen.IsProtected(x, y);

    private bool UseCustomUnbreakableHitSounds(int x, int y, int type, bool fail)
    {
        if (PermafrostKeepWorldGen.IsProtected(x, y))
        {
            SoundEngine.PlaySound(GennedAssets.Sounds.Mining.PermafrostKeepSealMine with { MaxInstances = 0 }, new Vector2(x, y).ToWorldCoordinates());
            return false;
        }

        return true;
    }

    private void UseCustomUnbreakableVfx(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (!PermafrostKeepWorldGen.IsProtected(i, j))
            return;

        if (!ignoreStandardImpactHitHook && Main.netMode != NetmodeID.MultiplayerClient)
            CreateImpact(new Vector2(i, j).ToWorldCoordinates(), StandardMaxImpactRadius * (fail ? 0.6f : 1f));

        if (!Main.rand.NextBool(4))
            return;

        ParticleOrchestraType orchestraType = ParticleOrchestraType.RainbowRodHit;
        Vector2 sparkleOffset = Main.rand.NextVector2Unit() * Main.rand.NextFloat(8f, 6f);
        Vector2 positionInWorld = new Vector2(i, j).ToWorldCoordinates() + sparkleOffset;
        ParticleOrchestrator.RequestParticleSpawn(true, orchestraType, new ParticleOrchestraSettings
        {
            PositionInWorld = positionInWorld,
            MovementVector = Main.rand.NextVector2Circular(3f, 3f)
        }, Main.myPlayer);
    }

    private void CreateImpactAtMousePosition(On_Player.orig_PickTile orig, Player self, int x, int y, int pickPower)
    {
        ignoreStandardImpactHitHook = true;
        orig(self, x, y, pickPower);
        ignoreStandardImpactHitHook = false;

        if (PermafrostKeepWorldGen.IsProtected(x, y) && Main.myPlayer == self.whoAmI)
            CreateImpact(Main.MouseWorld);
    }

    public override void PostUpdatePlayers()
    {
        for (int i = 0; i < Impacts.Length; i++)
        {
            if (Impacts[i].LifetimeRatio > 0f)
            {
                float slowdown = Exp(Impacts[i].MaxRadius * -0.0007f);
                Impacts[i].LifetimeRatio += slowdown * 0.045f;

                if (Impacts[i].LifetimeRatio >= 1f)
                    Impacts[i].LifetimeRatio = 0f;
            }
        }
    }

    private void PrepareTilesTarget()
    {
        if (TileDisablingSystem.TilesAreUninteractable)
            return;

        Rectangle generousScreenRectangle = new Rectangle((int)(Main.screenPosition.X / 16f) - 15, (int)(Main.screenPosition.Y / 16f) - 15, (int)(Main.screenWidth / 16f) + 30, (int)(Main.screenHeight / 16f) + 30);
        if (!PermafrostKeepWorldGen.KeepArea.Intersects(generousScreenRectangle))
            return;

        GraphicsDevice gd = Main.instance.GraphicsDevice;
        gd.SetRenderTarget(KeepTilesMaskTarget);
        gd.Clear(Color.Transparent);

        Main.spriteBatch.Begin();

        for (int x = PermafrostKeepWorldGen.KeepArea.Left; x <= PermafrostKeepWorldGen.KeepArea.Right; x++)
        {
            for (int y = PermafrostKeepWorldGen.KeepArea.Top; y <= PermafrostKeepWorldGen.KeepArea.Bottom; y++)
            {
                Vector2 drawPosition = new Vector2(x, y).ToWorldCoordinates(0f, 0f) - Main.screenPosition;
                Main.spriteBatch.Draw(WhitePixel, drawPosition, new Rectangle(0, 0, 16, 16), Color.White);
            }
        }

        Main.spriteBatch.End();
    }

    private void RenderMagicOverlay(On_Main.orig_DoDraw_WallsTilesNPCs orig, Main self)
    {
        orig(self);

        Rectangle generousScreenRectangle = new Rectangle((int)(Main.screenPosition.X / 16f) - 15, (int)(Main.screenPosition.Y / 16f) - 15, (int)(Main.screenWidth / 16f) + 30, (int)(Main.screenHeight / 16f) + 30);
        if (!PermafrostKeepWorldGen.KeepArea.Intersects(generousScreenRectangle))
            return;

        if (!Lighting.NotRetro)
            return;

        Main.spriteBatch.PrepareForShaders();

        Vector4 glowColorVector4 = GlowPalette.SampleVector(Cos01(Main.GlobalTimeWrappedHourly * 0.56f));
        Vector3 glowColor = new Vector3(glowColorVector4.X, glowColorVector4.Y, glowColorVector4.Z);

        float[] lifetimeRatios = new float[Impacts.Length];
        float[] impactMaxRadii = new float[Impacts.Length];
        Vector2[] impactPositions = new Vector2[Impacts.Length];
        for (int i = 0; i < impactPositions.Length; i++)
        {
            lifetimeRatios[i] = Impacts[i].LifetimeRatio;
            impactMaxRadii[i] = Impacts[i].MaxRadius;
            impactPositions[i] = Impacts[i].Position;
        }

        float opacity = 1f;
        if (PermafrostKeepWorldGen.DoorHasBeenUnlocked)
        {
            opacity = 1f - lifetimeRatios.Max();
            if (opacity >= 1f)
                opacity = 0f;
        }

        float universalGlow = 0f;
        if (PermafrostDoorUnlockSystem.DoorBrightnesses.Count >= 1)
            universalGlow = InverseLerpBump(0f, 1.35f, 1.5f, 1.75f, PermafrostDoorUnlockSystem.DoorBrightnesses.Values.Max());

        ManagedShader magicShader = ShaderManager.GetShader("NoxusBoss.ProtectiveMagicOverlayShader");
        magicShader.TrySetParameter("screenPosition", Main.screenPosition);
        magicShader.TrySetParameter("screenSize", new Vector2(Main.screenWidth, Main.screenHeight));
        magicShader.TrySetParameter("zoom", Main.GameViewMatrix.Zoom);
        magicShader.TrySetParameter("forcefieldCenter", PermafrostKeepWorldGen.KeepArea.Center.ToVector2() * 16f);
        magicShader.TrySetParameter("keepTopLeft", PermafrostKeepWorldGen.KeepArea.TopLeft() * 16f + Vector2.One * 32f);
        magicShader.TrySetParameter("keepBottomRight", PermafrostKeepWorldGen.KeepArea.BottomRight() * 16f - Vector2.One * 32f);
        magicShader.TrySetParameter("reciprocalNoiseBrightness", 0.02f);
        magicShader.TrySetParameter("reciprocalNoiseFloor", 0.01f);
        magicShader.TrySetParameter("pulseColor", new Vector4(glowColor, 0f));
        magicShader.TrySetParameter("tileImpactLifetimeRatios", lifetimeRatios);
        magicShader.TrySetParameter("tileImpactMaxRadii", impactMaxRadii);
        magicShader.TrySetParameter("tileImpactPositions", impactPositions);
        magicShader.TrySetParameter("universalGlow", universalGlow);
        magicShader.SetTexture(TileTargetManagers.TileTarget, 1, SamplerState.PointClamp);
        magicShader.SetTexture(TileTargetManagers.LiquidTarget, 2, SamplerState.PointClamp);
        magicShader.SetTexture(WavyBlotchNoise, 3, SamplerState.PointWrap);
        magicShader.Apply();

        Main.spriteBatch.Draw(KeepTilesMaskTarget, Main.screenLastPosition - Main.screenPosition, Color.White * opacity);

        Main.spriteBatch.ResetToDefault();
    }

    internal static void CreateImpactInner(Vector2 position, float maxRadius)
    {
        if (Main.netMode == NetmodeID.Server)
            return;

        int freeIndex = -1;
        for (int i = 0; i < Impacts.Length; i++)
        {
            if (Impacts[i].LifetimeRatio <= 0f)
                freeIndex = i;
        }

        if (freeIndex >= 0)
        {
            Impacts[freeIndex] = new TileImpact()
            {
                Position = position,
                MaxRadius = maxRadius,
                LifetimeRatio = 0.001f
            };
        }
    }

    /// <summary>
    /// Attempts to create a new tile impact effect at a given world position.
    /// </summary>
    /// <param name="position">The world position of the impact effect.</param>
    /// <param name="maxRadius">The maximum radius of the impact effect.</param>
    public static void CreateImpact(Vector2 position, float maxRadius = StandardMaxImpactRadius)
    {
        CreateImpactInner(position, maxRadius);
        if (Main.netMode != NetmodeID.SinglePlayer)
            PacketManager.SendPacket<CreatePermafrostKeepImpactPacket>(position.X, position.Y, maxRadius);
    }
}
