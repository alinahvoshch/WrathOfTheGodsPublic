using Luminance.Common.Easings;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SpecificEffectManagers;
using NoxusBoss.Content.Rarities;
using NoxusBoss.Core.Utilities;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items.HuntAuricSouls;

public class AvatarAuricSoul : ModItem
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
        Item.rare = ModContent.RarityType<AvatarRarity>();
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
        Color glowColor = Color.Lerp(Color.Crimson, Color.Wheat, Cos01(Main.GlobalTimeWrappedHourly * 1.5f));
        Color final = Color.Lerp(glowColor, Color.Red, Cos(Main.GlobalTimeWrappedHourly * 0.1f) * 0.1f + 0.3f);
        return final;
    }

    public override void UpdateInventory(Player player) => WorldExistTime = 0;

    public override void Update(ref float gravity, ref float maxFallSpeed)
    {
        AvatarOfEmptinessSky.InProximityOfMonolith = true;
        AvatarOfEmptinessSky.TimeSinceCloseToMonolith = 0;

        WorldExistTime++;

        if (WorldExistTime % 2 == 1)
        {
            Dust dust = Dust.NewDustPerfect(Item.position + Main.rand.NextVector2Circular(200f, 200f) * WorldScale, 261);
            dust.color = Main.rand.NextFromList(Color.Wheat, Color.Crimson, Color.Red);
            dust.velocity = -Vector2.UnitY * Main.rand.NextFloat(3f);
            dust.noGravity = true;
            dust.scale *= 0.8f;
        }
    }

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

    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        Main.spriteBatch.PrepareForShaders();

        Vector2 drawPosition = Item.position - Main.screenPosition;
        DrawSubtractiveBackBloom(drawPosition);
        DrawBloodVortex(drawPosition);
        DrawBlackHole(drawPosition);

        Main.spriteBatch.ResetToDefault();

        return false;
    }

    private void DrawSubtractiveBackBloom(Vector2 drawPosition)
    {
        Texture2D backglow = BloomCircleSmall.Value;
        Color backglowColor = new Color(189, 4, 122);
        Color subtractiveColor = new Color(Vector3.One - backglowColor.ToVector3());

        float scale = WorldScale * 2f;
        Main.spriteBatch.UseBlendState(SubtractiveBlending);
        Main.spriteBatch.Draw(backglow, drawPosition, null, subtractiveColor, 0f, backglow.Size() * 0.5f, scale, 0, 0f);
        Main.spriteBatch.Draw(backglow, drawPosition, null, subtractiveColor * 0.76f, 0f, backglow.Size() * 0.5f, scale * 2f, 0, 0f);
        Main.spriteBatch.Draw(backglow, drawPosition, null, subtractiveColor * 0.5f, 0f, backglow.Size() * 0.5f, scale * 3.25f, 0, 0f);
        Main.spriteBatch.PrepareForShaders();
    }

    private void DrawBlackHole(Vector2 drawPosition)
    {
        ManagedShader blackHoleShader = ShaderManager.GetShader("NoxusBoss.AvatarLoreItemBlackHoleShader");
        blackHoleShader.SetTexture(PerlinNoise, 1, SamplerState.LinearWrap);
        blackHoleShader.Apply();

        Main.spriteBatch.Draw(WhitePixel, drawPosition, null, Color.Red, 0f, WhitePixel.Size() * 0.5f, WorldScale * 190f, 0, 0f);
    }

    private void DrawBloodVortex(Vector2 drawPosition)
    {
        Texture2D vortexTexture = WavyBlotchNoise.Value;
        ManagedShader vortexShader = ShaderManager.GetShader("NoxusBoss.AvatarLoreItemBloodVortexShader");
        vortexShader.TrySetParameter("innerGlowColor", new Color(163, 19, 9).ToVector4());
        vortexShader.SetTexture(DendriticNoise, 1, SamplerState.LinearWrap);
        vortexShader.Apply();

        Main.spriteBatch.Draw(vortexTexture, drawPosition, null, new Color(163, 19, 9), 0f, vortexTexture.Size() * 0.5f, Vector2.One * WorldScale / vortexTexture.Size() * 700f, 0, 0f);
    }
}
