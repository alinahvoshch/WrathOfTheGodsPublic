using Microsoft.Xna.Framework;
using NoxusBoss.Content.Projectiles.Typeless;
using NoxusBoss.Content.Rarities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace NoxusBoss.Content.Items;

public class PerversePurse : ModItem
{
    /// <summary>
    /// The NPC ID of the victim that this purse is holding.
    /// </summary>
    public int[] VictimIDs
    {
        get;
        set;
    } = new int[MaxVictims];

    /// <summary>
    /// The given name of the victim this purse is holding.
    /// </summary>
    public string[] VictimNames
    {
        get;
        set;
    } = new string[MaxVictims];

    /// <summary>
    /// How many stored victims this purse is holding.
    /// </summary>
    public int TotalStoredVictims;

    /// <summary>
    /// The maximum number of town NPCs this bag can store.
    /// </summary>
    public const int MaxVictims = 3;

    public override string Texture => GetAssetPath("Content/Items", Name);

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults()
    {
        Item.width = 36;
        Item.height = 40;
        Item.useTime = 10;
        Item.useAnimation = Item.useTime;
        Item.rare = ModContent.RarityType<SolynRewardRarity>();
        Item.useStyle = ItemUseStyleID.Swing;

        if (!PerversePurseSpawnPreventionSystem.ActivePurses.Contains(Item) && !Main.gameMenu)
            PerversePurseSpawnPreventionSystem.ActivePurses.Add(Item);
    }

    public override bool AltFunctionUse(Player player) => TotalStoredVictims >= 1;

    public override void UseAnimation(Player player)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || player.itemAnimation != 0)
            return;

        Item.noMelee = player.altFunctionUse != 2;
        Item.channel = player.altFunctionUse != 2;
        Item.noUseGraphic = player.altFunctionUse != 2;
        Item.shoot = player.altFunctionUse == 2 ? 0 : ModContent.ProjectileType<PerversePurseHoldout>();

        if (player.altFunctionUse == 2)
            ReleaseLastNPC(player);
    }

    public void ReleaseLastNPC(Player player)
    {
        if (TotalStoredVictims <= 0)
            return;

        TotalStoredVictims--;
        int npc = NPC.NewNPC(Item.GetSource_ReleaseEntity(), (int)player.Center.X, (int)player.Center.Y, VictimIDs[TotalStoredVictims], 1);
        if (Main.npc.IndexInRange(npc))
            Main.npc[npc].GivenName = VictimNames[TotalStoredVictims] ?? Main.npc[npc].GivenName;

        VictimIDs[TotalStoredVictims] = NPCID.None;
        VictimNames[TotalStoredVictims] = string.Empty;
    }

    public void KidnapNPC(NPC npc)
    {
        if (TotalStoredVictims >= MaxVictims)
            return;

        VictimIDs[TotalStoredVictims] = npc.type;
        VictimNames[TotalStoredVictims] = npc.GivenName;
        TotalStoredVictims++;
        npc.active = false;
    }

    public override void SaveData(TagCompound tag)
    {
        List<string> victimIDs = new List<string>(MaxVictims);
        for (int i = 0; i < VictimIDs.Length; i++)
        {
            int id = VictimIDs[i];
            if (id >= NPCID.Count)
            {
                ModNPC npc = NPCLoader.GetNPC(id);
                victimIDs.Add($"{npc.Mod.Name},{npc.Name}");
            }
            else
                victimIDs.Add(id.ToString());
        }

        tag["TotalStoredVictims"] = TotalStoredVictims;
        tag["VictimIDs"] = victimIDs;

        for (int i = 0; i < VictimNames.Length; i++)
        {
            if (VictimNames[i] is null)
                VictimNames[i] = string.Empty;
        }

        tag["VictimNames"] = VictimNames.ToList();
    }

    public override void LoadData(TagCompound tag)
    {
        if (tag.TryGet("TotalStoredVictims", out int victimCount))
            TotalStoredVictims = victimCount;
        if (tag.TryGet("VictimIDs", out List<string> ids))
        {
            VictimIDs = new int[ids.Count];
            for (int i = 0; i < ids.Count; i++)
            {
                string idText = ids[i];
                if (int.TryParse(idText, out int id))
                    VictimIDs[i] = id;
                else
                {
                    string[] idParts = idText.Split(',');
                    string modName = idParts[0];
                    string npcName = idParts[1];
                    if (ModLoader.TryGetMod(modName, out Mod mod) && mod.TryFind<ModNPC>(npcName, out ModNPC npc))
                        VictimIDs[i] = npc.Type;
                }
            }
        }
        if (tag.TryGet("VictimNames", out List<string> names))
            VictimNames = names.ToArray();

        if (!PerversePurseSpawnPreventionSystem.ActivePurses.Contains(Item))
            PerversePurseSpawnPreventionSystem.ActivePurses.Add(Item);
    }

    public override void NetSend(BinaryWriter writer)
    {
        VictimIDs ??= new int[MaxVictims];
        VictimNames ??= new string[MaxVictims];
        for (int i = 0; i < MaxVictims; i++)
        {
            writer.Write(VictimIDs[i]);
            writer.Write(VictimNames[i] ?? string.Empty);
        }
    }

    public override void NetReceive(BinaryReader reader)
    {
        VictimIDs ??= new int[MaxVictims];
        VictimNames ??= new string[MaxVictims];
        for (int i = 0; i < MaxVictims; i++)
        {
            VictimIDs[i] = reader.ReadInt32();
            VictimNames[i] = reader.ReadString();
        }
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        for (int i = 0; i < MaxVictims; i++)
        {
            int victimID = VictimIDs[i];
            string victimName = VictimNames[i];
            if (victimID != NPCID.None && !string.IsNullOrEmpty(victimName))
            {
                TooltipLine infoLine = new TooltipLine(Mod, $"VictimInfo{i}", this.GetLocalization("InfoLine").Format(victimName, Lang.GetNPCNameValue(victimID)))
                {
                    OverrideColor = new Color(237, 15, 76)
                };
                tooltips.Add(infoLine);
            }
        }
    }
}
