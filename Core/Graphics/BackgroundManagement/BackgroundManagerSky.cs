using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Graphics.BackgroundManagement;

public class BackgroundManagerSky : CustomSky
{
    private bool isActive;

    public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth) =>
        BackgroundManager.Render(minDepth, maxDepth);

    public override void Update(GameTime gameTime) { }

    #region Boilerplate
    public override void Activate(Vector2 position, params object[] args) => isActive = true;

    public override void Deactivate(params object[] args) => isActive = false;

    public override float GetCloudAlpha()
    {
        float cloudOpacity = 1f;
        foreach (Background background in ModContent.GetContent<Background>().Where(b => b.IsActive))
            cloudOpacity = MathF.Min(cloudOpacity, background.CloudOpacity);

        return cloudOpacity;
    }

    public override bool IsActive() => isActive;

    public override void Reset() => isActive = false;

    #endregion Boilerplate
}
