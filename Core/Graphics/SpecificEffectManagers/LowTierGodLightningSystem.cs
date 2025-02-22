using System.Reflection;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Graphics.SpecificEffectManagers;

[Autoload(Side = ModSide.Client)]
public class LowTierGodLightningSystem : ModSystem
{
    /// <summary>
    /// The amount of frames remaining until lightning appears.
    /// </summary>
    public static int LightningDelayCountdown
    {
        get;
        set;
    }

    public override void PostUpdateDusts()
    {
        if (LightningDelayCountdown > 0)
        {
            LightningDelayCountdown--;
            if (LightningDelayCountdown == 23)
                SoundEngine.PlaySound(GennedAssets.Sounds.Custom.LowTierGodLightning with { Volume = 2f });

            if (LightningDelayCountdown == 23 || LightningDelayCountdown <= 0)
            {
                TotalScreenOverlaySystem.OverlayColor = Color.White;
                TotalScreenOverlaySystem.OverlayInterpolant += 0.75f;
                typeof(Main).GetField("lightningDecay", BindingFlags.NonPublic | BindingFlags.Static)?.SetValue(null, 0.09f);
                typeof(Main).GetField("lightningSpeed", BindingFlags.NonPublic | BindingFlags.Static)?.SetValue(null, 0.15f);
            }
        }
    }
}
