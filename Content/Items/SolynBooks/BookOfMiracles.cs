using System.Reflection;
using CalamityMod;
using CalamityMod.NPCs.ExoMechs.Apollo;
using CalamityMod.NPCs.ExoMechs.Ares;
using CalamityMod.NPCs.ExoMechs.Thanatos;
using Luminance.Core.Graphics;
using Luminance.Core.Hooking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using NoxusBoss.Core.CrossCompatibility.Inbound.BaseCalamity;
using NoxusBoss.Core.GlobalInstances;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader;
using static NoxusBoss.Core.Autoloaders.SolynBooks.SolynBookAutoloader;

namespace NoxusBoss.Content.Items;

public partial class SolynBooksSystem : ModSystem
{
    private static void LoadBookOfMiraclesObtainment_Wrapper()
    {
        new ManagedILEdit("Use shader on Book of Miracles tooltip background", ModContent.GetInstance<NoxusBoss>(), edit =>
        {
            IL_Main.MouseText_DrawItemTooltip += edit.SubscriptionWrapper;
        }, edit =>
        {
            IL_Main.MouseText_DrawItemTooltip -= edit.SubscriptionWrapper;
        }, UseBookOfMiraclesBgShader).Apply();

        if (CalamityCompatibility.Enabled)
            LoadBookOfMiraclesObtainment();
    }

    private static void UseBookOfMiraclesBgShader(ILContext context, ManagedILEdit edit)
    {
        ILCursor cursor = new ILCursor(context);
        MethodInfo drawInventoryBgMethod = typeof(Utils).GetMethod("DrawInvBG", UniversalBindingFlags, new Type[]
        {
            typeof(SpriteBatch),
            typeof(Rectangle),
            typeof(Color)
        })!;
        if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchCallOrCallvirt(drawInventoryBgMethod)))
        {
            edit.LogFailure("Could not find the DrawInvBG call.");
            return;
        }
        cursor.EmitDelegate(() =>
        {
            if (Main.HoverItem.type == Books["BookOfMiracles"].Type && !IsUnobtainedItemInUI(Main.HoverItem))
            {
                Main.spriteBatch.PrepareForShaders(null, true);

                ManagedShader miracleblightShader = ShaderManager.GetShader("NoxusBoss.MiracleblightImitationShader");
                miracleblightShader.TrySetParameter("baseErasureThreshold", 0.61f);
                miracleblightShader.TrySetParameter("zoom", 1f);
                miracleblightShader.SetTexture(PerlinNoise, 1, SamplerState.LinearWrap);
                miracleblightShader.Apply();
            }
        });

        cursor.Goto(0);
        if (!cursor.TryGotoNext(MoveType.After, i => i.MatchCallOrCallvirt(drawInventoryBgMethod)))
        {
            edit.LogFailure("Could not find the DrawInvBG call.");
            return;
        }
        cursor.EmitDelegate(() =>
        {
            if (Main.HoverItem.type == Books["BookOfMiracles"].Type && !IsUnobtainedItemInUI(Main.HoverItem))
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
            }
        });
    }

    [JITWhenModsEnabled(CalamityCompatibility.ModName)]
    private static bool LastExoMechCondition()
    {
        int totalExoMechs = NPC.CountNPCS(ModContent.NPCType<Apollo>()) + NPC.CountNPCS(ModContent.NPCType<AresBody>()) + NPC.CountNPCS(ModContent.NPCType<ThanatosHead>());
        return totalExoMechs <= 1;
    }

    [JITWhenModsEnabled(CalamityCompatibility.ModName)]
    private static void LoadBookOfMiraclesObtainment()
    {
        GlobalNPCEventHandlers.ModifyNPCLootEvent += (npc, loot) =>
        {
            void addWithCondition(Func<bool> condition)
            {
                IItemDropRuleCondition allMechsDefeatedCondition = DropHelper.If(() => condition() && LastExoMechCondition());
                LeadingConditionRule allMechsDefeatedRule = new LeadingConditionRule(allMechsDefeatedCondition);
                {
                    allMechsDefeatedRule.Add(new CommonDrop(Books["BookOfMiracles"].Type, 1));
                }

                loot.Add(allMechsDefeatedRule);
            }

            if (npc.type == ModContent.NPCType<AresBody>())
                addWithCondition(() => DownedBossSystem.downedArtemisAndApollo && DownedBossSystem.downedThanatos);
            if (npc.type == ModContent.NPCType<ThanatosHead>())
                addWithCondition(() => DownedBossSystem.downedArtemisAndApollo && DownedBossSystem.downedAres);
            if (npc.type == ModContent.NPCType<Apollo>())
                addWithCondition(() => DownedBossSystem.downedAres && DownedBossSystem.downedThanatos);
        };
    }
}
