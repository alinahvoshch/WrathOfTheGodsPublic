using Luminance.Common.StateMachines;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.Emotes;
using NoxusBoss.Content.Items.MiscOPTools;
using NoxusBoss.Content.NPCs.Bosses.Avatar;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SpecificEffectManagers;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity;
using NoxusBoss.Content.Particles;
using NoxusBoss.Content.Tiles;
using NoxusBoss.Core.CrossCompatibility.Inbound;
using NoxusBoss.Core.CrossCompatibility.Inbound.BaseCalamity;
using NoxusBoss.Core.Graphics.SpecificEffectManagers;
using NoxusBoss.Core.Graphics.UI.SolynDialogue;
using NoxusBoss.Core.World.GameScenes.AvatarAppearances;
using NoxusBoss.Core.World.GameScenes.AvatarUniverseExploration;
using NoxusBoss.Core.World.GameScenes.EndCredits;
using NoxusBoss.Core.World.GameScenes.SolynEventHandlers;
using NoxusBoss.Core.World.GameScenes.Stargazing;
using NoxusBoss.Core.World.Subworlds;
using NoxusBoss.Core.World.WorldGeneration;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Events;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static NoxusBoss.Core.CrossCompatibility.Inbound.CalamityRemix.CalRemixCompatibilitySystem;

namespace NoxusBoss.Content.NPCs.Friendly;

[AutoloadHead]
public partial class Solyn : ModNPC, IPixelatedPrimitiveRenderer
{
    public enum SolynAIType
    {
        StandStill,
        WanderAbout,
        ConfettiTeleportMagicTrick,
        SpeakToPlayer,
        FallFromTheSky,
        GetUpAfterStarFall,

        FollowPlayer,
        CryogenSummonNotice,
        CryogenSummonAnimation,

        WaitAtPermafrostKeep,
        WalkAroundPermafrostKeep,
        TeleportFromPermafrostKeep,

        FollowPlayerToCodebreaker,
        FollowPlayerToGenesis,
        PontificateAboutGenesis,

        IncospicuouslyFlyAwayToDungeon,
        WaitNearCeaselessVoidRift,
        FlyIntoRift,
        WaitInsideRift,
        ExitRift,

        TeleportHome,
        WalkHome,
        EnterTentToSleep,
        Eepy,

        Shimmering,

        EndCreditsCutscene,

        Count
    }

    public enum PathfindingState
    {
        Walk,
        Jump,
        Fly
    }

    #region Fields and Properties

    /// <summary>
    /// Whether Solyn is currently wearing a hat or not.
    /// </summary>
    public bool HasHat
    {
        get;
        set;
    }

    /// <summary>
    /// Whether Solyn jumped this frame.
    /// </summary>
    public bool JustJumped
    {
        get;
        set;
    }

    /// <summary>
    /// Solyn's immunity frame countdown value.
    /// </summary>
    public int ImmunityFrameCounter
    {
        get;
        set;
    }

    /// <summary>
    /// The direction in which Solyn will fall to the ground.
    /// </summary>
    public int SkyFallDirection
    {
        get;
        set;
    }

    /// <summary>
    /// Whether Solyn was summoned from a star-fall or not.
    /// </summary>
    public bool SummonedByStarFall
    {
        get;
        set;
    }

    /// <summary>
    /// Whether Solyn can depart due to time.
    /// </summary>
    public bool CanDepart
    {
        get;
        set;
    }

    /// <summary>
    /// Whether Solyn can be spoken to or not.
    /// </summary>
    public bool CanBeSpokenTo
    {
        get;
        set;
    }

    /// <summary>
    /// Whether Solyn has a bright glow around herself or not.
    /// </summary>
    public bool HasBackglow
    {
        get;
        set;
    }

    /// <summary>
    /// Whether a forced, manual conversation is ongoing.
    /// </summary>
    public bool ForcedConversation
    {
        get;
        set;
    }

    /// <summary>
    /// Whether Solyn should forcefully take damage from hostile projectiles.
    /// </summary>
    public bool ForceDamageFromHostileProjectiles
    {
        get;
        set;
    }

    /// <summary>
    /// Whether Solyn has released a kite out or not.
    /// </summary>
    public bool LetOutKite
    {
        get;
        set;
    }

    /// <summary>
    /// Whether Solyn should reel in her used kite or not.
    /// </summary>
    public bool ReelInKite
    {
        get;
        set;
    }

    /// <summary>
    /// Whether Solyn is actually Soulyn.
    /// </summary>
    public bool SoulForm
    {
        get;
        set;
    }

    /// <summary>
    /// How much Solyn should be visually squished.
    /// </summary>
    public float Squish
    {
        get;
        set;
    }

    /// <summary>
    /// How much Solyn's afterimages clump together.
    /// </summary>
    public float AfterimageClumpInterpolant
    {
        get;
        set;
    }

    /// <summary>
    /// How much Solyn's afterimages glow.
    /// </summary>
    public float AfterimageGlowInterpolant
    {
        get;
        set;
    }

    /// <summary>
    /// Solyn's desired position to wander to.
    /// </summary>
    public Vector2 WanderDestination
    {
        get;
        set;
    }

    /// <summary>
    /// The conversation Solyn will use when speaking to the player.
    /// </summary>
    public Conversation CurrentConversation
    {
        get;
        set;
    }

    /// <summary>
    /// Solyn's AI state before she shimmered.
    /// </summary>
    public SolynAIType StateBeforeShimmering
    {
        get;
        set;
    }

    /// <summary>
    /// The position of Solyn's hat.
    /// </summary>
    public Vector2 HatPosition => NPC.Center - Vector2.UnitY.RotatedBy(NPC.rotation) * NPC.scale * 34f;

    /// <summary>
    /// The amount of afterimages Solyn should draw.
    /// </summary>
    public int AfterimageCount
    {
        get;
        set;
    }

    /// <summary>
    /// The horizontal starting position Solyn was summoned at.
    /// </summary>
    /// <remarks>
    /// Chiefly used for the purpose of ensuring Solyn doesn't wander too far from her starting position.
    /// </remarks>
    public float SpawnPositionX
    {
        get;
        set;
    }

    /// <summary>
    /// The amount by which indicator arrows should fly away due to there being nothing important to discuss.
    /// </summary>
    public float TextFlyAwayDistance
    {
        get;
        set;
    } = 1000f;

    /// <summary>
    /// Solyn's effective scale, taking into account her <see cref="Squish"/>.
    /// </summary>
    public Vector2 EffectiveScale => new Vector2(1f + Squish, 1f - Squish) * NPC.scale;

    /// <summary>
    /// The chance Solyn has to trip and fall every frame when running to follow the player.
    /// </summary>
    public static int TripFallChance => 9600;

    /// <summary>
    /// Whether Solyn has anything important to discuss currently.
    /// </summary>
    public static bool AnythingImportantToDiscuss
    {
        get
        {
            if (CommonCalamityVariables.CryogenDefeated && !SolynDialogRegistry.SolynQuest_DormantKey.NodeSeen("Start") && StargazingQuestSystem.Completed)
                return true;
            if (PostMLRiftAppearanceSystem.AvatarHasCoveredMoon && !SolynDialogRegistry.SolynQuest_GenesisReveal.NodeSeen("Start") && PermafrostKeepQuestSystem.Completed)
                return true;
            if (CommonCalamityVariables.ProvidenceDefeated && !SolynDialogRegistry.SolynQuest_CeaselessVoidBeforeBattle.NodeSeen("Start") && PermafrostKeepQuestSystem.Completed && SolynDialogRegistry.SolynQuest_GenesisReveal.NodeSeen("Start"))
                return true;
            if (CommonCalamityVariables.DraedonDefeated && !SolynDialogRegistry.SolynQuest_DraedonBeforeCombatSimulation.NodeSeen("Start") && CeaselessVoidQuestSystem.Completed && SolynDialogRegistry.SolynQuest_GenesisReveal.NodeSeen("Start"))
                return true;

            return false;
        }
    }

    public PixelationPrimitiveLayer LayerToRenderTo => PixelationPrimitiveLayer.BeforeNPCs;

    /// <summary>
    /// The currently frame Solyn should use on her sprite sheet.
    /// </summary>
    public ref float Frame => ref NPC.localAI[0];

    /// <summary>
    /// The zoom-in interpolant Solyn should use for the current client due to talking with them.
    /// </summary>
    public ref float ZoomInInterpolant => ref NPC.localAI[1];

    /// <summary>
    /// How many frames of immunity Solyn receives upon taking damage.
    /// </summary>
    public static int ImmunityFramesGrantedOnHit => SecondsToFrames(0.45f);

    public override string Texture => GetAssetPath("Content/NPCs/Friendly", Name);

    #endregion Fields and Properties

    #region Initialization
    public override void Load() => Mod.AddNPCHeadTexture(Type, $"{Texture}_Shimmer_Head");

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 26;

        // Ensure Solyn is registered as a pseudo-town-NPC.
        // This allows for certain behaviors such as mutual interaction between town NPCs.
        NPCID.Sets.ActsLikeTownNPC[Type] = true;
        NPCID.Sets.ShimmerTownTransform[Type] = true;
        NPCID.Sets.FaceEmote[Type] = ModContent.EmoteBubbleType<SolynEmote>();

        NPCID.Sets.TrailingMode[Type] = 3;
        NPCID.Sets.TrailCacheLength[Type] = 45;

        EmptinessSprayer.NPCsToNotDelete[Type] = true;

        FannyDialog itsPeak = new FannyDialog("FannyFuckingHATESSolyn", "FannyCryptid").WithDuration(22f).WithCondition(_ =>
        {
            int solynIndex = NPC.FindFirstNPC(ModContent.NPCType<Solyn>());
            if (solynIndex != -1 && CommonCalamityVariables.DraedonDefeated && CommonCalamityVariables.CalamitasDefeated && Main.rand.NextBool(3600))
                return Main.LocalPlayer.WithinRange(Main.npc[solynIndex].Center, 450f);

            return false;
        }).WithoutClickability().WithDrawSizes(1420);
        itsPeak.Register();

        On_Main.HoverOverNPCs += DisableSolynChat;
        On_Main.DrawNPCHeadFriendly += FlipMapHead;
    }

    private void FlipMapHead(On_Main.orig_DrawNPCHeadFriendly orig, Entity entity, byte alpha, float headScale, SpriteEffects dir, int townHeadId, float x, float y)
    {
        if (entity is NPC npc && npc.type == ModContent.NPCType<Solyn>())
            dir ^= SpriteEffects.FlipHorizontally;

        orig(entity, alpha, headScale, dir, townHeadId, x, y);
    }

    private void DisableSolynChat(On_Main.orig_HoverOverNPCs orig, Main self, Rectangle mouseRectangle)
    {
        // This is all dumb. Whatever.
        int battleSolynID = ModContent.NPCType<BattleSolyn>();
        Player player = Main.LocalPlayer;
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC npc = Main.npc[i];
            if (!(npc.active & (npc.shimmerTransparency == 0f || npc.CanApplyHunterPotionEffects())))
                continue;

            if (npc.type == Type && !npc.As<Solyn>().CanBeSpokenTo)
                continue;
            if (npc.type == battleSolynID)
                continue;
            if (!npc.ShowNameOnHover && npc.type != Type)
                continue;

            Main.instance.LoadNPC(npc.type);
            npc.position += npc.netOffset;
            Rectangle npcArea = new Rectangle((int)npc.Bottom.X - npc.frame.Width / 2, (int)npc.Bottom.Y - npc.frame.Height, npc.frame.Width, npc.frame.Height);
            if (npc.type >= NPCID.WyvernHead && npc.type <= NPCID.WyvernTail)
                npcArea = new Rectangle((int)(npc.Center.X - 32f), (int)(npc.Center.Y - 32f), 64, 64);

            NPCLoader.ModifyHoverBoundingBox(npc, ref npcArea);

            bool mouseOverNPC = mouseRectangle.Intersects(npcArea);
            bool interactingWithNPC = mouseOverNPC || (Main.SmartInteractShowingGenuine && Main.SmartInteractNPC == i);

            if (interactingWithNPC && ((npc.type != NPCID.Mimic && npc.type != NPCID.PresentMimic && npc.type != NPCID.IceMimic && npc.aiStyle != 87) || npc.ai[0] != 0f) && npc.type != NPCID.TargetDummy)
            {
                if (npc.type == NPCID.BoundTownSlimeOld)
                {
                    player.cursorItemIconEnabled = true;
                    player.cursorItemIconID = 327;
                    player.cursorItemIconText = "";
                    player.noThrow = 2;
                    if (!player.dead)
                    {
                        PlayerInput.SetZoom_MouseInWorld();
                        if (Main.mouseRight && Main.npcChatRelease)
                        {
                            Main.npcChatRelease = false;
                            if (PlayerInput.UsingGamepad)
                            {
                                player.releaseInventory = false;
                            }
                            if (player.talkNPC != i && !player.tileInteractionHappened && (bool)(typeof(Main).GetMethod("TryFreeingElderSlime", UniversalBindingFlags)?.Invoke(null, [i]) ?? false))
                            {
                                NPC.TransformElderSlime(i);
                                SoundEngine.PlaySound(SoundID.Unlock);
                            }
                        }
                    }
                }
                else
                {
                    bool flag3 = Main.SmartInteractShowingGenuine && Main.SmartInteractNPC == i;
                    if (npc.townNPC || npc.type == NPCID.BoundGoblin || npc.type == NPCID.BoundWizard || npc.type == NPCID.BoundMechanic || npc.type == NPCID.WebbedStylist || npc.type == NPCID.SleepingAngler || npc.type == NPCID.BartenderUnconscious || npc.type == NPCID.SkeletonMerchant || npc.type == NPCID.GolferRescue)
                    {
                        Rectangle rectangle = new Rectangle((int)(player.position.X + (float)(player.width / 2) - (float)(Player.tileRangeX * 16)), (int)(player.position.Y + (float)(player.height / 2) - (float)(Player.tileRangeY * 16)), Player.tileRangeX * 16 * 2, Player.tileRangeY * 16 * 2);
                        Rectangle value2 = new Rectangle((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height);
                        if (rectangle.Intersects(value2))
                        {
                            flag3 = true;
                        }
                    }
                    if (player.ownedProjectileCounts[651] > 0)
                    {
                        flag3 = false;
                    }
                    if (flag3 && !player.dead)
                    {
                        PlayerInput.SetZoom_MouseInWorld();
                        Main.HoveringOverAnNPC = true;
                        Main.instance.currentNPCShowingChatBubble = i;
                        if (Main.mouseRight && Main.npcChatRelease)
                        {
                            Main.npcChatRelease = false;
                            if (PlayerInput.UsingGamepad)
                            {
                                player.releaseInventory = false;
                            }
                            if (player.talkNPC != i && !player.tileInteractionHappened)
                            {
                                Main.CancelHairWindow();
                                Main.SetNPCShopIndex(0);
                                Main.InGuideCraftMenu = false;
                                player.dropItemCheck();
                                Main.npcChatCornerItem = 0;
                                player.sign = -1;
                                Main.editSign = false;
                                player.SetTalkNPC(i);
                                Main.playerInventory = false;
                                player.chest = -1;
                                Recipe.FindRecipes();
                                Main.npcChatText = npc.GetChat();
                                SoundEngine.PlaySound(SoundID.Chat);
                            }
                        }
                    }
                    if (mouseOverNPC && !player.mouseInterface)
                    {
                        player.cursorItemIconEnabled = false;
                        string text = npc.GivenOrTypeName;
                        int effectiveIndex = i;
                        if (npc.realLife >= 0)
                            effectiveIndex = npc.realLife;

                        if (Main.npc[effectiveIndex].lifeMax > 1 && !Main.npc[effectiveIndex].dontTakeDamage)
                            text = text + ": " + Main.npc[effectiveIndex].life + "/" + Main.npc[effectiveIndex].lifeMax;

                        NPCNameFontSystem.npcIDForMouseTextHackZoom = Main.npc[effectiveIndex].type;
                        Main.instance.MouseTextHackZoom(text);
                        Main.mouseText = true;
                        npc.position -= npc.netOffset;
                        break;
                    }
                    if (interactingWithNPC)
                    {
                        npc.position -= npc.netOffset;
                        break;
                    }
                }
            }
            npc.position -= npc.netOffset;
        }
    }

    public override void SetDefaults()
    {
        NPC.npcSlots = 32f;

        // Set up hitbox data.
        NPC.width = 28;
        NPC.height = 48;

        // Define stats.
        NPC.damage = 0;
        NPC.defense = 0;
        NPC.lifeMax = 12000;

        // Fly through all liquids quickly.
        NPC.lavaMovementSpeed = 1f;
        NPC.waterMovementSpeed = 1f;
        NPC.honeyMovementSpeed = 1f;

        // Do not use any default AI states.
        NPC.aiStyle = -1;
        AIType = -1;

        // Use 60% knockback resistance.
        NPC.knockBackResist = 0.4f;

        // Enable gravity and tile collision.
        NPC.noGravity = false;
        NPC.noTileCollide = false;

        // Be immune to lava.
        NPC.lavaImmune = true;

        // Disable damage from hostile NPCs.
        NPC.dontTakeDamageFromHostiles = true;

        // Set the hit sound.
        NPC.HitSound = SoundID.NPCHit1;

        // Act as a town NPC.
        NPC.friendly = true;
        NPC.townNPC = true;
    }

    public override void OnSpawn(IEntitySource source)
    {
        // Choose the conversation to use.
        CurrentConversation = SolynDialogSystem.ChooseSolynConversation();

        // Spawn with a hat if there's a party.
        if (BirthdayParty.PartyIsUp)
            HasHat = true;
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
        {
            BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
            new FlavorTextBestiaryInfoElement($"Mods.{Mod.Name}.Bestiary.{Name}")
        });
    }

    #endregion Initialization

    #region Network Code

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(CurrentConversation?.IdentifierKey ?? string.Empty);

        BitsByte b1 = new BitsByte()
        {
            [0] = SummonedByStarFall,
            [1] = NPC.noGravity,
            [2] = NPC.noTileCollide,
            [3] = CanDepart,
            [4] = WaitingToEnterCVRift
        };

        writer.Write(b1);
        writer.Write((int)StateBeforeShimmering);
        writer.Write(SkyFallDirection);
        writer.Write(SpawnPositionX);
        writer.WriteVector2(WanderDestination);
        writer.WriteVector2(FollowPlayer_FlyDestinationOverride);
        writer.WriteVector2(CurrentPathfindingDestination);
        writer.WriteVector2(WaitNearCeaselessVoidRift_WaitPosition);

        // Write state data.
        var stateStack = (StateMachine?.StateStack ?? new Stack<EntityAIState<SolynAIType>>()).ToList();
        writer.Write(stateStack.Count);
        for (int i = stateStack.Count - 1; i >= 0; i--)
        {
            writer.Write(stateStack[i].Time);
            writer.Write((byte)stateStack[i].Identifier);
        }
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        string conversationKey = reader.ReadString();
        Conversation? conversation = SolynDialogRegistry.SolynConversations.FirstOrDefault(c => c.IdentifierKey == conversationKey);
        if (conversation is not null)
            CurrentConversation = conversation;

        BitsByte b1 = reader.ReadByte();
        SummonedByStarFall = b1[0];
        NPC.noGravity = b1[1];
        NPC.noTileCollide = b1[2];
        CanDepart = b1[3];
        WaitingToEnterCVRift = b1[4];

        StateBeforeShimmering = (SolynAIType)reader.ReadInt32();
        SkyFallDirection = reader.ReadInt32();
        SpawnPositionX = reader.ReadSingle();
        WanderDestination = reader.ReadVector2();
        FollowPlayer_FlyDestinationOverride = reader.ReadVector2();
        CurrentPathfindingDestination = reader.ReadVector2();
        WaitNearCeaselessVoidRift_WaitPosition = reader.ReadVector2();

        // Read state data.
        int stateStackCount = reader.ReadInt32();
        for (int i = 0; i < stateStackCount; i++)
        {
            int time = reader.ReadInt32();
            byte stateType = reader.ReadByte();
            StateMachine.StateStack.Push(StateMachine.StateRegistry[(SolynAIType)stateType]);
            StateMachine.StateRegistry[(SolynAIType)stateType].Time = time;
        }
    }

    #endregion Network Code

    #region AI
    public override void AI()
    {
        if (Main.netMode != NetmodeID.MultiplayerClient && SolynCampsiteWorldGen.CampSitePosition == Vector2.Zero && !EternalGardenUpdateSystem.WasInSubworldLastUpdateFrame)
        {
            SolynCampsiteWorldGen.CampSitePosition = Vector2.One;
            SolynCampsiteWorldGen.GenerateOnNewThread();
        }

        PerformStateSafetyCheck();

        // Reset things every frame.
        ReelInKite = CurrentConversation == SolynDialogRegistry.SolynWindyDay && Main.dayTime;
        NPC.noGravity = false;
        NPC.immortal = true;
        NPC.gfxOffY = 0f;
        NPC.townNPC = true;
        NPC.hide = false;
        NPC.breath = 200;
        NPC.breathCounter = 0;
        CanBeSpokenTo = true;
        CanDepart = !EternalGardenUpdateSystem.WasInSubworldLastUpdateFrame;
        ForcedConversation = false;
        JustJumped = false;
        HasBackglow = false;
        DescendThroughSlopes = false;
        ForceDamageFromHostileProjectiles = false;
        AfterimageCount = 8;
        AfterimageGlowInterpolant = 0.2f;
        AfterimageClumpInterpolant = 0f;
        NPC.Opacity = Saturate(NPC.Opacity + 0.01f);
        Squish *= 0.85f;
        if (ImmunityFrameCounter > 0)
            ImmunityFrameCounter--;
        if (EternalGardenUpdateSystem.WasInSubworldLastUpdateFrame)
        {
            SoulForm = true;
            HasBackglow = true;
        }
        if (CurrentConversation?.RerollCondition() ?? false)
            CurrentConversation = SolynDialogSystem.ChooseSolynConversation();

        // FUCK FUCK FUCK
        if (CurrentConversation == SolynDialogRegistry.SolynQuest_CeaselessVoidBeforeBattle && CommonCalamityVariables.CeaselessVoidDefeated)
            CurrentConversation = SolynDialogRegistry.SolynQuest_CeaselessVoidAfterBattle;

        // Disallow an undefined sprite direction.
        // If it ends up being 0, it'll become 1 by default.
        // If it is not -1 or 1, it'll become the the sign of the original value.
        NPC.spriteDirection = (NPC.spriteDirection >= 0).ToDirectionInt();

        StateMachine.PerformBehaviors();

        if (ModContent.GetInstance<BecomeDuskScene>().IsActive || StargazingScene.IsActive || CurrentConversation == SolynDialogRegistry.SolynQuest_Stargaze || CurrentConversation == SolynDialogRegistry.SolynQuest_Stargaze_Completed)
            CanDepart = false;

        StateMachine.PerformStateTransitionCheck();

        RiftEclipseSnow.CreateSnowWalkEffects(NPC, false);

        // Store the X spawn position on the first frame.
        if (SpawnPositionX == 0f)
        {
            AITimer = 0;
            SpawnPositionX = NPC.Center.X;
            NPC.netUpdate = true;
        }

        // Stay at home if the player is in the ceaseless void rift.
        if (AvatarUniverseExplorationSystem.InAvatarUniverse)
        {
            NPC.Center = SolynCampsiteWorldGen.CampSitePosition - Vector2.UnitY * 32f;
            NPC.velocity = Vector2.Zero;
        }

        // Emit a tiny bit of light.
        DelegateMethods.v3_1 = new Vector3(0.3f, 0.367f, 0.45f) * 0.8f;
        Utils.PlotTileLine(NPC.Top, NPC.Bottom, NPC.width, DelegateMethods.CastLightOpen);

        // Handle UI interactions.
        HandleUIInteractions();

        // Determine if Solyn has been spoken to or not.
        if (Main.netMode != NetmodeID.MultiplayerClient && CurrentState == SolynAIType.SpeakToPlayer && !RandomSolynSpawnSystem.SolynHasBeenSpokenTo)
            RandomSolynSpawnSystem.SolynHasBeenSpokenTo = true;

        // This is necessary to ensure that the map icon is correct.
        NPC.direction = -NPC.spriteDirection;

        // NOOOO SOLYN DON'T DESPAWN!
        NPC.timeLeft = 7200;

        // Increment the AI timer.
        PerformStateSafetyCheck();
        AITimer++;

        // Zoom in on Solyn based on the zoom interpolant.
        if (CurrentState == SolynAIType.SpeakToPlayer)
            CalamityCompatibility.ResetStealthBarOpacity(Main.LocalPlayer);

        if (ZoomInInterpolant > 0f)
        {
            CameraPanSystem.Zoom = Pow(ZoomInInterpolant, 0.7f) * 0.6f;
            CameraPanSystem.PanTowards(NPC.Center, ZoomInInterpolant);
        }
    }

    public void UseStarFlyEffects()
    {
        // Release star particles.
        int starPoints = Main.rand.Next(3, 9);
        float starScaleInterpolant = Main.rand.NextFloat();
        int starLifetime = (int)Lerp(11f, 30f, starScaleInterpolant);
        float starScale = Lerp(0.2f, 0.4f, starScaleInterpolant) * NPC.scale;
        Color starColor = Color.Lerp(new(1f, 0.41f, 0.51f), new(1f, 0.85f, 0.37f), Main.rand.NextFloat());
        Vector2 starSpawnPosition = NPC.Center + new Vector2(NPC.spriteDirection * 10f, 8f) + Main.rand.NextVector2Circular(16f, 16f);
        Vector2 starVelocity = Main.rand.NextVector2Circular(3f, 3f) + NPC.velocity;
        TwinkleParticle star = new TwinkleParticle(starSpawnPosition, starVelocity, starColor, starLifetime, starPoints, new Vector2(Main.rand.NextFloat(0.4f, 1.6f), 1f) * starScale, starColor * 0.5f);
        star.Spawn();

        Frame = 25f;
    }

    public void HandleUIInteractions()
    {
        // Toggle the UI as necessary.
        if ((Main.LocalPlayer.talkNPC == NPC.whoAmI && CanBeSpokenTo) || ForcedConversation)
        {
            CurrentConversation ??= SolynDialogSystem.ChooseSolynConversation();
            CurrentConversation.Start();
            SolynDialogSystem.ShowUI();

            // Zoom in on Solyn.
            ZoomInInterpolant = Saturate(ZoomInInterpolant + (CurrentState == SolynAIType.FollowPlayer ? -0.06f : 0.02f));

            // Switch to the speak-to-player AI state.
            if (CurrentState != SolynAIType.SpeakToPlayer && CurrentState != SolynAIType.FollowPlayer && CurrentState != SolynAIType.CryogenSummonNotice && CurrentState != SolynAIType.CryogenSummonAnimation)
            {
                while (Collision.SolidCollision(NPC.BottomLeft, NPC.width, 2))
                    NPC.position.Y -= 2f;

                AITimer = 0;
                StateMachine.StateStack.Clear();
                StateMachine.StateStack.Push(StateMachine.StateRegistry[SolynAIType.SpeakToPlayer]);
                NPC.netUpdate = true;
            }
            return;
        }

        if (ForcedConversation)
            return;

        if (Main.LocalPlayer.talkNPC == -1 || Main.npc[Main.LocalPlayer.talkNPC].type != Type)
            SolynDialogSystem.HideUI();

        // Zoom out.
        ZoomInInterpolant = Saturate(ZoomInInterpolant - 0.06f);
    }

    #endregion AI

    #region Saving

    public override void SaveData(TagCompound tag)
    {
        if (CurrentState == SolynAIType.FollowPlayer)
            tag["FollowingPlayer"] = true;
        if (CurrentState == SolynAIType.FollowPlayerToCodebreaker)
            tag["FollowingPlayerToCodebreaker"] = true;
        if (CurrentState == SolynAIType.FollowPlayerToGenesis)
            tag["FollowPlayerToGenesis"] = true;
    }

    public override void LoadData(TagCompound tag)
    {
        bool followingPlayer = tag.TryGet("FollowingPlayer", out bool f) && f;
        if (followingPlayer)
        {
            StateMachine.StateStack.Clear();
            StateMachine.StateStack.Push(StateMachine.StateRegistry[SolynAIType.FollowPlayer]);
        }

        followingPlayer = tag.TryGet("FollowingPlayerToCodebreaker", out f) && f;
        if (followingPlayer)
        {
            StateMachine.StateStack.Clear();
            StateMachine.StateStack.Push(StateMachine.StateRegistry[SolynAIType.FollowPlayerToCodebreaker]);
        }

        followingPlayer = tag.TryGet("FollowPlayerToGenesis", out f) && f;
        if (followingPlayer)
        {
            StateMachine.StateStack.Clear();
            StateMachine.StateStack.Push(StateMachine.StateRegistry[SolynAIType.FollowPlayerToGenesis]);
        }
    }

    #endregion Saving

    #region Collision

    public override bool? CanFallThroughPlatforms()
    {
        if (DescendThroughSlopes)
            return true;

        return null;
    }

    public override bool? CanBeHitByProjectile(Projectile projectile)
    {
        if (projectile.hostile && ForceDamageFromHostileProjectiles && ImmunityFrameCounter <= 0)
            return true;
        return null;
    }

    public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
    {
        ImmunityFrameCounter = ImmunityFramesGrantedOnHit;
    }

    #endregion Collision

    #region Drawing

    // This is a bit clunky but it's necessary for Solyn to be interactable as a town NPC.
    // Her actual UI is drawn separately.
    public override string GetChat() => string.Empty;

    public void PerformStandardFraming()
    {
        if (Abs(NPC.velocity.X) <= 0.1f)
        {
            int defaultFrame = 0;
            int blinkFrame = 20;
            if (ShockedExpressionCountdown > 0)
            {
                ShockedExpressionCountdown--;
                defaultFrame = 42;
            }

            Frame = AITimer % 150 >= 147 ? blinkFrame : defaultFrame;
        }
        else
        {
            NPC.frameCounter++;
            if (NPC.frameCounter >= 5)
            {
                Frame++;
                NPC.frameCounter = 0;
            }

            int minFrame = 3;
            int maxFrame = 15;
            bool running = Abs(NPC.velocity.X) >= 9f;
            if (running)
            {
                minFrame = 26;
                maxFrame = 39;
            }

            if (Frame < minFrame)
            {
                // Ensure that running frames are seamlessly moved into instead of resetting the entire animation.
                if (running)
                    Frame += minFrame;
                else
                    Frame = minFrame;
            }
            if (Frame >= maxFrame)
                Frame = minFrame;
        }
    }

    public override void FindFrame(int frameHeight)
    {
        if (NPC.IsABestiaryIconDummy)
        {
            NPC.velocity.X = 5f;
            PerformStandardFraming();
        }

        // Set Solyn's frame.
        NPC.frame.Width = 62;
        NPC.frame.X = (int)(Frame / Main.npcFrameCount[Type]) * NPC.frame.Width;
        NPC.frame.Y = (int)(Frame % Main.npcFrameCount[Type]) * frameHeight;
    }

    // Have a perpetual slight bright glow at all times.
    public override Color? GetAlpha(Color drawColor)
    {
        float immunityPulse = 1f - Cos01(TwoPi * ImmunityFrameCounter / ImmunityFramesGrantedOnHit * 2f);
        Color baseColor = Color.Lerp(drawColor, Color.White, 0.2f);
        Color immunityColor = Color.Lerp(drawColor, new(255, 0, 50), 0.9f);
        Color color = Color.Lerp(baseColor, immunityColor, immunityPulse) * Lerp(1f, 0.3f, immunityPulse);
        return color * NPC.Opacity * (1f - NPC.shimmerTransparency);
    }

    public override void ModifyTypeName(ref string typeName)
    {
        if (Main.gameMenu)
            return;

        // Choose Solyn's name.
        NPC.GivenName = string.Empty;
        if (!SolynDialogRegistry.SolynNameIsKnown)
            typeName = "???";
    }

    public override void BossHeadSpriteEffects(ref SpriteEffects spriteEffects)
    {
        spriteEffects = NPC.spriteDirection.ToSpriteDirection();
    }

    public override void DrawBehind(int index)
    {
        if (CurrentState == SolynAIType.FlyIntoRift)
            Main.instance.DrawCacheNPCsOverPlayers.Add(index);

        if (ModContent.GetInstance<EndCreditsScene>().IsActive && NamelessDeityBoss.Myself is not null)
        {
            Main.instance.DrawCacheNPCsBehindNonSolidTiles.Add(index);
            return;
        }

        SpecialLayeringSystem.DrawCacheOverTent.Add(index);
    }

    public void RenderArrow()
    {
        float opacity = Pow(InverseLerp(350f, 0f, TextFlyAwayDistance), 2.5f);
        Vector2 drawCenter = NPC.Center - Main.screenPosition - Vector2.UnitX * NPC.spriteDirection * 4f;

        // Calculate jingle variables.
        float jingleTime = Main.GlobalTimeWrappedHourly * 3.5f % 20f;
        float jingleDecayFactor = Exp(MathF.Max(jingleTime - Pi, 0f) * -0.67f);
        float jinglePeriodFactor = Sin(jingleTime * 2.2f);
        float jingleInterpolant = jingleDecayFactor * jinglePeriodFactor;

        // Draw the arrow that points at Permafrost.
        float arrowDirection = PiOver2;
        float arrowRotation = arrowDirection - PiOver2 + jingleInterpolant * 0.3f;
        Vector2 arrowScale = new Vector2(1f - jingleInterpolant * 0.2f, 1f + jingleInterpolant * 0.15f) * (0.7f + Abs(jingleInterpolant) * 0.26f);

        int arrowFrame = (int)(Main.GlobalTimeWrappedHourly * 11f) % 6;
        float arrowHoverOffset = TextFlyAwayDistance + Abs(jingleInterpolant) * 36f + 80f;
        Texture2D arrowTexture = GennedAssets.Textures.SolynTalkIndicator.TalkUIArrow.Value;
        Rectangle arrowFrameArea = arrowTexture.Frame(1, 6, 0, arrowFrame);

        Vector2 arrowDrawPosition = drawCenter - arrowDirection.ToRotationVector2() * arrowHoverOffset;
        Main.EntitySpriteDraw(arrowTexture, arrowDrawPosition, arrowFrameArea, Color.White * opacity, arrowRotation, arrowFrameArea.Size() * 0.5f, arrowScale, 0);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (Main.instance.currentNPCShowingChatBubble == NPC.whoAmI && !CanBeSpokenTo)
            Main.instance.currentNPCShowingChatBubble = -1;

        if (!AnythingImportantToDiscuss && TextFlyAwayDistance < 4000f)
            TextFlyAwayDistance = (TextFlyAwayDistance + 1.85f) * 1.09f;
        else if (AnythingImportantToDiscuss)
            TextFlyAwayDistance *= 0.81f;

        if (TextFlyAwayDistance < 350f && CurrentState != SolynAIType.Eepy)
            RenderArrow();

        Vector2 drawPosition = NPC.Center - screenPos + Vector2.UnitY * (NPC.gfxOffY - 6f);
        if (NPC.IsShimmerVariant)
        {
            Texture2D shimmerTexture = ModContent.Request<Texture2D>($"{Texture}_Shimmer").Value;
            Main.EntitySpriteDraw(shimmerTexture, drawPosition, null, NPC.GetAlpha(drawColor), NPC.rotation, shimmerTexture.Size() * 0.5f, EffectiveScale, 0);
            return false;
        }

        // Draw Solyn.
        Color glowmaskColor = Color.White;
        Rectangle frame = NPC.frame;
        Texture2D texture = TextureAssets.Npc[Type].Value;

        if (SoulForm)
        {
            Main.spriteBatch.PrepareForShaders();

            glowmaskColor = new(255, 178, 97);
            drawColor = glowmaskColor;

            ManagedShader soulShader = ShaderManager.GetShader("NoxusBoss.SoulynShader");
            soulShader.TrySetParameter("outlineOnly", true);
            soulShader.TrySetParameter("imageSize", texture.Size());
            soulShader.TrySetParameter("sourceRectangle", new Vector4(NPC.frame.X, NPC.frame.Y, NPC.frame.Width, NPC.frame.Height));
            soulShader.Apply();
        }

        SpriteEffects direction = NPC.spriteDirection.ToSpriteDirection() ^ SpriteEffects.FlipHorizontally;
        if (HasBackglow)
        {
            Color backglowColor = (SoulForm ? Color.White : Color.Cyan) with { A = 0 };
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                    Main.EntitySpriteDraw(texture, drawPosition + (TwoPi * i / 4f).ToRotationVector2() * 2f, frame, NPC.GetAlpha(backglowColor), NPC.rotation, frame.Size() * 0.5f, EffectiveScale, direction);
            }
        }

        if (SoulForm)
        {
            ManagedShader soulShader = ShaderManager.GetShader("NoxusBoss.SoulynShader");
            soulShader.TrySetParameter("outlineOnly", false);
            soulShader.Apply();
        }

        if (AvatarOfEmptinessSky.Dimension != AvatarDimensionVariants.AntishadowDimension)
        {
            for (int i = AfterimageCount; i >= 0; i--)
            {
                Vector2 afterimageDrawPosition = drawPosition + NPC.oldPos[i] - NPC.position;
                afterimageDrawPosition = Vector2.Lerp(afterimageDrawPosition, drawPosition, AfterimageClumpInterpolant);

                Color afterimageColor = new Color(0f, 0.25f, 1f, 0f);
                Main.EntitySpriteDraw(texture, afterimageDrawPosition, frame, NPC.GetAlpha(afterimageColor) * (1f - i / (float)AfterimageCount) * AfterimageGlowInterpolant, NPC.rotation, frame.Size() * 0.5f, EffectiveScale, direction);
            }
        }

        Main.EntitySpriteDraw(texture, drawPosition, frame, NPC.GetAlpha(drawColor), NPC.rotation, frame.Size() * 0.5f, EffectiveScale, direction);
        Main.EntitySpriteDraw(GennedAssets.Textures.Friendly.SolynGlow.Value, drawPosition, frame, NPC.GetAlpha(glowmaskColor) * 0.26f, NPC.rotation, frame.Size() * 0.5f, EffectiveScale, direction);

        if (SoulForm)
            Main.spriteBatch.ResetToDefault();

        // Draw a party hat if necessary.
        if (HasHat)
        {
            Texture2D hatTexture = ModContent.Request<Texture2D>("NoxusBoss/Assets/Textures/Content/NPCs/Friendly/SolynPartyHat").Value;
            Main.EntitySpriteDraw(hatTexture, HatPosition - screenPos, null, NPC.GetAlpha(drawColor), NPC.rotation, hatTexture.Size() * 0.5f, EffectiveScale, direction);
        }

        return false;
    }

    public float StarFallTrailWidthFunction(float completionRatio) => EffectiveScale.X * Utils.Remap(completionRatio, 0f, 0.9f, 32f, 1f);

    public Color StarFallTrailColorFunction(float completionRatio) => NPC.GetAlpha(new(75, 128, 250)) * Sqrt(1f - completionRatio);

    public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
    {
        if (CurrentState != SolynAIType.FallFromTheSky)
            return;

        PrimitiveSettings settings = new PrimitiveSettings(StarFallTrailWidthFunction, StarFallTrailColorFunction, _ => NPC.Size * 0.5f + NPC.velocity.SafeNormalize(Vector2.Zero) * 4f, Pixelate: true);
        PrimitiveRenderer.RenderTrail(NPC.oldPos.Take(5), settings, 42);
    }

    #endregion Drawing

    #region I love automatic despawning

    public override bool CheckActive() => false;

    #endregion I love automatic despawning
}
