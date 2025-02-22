using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Core.CrossCompatibility.Inbound.BaseCalamity;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace NoxusBoss.Content.NPCs.Bosses.Draedon.DraedonDialogue;

[JITWhenModsEnabled(CalamityCompatibility.ModName)]
[ExtendsFromMod(CalamityCompatibility.ModName)]
public sealed class DraedonWorldDialogueManager : ModSystem
{
    /// <summary>
    /// The set of active dialogue instances in the world.
    /// </summary>
    private static readonly List<DraedonWorldDialogue> DialogueInstances = new List<DraedonWorldDialogue>(8);

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        layers.Add(new LegacyGameInterfaceLayer("Wrath of the Gods: Draedon World Dialogue", () =>
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            foreach (DraedonWorldDialogue instance in DialogueInstances)
                instance.Render();

            Main.spriteBatch.ResetToDefault();
            return true;
        }));
    }

    public override void PreUpdateEntities()
    {
        foreach (DraedonWorldDialogue instance in DialogueInstances)
            instance.Update();

        DialogueInstances.RemoveAll(d => d.Time >= d.Lifetime);
    }

    /// <summary>
    /// Creates a new Draedon dialogue instance in the world.
    /// </summary>
    /// <param name="textLocalizationKey">The localization key for the text that should be displayed.</param>
    /// <param name="direction">The direction of the text in the world.</param>
    /// <param name="position">The position of the text in the world.</param>
    /// <param name="lifetime">How long the text should exist for.</param>
    public static void CreateNew(string textLocalizationKey, int direction, Vector2 position, int lifetime)
    {
        DraedonWorldDialogue instance = new DraedonWorldDialogue(textLocalizationKey, direction, position, lifetime);
        DialogueInstances.Add(instance);
    }
}
