using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Rendering;

public class CensorRenderManager : ModSystem
{
    private static readonly Queue<DrawData> drawQueue = [];

    public override void OnModLoad() => Main.OnPostDraw += ProcessDrawQueue;

    public override void OnModUnload() => Main.OnPostDraw -= ProcessDrawQueue;

    private static void ProcessDrawQueue(GameTime obj)
    {
        if (drawQueue.Count <= 0)
            return;

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, CullOnlyScreen, null, Main.GameViewMatrix.TransformationMatrix);

        while (drawQueue.TryDequeue(out DrawData drawData))
            drawData.Draw(Main.spriteBatch);

        Main.spriteBatch.End();
    }

    /// <summary>
    /// Enqueues a given draw data instance for rendering at the end of the rendering process.
    /// </summary>
    /// <param name="drawData">The draw data to render.</param>
    public static void Enqueue(DrawData drawData) => drawQueue.Enqueue(drawData);
}
