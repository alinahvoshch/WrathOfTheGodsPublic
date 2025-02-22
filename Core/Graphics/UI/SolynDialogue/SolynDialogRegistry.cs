using Luminance.Core.Cutscenes;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using NoxusBoss.Content.Items;
using NoxusBoss.Content.Items.GenesisComponents;
using NoxusBoss.Content.NPCs.Friendly;
using NoxusBoss.Core.CrossCompatibility.Inbound;
using NoxusBoss.Core.CrossCompatibility.Inbound.BaseCalamity;
using NoxusBoss.Core.GlobalInstances;
using NoxusBoss.Core.Graphics.UI.Books;
using NoxusBoss.Core.Netcode;
using NoxusBoss.Core.Netcode.Packets;
using NoxusBoss.Core.World;
using NoxusBoss.Core.World.GameScenes.AvatarAppearances;
using NoxusBoss.Core.World.GameScenes.EndCredits;
using NoxusBoss.Core.World.GameScenes.RiftEclipse;
using NoxusBoss.Core.World.GameScenes.SolynEventHandlers;
using NoxusBoss.Core.World.GameScenes.Stargazing;
using NoxusBoss.Core.World.Subworlds;
using NoxusBoss.Core.World.WorldGeneration;
using NoxusBoss.Core.World.WorldSaving;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Graphics.UI.SolynDialogue;

// I am well aware. This should absolutely use data-oriented principles.
// I tried to unroot the existing code to work under that framework, to no avail.
// It was too late; my fate was already sealed.
// My only recommendation to you, dear reader, is to not use this system as a base. You will not have a fun time if you do.
public class SolynDialogRegistry : ModSystem
{
    /// <summary>
    /// The roster of all potential Solyn conversations.
    /// </summary>
    internal static List<Conversation> SolynConversations = [];

    /// <summary>
    /// The leading introduction to Solyn. Displayed the first time she's encountered, barring exceptional circumstances.
    /// </summary>
    public static Conversation SolynIntroduction
    {
        get;
        private set;
    }

    /// <summary>
    /// The dialogue that Solyn uses at dawn.
    /// </summary>
    public static Conversation SolynDawnDialogue
    {
        get;
        private set;
    }

    /// <summary>
    /// The dialogue that Solyn uses during sunny days.
    /// </summary>
    public static Conversation SolynSunnyDayDialogue
    {
        get;
        private set;
    }

    /// <summary>
    /// The dialogue that Solyn uses during rainy days.
    /// </summary>
    public static Conversation SolynRainyDayDialogue
    {
        get;
        private set;
    }

    /// <summary>
    /// The dialogue that Solyn uses at dusk.
    /// </summary>
    public static Conversation SolynRainyDuskDialogue
    {
        get;
        private set;
    }

    /// <summary>
    /// Dialogue that Solyn can use during windy days.
    /// </summary>
    public static Conversation SolynWindyDay
    {
        get;
        private set;
    }

    /// <summary>
    /// Dialogue that Solyn can use during slime rain.
    /// </summary>
    public static Conversation SolynSlimeRainDialogue
    {
        get;
        private set;
    }

    /// <summary>
    /// A random conversation Solyn can use about storage sails.
    /// </summary>
    public static Conversation SolynStorageSailsDialogue
    {
        get;
        private set;
    }

    /// <summary>
    /// A random conversation Solyn can use about books.
    /// </summary>
    public static Conversation SolynBooksDialogue
    {
        get;
        private set;
    }

    /// <summary>
    /// The conversation Solyn uses once the player gives her a book via her bookshelf.
    /// </summary>
    public static Conversation SolynOneRedeemedBookDialogue
    {
        get;
        private set;
    }

    /// <summary>
    /// The conversation Solyn uses once the player redeems an unreadable book (such as the Book of Miracles).
    /// </summary>
    public static Conversation SolynUnreadableBookDialogue
    {
        get;
        private set;
    }

    /// <summary>
    /// The conversation Solyn uses once the player gives her many books via her bookshelf (specifically 50% of the available set).
    /// </summary>
    public static Conversation SolynManyRedeemedBooksDialogue
    {
        get;
        private set;
    }

    /// <summary>
    /// The conversation Solyn uses late into progression.
    /// </summary>
    public static Conversation SolynLategameDialogue
    {
        get;
        private set;
    }

    /// <summary>
    /// The conversation Solyn uses once the bookshelf is completed.
    /// </summary>
    public static Conversation SolynCompletedBookhselfDialogue
    {
        get;
        private set;
    }

    /// <summary>
    /// Dialogue that Solyn can use during parties.
    /// </summary>
    public static Conversation SolynParty
    {
        get;
        private set;
    }

    /// <summary>
    /// The dialogue Solyn uses when discussing the Stargaze quest.
    /// </summary>
    public static Conversation SolynQuest_Stargaze
    {
        get;
        private set;
    }

    /// <summary>
    /// The dialogue Solyn uses after the Stargaze quest has been completed.
    /// </summary>
    public static Conversation SolynQuest_Stargaze_Completed
    {
        get;
        private set;
    }

    /// <summary>
    /// The dialogue Solyn uses after the Stargaze quest has been completed and the player saw the rift in the telescope.
    /// </summary>
    public static Conversation SolynQuest_Stargaze_AfterRift
    {
        get;
        private set;
    }

    /// <summary>
    /// Dialogue that Solyn uses if the player has the dormant key.
    /// </summary>
    public static Conversation SolynQuest_DormantKey
    {
        get;
        private set;
    }

    /// <summary>
    /// Dialogue that Solyn uses if the player has yet to open the door to Permafrost's keep.
    /// </summary>
    public static Conversation SolynQuest_DormantKey_AtDoor
    {
        get;
        private set;
    }

    /// <summary>
    /// Dialogue that Solyn uses when near the seed in Permafrost's keep.
    /// </summary>
    public static Conversation SolynQuest_DormantKey_AtSeed
    {
        get;
        private set;
    }

    /// <summary>
    /// Dialogue that Solyn uses a bit after the frostblessed seedling has been obtained.
    /// </summary>
    public static Conversation SolynFrostblessedSeedlingDialogue
    {
        get;
        private set;
    }

    /// <summary>
    /// Dialogue that Solyn uses when near the seed in Permafrost's keep.
    /// </summary>
    public static Conversation SolynQuest_GenesisReveal
    {
        get;
        private set;
    }

    /// <summary>
    /// Dialogue that Solyn uses after Providence has been defeated that indicates the second Genesis quest.
    /// </summary>
    public static Conversation SolynQuest_CeaselessVoidBeforeBattle
    {
        get;
        private set;
    }

    /// <summary>
    /// Dialogue that Solyn uses after the Ceaseless Void has been defeated.
    /// </summary>
    public static Conversation SolynQuest_CeaselessVoidAfterBattle
    {
        get;
        private set;
    }

    /// <summary>
    /// Dialogue that Solyn uses before entering the Ceaseless Void's rift.
    /// </summary>
    public static Conversation SolynQuest_CeaselessVoidBeforeEnteringRift
    {
        get;
        private set;
    }

    /// <summary>
    /// Dialogue that Solyn uses after entering the Ceaseless Void's rift.
    /// </summary>
    public static Conversation SolynQuest_CeaselessVoidAfterEnteringRift
    {
        get;
        private set;
    }

    /// <summary>
    /// Dialogue that Solyn uses when initiating the Draedon quest, before the combat simulation.
    /// </summary>
    public static Conversation SolynQuest_DraedonBeforeCombatSimulation
    {
        get;
        private set;
    }

    /// <summary>
    /// Dialogue that Solyn uses once the Genesis is completed.
    /// </summary>
    public static Conversation SolynQuest_GenesisCompletion
    {
        get;
        private set;
    }

    /// <summary>
    /// Dialogue that Solyn uses as a fallback if no other dialogue is available to be chosen.
    /// </summary>
    public static Conversation SolynErrorDialogue
    {
        get;
        private set;
    }

    /// <summary>
    /// Whether the player knows Solyn's name yet or not.
    /// </summary>
    public static bool SolynNameIsKnown => SolynIntroduction.NodeSeen("Talk4");

    public const string KeyPrefix = "Mods.NoxusBoss.Solyn.";

    public override void OnModLoad()
    {
        LoadSolynIntroductionDialogue();
        LoadSolynWindyDayDialogue();
        LoadSolynPartyDialogue();
        LoadSolynRandomDialogue();

        LoadSolynQuestDialogue();

        LoadSolynGardenDialogue();

        LoadSolynErrorDialogue();

        GlobalNPCEventHandlers.OnKillEvent += RerollConversationOnBossDefeats;
    }

    private void RerollConversationOnBossDefeats(NPC npc)
    {
        // Cryogen death, for the Permafrost Keep quest.
        if (CalamityCompatibility.Calamity is not null && CalamityCompatibility.Calamity.TryFind("Cryogen", out ModNPC cryogen) && npc.type == cryogen.Type && !PermafrostKeepQuestSystem.Ongoing)
            SolynDialogSystem.ForceChangeConversationForSolyn(SolynQuest_DormantKey);

        // Providence death, for the Ceaseless Void quest.
        if (CalamityCompatibility.Calamity is not null && CalamityCompatibility.Calamity.TryFind("Providence", out ModNPC provi) && npc.type == provi.Type && !CeaselessVoidQuestSystem.Ongoing)
            SolynDialogSystem.ForceChangeConversationForSolyn(SolynQuest_CeaselessVoidBeforeBattle);

        // Ceaseless Void death, for the Ceaseless Void quest.
        if (CalamityCompatibility.Calamity is not null && CalamityCompatibility.Calamity.TryFind("CeaselessVoid", out ModNPC cv) && npc.type == cv.Type && CeaselessVoidQuestSystem.Ongoing && !CeaselessVoidQuestSystem.Completed)
        {
            CeaselessVoidQuestSystem.RiftDefeatPosition = npc.Center;
            SolynDialogSystem.ForceChangeConversationForSolyn(SolynQuest_CeaselessVoidAfterBattle);
        }

        // Exo Mech deaths, for the Mars quest.
        if (CalamityCompatibility.Calamity is not null &&
            CalamityCompatibility.Calamity.TryFind("AresBody", out ModNPC ares) &&
            CalamityCompatibility.Calamity.TryFind("Apollo", out ModNPC apollo) &&
            CalamityCompatibility.Calamity.TryFind("ThanatosHead", out ModNPC thanatos))
        {
            bool lastExoMech = NPC.CountNPCS(ares.Type) + NPC.CountNPCS(apollo.Type) + NPC.CountNPCS(thanatos.Type) <= 1;
            if (lastExoMech && (npc.type == ares.Type || npc.type == apollo.Type || npc.type == thanatos.Type) && !DraedonCombatQuestSystem.Ongoing)
            {
                SolynDialogSystem.ForceChangeConversationForSolyn(SolynQuest_DraedonBeforeCombatSimulation);
            }
        }
    }

    internal static void LoadSolynIntroductionDialogue()
    {
        // Store nodes.
        SolynIntroduction = CreateSolynConversation("SolynIntroduction", () =>
        {
            if (SolynIntroduction.NodeSeen("Talk8"))
                return "Repeat";

            if (SolynIntroduction.NodeSeen("Talk1"))
                return "Talk2";

            return "Start";
        }, ConversationPriority.Introduction, () => true);

        var start = SolynIntroduction.CreateFromKey("Start", false);
        var talk1 = SolynIntroduction.CreateFromKey("Talk1", false);
        var question1 = SolynIntroduction.CreateFromKey("Question1", true);
        var talk2 = SolynIntroduction.CreateFromKey("Talk2", false);
        var talk3 = SolynIntroduction.CreateFromKey("Talk3", false);
        var question2 = SolynIntroduction.CreateFromKey("Question2", true);
        var talk4 = SolynIntroduction.CreateFromKey("Talk4", false);
        var talk5 = SolynIntroduction.CreateFromKey("Talk5", false);
        var talk6 = SolynIntroduction.CreateFromKey("Talk6", false);
        var talk7 = SolynIntroduction.CreateFromKey("Talk7", false);
        var talk8 = SolynIntroduction.CreateFromKey("Talk8", false);
        var question3 = SolynIntroduction.CreateFromKey("Question3", true);

        SolynIntroduction.CreateFromKey("Repeat", false);

        LinkChain(start, talk1, question1, talk2, talk3, question2, talk4, talk5, talk6, talk7, question3, talk8);
    }

    internal static void LoadSolynWindyDayDialogue()
    {
        // Store nodes.
        SolynWindyDay = CreateSolynConversation("SolynWindyDayTalk", () =>
        {
            bool sawBaseDialogue = SolynWindyDay.NodeSeen("WindyResponse7");
            bool playerHasKite = StarKite.TotalKitesOwnedByPlayer(Main.LocalPlayer) >= 1;
            bool playerChangedMind = playerHasKite && SolynWindyDay.NodeSeen("WindyRepeatDefault") && !SolynWindyDay.NodeSeen("WindyRepeatPlayerUsingKiteChangedMind");
            if (sawBaseDialogue)
            {
                if (playerChangedMind)
                    return "WindyRepeatPlayerUsingKiteChangedMind";

                return playerHasKite ? "WindyRepeatPlayerUsingKite" : "WindyRepeatDefault";
            }

            return "WindyStart";
        }, ConversationPriority.ConnectionEvent + 1, () => Main.IsItAHappyWindyDay && !RiftEclipseManagementSystem.RiftEclipseOngoing).WithRerollCondition(() => !Main.IsItAHappyWindyDay);
        var start = SolynWindyDay.CreateFromKey("WindyStart", false);
        var question1 = SolynWindyDay.CreateFromKey("WindyQuestion1", true);
        var response1 = SolynWindyDay.CreateFromKey("WindyResponse1", false);
        var response2 = SolynWindyDay.CreateFromKey("WindyResponse2", false);
        var question2 = SolynWindyDay.CreateFromKey("WindyQuestion2", true);
        var response3 = SolynWindyDay.CreateFromKey("WindyResponse3", false);
        var response4 = SolynWindyDay.CreateFromKey("WindyResponse4", false);
        var response5 = SolynWindyDay.CreateFromKey("WindyResponse5", false);
        var response6 = SolynWindyDay.CreateFromKey("WindyResponse6", false).AddEndAction(() =>
        {
            // Gift the player a kite.
            if (Main.LocalPlayer.CountItem(ModContent.ItemType<StarKite>()) <= 0)
                Main.LocalPlayer.QuickSpawnItem(new EntitySource_WorldEvent(), ModContent.ItemType<StarKite>());
        });
        var response7 = SolynWindyDay.CreateFromKey("WindyResponse7", false);
        LinkChain(start, question1, response1, response2, question2, response3, response4, response5, response6, response7);

        SolynWindyDay.CreateFromKey("WindyRepeatPlayerUsingKite", false);
        SolynWindyDay.CreateFromKey("WindyRepeatPlayerUsingKiteChangedMind", false);
        SolynWindyDay.CreateFromKey("WindyRepeatDefault", false);
    }

    internal static void LoadSolynPartyDialogue()
    {
        // Store nodes.
        SolynParty = CreateSolynConversation("SolynPartyTalk", () =>
        {
            if (SolynParty.NodeSeen("PartyResponseYes3") || SolynParty.NodeSeen("PartyResponseNo2"))
                return "PartyRepeat1";
            if (SolynParty.NodeSeen("PartyResponseYes1"))
                return "PartyResponseYes2";

            return "PartyStart";
        }, ConversationPriority.ConnectionEvent, () => BirthdayParty.PartyIsUp && !RiftEclipseManagementSystem.RiftEclipseOngoing && false);

        var start = SolynParty.CreateFromKey("PartyStart", false);
        var response1 = SolynParty.CreateFromKey("PartyResponse1", false);
        var response2 = SolynParty.CreateFromKey("PartyResponse2", false);
        var response3 = SolynParty.CreateFromKey("PartyResponse3", false);
        var response4 = SolynParty.CreateFromKey("PartyResponse4", false);
        var magicTrickYes = SolynParty.CreateFromKey("PartyQuestionYes", true);
        var magicTrickNo = SolynParty.CreateFromKey("PartyQuestionNo", true);

        var yesResponse1 = SolynParty.CreateFromKey("PartyResponseYes1", false).AddEndAction(() =>
        {
            int solynID = ModContent.NPCType<Solyn>();
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (n.type != solynID || !n.active)
                    continue;

                n.As<Solyn>().StateMachine.StateStack.Clear();
                n.As<Solyn>().StateMachine.StateStack.Push(n.As<Solyn>().StateMachine.StateRegistry[Solyn.SolynAIType.ConfettiTeleportMagicTrick]);
                n.As<Solyn>().AITimer = 0;
                n.velocity.X *= 0.4f;
                n.netUpdate = true;

                if (Main.LocalPlayer.talkNPC == i)
                    Main.LocalPlayer.SetTalkNPC(-1);
            }
        });
        var yesResponse2AfterTP = SolynParty.CreateFromKey("PartyResponseYes2", false);
        var yesResponse3 = SolynParty.CreateFromKey("PartyResponseYes3", false);
        var noResponse1 = SolynParty.CreateFromKey("PartyResponseNo1", false);
        var noResponse2 = SolynParty.CreateFromKey("PartyResponseNo2", false);
        SolynParty.CreateFromKey("PartyRepeat1", false);

        // Link nodes.
        LinkChain(start, response1, response2, response3, response4);
        response4.AddChildren(magicTrickYes, magicTrickNo);
        LinkChain(magicTrickYes, yesResponse1);
        LinkChain(magicTrickNo, noResponse1, noResponse2);
        LinkChain(yesResponse2AfterTP, yesResponse3);
    }

    internal static void LoadSolynRandomDialogue()
    {
        LoadSolynRandomDialogue_Dawn();
        LoadSolynRandomDialogue_SunnyDay();
        LoadSolynRandomDialogue_Dusk();
        LoadSolynRandomDialogue_RainyDay();
        LoadSolynRandomDialogue_SlimeRain();
        LoadSolynRandomDialogue_StorageSails();
        LoadSolynRandomDialogue_Books();
        LoadSolynRandomDialogue_OneRedeemedBook();
        LoadSolynRandomDialogue_UnreadableBook();
        LoadSolynRandomDialogue_ManyBooks();
        LoadSolynRandomDialogue_Lategame();
        LoadSolynRandomDialogue_CompletedBookshelf();
    }

    internal static void LoadSolynGardenDialogue()
    {
        // Store nodes.
        SolynParty = CreateSolynConversation("PostNamelessDiscussion", () =>
        {
            if (SolynParty.NodeSeen("ApologyResponse"))
                return "QuestionsStart";

            return "Apology1";
        }, ConversationPriority.Endgame, () => EternalGardenUpdateSystem.WasInSubworldLastUpdateFrame, () => true);

        var apology1 = SolynParty.CreateFromKey("Apology1", false);
        var apology2 = SolynParty.CreateFromKey("Apology2", false);
        var apology3 = SolynParty.CreateFromKey("Apology3", false);
        var apology4 = SolynParty.CreateFromKey("Apology4", false);
        var apology5 = SolynParty.CreateFromKey("Apology5", false);
        var apology6 = SolynParty.CreateFromKey("Apology6", false);
        var apologyResponse = SolynParty.CreateFromKey("ApologyResponse", true);
        var questionsStart = SolynParty.CreateFromKey("QuestionsStart", false);

        var endUpHereQuestion = SolynParty.CreateFromKey("HowDidYouEndUpHereQuestion", true);
        var endUpHere1 = SolynParty.CreateFromKey("HowDidYouEndUpHere1", false);
        var endUpHere2 = SolynParty.CreateFromKey("HowDidYouEndUpHere2", false);
        var endUpHere3 = SolynParty.CreateFromKey("HowDidYouEndUpHere3", false);
        var endUpHere4 = SolynParty.CreateFromKey("HowDidYouEndUpHere4", false);
        var endUpHere5 = SolynParty.CreateFromKey("HowDidYouEndUpHere5", false);

        var saveMeQuestion = SolynParty.CreateFromKey("WhyDidYouSaveMeQuestion", true);
        var saveMe1 = SolynParty.CreateFromKey("WhyDidYouSaveMe1", false);
        var saveMe2 = SolynParty.CreateFromKey("WhyDidYouSaveMe2", false);
        var saveMe3 = SolynParty.CreateFromKey("WhyDidYouSaveMe3", false);
        var saveMe4 = SolynParty.CreateFromKey("WhyDidYouSaveMe4", false);
        var saveMe5 = SolynParty.CreateFromKey("WhyDidYouSaveMe5", false);

        var whatNowQuestion = SolynParty.CreateFromKey("WhatNowQuestion", true);
        var whatNow1 = SolynParty.CreateFromKey("WhatNow1", false);
        var whatNow2 = SolynParty.CreateFromKey("WhatNow2", false);
        var whatNow3 = SolynParty.CreateFromKey("WhatNow3", false);
        var whatNow4 = SolynParty.CreateFromKey("WhatNow4", false);
        var whatNow5 = SolynParty.CreateFromKey("WhatNow5", false);

        var deityQuestion = SolynParty.CreateFromKey("DeityQuestion", true);
        var deity1 = SolynParty.CreateFromKey("Deity1", false);
        var deity2 = SolynParty.CreateFromKey("Deity2", false);
        var deity3 = SolynParty.CreateFromKey("Deity3", false);

        var treeQuestion = SolynParty.CreateFromKey("TreeQuestion", true, () => Main.netMode == NetmodeID.SinglePlayer && SolynBookExchangeRegistry.RedeemedAllBooks, () =>
        {
            return Color.Lerp(new(40, 175, 65), new(255, 251, 81), Cos01(Main.GlobalTimeWrappedHourly * 0.9f));
        });
        var tree1 = SolynParty.CreateFromKey("Tree1", false);
        var tree2 = SolynParty.CreateFromKey("Tree2", false).AddClickAwayAction(() =>
        {
            CutsceneManager.QueueCutscene(ModContent.GetInstance<EndCreditsScene>());
        });

        LinkChain(apology1, apology2, apology3, apology4, apology5, apology6, apologyResponse, questionsStart);
        LinkChain(endUpHereQuestion, endUpHere1, endUpHere2, endUpHere3, endUpHere4, endUpHere5, questionsStart);
        LinkChain(saveMeQuestion, saveMe1, saveMe2, saveMe3, saveMe4, saveMe5, questionsStart);
        LinkChain(whatNowQuestion, whatNow1, whatNow2, whatNow3, whatNow4, whatNow5, questionsStart);
        LinkChain(deityQuestion, deity1, deity2, deity3, questionsStart);
        LinkChain(treeQuestion, tree1, tree2);

        questionsStart.AddChildren(endUpHereQuestion, saveMeQuestion, whatNowQuestion, deityQuestion, treeQuestion);
    }

    internal static void LoadSolynQuestDialogue()
    {
        if (!ModLoader.TryGetMod("CalamityMod", out _))
            return;

        LoadSolynQuestDialogue_Stargaze();
        LoadSolynQuestDialogue_Stargaze_Completed();
        LoadSolynQuestDialogue_Stargaze_SawRift();

        LoadSolynDormantKeyDialogue();
        LoadSolynPermafrostKeepDialog();

        LoadSolynFrostblessedSeedlingDialog();

        LoadSolynGenesisRevealDialog();

        LoadSolynCeaselessVoidBeforeBattleDialogue();
        LoadSolynCeaselessVoidAfterBattleDialogue();
        LoadSolynCeaselessVoidBeforeEnteringRiftDialogue();
        LoadSolynCeaselessVoidAfterEnteringRiftDialogue();

        LoadSolynDraedonBeforeCombatSimulationDialogue();

        LoadSolynCompletedGenesisDialogue();
    }

    internal static void LoadSolynQuestDialogue_Stargaze()
    {
        SolynQuest_Stargaze = CreateSolynConversation("StargazeQuest", () =>
        {
            if (SolynQuest_Stargaze.NodeSeen("Solyn3"))
                return "Repeat";

            return "Solyn1";
        }, ConversationPriority.Quest, () => !StargazingQuestSystem.Completed && !StargazingQuestSystem.TelescopeRepaired && !RiftEclipseManagementSystem.RiftEclipseOngoing, () => !StargazingQuestSystem.Completed && !StargazingQuestSystem.TelescopeRepaired);

        var solyn1 = SolynQuest_Stargaze.CreateFromKey("Solyn1", false);
        var player1 = SolynQuest_Stargaze.CreateFromKey("Player1", true);
        var solyn2 = SolynQuest_Stargaze.CreateFromKey("Solyn2", false);
        var player2 = SolynQuest_Stargaze.CreateFromKey("Player2", true);
        var solyn3 = SolynQuest_Stargaze.CreateFromKey("Solyn3", false);
        var repeat = SolynQuest_Stargaze.CreateFromKey("Repeat", false);

        LinkChain(solyn1, player1, solyn2, player2, solyn3);
    }

    internal static void LoadSolynQuestDialogue_Stargaze_Completed()
    {
        SolynQuest_Stargaze_Completed = CreateSolynConversation("StargazeQuestCompletion", () => "Response1", ConversationPriority.Quest,
            () => !StargazingQuestSystem.Completed && StargazingQuestSystem.TelescopeRepaired && !RiftEclipseManagementSystem.RiftEclipseOngoing, () => !StargazingQuestSystem.Completed && StargazingQuestSystem.TelescopeRepaired);

        var response1 = SolynQuest_Stargaze_Completed.CreateFromKey("Response1", false).AddClickAwayAction(() =>
        {
            BlockerSystem.Start(true, false, () => !StargazingScene.IsActive);
            Main.LocalPlayer.SetTalkNPC(-1);

            CutsceneManager.QueueCutscene(ModContent.GetInstance<BecomeDuskScene>());
            BecomeDuskScene.EndAction = () =>
            {
                CutsceneManager.QueueCutscene(ModContent.GetInstance<UseTelescopeScene>());
            };
        });
        LinkChain(response1);
    }

    internal static void LoadSolynQuestDialogue_Stargaze_SawRift()
    {
        SolynQuest_Stargaze_AfterRift = CreateSolynConversation("StargazeQuestSawRift", () =>
        {
            if (SolynQuest_Stargaze_AfterRift.NodeSeen("Talk9"))
                return "Talk10";

            return "Talk1";
        }, ConversationPriority.Quest, () => false);

        var talk1 = SolynQuest_Stargaze_AfterRift.CreateFromKey("Talk1", false);
        var talk2 = SolynQuest_Stargaze_AfterRift.CreateFromKey("Talk2", false);
        var response1 = SolynQuest_Stargaze_AfterRift.CreateFromKey("Response1", true);
        var talk3 = SolynQuest_Stargaze_AfterRift.CreateFromKey("Talk3", false);
        var talk4 = SolynQuest_Stargaze_AfterRift.CreateFromKey("Talk4", false);
        var response2 = SolynQuest_Stargaze_AfterRift.CreateFromKey("Response2", true);
        var talk5 = SolynQuest_Stargaze_AfterRift.CreateFromKey("Talk5", false);
        var talk6 = SolynQuest_Stargaze_AfterRift.CreateFromKey("Talk6", false);
        var talk7 = SolynQuest_Stargaze_AfterRift.CreateFromKey("Talk7", false);
        var talk8 = SolynQuest_Stargaze_AfterRift.CreateFromKey("Talk8", false).AddClickAwayAction(() =>
        {
            if (Main.LocalPlayer.CountItem(ModContent.ItemType<StarCharm>()) <= 0)
                Main.LocalPlayer.QuickSpawnItem(new EntitySource_WorldEvent(), ModContent.ItemType<StarCharm>());
        });
        var talk9 = SolynQuest_Stargaze_AfterRift.CreateFromKey("Talk9", false);
        SolynQuest_Stargaze_AfterRift.CreateFromKey("Talk10", false);
        LinkChain(talk1, talk2, response1, talk3, talk4, response2, talk5, talk6, talk7, talk8, talk9);
    }

    internal static void LoadSolynDormantKeyDialogue()
    {
        SolynQuest_DormantKey = CreateSolynConversation("DormantKeyDiscussion", () =>
        {
            if (SolynQuest_DormantKey.NodeSeen("Conversation8"))
                return "Conversation8";

            return "Start";
        }, ConversationPriority.Quest, () =>
        {
            bool progressCondition = StargazingQuestSystem.Completed || RiftEclipseManagementSystem.RiftEclipseOngoing;
            return Main.LocalPlayer.GetValueRef<bool>(PermafrostKeepWorldGen.PlayerWasGivenKeyVariableName) && PermafrostKeepWorldGen.KeepArea != Rectangle.Empty && progressCondition;
        }, () => !PermafrostKeepQuestSystem.Completed);

        var start = SolynQuest_DormantKey.CreateFromKey("Start", false);
        var startingQuestion = SolynQuest_DormantKey.CreateFromKey("StartingQuestion", true);
        var conversation1 = SolynQuest_DormantKey.CreateFromKey("Conversation1", false);
        var conversation2 = SolynQuest_DormantKey.CreateFromKey("Conversation2", false);
        var conversation3 = SolynQuest_DormantKey.CreateFromKey("Conversation3", false);
        var conversation4 = SolynQuest_DormantKey.CreateFromKey("Conversation4", true);
        var conversation5 = SolynQuest_DormantKey.CreateFromKey("Conversation5", false);
        var conversation6 = SolynQuest_DormantKey.CreateFromKey("Conversation6", false);
        var conversation7 = SolynQuest_DormantKey.CreateFromKey("Conversation7", false).AddEndAction(() =>
        {
            PermafrostKeepQuestSystem.KeepVisibleOnMap = true;
        });
        var conversation8 = SolynQuest_DormantKey.CreateFromKey("Conversation8", false).AddClickAwayAction(() =>
        {
            PermafrostKeepQuestSystem.KeepVisibleOnMap = true;
            PermafrostKeepQuestSystem.Ongoing = true;

            int solynID = ModContent.NPCType<Solyn>();
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (n.type != solynID || !n.active)
                    continue;

                n.As<Solyn>().CurrentConversation = SolynQuest_DormantKey_AtDoor;
                n.As<Solyn>().StateMachine.StateStack.Clear();
                n.As<Solyn>().StateMachine.StateStack.Push(n.As<Solyn>().StateMachine.StateRegistry[Solyn.SolynAIType.WaitAtPermafrostKeep]);
                n.As<Solyn>().AITimer = 0;
                n.velocity.X = 0f;
                n.netUpdate = true;

                if (Main.LocalPlayer.talkNPC == i)
                    Main.LocalPlayer.SetTalkNPC(-1);
            }
        });
        LinkChain(start, startingQuestion, conversation1, conversation2, conversation3, conversation4, conversation5, conversation6, conversation7, conversation8);
    }

    internal static void LoadSolynPermafrostKeepDialog()
    {
        SolynQuest_DormantKey_AtDoor = CreateSolynConversation("PermafrostKeepDiscussion_BeforeOpening", () =>
        {
            return "Start";
        }, ConversationPriority.Quest, () => false, () => !PermafrostKeepWorldGen.DoorHasBeenUnlocked);
        SolynQuest_DormantKey_AtDoor.CreateFromKey("Start", false);

        SolynQuest_DormantKey_AtSeed = CreateSolynConversation("PermafrostKeepDiscussion_AtSeed", () =>
        {
            return "Start";
        }, ConversationPriority.Quest, () => false);

        var start = SolynQuest_DormantKey_AtSeed.CreateFromKey("Start", false);
        var conversation1 = SolynQuest_DormantKey_AtSeed.CreateFromKey("Conversation1", false);
        var conversation2 = SolynQuest_DormantKey_AtSeed.CreateFromKey("Conversation2", false);
        var conversation3 = SolynQuest_DormantKey_AtSeed.CreateFromKey("Conversation3", false);
        var conversation4 = SolynQuest_DormantKey_AtSeed.CreateFromKey("Conversation4", false);
        var response1 = SolynQuest_DormantKey_AtSeed.CreateFromKey("Response1", true);
        var conversation5 = SolynQuest_DormantKey_AtSeed.CreateFromKey("Conversation5", false);
        var conversation6 = SolynQuest_DormantKey_AtSeed.CreateFromKey("Conversation6", false).AddClickAwayAction(() =>
        {
            PermafrostKeepQuestSystem.Ongoing = false;
            PermafrostKeepQuestSystem.Completed = false;

            int solynID = ModContent.NPCType<Solyn>();
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (n.type != solynID || !n.active)
                    continue;

                n.As<Solyn>().CurrentConversation = SolynQuest_DormantKey_AtDoor;
                n.As<Solyn>().StateMachine.StateStack.Clear();
                n.As<Solyn>().StateMachine.StateStack.Push(n.As<Solyn>().StateMachine.StateRegistry[Solyn.SolynAIType.TeleportFromPermafrostKeep]);
                n.As<Solyn>().AITimer = 0;
                n.velocity.X = 0f;
                n.netUpdate = true;

                if (Main.LocalPlayer.talkNPC == i)
                    Main.LocalPlayer.SetTalkNPC(-1);
            }
        });
        LinkChain(start, conversation1, conversation2, conversation3, conversation4, response1, conversation5, conversation6);
    }

    internal static void LoadSolynFrostblessedSeedlingDialog()
    {
        SolynFrostblessedSeedlingDialogue = CreateSolynConversation("FrostblessedSeedlingDiscussion", () =>
        {
            return "Start";
        }, ConversationPriority.ConnectionEvent, () => PermafrostKeepQuestSystem.Completed && NPC.downedPlantBoss);

        var start = SolynFrostblessedSeedlingDialogue.CreateFromKey("Start", false);
        var solyn1 = SolynFrostblessedSeedlingDialogue.CreateFromKey("Solyn1", false);
        var solyn2 = SolynFrostblessedSeedlingDialogue.CreateFromKey("Solyn2", false);
        var solyn3 = SolynFrostblessedSeedlingDialogue.CreateFromKey("Solyn3", false);
        var solyn4 = SolynFrostblessedSeedlingDialogue.CreateFromKey("Solyn4", false);

        LinkChain(start, solyn1, solyn2, solyn3, solyn4);
    }

    internal static void LoadSolynGenesisRevealDialog()
    {
        SolynQuest_GenesisReveal = CreateSolynConversation("GenesisRevealDiscussion", () =>
        {
            return "Start";
        }, ConversationPriority.Quest + 1, () => PostMLRiftAppearanceSystem.AvatarHasCoveredMoon && PermafrostKeepQuestSystem.Completed);

        var start = SolynQuest_GenesisReveal.CreateFromKey("Start", false);
        var solyn1 = SolynQuest_GenesisReveal.CreateFromKey("Solyn1", false);
        var solyn2 = SolynQuest_GenesisReveal.CreateFromKey("Solyn2", false);
        var solyn3 = SolynQuest_GenesisReveal.CreateFromKey("Solyn3", false);
        var player1 = SolynQuest_GenesisReveal.CreateFromKey("Player1", true);
        var solyn4 = SolynQuest_GenesisReveal.CreateFromKey("Solyn4", false);
        var player2 = SolynQuest_GenesisReveal.CreateFromKey("Player2", true);
        var solyn5 = SolynQuest_GenesisReveal.CreateFromKey("Solyn5", false);
        var solyn6 = SolynQuest_GenesisReveal.CreateFromKey("Solyn6", false);
        var solyn7 = SolynQuest_GenesisReveal.CreateFromKey("Solyn7", false);
        var solyn8 = SolynQuest_GenesisReveal.CreateFromKey("Solyn8", false);
        var solyn9 = SolynQuest_GenesisReveal.CreateFromKey("Solyn9", false);

        LinkChain(start, solyn1, solyn2, solyn3, player1, solyn4, player2, solyn5, solyn6, solyn7, solyn8, solyn9);
    }

    internal static void LoadSolynCeaselessVoidBeforeBattleDialogue()
    {
        SolynQuest_CeaselessVoidBeforeBattle = CreateSolynConversation("CeaselessVoidDiscussionBeforeBattle", () =>
        {
            if (SolynQuest_CeaselessVoidBeforeBattle.NodeSeen("Conversation9"))
                return "Conversation10";
            if (SolynQuest_CeaselessVoidBeforeBattle.NodeSeen("Conversation5"))
                return "Conversation6";

            return "Start";
        }, ConversationPriority.Quest + 1, () =>
        {
            if (CeaselessVoidQuestSystem.Ongoing && CommonCalamityVariables.CeaselessVoidDefeated)
                return false;

            return CommonCalamityVariables.ProvidenceDefeated && PermafrostKeepQuestSystem.Completed && !CeaselessVoidQuestSystem.Completed;
        }, () => !CeaselessVoidQuestSystem.Completed);

        SolynQuest_CeaselessVoidBeforeBattle.WithRerollCondition(() => CommonCalamityVariables.CeaselessVoidDefeated);

        var start = SolynQuest_CeaselessVoidBeforeBattle.CreateFromKey("Start", false);
        var startingQuestion = SolynQuest_CeaselessVoidBeforeBattle.CreateFromKey("StartingQuestion", true);
        var conversation1 = SolynQuest_CeaselessVoidBeforeBattle.CreateFromKey("Conversation1", false);
        var conversation2 = SolynQuest_CeaselessVoidBeforeBattle.CreateFromKey("Conversation2", false);
        var conversation3 = SolynQuest_CeaselessVoidBeforeBattle.CreateFromKey("Conversation3", false);
        var question1 = SolynQuest_CeaselessVoidBeforeBattle.CreateFromKey("ConversationQuestion1", true);
        var conversation4 = SolynQuest_CeaselessVoidBeforeBattle.CreateFromKey("Conversation4", false);
        var conversation5 = SolynQuest_CeaselessVoidBeforeBattle.CreateFromKey("Conversation5", false);
        var conversation6 = SolynQuest_CeaselessVoidBeforeBattle.CreateFromKey("Conversation6", false);

        var conversation6ResponseYes = SolynQuest_CeaselessVoidBeforeBattle.CreateFromKey("Conversation6ResponseYes", true);
        var conversation6ResponseNo = SolynQuest_CeaselessVoidBeforeBattle.CreateFromKey("Conversation6ResponseNo", true);

        var conversationRejection = SolynQuest_CeaselessVoidBeforeBattle.CreateFromKey("ConversationRejection", false);

        var conversation7 = SolynQuest_CeaselessVoidBeforeBattle.CreateFromKey("Conversation7", false).AddEndAction(() =>
        {
            CeaselessVoidQuestSystem.Ongoing = true;
        }).AddClickAwayAction(() =>
        {
            CeaselessVoidQuestSystem.Ongoing = true;
        });
        var conversation8 = SolynQuest_CeaselessVoidBeforeBattle.CreateFromKey("Conversation8", false);
        var conversation9 = SolynQuest_CeaselessVoidBeforeBattle.CreateFromKey("Conversation9", false);
        var conversation10 = SolynQuest_CeaselessVoidBeforeBattle.CreateFromKey("Conversation10", false).AddClickAwayAction(() =>
        {
            CeaselessVoidQuestSystem.Ongoing = true;
        }).AddEndAction(() =>
        {
            CeaselessVoidQuestSystem.Ongoing = true;
        });

        LinkChain(start, startingQuestion, conversation1, conversation2, conversation3, conversation4, conversation5, conversation6);
        conversation6.AddChildren(conversation6ResponseYes, conversation6ResponseNo);

        LinkChain(conversation6ResponseNo, conversationRejection);
        LinkChain(conversation6ResponseYes, conversation7, conversation8, conversation9, conversation10);
    }

    internal static void LoadSolynCeaselessVoidAfterBattleDialogue()
    {
        SolynQuest_CeaselessVoidAfterBattle = CreateSolynConversation("CeaselessVoidDiscussionAfterBattle", () =>
        {
            if (SolynQuest_CeaselessVoidAfterBattle.NodeSeen("Conversation7"))
                return "Repeat";

            return "Start";
        }, ConversationPriority.Quest + 2, () => CommonCalamityVariables.CeaselessVoidDefeated && PermafrostKeepQuestSystem.Completed && CeaselessVoidQuestSystem.Ongoing && !CeaselessVoidQuestSystem.Completed && CeaselessVoidQuestSystem.RiftDefeatPosition != Vector2.Zero);

        var start = SolynQuest_CeaselessVoidAfterBattle.CreateFromKey("Start", false);
        var conversation1 = SolynQuest_CeaselessVoidAfterBattle.CreateFromKey("Conversation1", true);
        var conversation2 = SolynQuest_CeaselessVoidAfterBattle.CreateFromKey("Conversation2", false);
        var conversation3 = SolynQuest_CeaselessVoidAfterBattle.CreateFromKey("Conversation3", false);
        var conversationResponse1 = SolynQuest_CeaselessVoidAfterBattle.CreateFromKey("ConversationResponse1", true);
        var conversation4 = SolynQuest_CeaselessVoidAfterBattle.CreateFromKey("Conversation4", false);
        var conversation5 = SolynQuest_CeaselessVoidAfterBattle.CreateFromKey("Conversation5", false);
        var conversationResponse2 = SolynQuest_CeaselessVoidAfterBattle.CreateFromKey("ConversationResponse2", true);
        var conversation6 = SolynQuest_CeaselessVoidAfterBattle.CreateFromKey("Conversation6", false);
        var conversation7 = SolynQuest_CeaselessVoidAfterBattle.CreateFromKey("Conversation7", false);
        var conversation8 = SolynQuest_CeaselessVoidAfterBattle.CreateFromKey("Conversation8", false).AddClickAwayAction(() =>
        {
            int solynID = ModContent.NPCType<Solyn>();
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (n.type != solynID || !n.active)
                    continue;

                n.As<Solyn>().StateMachine.StateStack.Clear();
                n.As<Solyn>().StateMachine.StateStack.Push(n.As<Solyn>().StateMachine.StateRegistry[Solyn.SolynAIType.IncospicuouslyFlyAwayToDungeon]);
                n.As<Solyn>().AITimer = 0;
                n.netUpdate = true;

                if (Main.LocalPlayer.talkNPC == i)
                    Main.LocalPlayer.SetTalkNPC(-1);
            }
        });
        SolynQuest_CeaselessVoidAfterBattle.CreateFromKey("Repeat", false);

        LinkChain(start, conversation1, conversation2, conversation3, conversationResponse1, conversation4, conversation5, conversationResponse2, conversation6, conversation7, conversation8);
    }

    internal static void LoadSolynCeaselessVoidBeforeEnteringRiftDialogue()
    {
        SolynQuest_CeaselessVoidBeforeEnteringRift = CreateSolynConversation("CeaselessVoidDiscussionBeforeEnteringRift", () =>
        {
            return "Start";
        }, ConversationPriority.Quest + 1, () => false); // Activated manually by Solyn's AI.

        var start = SolynQuest_CeaselessVoidBeforeEnteringRift.CreateFromKey("Start", false).AddEndAction(() =>
        {
            BlockerSystem.Start(true, false, () => !CeaselessVoidQuestSystem.Completed);
        });
        var conversation1 = SolynQuest_CeaselessVoidBeforeEnteringRift.CreateFromKey("Conversation1", false);
        var confirmation1 = SolynQuest_CeaselessVoidBeforeEnteringRift.CreateFromKey("Confirmation", true);
        var conversation2 = SolynQuest_CeaselessVoidBeforeEnteringRift.CreateFromKey("Conversation2", false);
        var conversation3 = SolynQuest_CeaselessVoidBeforeEnteringRift.CreateFromKey("Conversation3", false).AddClickAwayAction(() =>
        {
            int solynID = ModContent.NPCType<Solyn>();
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (n.type != solynID || !n.active)
                    continue;

                n.As<Solyn>().StateMachine.StateStack.Clear();
                n.As<Solyn>().StateMachine.StateStack.Push(n.As<Solyn>().StateMachine.StateRegistry[Solyn.SolynAIType.FlyIntoRift]);
                n.As<Solyn>().AITimer = 0;
                n.netUpdate = true;

                if (Main.LocalPlayer.talkNPC == i)
                    Main.LocalPlayer.SetTalkNPC(-1);
            }
        });

        LinkChain(start, conversation1, confirmation1, conversation2, conversation3);
    }

    internal static void LoadSolynCeaselessVoidAfterEnteringRiftDialogue()
    {
        SolynQuest_CeaselessVoidAfterEnteringRift = CreateSolynConversation("CeaselessVoidDiscussionAfterEnteringRift", () =>
        {
            return "Start";
        }, ConversationPriority.Quest + 1, () => false); // Activated manually by Solyn's AI.

        var start = SolynQuest_CeaselessVoidAfterEnteringRift.CreateFromKey("Start", false);
        var question1 = SolynQuest_CeaselessVoidAfterEnteringRift.CreateFromKey("Question1", true);
        var conversation1 = SolynQuest_CeaselessVoidAfterEnteringRift.CreateFromKey("Conversation1", false);
        var question2 = SolynQuest_CeaselessVoidAfterEnteringRift.CreateFromKey("Question2", true);
        var conversation2 = SolynQuest_CeaselessVoidAfterEnteringRift.CreateFromKey("Conversation2", false);
        var conversation3 = SolynQuest_CeaselessVoidAfterEnteringRift.CreateFromKey("Conversation3", false);
        var conversation4 = SolynQuest_CeaselessVoidAfterEnteringRift.CreateFromKey("Conversation4", false);
        var conversation5 = SolynQuest_CeaselessVoidAfterEnteringRift.CreateFromKey("Conversation5", false);
        var conversation6 = SolynQuest_CeaselessVoidAfterEnteringRift.CreateFromKey("Conversation6", false).AddClickAwayAction(() =>
        {
            if (Main.LocalPlayer.CountItem(ModContent.ItemType<TheAntiseed>()) <= 0)
                Main.LocalPlayer.QuickSpawnItem(new EntitySource_WorldEvent(), ModContent.ItemType<TheAntiseed>());
        });
        var conversation7 = SolynQuest_CeaselessVoidAfterEnteringRift.CreateFromKey("Conversation7", false).AddClickAwayAction(() =>
        {
            CeaselessVoidQuestSystem.Ongoing = false;
            CeaselessVoidQuestSystem.Completed = true;

            int solynID = ModContent.NPCType<Solyn>();
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (n.type != solynID || !n.active)
                    continue;

                n.As<Solyn>().StateMachine.StateStack.Clear();
                n.As<Solyn>().StateMachine.StateStack.Push(n.As<Solyn>().StateMachine.StateRegistry[Solyn.SolynAIType.TeleportHome]);
                n.As<Solyn>().AITimer = 0;
                n.netUpdate = true;

                if (Main.LocalPlayer.talkNPC == i)
                    Main.LocalPlayer.SetTalkNPC(-1);
            }

            SolynDialogSystem.RerollConversationForSolyn();
        });

        LinkChain(start, question1, conversation1, question2, conversation2, conversation3, conversation4, conversation5, conversation6, conversation7);
    }

    internal static void LoadSolynDraedonBeforeCombatSimulationDialogue()
    {
        SolynQuest_DraedonBeforeCombatSimulation = CreateSolynConversation("DraedonBeforeCombatSimulation", () =>
        {
            return "Start";
        }, ConversationPriority.Quest, () => CommonCalamityVariables.DraedonDefeated && CeaselessVoidQuestSystem.Completed,
            () => !DraedonCombatQuestSystem.Completed);

        var start = SolynQuest_DraedonBeforeCombatSimulation.CreateFromKey("Start", false);
        var question1 = SolynQuest_DraedonBeforeCombatSimulation.CreateFromKey("Question1", true);
        var conversation1 = SolynQuest_DraedonBeforeCombatSimulation.CreateFromKey("Conversation1", false);
        var conversation2 = SolynQuest_DraedonBeforeCombatSimulation.CreateFromKey("Conversation2", false);
        var question2 = SolynQuest_DraedonBeforeCombatSimulation.CreateFromKey("Question2", true);
        var conversation3 = SolynQuest_DraedonBeforeCombatSimulation.CreateFromKey("Conversation3", false);
        var question3 = SolynQuest_DraedonBeforeCombatSimulation.CreateFromKey("Question3", true);
        var conversation4 = SolynQuest_DraedonBeforeCombatSimulation.CreateFromKey("Conversation4", false);
        var conversation5 = SolynQuest_DraedonBeforeCombatSimulation.CreateFromKey("Conversation5", false).AddClickAwayAction(() =>
        {
            DraedonCombatQuestSystem.Ongoing = true;

            int solynID = ModContent.NPCType<Solyn>();
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (n.type != solynID || !n.active)
                    continue;

                n.As<Solyn>().StateMachine.StateStack.Clear();
                n.As<Solyn>().StateMachine.StateStack.Push(n.As<Solyn>().StateMachine.StateRegistry[Solyn.SolynAIType.FollowPlayerToCodebreaker]);
                n.As<Solyn>().AITimer = 0;
                n.netUpdate = true;

                if (Main.LocalPlayer.talkNPC == i)
                    Main.LocalPlayer.SetTalkNPC(-1);
            }
        });

        LinkChain(start, question1, conversation1, conversation2, question2, conversation3, question3, conversation4, conversation5);
    }

    internal static void LoadSolynCompletedGenesisDialogue()
    {
        SolynQuest_GenesisCompletion = CreateSolynConversation("GenesisCompletion", () =>
        {
            return "Start";
        }, ConversationPriority.Urgent, () => DraedonCombatQuestSystem.Completed && WorldSaveSystem.HasCompletedGenesis, () => DraedonCombatQuestSystem.Completed && WorldSaveSystem.HasCompletedGenesis);

        var start = SolynQuest_GenesisCompletion.CreateFromKey("Start", false);
        var player1 = SolynQuest_GenesisCompletion.CreateFromKey("Player1", true);
        var solyn1 = SolynQuest_GenesisCompletion.CreateFromKey("Solyn1", false);
        var solyn2 = SolynQuest_GenesisCompletion.CreateFromKey("Solyn2", false).AddClickAwayAction(() =>
        {
            int solynID = ModContent.NPCType<Solyn>();
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (n.type != solynID || !n.active)
                    continue;

                n.As<Solyn>().StateMachine.StateStack.Clear();
                n.As<Solyn>().StateMachine.StateStack.Push(n.As<Solyn>().StateMachine.StateRegistry[Solyn.SolynAIType.FollowPlayerToGenesis]);
                n.As<Solyn>().AITimer = 0;
                n.netUpdate = true;

                if (Main.LocalPlayer.talkNPC == i)
                    Main.LocalPlayer.SetTalkNPC(-1);
            }
        });
        LinkChain(start, player1, solyn1, solyn2);
    }

    internal static void LoadSolynErrorDialogue()
    {
        // This dialogue uses a return false lambda to ensure that it doesn't trigger as a result of the natural dialogue roster.
        // It should only trigger as an explicit fallback case in the SolynDialogSystem.ChooseSolynConversation method.
        // It would be possible to just have it return true with the minimum priority, but I feel that making it an explicit exception ensures that this is easier to identify in the event of an error.
        SolynErrorDialogue = CreateSolynConversation("SolynErrorFallback", () => "ErrorMessage1", ConversationPriority.Error, () => false);
        SolynErrorDialogue.CreateFromKey("ErrorMessage1", false);
    }

    internal static void LoadSolynRandomDialogue_Dawn()
    {
        SolynDawnDialogue = CreateSolynConversation("SolynDawnDialogue", () =>
        {
            return "Start";
        }, ConversationPriority.RandomDiscussion, () => Main.dayTime && Main.time <= 10800, () => true);
        SolynDawnDialogue.WithRerollCondition(() => !SolynDawnDialogue.AppearanceCondition());

        var start = SolynDawnDialogue.CreateFromKey("Start", false);
        var player1 = SolynDawnDialogue.CreateFromKey("Player1", true);
        var solyn1 = SolynDawnDialogue.CreateFromKey("Solyn1", false);
        var player2 = SolynDawnDialogue.CreateFromKey("Player2", true);
        var solyn2 = SolynDawnDialogue.CreateFromKey("Solyn2", false);
        var player3 = SolynDawnDialogue.CreateFromKey("Player3", true);
        LinkChain(start, player1, solyn1, player2, solyn2, player3);

        var player4 = SolynDawnDialogue.CreateFromKey("Player4", true);
        var solyn3 = SolynDawnDialogue.CreateFromKey("Solyn3", false);

        int totalDreams = 5;

        // Dream 1.
        var dream1_1 = SolynDawnDialogue.CreateFromKey("Dream1_Response1", false, () => DaysCounterSystem.DayCounter % totalDreams == 0);
        var dream1_2 = SolynDawnDialogue.CreateFromKey("Dream1_Response2", false);
        var dream1_3 = SolynDawnDialogue.CreateFromKey("Dream1_Response3", false);
        var dream1_4 = SolynDawnDialogue.CreateFromKey("Dream1_Response4", false);
        var dream1_5 = SolynDawnDialogue.CreateFromKey("Dream1_Response5", false);
        var dream1_6 = SolynDawnDialogue.CreateFromKey("Dream1_Response6", false);
        LinkChain(player3, dream1_1);
        LinkChain(dream1_1, dream1_2, dream1_3, dream1_4, dream1_5, dream1_6, player4);

        // Dream 2.
        var dream2_1 = SolynDawnDialogue.CreateFromKey("Dream2_Response1", false, () => DaysCounterSystem.DayCounter % totalDreams == 1);
        var dream2_2 = SolynDawnDialogue.CreateFromKey("Dream2_Response2", false);
        var dream2_3 = SolynDawnDialogue.CreateFromKey("Dream2_Response3", false);
        var dream2_4 = SolynDawnDialogue.CreateFromKey("Dream2_Response4", false);
        var dream2_5 = SolynDawnDialogue.CreateFromKey("Dream2_Response5", false);
        var dream2_6 = SolynDawnDialogue.CreateFromKey("Dream2_Response6", false);
        LinkChain(player3, dream2_1);
        LinkChain(dream2_1, dream2_2, dream2_3, dream2_4, dream2_5, dream2_6, player4);

        // Dream 3.
        var dream3_1 = SolynDawnDialogue.CreateFromKey("Dream3_Response1", false, () => DaysCounterSystem.DayCounter % totalDreams == 2);
        var dream3_2 = SolynDawnDialogue.CreateFromKey("Dream3_Response2", false);
        var dream3_3 = SolynDawnDialogue.CreateFromKey("Dream3_Response3", false);
        var dream3_4 = SolynDawnDialogue.CreateFromKey("Dream3_Response4", false);
        var dream3_5 = SolynDawnDialogue.CreateFromKey("Dream3_Response5", false);
        var dream3_6 = SolynDawnDialogue.CreateFromKey("Dream3_Response6", false);
        LinkChain(player3, dream3_1);
        LinkChain(dream3_1, dream3_2, dream3_3, dream3_4, dream3_5, dream3_6, player4);

        // Dream 4.
        var dream4_1 = SolynDawnDialogue.CreateFromKey("Dream4_Response1", false, () => DaysCounterSystem.DayCounter % totalDreams == 3);
        var dream4_2 = SolynDawnDialogue.CreateFromKey("Dream4_Response2", false);
        var dream4_3 = SolynDawnDialogue.CreateFromKey("Dream4_Response3", false);
        var dream4_4 = SolynDawnDialogue.CreateFromKey("Dream4_Response4", false);
        var dream4_5 = SolynDawnDialogue.CreateFromKey("Dream4_Response5", false);
        var dream4_6 = SolynDawnDialogue.CreateFromKey("Dream4_Response6", false);
        LinkChain(player3, dream4_1);
        LinkChain(dream4_1, dream4_2, dream4_3, dream4_4, dream4_5, dream4_6, player4);

        // Dream 5.
        var dream5_1 = SolynDawnDialogue.CreateFromKey("Dream5_Response1", false, () => DaysCounterSystem.DayCounter % totalDreams == 4);
        var dream5_2 = SolynDawnDialogue.CreateFromKey("Dream5_Response2", false);
        var dream5_3 = SolynDawnDialogue.CreateFromKey("Dream5_Response3", false);
        var dream5_4 = SolynDawnDialogue.CreateFromKey("Dream5_Response4", false);
        var dream5_5 = SolynDawnDialogue.CreateFromKey("Dream5_Response5", false);
        var dream5_6 = SolynDawnDialogue.CreateFromKey("Dream5_Response6", false);
        LinkChain(player3, dream5_1);
        LinkChain(dream5_1, dream5_2, dream5_3, dream5_4, dream5_5, dream5_6, player4);

        player4.AddChildren(solyn3);
    }

    internal static void LoadSolynRandomDialogue_SunnyDay()
    {
        SolynSunnyDayDialogue = CreateSolynConversation("SolynSunnyDayDialogue", () =>
        {
            return "Start";
        }, ConversationPriority.RandomDiscussion, () => Main.dayTime && Main.time > 10800 && Main.time <= 47800 && !Main.raining, () => true);
        SolynSunnyDayDialogue.WithRerollCondition(() => !SolynSunnyDayDialogue.AppearanceCondition());

        var start = SolynSunnyDayDialogue.CreateFromKey("Start", false);
        var player1 = SolynSunnyDayDialogue.CreateFromKey("Player1", true);
        var solyn1 = SolynSunnyDayDialogue.CreateFromKey("Solyn1", false);
        var player2 = SolynSunnyDayDialogue.CreateFromKey("Player2", true);
        var solyn2 = SolynSunnyDayDialogue.CreateFromKey("Solyn2", false);
        var solyn3 = SolynSunnyDayDialogue.CreateFromKey("Solyn3", false);

        LinkChain(start, player1, solyn1, player2, solyn2, solyn3);
    }

    internal static void LoadSolynRandomDialogue_RainyDay()
    {
        SolynRainyDayDialogue = CreateSolynConversation("SolynRainyDayDialogue", () =>
        {
            return "Start";
        }, ConversationPriority.RandomDiscussion, () => Main.dayTime && Main.time > 10800 && Main.time <= 47800 && Main.raining, () => true);
        SolynRainyDayDialogue.WithRerollCondition(() => !SolynRainyDayDialogue.AppearanceCondition());

        var start = SolynRainyDayDialogue.CreateFromKey("Start", false);
        var player1 = SolynRainyDayDialogue.CreateFromKey("Player1", true);
        var solyn1 = SolynRainyDayDialogue.CreateFromKey("Solyn1", false);
        var player2 = SolynRainyDayDialogue.CreateFromKey("Player2", true);
        var solyn2 = SolynRainyDayDialogue.CreateFromKey("Solyn2", false);
        var solyn3 = SolynRainyDayDialogue.CreateFromKey("Solyn3", false);

        LinkChain(start, player1, solyn1, player2, solyn2, solyn3);
    }

    internal static void LoadSolynRandomDialogue_Dusk()
    {
        SolynRainyDuskDialogue = CreateSolynConversation("SolynDuskDialogue", () =>
        {
            return "Start";
        }, ConversationPriority.RandomDiscussion, () => Main.dayTime && Main.time > 47800, () => true);
        SolynRainyDuskDialogue.WithRerollCondition(() => !SolynRainyDuskDialogue.AppearanceCondition());

        var start = SolynRainyDuskDialogue.CreateFromKey("Start", false);
        var solyn1 = SolynRainyDuskDialogue.CreateFromKey("Solyn1", false);
        var solyn2 = SolynRainyDuskDialogue.CreateFromKey("Solyn2", false);
        var solyn3 = SolynRainyDuskDialogue.CreateFromKey("Solyn3", false);

        LinkChain(start, solyn1, solyn2, solyn3);
    }

    internal static void LoadSolynRandomDialogue_SlimeRain()
    {
        SolynSlimeRainDialogue = CreateSolynConversation("SolynSlimeRainDialogue", () =>
        {
            return "Start";
        }, ConversationPriority.RandomDiscussion + 1, () => Main.slimeRain && !RiftEclipseManagementSystem.RiftEclipseOngoing, () => true);
        SolynSlimeRainDialogue.WithRerollCondition(() => !SolynSlimeRainDialogue.AppearanceCondition());

        var start = SolynSlimeRainDialogue.CreateFromKey("Start", false);
        var solyn1 = SolynSlimeRainDialogue.CreateFromKey("Solyn1", false);
        var player1 = SolynSlimeRainDialogue.CreateFromKey("Player1", true);
        var solyn2 = SolynSlimeRainDialogue.CreateFromKey("Solyn2", false);
        var solyn3 = SolynSlimeRainDialogue.CreateFromKey("Solyn3", false);
        var solyn4 = SolynSlimeRainDialogue.CreateFromKey("Solyn4", false);

        LinkChain(start, solyn1, player1, solyn2, solyn3, solyn4);
    }

    internal static void LoadSolynRandomDialogue_StorageSails()
    {
        SolynStorageSailsDialogue = CreateSolynConversation("SolynStorageSailsDialogue", () =>
        {
            return "Start";
        }, ConversationPriority.RandomDiscussion + 1, () => Main.rand.NextBool(3) && !RiftEclipseManagementSystem.RiftEclipseOngoing, () => true);
        SolynStorageSailsDialogue.WithRerollCondition(() => !SolynStorageSailsDialogue.AppearanceCondition());

        var start = SolynStorageSailsDialogue.CreateFromKey("Start", false);
        var player1 = SolynStorageSailsDialogue.CreateFromKey("Player1", true);
        var solyn1 = SolynStorageSailsDialogue.CreateFromKey("Solyn1", false);
        var solyn2 = SolynStorageSailsDialogue.CreateFromKey("Solyn2", false);
        var solyn3 = SolynStorageSailsDialogue.CreateFromKey("Solyn3", false);
        var player2 = SolynStorageSailsDialogue.CreateFromKey("Player2", true);
        var solyn4 = SolynStorageSailsDialogue.CreateFromKey("Solyn4", false);

        LinkChain(start, player1, solyn1, solyn2, solyn3, player2, solyn4);
    }

    internal static void LoadSolynRandomDialogue_Books()
    {
        SolynBooksDialogue = CreateSolynConversation("SolynBooksDialogue", () =>
        {
            return "Start";
        }, ConversationPriority.RandomDiscussion + 1, () => Main.rand.NextBool(3) && !RiftEclipseManagementSystem.RiftEclipseOngoing, () => true);
        SolynBooksDialogue.WithRerollCondition(() => !SolynBooksDialogue.AppearanceCondition());

        static bool PlayerIsProbablyMage()
        {
            Item[] weapons = new Span<Item>(Main.LocalPlayer.inventory, 0, 39).ToArray().Where(i => i.axe <= 0 && i.pick <= 0 && i.hammer <= 0 && i.damage >= 1).OrderByDescending(i => i.damage).ToArray();
            bool playerIsProbablyMage = false;
            for (int i = 0; i < Math.Min(3, weapons.Length); i++)
            {
                if (weapons[i].DamageType == DamageClass.Magic)
                {
                    playerIsProbablyMage = true;
                    break;
                }
            }

            return playerIsProbablyMage;
        }

        var start = SolynBooksDialogue.CreateFromKey("Start", false);
        var solyn1 = SolynBooksDialogue.CreateFromKey("Solyn1", false);
        var solyn2 = SolynBooksDialogue.CreateFromKey("Solyn2", false);
        var solyn3 = SolynBooksDialogue.CreateFromKey("Solyn3", false);

        var solyn4Mage = SolynBooksDialogue.CreateFromKey("Solyn4_Mage", false, PlayerIsProbablyMage);
        var player1Mage = SolynBooksDialogue.CreateFromKey("Player1_Mage", true);

        var solyn4OtherClass = SolynBooksDialogue.CreateFromKey("Solyn4_OtherClass", false, () => !PlayerIsProbablyMage());
        var solyn5 = SolynBooksDialogue.CreateFromKey("Solyn5", false);

        LinkChain(start, solyn1, solyn2, solyn3);
        solyn3.AddChildren(solyn4Mage, solyn4OtherClass);

        LinkChain(solyn4Mage, player1Mage, solyn5);
        LinkChain(solyn4OtherClass, solyn5);
    }

    internal static void LoadSolynRandomDialogue_OneRedeemedBook()
    {
        SolynOneRedeemedBookDialogue = CreateSolynConversation("SolynOneRedeemedBookDialogue", () =>
        {
            return "Start";
        }, ConversationPriority.ConnectionEvent, () => SolynBookExchangeRegistry.TotalRedeemedBooks >= 1);

        var start = SolynOneRedeemedBookDialogue.CreateFromKey("Start", false);
        var solyn1 = SolynOneRedeemedBookDialogue.CreateFromKey("Solyn1", false);

        LinkChain(start, solyn1);
    }

    internal static void LoadSolynRandomDialogue_UnreadableBook()
    {
        SolynUnreadableBookDialogue = CreateSolynConversation("SolynUnreadableBookDialogue", () =>
        {
            return "Start";
        }, ConversationPriority.ConnectionEvent, () => SolynBookExchangeRegistry.RedeemedBooks.Contains("BookOfMiracles"));

        var start = SolynUnreadableBookDialogue.CreateFromKey("Start", false);
        var solyn1 = SolynUnreadableBookDialogue.CreateFromKey("Solyn1", false);

        LinkChain(start, solyn1);
    }

    internal static void LoadSolynRandomDialogue_ManyBooks()
    {
        SolynManyRedeemedBooksDialogue = CreateSolynConversation("SolynManyRedeemedBooksDialogue", () =>
        {
            return "Start";
        }, ConversationPriority.ConnectionEvent, () => SolynBookExchangeRegistry.TotalRedeemedBooks >= SolynBookExchangeRegistry.ObtainableBooks.Count / 2);

        var start = SolynManyRedeemedBooksDialogue.CreateFromKey("Start", false);
        var solyn1 = SolynManyRedeemedBooksDialogue.CreateFromKey("Solyn1", false);

        LinkChain(start, solyn1);
    }

    internal static void LoadSolynRandomDialogue_CompletedBookshelf()
    {
        SolynCompletedBookhselfDialogue = CreateSolynConversation("SolynCompletedBookhselfDialogue", () =>
        {
            return "Start";
        }, ConversationPriority.ConnectionEvent + 1, () => SolynBookExchangeRegistry.RedeemedAllBooks);

        var start = SolynCompletedBookhselfDialogue.CreateFromKey("Start", false);
        var solyn1 = SolynCompletedBookhselfDialogue.CreateFromKey("Solyn1", false);
        var solyn2 = SolynCompletedBookhselfDialogue.CreateFromKey("Solyn2", false);

        LinkChain(start, solyn1, solyn2);
    }

    internal static void LoadSolynRandomDialogue_Lategame()
    {
        SolynLategameDialogue = CreateSolynConversation("SolynLategameDialogue", () =>
        {
            return "Start";
        }, ConversationPriority.RandomDiscussion + 1, () => CeaselessVoidQuestSystem.Completed, () => true);
    }

    public static void RegisterDialogueAsSeen(DialogueNode dialogue)
    {
        if (!ConversationDataSaveSystem.seenDialogue.Contains(dialogue.IdentifierKey))
            ConversationDataSaveSystem.seenDialogue.Add(dialogue.IdentifierKey);
        if (Main.netMode == NetmodeID.MultiplayerClient)
            PacketManager.SendPacket<SeenDialoguePacket>(dialogue.IdentifierKey);
    }

    /// <summary>
    /// Generates a conversation, storing it in the central registry.
    /// </summary>
    public static Conversation CreateSolynConversation(string identifierKey, Func<string> dialogueStartSelector, ConversationPriority priority, Func<bool>? appearanceCondition = null, Func<bool>? repeatCondition = null)
    {
        Conversation conversation = new Conversation(identifierKey, dialogueStartSelector, priority, appearanceCondition, repeatCondition);
        SolynConversations.Add(conversation);
        return conversation;
    }

    /// <summary>
    /// Links dialogue nodes together, effectively resulting in a short collection of dialogue text units that are strung together without multiple options.
    /// </summary>
    /// <param name="dialogue">The set of dialogue to link together.</param>
    public static void LinkChain(params DialogueNode[] dialogue)
    {
        for (int i = 0; i < dialogue.Length - 1; i++)
            dialogue[i].AddChildren(dialogue[i + 1]);
    }
}
