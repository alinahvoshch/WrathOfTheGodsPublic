using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.Configuration;
using Terraria;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Rendering.RenderSteps;

public class SideFlowersStep : INamelessDeityRenderStep
{
    public int LayerIndex => 50;

    public NamelessDeityRenderComposite Composite
    {
        get;
        set;
    }

    /// <summary>
    /// The texture of the flowers.
    /// </summary>
    public NamelessDeitySwappableTexture FlowerTexture
    {
        get;
        private set;
    }

    public void Initialize()
    {
        FlowerTexture = Composite.RegisterSwappableTexture("SideFlower", 7, Composite.UsedPreset?.Data.PreferredFlowerTextures).WithAutomaticSwapRule(() =>
        {
            int swapRate = WoTGConfig.Instance.PhotosensitivityMode ? 13 : 7;
            return Composite.Time % swapRate == 0;
        });
    }

    public void Render(Entity owner, Vector2 drawCenter)
    {
        Texture2D flower = FlowerTexture.UsedTexture;
        bool isClockFlower = FlowerTexture.TexturePath == "SideFlower6";

        // Calculate individualized flower draw variables for scale and rotation.
        Vector2 baseScale = Vector2.One;
        float leftRotation = Lerp(-0.2f, 0.67f, Cos01(Main.GlobalTimeWrappedHourly * 0.6f)) - Main.GlobalTimeWrappedHourly * 2.84f;
        Vector2 leftScale = baseScale * Lerp(0.95f, 1.05f, Cos01(Main.GlobalTimeWrappedHourly * 0.45f));
        float rightRotation = Lerp(-0.2f, 0.67f, Cos01(Main.GlobalTimeWrappedHourly * 0.59f)) + Main.GlobalTimeWrappedHourly * 2.84f;
        Vector2 rightScale = baseScale * Lerp(0.95f, 1.05f, Cos01(Main.GlobalTimeWrappedHourly * 0.46f));

        void drawFlowerAtPosition(Vector2 drawPosition, float rotation, Vector2 scale)
        {
            // SPECIAL CASE: If this is actually a clock, don't spin the clock in place, spin its hands instead.
            float minuteHandRotation = Main.GlobalTimeWrappedHourly * 9f;
            float hourHandRotation = minuteHandRotation / 12f;
            if (isClockFlower)
                rotation = 0f;

            Main.EntitySpriteDraw(flower, drawPosition, null, Color.White, rotation, flower.Size() * 0.5f, scale, 0);

            if (isClockFlower)
            {
                Vector2 minuteHandOrigin = new Vector2(0f, 12f);
                Vector2 hourHandOrigin = new Vector2(45f, 3f);
                Main.EntitySpriteDraw(GennedAssets.Textures.NamelessDeity.SideFlowerHourHand.Value, drawPosition, null, Color.White, hourHandRotation, hourHandOrigin, scale, 0);
                Main.EntitySpriteDraw(GennedAssets.Textures.NamelessDeity.SideFlowerMinuteHand.Value, drawPosition, null, Color.White, minuteHandRotation, minuteHandOrigin, scale, 0);
            }
        }

        // Perform the draw calls.
        drawFlowerAtPosition(drawCenter - Vector2.UnitX * 280f, leftRotation, leftScale);
        drawFlowerAtPosition(drawCenter + Vector2.UnitX * 280f, rightRotation, rightScale);
    }
}
