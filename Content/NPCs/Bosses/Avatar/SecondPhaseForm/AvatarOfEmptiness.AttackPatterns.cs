using Terraria;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;

public partial class AvatarOfEmptiness
{
    #region Phase 2

    /// <summary>
    /// The first attack the Avatar performs at the start of the second phase.
    /// </summary>
    public static AvatarAIType[] StartingAttacks => Main.zenithWorld ? [AvatarAIType.GivePlayerHeadpats] : [AvatarAIType.DyingStarsWind, AvatarAIType.LilyStars_ChargePower];

    /// <summary>
    /// The set of attack patterns the Avatar can choose in phase 2 when the player is nearby.
    /// </summary>
    public static AvatarAIType[][] Phase2Attacks_Close => new AvatarAIType[][]
    {
        [AvatarAIType.Erasure],
        [AvatarAIType.RubbleGravitySlam_ApplyExtremeGravity],
        [AvatarAIType.WorldSmash]
    };

    /// <summary>
    /// The set of attack patterns the Avatar can choose in phase 2 when the player is far away.
    /// </summary>
    public static AvatarAIType[][] Phase2Attacks_Far => new AvatarAIType[][]
    {
        [AvatarAIType.RubbleGravitySlam_ApplyExtremeGravity],
        [AvatarAIType.WorldSmash],
        [AvatarAIType.DyingStarsWind, AvatarAIType.LilyStars_ChargePower]
    };

    /// <summary>
    /// The set of attack patterns the Avatar can choose in phase 2 as a distance invariant pool.
    /// </summary>
    public static AvatarAIType[][] Phase2Attacks_Neutral => new AvatarAIType[][]
    {
        [AvatarAIType.Erasure, AvatarAIType.BloodiedWeep],
        [AvatarAIType.DyingStarsWind, AvatarAIType.LilyStars_ChargePower],
        [AvatarAIType.RubbleGravitySlam_ApplyExtremeGravity]
    };

    #endregion Phase 2

    #region Phase 3 and 4

    /// <summary>
    /// The attacks the Avatar performs at the start of phase 3.
    /// </summary>
    public static AvatarAIType[] Phase3StartingAttacks => [AvatarAIType.TeleportAbovePlayer, AvatarAIType.Phase3TransitionScream];

    /// <summary>
    /// The set of randomly choosable attacks in the Avatar's Foggy Cryonic Dimension.
    /// </summary>
    public AvatarAIType[][] SelectableCryonicFogAttacks => [[AvatarAIType.FrostScreenSmash_CreateFrost]];

    /// <summary>
    /// The set of randomly choosable attacks in the Avatar's Cryonic Dimension.
    /// </summary>
    public AvatarAIType[][] SelectableCryonicAttacks => [[AvatarAIType.WhirlingIceStorm]];

    /// <summary>
    /// The set of randomly choosable attacks in the Avatar's Viscera Dimension.
    /// </summary>
    public AvatarAIType[][] SelectableVisceraAttacks => [[AvatarAIType.BloodiedFountainBlasts], [AvatarAIType.Unclog]];

    /// <summary>
    /// The set of randomly choosable attacks in the Avatar's Darkness Dimension.
    /// </summary>
    public static AvatarAIType[][] SelectableDarknessAttacks => [[AvatarAIType.RealityShatter_CreateAndWaitForTelegraph], [AvatarAIType.ArmPortalStrikes]];

    #endregion Phase 3

    #region Phase 4

    /// <summary>
    /// The set of attacks the Avatar does upon starting his fourth phase.
    /// </summary>
    public static AvatarAIType[] Phase4StartingAttackSet => [AvatarAIType.SendPlayerToMyUniverse, AvatarAIType.UniversalAnnihilation, AvatarAIType.TeleportAbovePlayer];

    #endregion Phase 4
}
