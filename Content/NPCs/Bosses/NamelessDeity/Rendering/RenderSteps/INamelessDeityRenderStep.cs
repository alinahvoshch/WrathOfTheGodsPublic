using Microsoft.Xna.Framework;
using Terraria;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Rendering.RenderSteps;

public interface INamelessDeityRenderStep
{
    /// <summary>
    /// The layering index of this step. Later values correspond to later rendering.
    /// </summary>
    public int LayerIndex
    {
        get;
    }

    /// <summary>
    /// The composite that owns this step.
    /// </summary>
    public NamelessDeityRenderComposite Composite
    {
        get;
        set;
    }

    /// <summary>
    /// Performs initialization for this step.
    /// </summary>
    void Initialize()
    {

    }

    /// <summary>
    /// Performs arbitrary rendering for this step.
    /// </summary>
    void Render(Entity owner, Vector2 drawCenter);
}
