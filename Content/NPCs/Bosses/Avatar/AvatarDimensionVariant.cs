using Terraria.Audio;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar;

/// <summary>
/// Represents a part of the Avatar's home universe that he can send the player to.
/// </summary>
/// <param name="BackgroundDrawAction">The background draw action that should be used when players are in the dimension.</param>
/// <param name="AmbientLoopSound">An option looping sound to play when players are in the dimension.</param>
/// <param name="AppliesSilhouetteShader">Whether this dimension applies a universal silhouette shader or not.</param>
public record AvatarDimensionVariant(Action BackgroundDrawAction, SoundStyle? AmbientLoopSound, bool AppliesSilhouetteShader);
