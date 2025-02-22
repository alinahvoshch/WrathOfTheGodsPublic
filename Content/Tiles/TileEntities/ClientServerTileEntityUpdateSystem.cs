using Terraria.DataStructures;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Tiles.TileEntities;

public class ClientServerTileEntityUpdateSystem : ModSystem
{
    public override void PreUpdateEntities()
    {
        foreach (TileEntity te in TileEntity.ByID.Values)
        {
            if (te is IClientSideTileEntityUpdater updater)
                updater.ClientSideUpdate();
        }
    }

    public override void PostDrawTiles()
    {
        foreach (TileEntity te in TileEntity.ByID.Values)
        {
            if (te is IClientSideTileEntityUpdater updater)
                updater.ClientSideUpdate_RenderCycle();
        }
    }
}
