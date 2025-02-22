using CalamityMod;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.CrossCompatibility.Inbound.BaseCalamity;
using NoxusBoss.Core.GlobalInstances;
using NoxusBoss.Core.Netcode;
using NoxusBoss.Core.Netcode.Packets;
using NoxusBoss.Core.World.GameScenes.SolynEventHandlers;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Draedon.Draedon;

[JITWhenModsEnabled(CalamityCompatibility.ModName)]
[ExtendsFromMod(CalamityCompatibility.ModName)]
public partial class QuestDraedon : ModNPC
{
    public enum DraedonAIType
    {
        AppearAsHologram,
        DialogueWithSolyn,
        WaitForMarsToArrive,
        ObserveBattle,
        WaitForSomeoneToTakeSeed,
        EndingMonologue,
        Leave
    }

    #region Fields and Properties

    /// <summary>
    /// Draedon's current frame.
    /// </summary>
    public int Frame
    {
        get;
        set;
    }

    /// <summary>
    /// Draedon's current state.
    /// </summary>
    public DraedonAIType CurrentState
    {
        get => (DraedonAIType)NPC.ai[0];
        set => NPC.ai[0] = (int)value;
    }

    /// <summary>
    /// The general-purpose AI timer for Draedon's AI state.
    /// </summary>
    public int AITimer
    {
        get => (int)NPC.ai[1];
        set => NPC.ai[1] = value;
    }

    /// <summary>
    /// The player that Draedon is following.
    /// </summary>
    public Player PlayerToFollow => Main.player[NPC.target];

    /// <summary>
    /// A dedicated timer for use with sheet framing.
    /// </summary>
    public ref float FrameTimer => ref NPC.localAI[0];

    /// <summary>
    /// The 0-1 interpolant which dictates how much Draedon looks like a hologram.
    /// </summary>
    public ref float HologramOverlayInterpolant => ref NPC.localAI[1];

    public override string Texture => GetAssetPath("Content/NPCs/Bosses/Draedon", Name);

    #endregion Fields and Properties

    #region Initialization
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[NPC.type] = 12;
        NPCID.Sets.MustAlwaysDraw[NPC.type] = true;

        this.HideFromBestiary();
        GlobalNPCEventHandlers.PreAIEvent += HandleVanillaDraedonReplacement;
    }

    private bool HandleVanillaDraedonReplacement(NPC npc)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient && npc.type == ModContent.NPCType<CalamityMod.NPCs.ExoMechs.Draedon>())
        {
            bool replaceDraedon = DraedonCombatQuestSystem.MarsBeingSummoned && !NPC.AnyNPCs(Type);
            if (replaceDraedon)
            {
                DraedonCombatQuestSystem.MarsBeingSummoned = false;
                if (Main.netMode == NetmodeID.Server)
                    PacketManager.SendPacket<MarsSummonStatusPacket>();

                npc.Transform(Type);
                return false;
            }
        }
        return true;
    }

    public override void SetDefaults()
    {
        NPC.npcSlots = 50f;

        // Set up hitbox data.
        NPC.width = 86;
        NPC.height = 86;

        // Define stats.
        NPC.damage = 0;
        NPC.defense = 72;
        NPC.lifeMax = 16000;

        // Do not use any default AI states.
        NPC.aiStyle = -1;
        AIType = -1;

        // Use 100% knockback resistance.
        NPC.knockBackResist = 0f;

        // Be immune to lava.
        NPC.lavaImmune = true;

        // Disable tile collision and gravity.
        NPC.noGravity = true;
        NPC.noTileCollide = true;

        // Disable incoming damage.
        NPC.dontTakeDamage = true;

        // Prevent Draedon from providing rage for no cost to the player.
        NPC.Calamity().ProvidesProximityRage = false;
    }

    #endregion Initialization

    #region Network Code

    public override void SendExtraAI(BinaryWriter writer)
    {

    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {

    }

    #endregion Network Code

    #region AI
    public override void AI()
    {
        if (PlayerToFollow.dead || !PlayerToFollow.active)
        {
            NPC.TargetClosest();
            if (PlayerToFollow.dead || !PlayerToFollow.active)
            {
                NPC.active = false;
                return;
            }
        }

        WaitingOnPlayerResponse = false;
        ExecuteCurrentState();
        AITimer++;
        FrameTimer++;
    }

    /// <summary>
    /// Executes Draedon's current AI state.
    /// </summary>
    public void ExecuteCurrentState()
    {
        NPC.boss = false;
        switch (CurrentState)
        {
            case DraedonAIType.AppearAsHologram:
                DoBehavior_AppearAsHologram();
                break;
            case DraedonAIType.DialogueWithSolyn:
                DoBehavior_DialogueWithSolyn();
                break;
            case DraedonAIType.WaitForMarsToArrive:
                DoBehavior_WaitForMarsToArrive();
                break;
            case DraedonAIType.ObserveBattle:
                DoBehavior_ObserveBattle();
                break;
            case DraedonAIType.WaitForSomeoneToTakeSeed:
                DoBehavior_WaitForSomeoneToTakeSeed();
                break;
            case DraedonAIType.EndingMonologue:
                DoBehavior_EndingMonologue();
                break;
            case DraedonAIType.Leave:
                DoBehavior_Leave();
                break;
        }
    }

    /// <summary>
    /// Switches Draedon's AI state to a new one, resetting various things in the process.
    /// </summary>
    /// <param name="newState">The AI that Draedon should switch to.</param>
    public void ChangeAIState(DraedonAIType newState)
    {
        CurrentState = newState;
        AITimer = 0;
        NPC.ai[2] = 0f;
        NPC.ai[3] = 0f;

        NPC.netUpdate = true;
    }

    #endregion AI

    #region Drawing

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color lightColor)
    {
        Texture2D texture = TextureAssets.Npc[NPC.type].Value;
        Texture2D glowmask = GennedAssets.Textures.Draedon.QuestDraedonGlowmask.Value;
        Rectangle frame = texture.Frame(4, Main.npcFrameCount[NPC.type], Frame / Main.npcFrameCount[NPC.type], Frame % Main.npcFrameCount[NPC.type]);
        Vector2 drawPosition = NPC.Center - screenPos;
        Color drawColor = lightColor * NPC.Opacity * Sqrt(1f - HologramOverlayInterpolant);
        Color glowmaskColor = Color.White * NPC.Opacity * Sqrt(1f - HologramOverlayInterpolant);

        bool drawHologramShader = HologramOverlayInterpolant > 0f;
        if (drawHologramShader)
        {
            Main.spriteBatch.PrepareForShaders();

            Vector4 frameArea = new Vector4(frame.Left / (float)texture.Width, frame.Top / (float)texture.Height, frame.Right / (float)texture.Width, frame.Bottom / (float)texture.Height);
            ManagedShader hologramShader = ShaderManager.GetShader("NoxusBoss.DraedonHologramShader");
            hologramShader.TrySetParameter("hologramInterpolant", HologramOverlayInterpolant);
            hologramShader.TrySetParameter("hologramSinusoidalOffset", Pow(HologramOverlayInterpolant, 7f) * 0.02f + InverseLerp(0.4f, 1f, HologramOverlayInterpolant) * 0.04f);
            hologramShader.TrySetParameter("textureSize0", texture.Size());
            hologramShader.TrySetParameter("frameArea", frameArea);
            hologramShader.Apply();
        }

        Main.spriteBatch.Draw(texture, drawPosition, frame, drawColor, NPC.rotation, frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection.ToSpriteDirection() ^ SpriteEffects.FlipHorizontally, 0f);
        Main.spriteBatch.Draw(glowmask, drawPosition, frame, glowmaskColor, NPC.rotation, frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection.ToSpriteDirection() ^ SpriteEffects.FlipHorizontally, 0f);

        if (drawHologramShader)
            Main.spriteBatch.ResetToDefault();
        return false;
    }

    #endregion Drawing

    #region I love automatic despawning

    public override bool CheckActive() => false;

    #endregion I love automatic despawning
}
