// Look, I know this sucks, but playing around with the project keeps fucking this up and adding/removing global using directives for basic things.
// I'd rather than all just be defined here so that I don't ever have a "Oops! You lost the global directives and removed a bunch of local directives in the past because your IDE cleans them on save, go fix
// 400 errors!" case again.
global using static System.MathF;
global using static Luminance.Common.Utilities.Utilities;
global using static Microsoft.Xna.Framework.MathHelper;
global using static NoxusBoss.Assets.GennedAssets.Textures.GreyscaleTextures;
global using static NoxusBoss.Assets.GennedAssets.Textures.Noise;
global using static NoxusBoss.Core.Utilities.Utilities;
using Luminance.Core.ModCalls;
using NoxusBoss.Core.Netcode;
using Terraria.ModLoader;

namespace NoxusBoss;

public class NoxusBoss : Mod
{
    // Defer packet reading to a separate class.
    public override void HandlePacket(BinaryReader reader, int whoAmI) => PacketManager.ReceivePacket(reader);

    public override object Call(params object[] args) => ModCallManager.ProcessAllModCalls(this, args);
}
