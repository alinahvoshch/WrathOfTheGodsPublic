using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Core.Autoloaders;

public static class TreasureBagAutoloader
{
    [Autoload(false)]
    public class AutoloadableTreasureBag : ModItem
    {
        /// <summary>
        /// The texture path of the autoloaded treasure bag.
        /// </summary>
        private readonly string texturePath;

        /// <summary>
        /// The internal name of autoloaded treasure bag.
        /// </summary>
        private readonly string name;

        /// <summary>
        /// Optional behaviors that should occur when <see cref="SetDefaults"/> is called.
        /// </summary>
        private readonly Action<Item> setDefaults;

        /// <summary>
        /// The behavior that defines how <see cref="ModifyItemLoot"/> executes, defining the loot of the treasure bag.
        /// </summary>
        private readonly Action<ItemLoot> lootSet;

        public override string Name => name;

        public override string Texture => texturePath;

        // This is necessary for autoloaded types since the constructor is important in determining the behavior of the given instance, making it impossible to rely on an a parameterless one for
        // managing said instances.
        protected override bool CloneNewInstances => true;

        public AutoloadableTreasureBag(string texturePath, Action<Item> setDefaults, Action<ItemLoot> lootSet)
        {
            string name = Path.GetFileName(texturePath);
            this.texturePath = texturePath;
            this.name = name;
            this.setDefaults = setDefaults;
            this.lootSet = lootSet;
        }

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 3;

            // Mark this item as a boss bag. This will make it glow when in the world.
            ItemID.Sets.BossBag[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.maxStack = Item.CommonMaxStack;
            Item.width = 24;
            Item.height = 24;
            Item.expert = true;
            setDefaults(Item);
        }

        public override bool CanRightClick() => true;

        public override void ModifyItemLoot(ItemLoot itemLoot) => lootSet(itemLoot);
    }

    public static int Create(Mod mod, string bagPath, Action<Item> setDefaults, Action<ItemLoot> lootSet)
    {
        AutoloadableTreasureBag bag = new AutoloadableTreasureBag(bagPath, setDefaults, lootSet);
        mod.AddContent(bag);

        return bag.Type;
    }
}
