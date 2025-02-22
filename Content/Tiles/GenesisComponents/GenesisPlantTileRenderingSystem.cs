using Luminance.Common.Easings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Core.Graphics.LightingMask;
using NoxusBoss.Core.Graphics.RenderTargets;
using NoxusBoss.Core.Netcode;
using NoxusBoss.Core.Netcode.Packets;
using NoxusBoss.Core.World.TileDisabling;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace NoxusBoss.Content.Tiles.GenesisComponents;

public abstract class GenesisPlantTileRenderingSystem : ModSystem
{
    public class PlantTileData
    {
        public Point Position;

        public float GrowthInterpolant;

        public PlantTileData(Point p) => Position = p;
    }

    /// <summary>
    /// The set of all tile points within the world.
    /// </summary>
    protected readonly List<PlantTileData> tilePoints = [];

    /// <summary>
    /// The set of all rendering systems.
    /// </summary>
    internal static Dictionary<string, GenesisPlantTileRenderingSystem> renderSystems = [];

    /// <summary>
    /// The set of all tile points across all systems.
    /// </summary>
    internal static Dictionary<GenesisPlantTileRenderingSystem, List<PlantTileData>> allTilePoints
    {
        get
        {
            Dictionary<GenesisPlantTileRenderingSystem, List<PlantTileData>> allPoints = [];
            foreach (GenesisPlantTileRenderingSystem system in renderSystems.Values)
                allPoints[system] = system.tilePoints;

            return allPoints;
        }
    }

    /// <summary>
    /// Whether associated items should be dropped via this system or not.
    /// </summary>
    public virtual bool DropAfterAnimation => true;

    /// <summary>
    /// Whether this system's plant should be affected by light or not.
    /// </summary>
    public virtual bool AffectedByLight => true;

    /// <summary>
    /// The item ID associated with this rendering system.
    /// </summary>
    public abstract int ItemID
    {
        get;
    }

    /// <summary>
    /// The tile ID associated with this rendering system.
    /// </summary>
    public abstract int TileID
    {
        get;
    }

    /// <summary>
    /// God weeps.
    /// </summary>
    public InstancedRequestableTarget OverallTarget
    {
        get;
        private set;
    } = new InstancedRequestableTarget();

    public abstract void InstaceRenderFunction(bool disappearing, float growthInterpolant, float growthInterpolantModified, int i, int j, SpriteBatch spriteBatch);

    public override void OnModLoad()
    {
        renderSystems[Name] = this;
        On_Main.DoDraw_Tiles_Solid += RenderPlantInstancesWrapper;
        Main.ContentThatNeedsRenderTargets.Add(OverallTarget);
    }

    public override void OnWorldLoad() => tilePoints.Clear();

    public override void OnWorldUnload() => tilePoints.Clear();

    public override void PostUpdateEverything()
    {
        int tileID = TileID;
        for (int i = 0; i < tilePoints.Count; i++)
        {
            Tile tile = Framing.GetTileSafely(tilePoints[i].Position);
            bool seedIsGone = !tile.HasTile || tile.TileType != tileID;
            tilePoints[i].GrowthInterpolant = Saturate(tilePoints[i].GrowthInterpolant - seedIsGone.ToDirectionInt() * 0.01f);

            UpdatePoint(tilePoints[i].Position);
        }

        if (TileDisablingSystem.TilesAreUninteractable)
            return;

        // Handle the destruction of plant instances if they've ceased to be growing.
        for (int i = 0; i < tilePoints.Count; i++)
        {
            if (tilePoints[i].GrowthInterpolant <= 0f)
            {
                Point dropPoint = tilePoints[i].Position;
                if (Main.netMode != NetmodeID.MultiplayerClient && DropAfterAnimation)
                {
                    int itemIndex = Item.NewItem(new EntitySource_TileBreak(dropPoint.X, dropPoint.Y), dropPoint.ToWorldCoordinates(0f, 12f), ItemID);
                    Item item = Main.item[itemIndex];
                    item.velocity = Vector2.UnitY * -0.4f;
                }

                tilePoints.RemoveAt(i);
                i--;
            }
        }
    }

    private void RenderPlantInstancesWrapper(On_Main.orig_DoDraw_Tiles_Solid orig, Main self)
    {
        if (tilePoints.Count >= 1 && !TileDisablingSystem.TilesAreUninteractable)
        {
            OverallTarget.Request(ViewportArea.Width, ViewportArea.Height, 0, RenderInstances);
            if (OverallTarget.TryGetTarget(0, out RenderTarget2D? target) && target is not null)
            {
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                if (AffectedByLight)
                    LightingMaskTargetManager.PrepareShader();
                Main.spriteBatch.Draw(target, Main.screenLastPosition - Main.screenPosition, Color.White);
                Main.spriteBatch.End();
            }
        }

        orig(self);
    }

    private void RenderInstances()
    {
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);

        try
        {
            int tileID = TileID;
            foreach (PlantTileData data in tilePoints)
            {
                float baseGrowthInterpolant = data.GrowthInterpolant;
                float easedGrowthInterpolant = EasingCurves.Elastic.Evaluate(EasingType.Out, baseGrowthInterpolant.Squared());
                easedGrowthInterpolant = Lerp(easedGrowthInterpolant, 1f, InverseLerp(0.25f, 0.5f, baseGrowthInterpolant));

                Tile tile = Framing.GetTileSafely(data.Position);
                bool seedIsGone = !tile.HasTile || tile.TileType != tileID;
                InstaceRenderFunction(seedIsGone, baseGrowthInterpolant, easedGrowthInterpolant, data.Position.X, data.Position.Y, Main.spriteBatch);
            }
        }
        finally
        {
            Main.spriteBatch.End();
        }
    }

    internal void AddPointInternal(Point point)
    {
        if (!tilePoints.Any(p => p.Position == point))
            tilePoints.Add(new(point));
    }

    /// <summary>
    /// Registers a given point as a position where a plant instance should be rendered.
    /// </summary>
    /// <param name="point">The plant's position in world coordinates.</param>
    public void AddPoint(Point point)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient && !tilePoints.Any(p => p.Position == point))
            PacketManager.SendPacket<AddGenesisPlantPointPacket>(Name, point.X, point.Y);
        AddPointInternal(point);
    }

    public virtual void UpdatePoint(Point p) { }

    public override void SaveWorldData(TagCompound tag)
    {
        tag["PlantPoints"] = tilePoints.Select(d => d.Position).ToList();
    }

    public override void LoadWorldData(TagCompound tag)
    {
        List<Point> positions = tag.GetList<Point>("PlantPoints").ToList();
        for (int i = 0; i < positions.Count; i++)
        {
            tilePoints.Add(new(positions[i])
            {
                GrowthInterpolant = 1f
            });
        }
    }
}
