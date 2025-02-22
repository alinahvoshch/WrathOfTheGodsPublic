namespace NoxusBoss.Core.Graphics.UI.Books;

public struct SolynProgressionRatioReward
{
    /// <summary>
    /// The percentage of books that need to be unlocked as a baseline for the player to receive this.
    /// </summary>
    public float BookProgressionPercentage;

    /// <summary>
    /// The reward to be granted to the player.
    /// </summary>
    public SolynReward Reward;
}
