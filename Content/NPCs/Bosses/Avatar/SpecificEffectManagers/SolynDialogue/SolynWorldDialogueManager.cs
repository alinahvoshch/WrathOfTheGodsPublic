using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.SpecificEffectManagers.SolynDialogue;

public sealed class SolynWorldDialogueManager : ModSystem
{
    /// <summary>
    /// The set of active dialogue instances in the world.
    /// </summary>
    private static readonly List<SolynWorldDialogue> DialogueInstances = new List<SolynWorldDialogue>(8);

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        layers.Add(new LegacyGameInterfaceLayer("Wrath of the Gods: Solyn World Dialogue", () =>
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            foreach (SolynWorldDialogue instance in DialogueInstances)
                instance.Render();

            Main.spriteBatch.ResetToDefault();
            return true;
        }));
    }

    public override void PreUpdateEntities()
    {
        foreach (SolynWorldDialogue instance in DialogueInstances)
            instance.Update();

        DialogueInstances.RemoveAll(d => d.Time >= d.Lifetime);
    }

    /// <summary>
    /// Creates a new Solyn dialogue instance in the world.
    /// </summary>
    /// <param name="textLocalizationKey">The localization key for the text that should be displayed.</param>
    /// <param name="direction">The direction of the text in the world.</param>
    /// <param name="position">The position of the text in the world.</param>
    /// <param name="lifetime">How long the text should exist for.</param>
    /// <param name="yell">Whether the text should be yelled or not.</param>
    public static void CreateNew(string textLocalizationKey, int direction, Vector2 position, int lifetime, bool yell)
    {
        SolynWorldDialogue instance = new SolynWorldDialogue(textLocalizationKey, direction, position, lifetime, yell);
        DialogueInstances.Add(instance);
    }
}
