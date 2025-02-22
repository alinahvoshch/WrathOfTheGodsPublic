using System.Reflection;
using System.Text;
using Luminance.Core.Hooking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets.Fonts;
using NoxusBoss.Core.CrossCompatibility.Inbound.BaseCalamity;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using static CalamityMod.UI.BossHealthBarManager;
using static CalamityMod.UI.BossHealthBarManager.BossHPUI;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers;

[JITWhenModsEnabled(CalamityCompatibility.ModName)]
[ExtendsFromMod(CalamityCompatibility.ModName)]
public class TestOfResolveCalBarReplacementSystem : ModSystem
{
    public delegate void BossUIBarDrawDelegate(BossHPUI instance, SpriteBatch sb, int x, int y);

    public delegate void BossUIBarDrawHook(BossUIBarDrawDelegate orig, BossHPUI instance, SpriteBatch sb, int x, int y);

    public override void OnModLoad()
    {
        MethodInfo? bossUIDrawMethod = typeof(BossHPUI).GetMethod("Draw");
        if (bossUIDrawMethod is null)
            return;

        HookHelper.ModifyMethodWithDetour(bossUIDrawMethod, RenderEternalResolveBarWrapper);
    }

    private static void RenderEternalResolveBarWrapper(BossUIBarDrawDelegate orig, BossHPUI instance, SpriteBatch sb, int x, int y)
    {
        if (instance.NPCType == ModContent.NPCType<NamelessDeityBoss>())
        {
            if (!Main.npc[instance.NPCIndex].active)
                return;

            if (TestOfResolveSystem.IsActive)
            {
                RenderEternalResolveBar(instance, sb, x, y);
                return;
            }
        }

        orig(instance, sb, x, y);
    }

    private static void DrawBorderStringEightWay(SpriteBatch sb, DynamicSpriteFont font, string text, Vector2 baseDrawPosition, Color main, Color border, float scale = 1f)
    {
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                Vector2 position = baseDrawPosition + new Vector2(i, j);
                if (i != 0 || j != 0)
                {
                    sb.DrawString(font, text, position, border, 0f, default(Vector2), scale, SpriteEffects.None, 0f);
                }
            }
        }

        sb.DrawString(font, text, baseDrawPosition, main, 0f, default(Vector2), scale, SpriteEffects.None, 0f);
    }

    private static void RenderEternalResolveBar(BossHPUI instance, SpriteBatch sb, int x, int y)
    {
        // Draw a white separator bar.
        // Enrage bar color takes priority over defense or DR increase bar color, because it's more important to display the enrage.
        Color separatorColor = Color.Black;

        // Draw the bar.
        sb.Draw(BossSeperatorBar, new Rectangle(x, y + SeparatorBarYOffset, BarMaxWidth + 5000, 6), separatorColor);

        // Draw the text.
        DynamicSpriteFont hpFont = FontAssets.DeathText.Value;
        string percentHealthText = "∞";
        Vector2 textSize = hpFont.MeasureString(percentHealthText);
        DrawBorderStringEightWay(sb, hpFont, percentHealthText, new Vector2(x, y + 52 - textSize.Y), Color.White, MainBorderColour * 0.25f);

        string baseName = instance.OverridingName ?? instance.AssociatedNPC.FullName;
        List<int> replacementIndices = [];
        for (int i = 0; i < 1; i++)
            replacementIndices.Add(Main.rand.Next(baseName.Length));

        StringBuilder nameBuilder = new StringBuilder();
        for (int i = 0; i < baseName.Length; i++)
            nameBuilder.Append(replacementIndices.Contains(i) ? (char)Main.rand.Next(300) : baseName[i]);

        string name = nameBuilder.ToString();
        float nameScale = 0.67f;
        DynamicSpriteFont nameFont = FontRegistry.Instance.NamelessDeityText;
        Vector2 nameSize = nameFont.MeasureString(name) * nameScale;

        // And draw the text to indicate the name of the boss.
        DrawBorderStringEightWay(sb, nameFont, name, new Vector2(x + BarMaxWidth - nameSize.X, y + 23 - nameSize.Y), Color.White, Color.Black * 0.2f, nameScale);

        NamelessBossBar.RenderTimeElapsedText(new Vector2(x + BarMaxWidth - 70f, y + 70f));
    }
}
