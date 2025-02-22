using Luminance.Common.Easings;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers;
using NoxusBoss.Content.Rarities;
using NoxusBoss.Core.Utilities;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items.HuntAuricSouls;

public class NamelessAuricSoul : ModItem
{
    /// <summary>
    /// How long this lore item has existed in the game world, outside of a player's inventory.
    /// </summary>
    public int WorldExistTime
    {
        get;
        set;
    }

    /// <summary>
    /// The scale of this lore item in the world based on how long it's existed in said game world.
    /// </summary>
    public float WorldScale
    {
        get
        {
            float scaleInterpolant = InverseLerp(0f, 60f, WorldExistTime);
            return EasingCurves.Cubic.Evaluate(EasingType.InOut, scaleInterpolant);
        }
    }

    public Vector2 IrisPosition
    {
        get;
        set;
    }

    public Vector2 IrisDestination
    {
        get;
        set;
    }

    public static float OpenEyeInterpolant => SmoothStep(0f, 1f, NamelessDeityAuricSoulDistortionSystem.AnimationCompletion);

    public override string Texture => $"NoxusBoss/Assets/Textures/Content/Items/HuntAuricSouls/{Name}";

    public override void SetStaticDefaults()
    {
        ItemID.Sets.ItemNoGravity[Type] = true;
        ItemID.Sets.ItemsThatShouldNotBeInInventory[Type] = true;
        ItemID.Sets.IgnoresEncumberingStone[Type] = true;
        ItemID.Sets.AnimatesAsSoul[Type] = true;
        ItemID.Sets.ItemIconPulse[Type] = true;
        ItemID.Sets.IsLavaImmuneRegardlessOfRarity[Type] = true;
        Main.RegisterItemAnimation(Type, new DrawAnimationVertical(5, 4));
    }

    public override void SetDefaults()
    {
        Item.width = 20;
        Item.height = 20;
        Item.value = 0;
        Item.rare = ModContent.RarityType<NamelessDeityRarity>();
    }

    public override bool OnPickup(Player player)
    {
        for (int i = 0; i < 150; i++)
        {
            Dust soul = Dust.NewDustPerfect(Item.Center, DustID.PortalBoltTrail, Main.rand.NextVector2Circular(10, 10), 0, GetAlpha(Color.White) ?? Color.White, Main.rand.NextFloat(2f));
            soul.noGravity = true;
        }
        return false;
    }

    public override Color? GetAlpha(Color lightColor)
    {
        Color glowColor = Color.Lerp(Color.Teal, Color.Wheat, Cos01(Main.GlobalTimeWrappedHourly * 1.5f));
        Color final = Color.Lerp(glowColor, Color.Yellow, Cos(Main.GlobalTimeWrappedHourly * 0.1f) * 0.1f + 0.3f);
        return final;
    }

    public override void UpdateInventory(Player player) => WorldExistTime = 0;

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        Texture2D texture = TextureAssets.Item[Type].Value;
        Texture2D glowTexture = BloomCircleSmall.Value;

        Color glowColor = GetAlpha(Color.White) ?? Color.White;
        spriteBatch.Draw(texture, position, frame, glowColor, 0, frame.Size() * 0.5f, scale + 0.2f, 0, 0);
        spriteBatch.Draw(texture, position, frame, new Color(200, 200, 200, 0), 0, frame.Size() * 0.5f, scale + 0.2f, 0, 0);
        spriteBatch.Draw(glowTexture, position, glowTexture.Frame(), (glowColor with { A = 0 }) * 0.7f, 0, glowTexture.Size() * 0.5f, scale * 0.3f, 0, 0);

        return false;
    }


    public void DrawSelfInWorld()
    {
        if (IrisDestination == Vector2.Zero)
            IrisDestination = new Vector2(50f, 20f);

        NamelessDeityDimensionSkyGenerator.InProximityOfMonolith = true;
        NamelessDeityDimensionSkyGenerator.TimeSinceCloseToMonolith = 0;

        IrisPosition = IrisPosition.MoveTowards(IrisDestination, 16f);
        if (IrisPosition.WithinRange(IrisDestination, 2f) && Main.rand.NextBool(40))
        {
            if (Main.rand.NextBool(24))
                IrisDestination = -IrisDestination.RotatedByRandom(Pi);
            else
                IrisDestination = Item.SafeDirectionTo(Main.LocalPlayer.Center) * new Vector2(50f, 20f);
        }

        Vector2 drawPosition = Item.position - Main.screenPosition;
        DrawBackglow(drawPosition);
        DrawRadialGodRays(drawPosition);
        DrawBackGlowRings(drawPosition - Vector2.UnitY * 24f);
        DrawRealisticEye(drawPosition);
    }

    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) => false;

    public override void Update(ref float gravity, ref float maxFallSpeed)
    {
        if (OpenEyeInterpolant <= 0.6f)
            return;

        if (Main.rand.NextBool(2))
        {
            float leafSizeInterpolant = Main.rand.NextFloat();
            Vector2 leafSpawnPosition = Item.position + Main.rand.NextVector2Circular(1f, 0.78f) * Main.rand.NextFloat(100f, 196f);
            Vector2 leafVelocity = Item.SafeDirectionTo(leafSpawnPosition).RotatedBy(-PiOver2) * Lerp(12f, 2.5f, leafSizeInterpolant);
            Vector2 leafSize = new Vector2(3f, 3f) * Lerp(0.5f, 1.5f, leafSizeInterpolant);
            NamelessDeityAuricSoulParticleHandlers.LeafParticleSystem.CreateNew(leafSpawnPosition, leafVelocity, leafSize, Color.White);
        }
        if (Main.rand.NextBool(3))
        {
            float blossomSizeInterpolant = Main.rand.NextFloat();
            Vector2 blossomSpawnPosition = Item.position + Main.rand.NextVector2Unit() * Main.rand.NextFloat(100f, 256f);
            Vector2 blossomVelocity = Item.SafeDirectionTo(blossomSpawnPosition).RotatedBy(-PiOver2) * Lerp(8f, 2.2f, blossomSizeInterpolant);
            Vector2 blossomSize = new Vector2(2f, 2f) * Lerp(0.8f, 2f, blossomSizeInterpolant);
            NamelessDeityAuricSoulParticleHandlers.BlossomParticleSystem.CreateNew(blossomSpawnPosition, blossomVelocity, blossomSize, Color.White);
        }
        base.Update(ref gravity, ref maxFallSpeed);
    }

    private static void DrawBackGlowRings(Vector2 drawPosition)
    {
        float opacity = InverseLerp(0.42f, 1f, OpenEyeInterpolant);
        Color ringColor = new Color(255, 227, 72) * opacity;
        Texture2D largeRing = RingLarge.Value;
        Main.spriteBatch.Draw(largeRing, drawPosition, null, ringColor, 0f, largeRing.Size() * 0.5f, 0.47f, 0, 0f);
    }

    private static void DrawBackglow(Vector2 drawPosition)
    {
        float opacity = InverseLerp(0.45f, 1f, OpenEyeInterpolant);
        Main.spriteBatch.Draw(BloomCircleSmall, drawPosition, null, new Color(255, 0, 0, 0) * opacity, 0f, BloomCircleSmall.Size() * 0.5f, 4f, 0, 0f);
        Main.spriteBatch.Draw(BloomCircleSmall, drawPosition, null, new Color(160, 0, 90, 0) * opacity * 0.4f, 0f, BloomCircleSmall.Size() * 0.5f, 12f, 0, 0f);
    }

    private static void DrawRadialGodRays(Vector2 drawPosition)
    {
        ManagedShader godRayShader = ShaderManager.GetShader("NoxusBoss.RadialGodRayShader");
        godRayShader.SetTexture(MilkyNoise2, 1, SamplerState.LinearWrap);
        godRayShader.Apply();

        float opacity = InverseLerp(0.5f, 1f, OpenEyeInterpolant);
        float scale = SmoothStep(1200f, 640f, opacity);
        Main.spriteBatch.Draw(WhitePixel, drawPosition, null, new Color(255, 255, 205) * opacity * 0.5f, 0f, WhitePixel.Size() * 0.5f, scale, 0, 0f);

        Main.pixelShader.CurrentTechnique.Passes[0].Apply();
    }

    private void DrawRealisticEye(Vector2 drawPosition)
    {
        float irisScale = InverseLerp(0f, 0.5f, OpenEyeInterpolant) * 1.1f;
        float pupilScale = Utils.Remap(IrisPosition.Distance(IrisDestination), 60f, 5f, 0.65f, 1f);

        Vector2 eyeSize = new Vector2(270f, 210f);
        Vector2 pupilOffset = IrisPosition.RotatedBy(SmoothStep(0f, -TwoPi, OpenEyeInterpolant)) / 420f;

        ManagedShader eyeShader = ShaderManager.GetShader("NoxusBoss.NamelessLoreItemEyeShader");
        eyeShader.TrySetParameter("irisColorA", new Color(45, 124, 158).ToVector3());
        eyeShader.TrySetParameter("irisColorB", new Color(107, 184, 255).ToVector3());
        eyeShader.TrySetParameter("size", eyeSize);
        eyeShader.TrySetParameter("pupilOffset", pupilOffset);
        eyeShader.TrySetParameter("openEyeInterpolant", OpenEyeInterpolant);
        eyeShader.TrySetParameter("pupilScale", pupilScale);
        eyeShader.TrySetParameter("irisScale", irisScale);
        eyeShader.TrySetParameter("baseScleraColor", new Color(0, 0, 4).ToVector3());
        eyeShader.SetTexture(WavyBlotchNoiseDetailed, 1, SamplerState.LinearWrap);
        eyeShader.SetTexture(GennedAssets.Textures.Extra.Cosmos, 2, SamplerState.LinearWrap);
        eyeShader.Apply();

        Main.EntitySpriteDraw(WhitePixel, drawPosition, null, new Color(255, 255, 255), 0f, WhitePixel.Size() * 0.5f, eyeSize, 0);
    }
}
