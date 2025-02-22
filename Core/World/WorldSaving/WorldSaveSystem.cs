using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity;
using NoxusBoss.Core.World.GameScenes.SolynEventHandlers;
using SubworldLibrary;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace NoxusBoss.Core.World.WorldSaving;

public class WorldSaveSystem : ModSystem
{
    public static int NamelessDeityDeathCount
    {
        get;
        set;
    }

    public static bool HasMetNamelessDeity
    {
        get;
        set;
    }

    public static bool HasPlacedCattail
    {
        get;
        set;
    }

    public static bool AvatarHasKilledOldDuke
    {
        get;
        set;
    }

    public static bool OgsculeRulesOverTheUniverse
    {
        get;
        set;
    }

    public static bool HasCompletedGenesis
    {
        get;
        set;
    }

    public static bool CanUseGenesis
    {
        get;
        set;
    }

    public static int GardenTreeSurfaceYOffset
    {
        get;
        set;
    }

    public override void OnWorldLoad()
    {
        NamelessDeityBoss.Myself = null;
        if (SubworldSystem.AnyActive())
            return;

        NamelessDeityDeathCount = 0;
        HasMetNamelessDeity = false;
        OgsculeRulesOverTheUniverse = false;
        HasCompletedGenesis = false;
        CanUseGenesis = false;
        HasPlacedCattail = false;
        GardenTreeSurfaceYOffset = 0;
        AvatarHasKilledOldDuke = false;
    }

    public override void OnWorldUnload()
    {
        if (SubworldSystem.AnyActive())
            return;

        NamelessDeityDeathCount = 0;
        HasMetNamelessDeity = false;
        OgsculeRulesOverTheUniverse = false;
        HasCompletedGenesis = false;
        CanUseGenesis = false;
        HasPlacedCattail = false;
        GardenTreeSurfaceYOffset = 0;
        AvatarHasKilledOldDuke = false;
    }

    public override void SaveWorldData(TagCompound tag)
    {
        if (HasMetNamelessDeity)
            tag["HasMetNamelessDeity"] = true;
        if (OgsculeRulesOverTheUniverse)
            tag["OgsculeRulesOverTheUniverse"] = true;
        if (HasCompletedGenesis)
            tag["HasCompletedGenesis"] = true;
        if (CanUseGenesis)
            tag["CanUseGenesis"] = true;
        if (HasPlacedCattail)
            tag["HasPlacedCattail"] = true;
        if (RandomSolynSpawnSystem.SolynHasAppearedBefore)
            tag["SolynHasAppearedBefore"] = true;
        if (RandomSolynSpawnSystem.SolynHasBeenSpokenTo)
            tag["SolynHasBeenSpokenTo"] = RandomSolynSpawnSystem.SolynHasBeenSpokenTo;
        if (AvatarHasKilledOldDuke)
            tag["AvatarHasKilledOldDuke"] = AvatarHasKilledOldDuke;

        tag["NamelessDeityDeathCount"] = NamelessDeityDeathCount;
        tag["GardenTreeSurfaceYOffset"] = GardenTreeSurfaceYOffset;
    }

    public override void LoadWorldData(TagCompound tag)
    {
        HasMetNamelessDeity = tag.ContainsKey("HasMetNamelessDeity");
        OgsculeRulesOverTheUniverse = tag.ContainsKey("OgsculeRulesOverTheUniverse");
        HasCompletedGenesis = tag.ContainsKey("HasCompletedGenesis");
        CanUseGenesis = tag.ContainsKey("CanUseGenesis");
        HasPlacedCattail = tag.ContainsKey("HasPlacedCattail");
        RandomSolynSpawnSystem.SolynHasAppearedBefore = tag.ContainsKey("SolynHasAppearedBefore");
        RandomSolynSpawnSystem.SolynHasBeenSpokenTo = tag.ContainsKey("SolynHasBeenSpokenTo");
        AvatarHasKilledOldDuke = tag.ContainsKey("AvatarHasKilledOldDuke");

        NamelessDeityDeathCount = tag.GetInt("NamelessDeityDeathCount");
        GardenTreeSurfaceYOffset = tag.GetInt("GardenTreeSurfaceYOffset");
    }

    public override void NetSend(BinaryWriter writer)
    {
        BitsByte b1 = new BitsByte();
        b1[0] = HasMetNamelessDeity;
        b1[1] = OgsculeRulesOverTheUniverse;
        b1[2] = HasPlacedCattail;
        b1[3] = RandomSolynSpawnSystem.SolynHasAppearedBefore;
        b1[4] = RandomSolynSpawnSystem.SolynHasBeenSpokenTo;
        b1[5] = AvatarHasKilledOldDuke;
        b1[6] = HasCompletedGenesis;
        b1[7] = CanUseGenesis;

        writer.Write(b1);
        writer.Write(NamelessDeityDeathCount);
        writer.Write(GardenTreeSurfaceYOffset);
    }

    public override void NetReceive(BinaryReader reader)
    {
        BitsByte b1 = reader.ReadByte();
        HasMetNamelessDeity = b1[0];
        OgsculeRulesOverTheUniverse = b1[1];
        HasPlacedCattail = b1[2];
        RandomSolynSpawnSystem.SolynHasAppearedBefore = b1[3];
        RandomSolynSpawnSystem.SolynHasBeenSpokenTo = b1[4];
        AvatarHasKilledOldDuke = b1[5];
        HasCompletedGenesis = b1[6];
        CanUseGenesis = b1[7];

        NamelessDeityDeathCount = reader.ReadInt32();
        GardenTreeSurfaceYOffset = reader.ReadInt32();
    }

    public override void PreUpdateEntities()
    {
        if (BossDownedSaveSystem.HasDefeated<AvatarOfEmptiness>() || WorldVersionSystem.PreAvatarUpdateWorld)
            CanUseGenesis = true;
    }
}
