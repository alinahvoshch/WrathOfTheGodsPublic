using Luminance.Assets;
using Luminance.Common.DataStructures;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SpecificEffectManagers.ParadiseReclaimed;
using NoxusBoss.Core.CrossCompatibility.Inbound;
using NoxusBoss.Core.CrossCompatibility.Inbound.BaseCalamity;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Avatar.Projectiles;

public class FallingObject : ModProjectile, IProjOwnedByBoss<AvatarOfEmptiness>
{
    /// <summary>
    /// The depth of this object in the static.
    /// </summary>
    public int Depth => (int)Projectile.ai[0];

    /// <summary>
    /// A unique identifier for this object.
    /// </summary>
    public int Identifier
    {
        get => (int)Projectile.localAI[0];
        set => Projectile.localAI[0] = value;
    }

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
    }

    public override void SetDefaults()
    {
        Projectile.width = 2;
        Projectile.height = 2;
        Projectile.hostile = true;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 420;
        Projectile.scale = Main.rand?.NextFloat(1.4f, 2.3f) ?? 1f;
        CooldownSlot = ImmunityCooldownID.Bosses;
    }

    public override void AI()
    {
        Projectile.rotation += Abs(Projectile.velocity.Y * 0.002f) * Projectile.velocity.X.NonZeroSign();
        Projectile.velocity.Y = Clamp(Projectile.velocity.Y * 1.01f + 0.2f, -12f, 12f);

        if (Projectile.timeLeft == 419)
        {
            Identifier = Main.rand.Next(23);
            if (Main.rand.NextBool(12))
                Identifier = Main.rand.Next(20, 23);

            // Apollo.
            if (Main.rand.NextBool(50))
                Identifier = 50;
        }
        if (Identifier == 50)
            Projectile.rotation = Projectile.velocity.ToRotation() + PiOver2;
    }

    private void RenderSelf()
    {
        int npcToCopy = NPCID.Zombie;
        int horizontalFrames = 1;
        int? verticalFramesOverride = null;
        int frameX = 0;
        int frameY = 0;
        switch (Identifier)
        {
            case 0:
                npcToCopy = NPCID.DungeonSlime;
                break;
            case 1:
                npcToCopy = NPCID.GiantCursedSkull;
                break;
            case 2:
                npcToCopy = NPCID.MartianProbe;
                break;
            case 3:
                npcToCopy = NPCID.MisterStabby;
                break;
            case 4:
                npcToCopy = NPCID.Shark;
                break;
            case 5:
                npcToCopy = NPCID.Harpy;
                break;
            case 6:
                npcToCopy = NPCID.DemonEye;
                break;
            case 7:
                npcToCopy = NPCID.SandElemental;
                break;
            case 8:
                if (CalamityCompatibility.Enabled && CalamityCompatibility.Calamity.TryFind("ShockstormShuttle", out ModNPC shuttle))
                    npcToCopy = shuttle.Type;
                break;
            case 9:
                if (CalamityCompatibility.Enabled && CalamityCompatibility.Calamity.TryFind("AeroSlime", out ModNPC yuh))
                {
                    frameY = 2;
                    npcToCopy = yuh.Type;
                }
                break;
            case 10:
                if (CalamityCompatibility.Enabled && CalamityCompatibility.Calamity.TryFind("SuperDummyNPC", out ModNPC dummy))
                    npcToCopy = dummy.Type;
                break;
            case 11:
                if (CommonCalamityVariables.OldDukeDefeated)
                {
                    if (CalamityCompatibility.Enabled && CalamityCompatibility.Calamity.TryFind("OldDukeToothBall", out ModNPC toothBall))
                        npcToCopy = toothBall.Type;
                }
                else
                    npcToCopy = NPCID.GreenJellyfish;

                break;
            case 12:
                if (CalamityCompatibility.Enabled && CalamityCompatibility.Calamity.TryFind("Stormlion", out ModNPC stormlion))
                    npcToCopy = stormlion.Type;
                break;
            case 13:
                npcToCopy = NPCID.PossessedArmor;
                break;
            case 14:
                npcToCopy = NPCID.GiantTortoise;
                break;
            case 15:
                npcToCopy = NPCID.Paladin;
                break;
            case 16:
                npcToCopy = NPCID.Corruptor;
                break;
            case 17:
                npcToCopy = NPCID.FloatyGross;
                break;
            case 18:
                npcToCopy = NPCID.RedDevil;
                break;
            case 19:
                npcToCopy = NPCID.SkeletonMerchant;
                break;
            case 50:
                if (CalamityCompatibility.Enabled && CalamityCompatibility.Calamity.TryFind("Apollo", out ModNPC apollo))
                {
                    npcToCopy = apollo.Type;
                    horizontalFrames = 10;
                    verticalFramesOverride = 9;
                    frameX = 6;
                    frameY = (int)(Main.GlobalTimeWrappedHourly * 13f) % 6;
                }
                break;
        }

        if (npcToCopy < NPCID.Count)
            Main.instance.LoadNPC(npcToCopy);

        float[] blurWeights = new float[9];
        for (int i = 0; i < blurWeights.Length; i++)
            blurWeights[i] = GaussianDistribution(i - blurWeights.Length * 0.5f, 2f) * 0.3f;

        Texture2D texture = TextureAssets.Npc[npcToCopy].Value;
        float inBackgroundInterpolant = Depth * 0.185f;
        ManagedShader shader = ShaderManager.GetShader("NoxusBoss.ParadiseReclaimedObjectShader");
        shader.TrySetParameter("blurWeights", blurWeights);
        shader.TrySetParameter("blurOffset", 0f);
        shader.Apply();

        Rectangle frame = texture.Frame(horizontalFrames, verticalFramesOverride ?? Main.npcFrameCount[npcToCopy], frameX, frameY);

        float scale = Projectile.scale / (Depth * 0.34f + 1f);
        if (Identifier == 20)
        {
            texture = GennedAssets.Textures.AvatarOfEmptiness.FallingTree1;
            frame = texture.Frame();
        }
        if (Identifier == 21)
        {
            texture = GennedAssets.Textures.AvatarOfEmptiness.FallingTree2;
            frame = texture.Frame();
        }
        if (Identifier == 22)
        {
            texture = GennedAssets.Textures.AvatarOfEmptiness.FallingTree3;
            frame = texture.Frame();
        }

        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Main.spriteBatch.Draw(texture, drawPosition, frame, Projectile.GetAlpha(Color.White.MultiplyRGB(Color.White * (1.2f - inBackgroundInterpolant))), Projectile.rotation, frame.Size() * 0.5f, scale, 0, 0f);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        ParadiseStaticLayerHandlers.GetLayerByDepth(Depth).IndependentRenderQueue.Enqueue(RenderSelf);
        return false;
    }
}
