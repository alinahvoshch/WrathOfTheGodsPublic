using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.FormPresets;
using NoxusBoss.Core.Configuration;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Rendering;

public class NamelessDeitySwappableTexture
{
    /// <summary>
    /// How long the texture has to wait until swaps can be performed again.
    /// </summary>
    public int SwapBlockCountdown;

    /// <summary>
    /// The currently used texture path.
    /// </summary>
    public string TexturePath;

    /// <summary>
    /// A simple event that is fired whenever a texture swap happens.
    /// </summary>
    public event Action OnSwap;

    /// <summary>
    /// An automatic condition that when true causes a texture swap to happen. Check via <see cref="Update"/>.
    /// </summary>
    public Func<bool> SwapRule;

    /// <summary>
    /// A list of all possible texture paths that can be used.
    /// </summary>
    public string[] PossibleTexturePaths;

    /// <summary>
    /// The amount of possible variants this swappable texture can choose from.
    /// </summary>
    public int PossibleVariants => PossibleTexturePaths.Length;

    /// <summary>
    /// The suffix of this path, indicating the name of the texture.
    /// </summary>
    public string TextureName => TexturePath.Replace("NoxusBoss/Assets/Textures/Content/NPCs/Bosses/NamelessDeity/", string.Empty);

    /// <summary>
    /// The currently used texture.
    /// </summary>
    public Texture2D UsedTexture => ModContent.Request<Texture2D>(TexturePath).Value;

    public NamelessDeitySwappableTexture(string partPrefix, int totalVariants)
    {
        PossibleTexturePaths = new string[totalVariants];
        TexturePath = Main.rand.Next(PossibleTexturePaths);
        for (int i = 0; i < totalVariants; i++)
            PossibleTexturePaths[i] = $"NoxusBoss/Assets/Textures/Content/NPCs/Bosses/NamelessDeity/{partPrefix}{i + 1}";
    }

    public NamelessDeitySwappableTexture(string[] possibleTexturePaths)
    {
        PossibleTexturePaths = possibleTexturePaths;
        TexturePath = Main.rand.Next(PossibleTexturePaths);
    }

    /// <summary>
    /// Assigns an automatic swap rule to this texture set.
    /// </summary>
    /// <param name="rule">The swap rule.</param>
    public NamelessDeitySwappableTexture WithAutomaticSwapRule(Func<bool> rule)
    {
        SwapRule = rule;
        return this;
    }

    /// <summary>
    /// Temporarily forces this texture to a specific texture.
    /// </summary>
    public void ForceToTexture(string relativeTexturePath)
    {
        TexturePath = $"NoxusBoss/Assets/Textures/Content/NPCs/Bosses/NamelessDeity/{relativeTexturePath}";
        SwapBlockCountdown = 20;
    }

    /// <summary>
    /// Checks if a swap should happen in accordance with the <see cref="SwapRule"/>, assuming one exists.
    /// </summary>
    public void Update()
    {
        if (SwapBlockCountdown >= 1)
            SwapBlockCountdown--;
        else
        {
            bool performSwap = SwapRule?.Invoke() ?? false;
            if (NamelessDeityFormPresetRegistry.UsingYuHPreset && !WoTGConfig.Instance.PhotosensitivityMode)
                performSwap = true;

            if (performSwap)
                Swap();
        }
    }

    /// <summary>
    /// Selects a new texture variant, ensuring that a different selection is made. This does not do anything on servers.
    /// </summary>
    public void Swap()
    {
        if (Main.netMode == NetmodeID.Server || SwapBlockCountdown >= 1)
            return;

        if (PossibleVariants >= 2)
        {
            string originalTexture = TexturePath;
            do
            {
                TexturePath = Main.rand.Next(PossibleTexturePaths);
            }
            while (TexturePath == originalTexture);

            // Call the OnSwap event.
            OnSwap?.Invoke();
        }

        if (PossibleTexturePaths.Length == 1)
            TexturePath = PossibleTexturePaths[0];
    }
}
