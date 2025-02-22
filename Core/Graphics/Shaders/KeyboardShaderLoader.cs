using NoxusBoss.Content.NPCs.Bosses.Avatar.FirstPhaseForm;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SpecificEffectManagers;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers;
using NoxusBoss.Core.Graphics.Nilk;
using NoxusBoss.Core.World.GameScenes.RiftEclipse;
using NoxusBoss.Core.World.GameScenes.TerminusStairway;
using NoxusBoss.Core.World.Subworlds;
using ReLogic.Peripherals.RGB;
using Terraria;
using Terraria.GameContent.RGB;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Graphics.Shaders;

public class KeyboardShaderLoader : ModSystem
{
    internal const string IDE0051SuppressionReason = "Execution of method depends on the RgbProcessor attribute. This method is not unused.";

    internal const string IDE0060SuppressionReason = "Method layout must be consistent for use with the RgbProcessor attribute. These parameters are necessary.";

    public class SimpleCondition(Func<Player, bool> condition) : CommonConditions.ConditionBase
    {
        /// <summary>
        /// The condition that dictates whether a given keyboard shader should be active.
        /// </summary>
        public readonly Func<Player, bool> Condition = condition;

        public override bool IsActive() => Condition(CurrentPlayer);
    }

    /// <summary>
    /// The set of all loaded keyboard shaders.
    /// </summary>
    private static readonly List<ChromaShader> loadedShaders = [];

    /// <summary>
    /// Whether all keyboard shaders have been loaded.
    /// </summary>
    public static bool HasLoaded
    {
        get;
        private set;
    }

    public override void OnModLoad()
    {
        if (Main.netMode == NetmodeID.Server)
            return;

        // Allow for custom boss tracking with the keyboard shader system.
        On_NPC.UpdateRGBPeriheralProbe += TrackCustomBosses;
    }

    public override void OnModUnload()
    {
        // Manually remove all shaders from the central registry.
        foreach (ChromaShader loadedShader in loadedShaders)
            Main.Chroma.UnregisterShader(loadedShader);
    }

    public override void PostUpdateWorld()
    {
        if (HasLoaded || Main.netMode == NetmodeID.Server)
            return;

        // Register shaders.
        RegisterShader(new AvatarPhase1KeyboardShader(), AvatarPhase1KeyboardShader.IsActive, ShaderLayer.Boss);
        RegisterShader(new AvatarPhase2KeyboardShader(), AvatarPhase2KeyboardShader.IsActive, ShaderLayer.Boss);
        RegisterShader(new NamelessDeityKeyboardShader(), NamelessDeityKeyboardShader.IsActive, ShaderLayer.Boss);
        RegisterShader(new EternalGardenKeyboardShader(), EternalGardenKeyboardShader.IsActive, ShaderLayer.Weather);
        RegisterShader(new RiftEclipseFogKeyboardShader(), RiftEclipseFogKeyboardShader.IsActive, ShaderLayer.Weather);
        RegisterShader(new TerminusStairSkyKeyboardShader(), TerminusStairSkyKeyboardShader.IsActive, ShaderLayer.Weather);
        RegisterShader(new NilkKeyboardShader(), NilkKeyboardShader.IsActive, ShaderLayer.Top);

        HasLoaded = true;
    }

    private void TrackCustomBosses(On_NPC.orig_UpdateRGBPeriheralProbe orig)
    {
        orig();

        // The Avatar of Emptiness.
        if (AvatarRift.Myself is not null)
            CommonConditions.Boss.HighestTierBossOrEvent = ModContent.NPCType<AvatarRift>();
        if (AvatarOfEmptiness.Myself is not null)
            CommonConditions.Boss.HighestTierBossOrEvent = ModContent.NPCType<AvatarOfEmptiness>();

        // Nameless Deity.
        if (NamelessDeityBoss.Myself is not null)
            CommonConditions.Boss.HighestTierBossOrEvent = ModContent.NPCType<NamelessDeityBoss>();
    }

    private static void RegisterShader(ChromaShader keyboardShader, ChromaCondition condition, ShaderLayer layer)
    {
        Main.QueueMainThreadAction(() =>
        {
            Main.Chroma.RegisterShader(keyboardShader, condition, layer);
            loadedShaders.Add(keyboardShader);
        });
    }
}
