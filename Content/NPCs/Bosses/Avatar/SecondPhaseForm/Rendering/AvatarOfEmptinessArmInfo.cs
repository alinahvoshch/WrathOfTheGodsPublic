using Microsoft.Xna.Framework;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm.Rendering;

/// <summary>
/// Represents information about an arm used by the Avatar of Emptiness.
/// </summary>
/// <param name="ElbowPosition">The world position of the arm's elbow.</param>
/// <param name="EffectiveHandPosition">The world position of the arm's hand.</param>
public record AvatarOfEmptinessArmInfo(Vector2 ElbowPosition, Vector2 EffectiveHandPosition);
