using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Autoloaders;

public static class PetBuffAutoloader
{
    [Autoload(false)]
    public class PetBuff : ModBuff
    {
        private int? petProjectileID;

        // This kind of sucks but AddContent only works in Load hooks, but it's not guaranteed direct projectile IDs will be loaded at that point.
        private readonly string petProjectileName;

        private readonly string name;

        private readonly string texturePath;

        private readonly bool lightPet;

        public override string Name => name;

        public override string Texture => texturePath;

        public PetBuff(string petProjectileName, string name, bool lightPet = false)
        {
            this.petProjectileName = petProjectileName;
            this.name = name;
            this.lightPet = lightPet;
            texturePath = GetAssetPath("Content/Buffs", name);
        }

        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;

            if (lightPet)
                Main.lightPet[Type] = true;
            else
                Main.vanityPet[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            bool _ = false;
            player.BuffHandle_SpawnPetIfNeededAndSetTime(buffIndex, ref _, petProjectileID ??= ModContent.Find<ModProjectile>(petProjectileName).Type);
        }
    }

    public static int Create(Mod mod, string petProjectileName, string name, bool lightPet = false)
    {
        PetBuff buff = new PetBuff(petProjectileName, name, lightPet);
        mod.AddContent(buff);

        return buff.Type;
    }
}
