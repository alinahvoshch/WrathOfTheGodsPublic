using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.Items.GenesisComponents;
using NoxusBoss.Core.CrossCompatibility.Inbound.BossChecklist;
using NoxusBoss.Core.World.WorldSaving;
using Terraria;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Draedon;

public partial class MarsBody : ModNPC, IBossChecklistSupport
{
    private static NPC? bossChecklistPortait;

    public bool IsMiniboss => false;

    public string ChecklistEntryName => "Mars";

    public float ProgressionValue => 23.25f;

    public bool IsDefeated => BossDownedSaveSystem.HasDefeated<MarsBody>();

    public bool UsesCustomPortraitDrawing => true;

    public List<int> Collectibles => [ModContent.ItemType<SyntheticSeedling>()];

    public void DrawCustomPortrait(SpriteBatch spriteBatch, Rectangle area, Color color)
    {
        if (bossChecklistPortait is null)
        {
            bossChecklistPortait = new NPC();
            bossChecklistPortait.SetDefaults(Type);
        }

        bossChecklistPortait.IsABestiaryIconDummy = true;
        bossChecklistPortait.scale = 0.85f;
        bossChecklistPortait.Center = area.Center() - Vector2.UnitY * 40f;

        MarsBody mars = bossChecklistPortait.As<MarsBody>();
        mars.LeftHandPosition = bossChecklistPortait.Center + new Vector2(-100f, 60f);

        bossChecklistPortait.ModNPC.PreDraw(spriteBatch, Vector2.Zero, Color.White);
    }
}
