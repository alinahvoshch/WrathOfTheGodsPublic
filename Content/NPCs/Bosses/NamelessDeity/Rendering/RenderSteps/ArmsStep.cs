using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using static NoxusBoss.Content.NPCs.Bosses.NamelessDeity.NamelessDeityBoss;

namespace NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Rendering.RenderSteps;

public class ArmsStep : INamelessDeityRenderStep
{
    public int LayerIndex => 95;

    public bool UsingPreset
    {
        get
        {
            if (Composite.UsedPreset?.Data.PreferredArmTextures is not null)
                return true;
            if (Composite.UsedPreset?.Data.PreferredForearmTextures is not null)
                return true;
            if (Composite.UsedPreset?.Data.PreferredHandTextures is not null)
                return true;

            return false;
        }
    }

    public NamelessDeityRenderComposite Composite
    {
        get;
        set;
    }

    /// <summary>
    /// The set of hands associated with this step.
    /// </summary>
    public List<NamelessDeityHand> Hands
    {
        get;
        set;
    } = [];

    /// <summary>
    /// The texture used for the arm.
    /// </summary>
    public NamelessDeitySwappableTexture ArmTexture
    {
        get;
        private set;
    }

    /// <summary>
    /// The texture used for the forearm.
    /// </summary>
    public NamelessDeitySwappableTexture ForearmTexture
    {
        get;
        private set;
    }

    /// <summary>
    /// The texture used for the hand.
    /// </summary>
    public NamelessDeitySwappableTexture HandTexture
    {
        get;
        private set;
    }

    public void Initialize()
    {
        int armVariantCount = 5;
        ArmTexture = Composite.RegisterSwappableTexture("Arm", armVariantCount, Composite.UsedPreset?.Data.PreferredArmTextures);
        ForearmTexture = Composite.RegisterSwappableTexture("Forearm", armVariantCount, Composite.UsedPreset?.Data.PreferredForearmTextures);
        HandTexture = Composite.RegisterSwappableTexture("Hand", armVariantCount, Composite.UsedPreset?.Data.PreferredHandTextures).WithAutomaticSwapRule(() =>
        {
            return Composite.Time % 164 == 0;
        });

        HandTexture.OnSwap += () =>
        {
            // Keep the arm and forearm textures synced with the hand by default. Presets may override this.
            int handTextureVariant = int.Parse(string.Concat(HandTexture.TexturePath.Where(char.IsDigit).ToArray()));
            if (Composite.UsedPreset?.Data.PreferredArmTextures is null)
                ArmTexture.TexturePath = $"NoxusBoss/Assets/Textures/Content/NPCs/Bosses/NamelessDeity/Arm{handTextureVariant}";
            if (Composite.UsedPreset?.Data.PreferredForearmTextures is null)
                ForearmTexture.TexturePath = $"NoxusBoss/Assets/Textures/Content/NPCs/Bosses/NamelessDeity/Forearm{handTextureVariant}";
        };
    }

    public void RenderArms(Entity owner, bool drawingToTarget)
    {
        float zPosition = 0f;
        float handOpacity = 1f;
        bool performingRealityTearPunches = false;
        if (owner is NPC npc && npc.ModNPC is NamelessDeityBoss nameless)
        {
            zPosition = nameless.ZPosition;
            if (nameless.DrawCongratulatoryText)
                return;

            if (nameless.CurrentState == NamelessAIType.DeathAnimation && npc.ai[2] == 1f)
                return;

            if (nameless.HandsShouldInheritOpacity)
                handOpacity = npc.Opacity;

            performingRealityTearPunches = nameless.CurrentState == NamelessAIType.RealityTearPunches;
        }

        Vector2 screenPos = Main.screenPosition;
        if (drawingToTarget)
            screenPos = owner.Center - ViewportSize * 0.5f;

        Texture2D handVariantTexture = HandTexture.UsedTexture;
        Texture2D forearmTexture = ForearmTexture.UsedTexture;
        Texture2D armTexture = ArmTexture.UsedTexture;
        float zPositionDarkness = Utils.Remap(zPosition, 0f, 2.1f, 0f, 0.42f);
        foreach (NamelessDeityHand hand in Hands)
        {
            if (hand.HasArms && performingRealityTearPunches && !drawingToTarget)
                continue;
            if (!hand.HasArms && performingRealityTearPunches && drawingToTarget)
                continue;

            hand.Draw(screenPos, owner.Center, zPositionDarkness, handOpacity, HandTexture.TextureName, new NamelessDeityHand.HandArmTextureSet(handVariantTexture, armTexture, forearmTexture));
        }
    }

    public void Render(Entity owner, Vector2 drawCenter)
    {
        if (owner is NPC npc && npc.ModNPC is NamelessDeityBoss nameless)
        {
            if (nameless.UniversalBlackOverlayInterpolant >= 1f)
                return;
        }

        RenderArms(owner, true);
    }
}
