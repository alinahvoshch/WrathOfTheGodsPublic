using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;

namespace NoxusBoss.Core.Graphics.Shaders.Screen;

public class GenericScreenShaderData : ScreenShaderData
{
    public GenericScreenShaderData(string passName)
        : base(passName)
    {
    }

    public GenericScreenShaderData(Asset<Effect> shader, string passName)
        : base(shader, passName)
    {
    }

    public override void Apply()
    {
        UseTargetPosition(Main.LocalPlayer.Center);
        UseColor(Color.Transparent);
        base.Apply();
    }
}
