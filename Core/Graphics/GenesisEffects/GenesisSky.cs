using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Core.CrossCompatibility.Inbound.RealisticSky;
using Terraria;
using Terraria.Graphics.Effects;

namespace NoxusBoss.Core.Graphics.GenesisEffects;

public class GenesisSky : CustomSky
{
    private bool isActive;

    internal static float intensity;

    /// <summary>
    /// The extent by which the background should be dimmed.
    /// </summary>
    public static float BackgroundDimness
    {
        get;
        set;
    }

    /// <summary>
    /// The key associated with this sky.
    /// </summary>
    public const string ScreenShaderKey = "NoxusBoss:GenesisSky";

    public override void Update(GameTime gameTime)
    {
        intensity = Saturate(intensity + isActive.ToDirectionInt() * 0.05f);
    }

    public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
    {
        Vector2 screenSize = ViewportSize;
        Color dimColor = Color.Black * intensity * BackgroundDimness * 0.5f;

        RealisticSkyCompatibility.SunBloomOpacity = 1f - intensity;

        Main.spriteBatch.Draw(WhitePixel, new Rectangle(0, 0, (int)screenSize.X, (int)screenSize.Y), dimColor);
    }

    public override Color OnTileColor(Color inColor)
    {
        Main.ColorOfTheSkies = Color.Lerp(Main.ColorOfTheSkies, new(0, 0, 10), intensity * BackgroundDimness * 0.94f);
        return Color.Lerp(inColor, new(10, 0, 51), intensity * BackgroundDimness * 0.98f);
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

    public override float GetCloudAlpha() => 1f;

    public override bool IsActive()
    {
        return isActive || intensity > 0f;
    }

    public override void Reset()
    {
        isActive = false;
    }
    #endregion Boilerplate
}
