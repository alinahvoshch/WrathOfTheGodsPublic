using Luminance.Assets;
using Luminance.Common.Easings;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Draedon.Projectiles;
using NoxusBoss.Content.NPCs.Bosses.Draedon.Rendering;
using NoxusBoss.Content.NPCs.Bosses.Draedon.SpecificEffectManagers;
using NoxusBoss.Core.DataStructures;
using NoxusBoss.Core.Graphics.RenderTargets;
using NoxusBoss.Core.World.GameScenes.AvatarAppearances;
using NoxusBoss.Core.World.GameScenes.EndCredits;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace NoxusBoss.Content.NPCs.Bosses.Draedon;

public partial class MarsBody : ModNPC
{
    internal Vector2 leftElbowPosition;

    internal Vector2 rightElbowPosition;

    internal static InstancedRequestableTarget BackRopeTarget;

    internal static InstancedRequestableTarget ThrusterTarget;

    internal static InstancedRequestableTarget OverallTarget;

    internal static InstancedRequestableTarget BurnMarkTarget;

    /// <summary>
    /// The strength of Mars' thrusters.
    /// </summary>
    public float ThrusterStrength
    {
        get;
        set;
    }

    /// <summary>
    /// How effective Mars' silhouette effect is.
    /// </summary>
    public float SilhouetteInterpolant
    {
        get;
        set;
    }

    /// <summary>
    /// A 0-1 interpolant that dictates how much Mars' railgun cannon gets swapped to a forcefield projector.
    /// </summary>
    public float AltCannonVisualInterpolant
    {
        get;
        set;
    }

    /// <summary>
    /// A general-purpose visuals timer used by Mars that plays nicely with <see cref="GameSceneSlowdownSystem.SlowdownInterpolant"/> for the purposes of vfx animation.
    /// </summary>
    public float VisualsTimer
    {
        get;
        set;
    }

    /// <summary>
    /// The color of Mars' glowmasks.
    /// </summary>
    public Color GlowmaskColor
    {
        get;
        set;
    }

    /// <summary>
    /// The length of Mars' earms.
    /// </summary>
    public float ArmLength => NPC.scale * 164f;

    /// <summary>
    /// The length of Mars' forearms.
    /// </summary>
    public float ForearmLength => NPC.scale * 138f;

    /// <summary>
    /// The position of Mars' left shoulder.
    /// </summary>
    public Vector2 LeftShoulderPosition => NPC.Center + new Vector2(-112f, -10f).RotatedBy(NPC.rotation) * NPC.scale;

    /// <summary>
    /// The position of Mars' right shoulder.
    /// </summary>
    public Vector2 RightShoulderPosition => NPC.Center + new Vector2(112f, -10f).RotatedBy(NPC.rotation) * NPC.scale;

    /// <summary>
    /// Mars' top left red pipe.
    /// </summary>
    public AttachedRope TopLeftRedPipe
    {
        get;
        private set;
    } = new(new Vector2(-69f, 45f), new Vector2(-37f, 65f));

    /// <summary>
    /// Mars' top right red pipe.
    /// </summary>
    public AttachedRope TopRightRedPipe
    {
        get;
        private set;
    } = new(new Vector2(69f, 45f), new Vector2(37f, 65f));

    /// <summary>
    /// Mars' bottom left red pipe.
    /// </summary>
    public AttachedRope BottomLeftRedPipe
    {
        get;
        private set;
    } = new(new Vector2(-41f, 45f), new Vector2(-31f, 81f));

    /// <summary>
    /// Mars' bottom right red pipe.
    /// </summary>
    public AttachedRope BottomRightRedPipe
    {
        get;
        private set;
    } = new(new Vector2(41f, 45f), new Vector2(31f, 81f));

    /// <summary>
    /// Mars' left big, black pipe.
    /// </summary>
    public AttachedRope BigLeftPipe
    {
        get;
        private set;
    } = new(Vector2.Zero, Vector2.Zero);

    /// <summary>
    /// Mars' right big, black pipe.
    /// </summary>
    public AttachedRope BigRightPipe
    {
        get;
        private set;
    } = new(Vector2.Zero, Vector2.Zero);

    /// <summary>
    /// Mars' left small, black pipe.
    /// </summary>
    public AttachedRope SmallLeftPipe
    {
        get;
        private set;
    } = new(Vector2.Zero, Vector2.Zero);

    /// <summary>
    /// Mars' first left, black shoulder pipe.
    /// </summary>
    public AttachedRope LeftShoulderPipeA
    {
        get;
        private set;
    } = new(Vector2.Zero, Vector2.Zero);

    /// <summary>
    /// Mars' second left, black shoulder pipe.
    /// </summary>
    public AttachedRope LeftShoulderPipeB
    {
        get;
        private set;
    } = new(Vector2.Zero, Vector2.Zero);

    /// <summary>
    /// Mars' first right, black shoulder pipe.
    /// </summary>
    public AttachedRope RightShoulderPipeA
    {
        get;
        private set;
    } = new(Vector2.Zero, Vector2.Zero);

    /// <summary>
    /// Mars' second right, black shoulder pipe.
    /// </summary>
    public AttachedRope RightShoulderPipeB
    {
        get;
        private set;
    } = new(Vector2.Zero, Vector2.Zero);

    /// <summary>
    /// Mars' right small, black pipe.
    /// </summary>
    public AttachedRope SmallRightPipe
    {
        get;
        private set;
    } = new(Vector2.Zero, Vector2.Zero);

    /// <summary>
    /// Mars' wires.
    /// </summary>
    public AttachedRope[] Wires
    {
        get;
        private set;
    } = Array.Empty<AttachedRope>();

    /// <summary>
    /// The set of all burn marks created against Mars.
    /// </summary>
    public readonly List<BurnMarkImpact> BurnMarks = [];

    /// <summary>
    /// The palette that Mars' thrusters cycle through.
    /// </summary>
    public static readonly Palette ThrusterFlamePalette = new Palette().
        AddColor(Color.White).
        AddColor(Color.Wheat).
        AddColor(new Color(255, 240, 130)).
        AddColor(Color.Orange).
        AddColor(Color.OrangeRed);

    /// <summary>
    /// The color of Mars' string wires.
    /// </summary>
    public static readonly Color WireColor = new Color(12, 217, 105);

    /// <summary>
    /// The colors of railgun cannon telegraph lines.
    /// </summary>
    public static readonly Color RailgunCannonTelegraphColor = new Color(255, 4, 121);

    private void ResetSpriteBatch(SpriteSortMode sortMode, BlendState blendState)
    {
        Matrix matrix = Matrix.Identity;
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(sortMode, blendState, Main.DefaultSamplerState, DepthStencilState.None, DefaultRasterizerScreenCull, null, matrix);
    }

    private void DrawWithPixelation(Action drawAction, Vector2 pixelationFactor)
    {
        ResetSpriteBatch(SpriteSortMode.Immediate, BlendState.AlphaBlend);

        ManagedShader pixelationShader = ShaderManager.GetShader("Luminance.PixelationShader");
        pixelationShader.TrySetParameter("pixelationFactor", pixelationFactor);
        pixelationShader.Apply();

        drawAction();
        ResetSpriteBatch(SpriteSortMode.Deferred, BlendState.AlphaBlend);
    }

    private AttachedRope[] GenerateWires()
    {
        ulong seed = (ulong)(NPC.whoAmI + 151);
        AttachedRope[] wires = new AttachedRope[6];

        for (int i = 0; i < wires.Length; i++)
        {
            float wireCompletion = i / (float)(wires.Length - 1f);
            Vector2 wireOffset = new Vector2(SmoothStep(-80f, 80f, wireCompletion), -10f);
            wireOffset.X += Lerp(-25f, 25f, Utils.RandomFloat(ref seed));

            wires[i] = new(wireOffset, wireOffset - Vector2.UnitY * 900f);
        }

        return wires;
    }

    private static void InitializeTargets()
    {
        BackRopeTarget = new InstancedRequestableTarget();
        Main.ContentThatNeedsRenderTargets.Add(BackRopeTarget);

        ThrusterTarget = new InstancedRequestableTarget();
        Main.ContentThatNeedsRenderTargets.Add(ThrusterTarget);

        OverallTarget = new InstancedRequestableTarget();
        Main.ContentThatNeedsRenderTargets.Add(OverallTarget);

        BurnMarkTarget = new InstancedRequestableTarget();
        Main.ContentThatNeedsRenderTargets.Add(BurnMarkTarget);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (!NPC.IsABestiaryIconDummy && !ModContent.GetInstance<EndCreditsScene>().IsActive)
        {
            RiftEclipseSky.IsEnabled = true;
            RiftEclipseSky.MoveOverSunInterpolant = 1f;
            RiftEclipseSky.RiftScaleFactor = RiftEclipseSky.ScaleWhenOverSun;
        }

        // Increment the visuals timer.
        VisualsTimer += (1f - GameSceneSlowdownSystem.SlowdownInterpolant) / 60f;

        Texture2D texture = TextureAssets.Npc[Type].Value;
        Texture2D glowmask = GennedAssets.Textures.Mars.MarsBodyGlow.Value;

        float angleOffset = WrapAngle(NPC.rotation - NPC.oldRot[1]);
        Vector2 ropeDrawOffset = (NPC.position - NPC.oldPosition).RotatedBy(angleOffset);
        if (NPC.IsABestiaryIconDummy)
            ropeDrawOffset = Vector2.Zero;

        Vector2 targetSize = new Vector2(1050f, 810f);
        ProcessBurnMarks(targetSize);

        OverallTarget.Request((int)targetSize.X, (int)targetSize.Y, RenderTargetIdentifier, () =>
        {
            Vector2 renderTargetScreenPos = NPC.Center - targetSize * 0.5f;
            Vector2 renderTargetDrawPosition = NPC.Center - renderTargetScreenPos;
            Main.spriteBatch.Begin();

            ResetSpriteBatch(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            DrawThrusterFlames(-renderTargetScreenPos, out Vector2 leftThrusterPosition, out Vector2 rightThrusterPosition);
            ResetSpriteBatch(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            if (NPC.IsABestiaryIconDummy)
                UpdateRopes();

            // Keep the arms locked in a fixed location if this is a bestiary dummy.
            if (NPC.IsABestiaryIconDummy)
            {
                IdealLeftHandPosition = NPC.Center + new Vector2(-64f, 156f) * NPC.scale;
                IdealRightHandPosition = NPC.Center + new Vector2(64f, 156f) * NPC.scale;

                RailgunCannonAngle = 0.6f;
                EnergyCannonAngle = Pi - 0.6f;
            }

            PerformArmIKCalculations();

            Main.EntitySpriteDraw(texture, renderTargetDrawPosition, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, 0);
            Main.EntitySpriteDraw(glowmask, renderTargetDrawPosition, NPC.frame, NPC.GetAlpha(GlowmaskColor), NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, 0);

            Main.spriteBatch.Draw(BloomCircleSmall, leftThrusterPosition, null, new Color(255, 215, 99, 0) * ThrusterStrength, 0f, BloomCircleSmall.Size() * 0.5f, ThrusterStrength * 0.3f, 0, 0f);
            Main.spriteBatch.Draw(BloomCircleSmall, rightThrusterPosition, null, new Color(255, 215, 99, 0) * ThrusterStrength, 0f, BloomCircleSmall.Size() * 0.5f, ThrusterStrength * 0.3f, 0, 0f);

            DrawRailgunCannon(renderTargetScreenPos);
            DrawUnstableEnergyCannon(renderTargetScreenPos);

            Main.spriteBatch.End();
        });

        if (OverallTarget.TryGetTarget(RenderTargetIdentifier, out RenderTarget2D? target) && target is not null &&
            BurnMarkTarget.TryGetTarget(RenderTargetIdentifier, out RenderTarget2D? burnMarkTarget) && burnMarkTarget is not null)
        {
            DrawBackRopes(ropeDrawOffset - screenPos);
            DrawRailgunCannonTelegraphAndBeam();

            SamplerState samplerState = SamplerState.PointClamp;
            if (NPC.scale != 1f)
                samplerState = SamplerState.LinearClamp;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, samplerState, DepthStencilState.None, RasterizerState.CullNone, null, NPC.IsABestiaryIconDummy ? Main.UIScaleMatrix : Main.GameViewMatrix.TransformationMatrix);

            ManagedShader silhouetteShader = ShaderManager.GetShader("NoxusBoss.MarsSilhouetteShader");
            silhouetteShader.TrySetParameter("silhouetteInterpolant", SilhouetteInterpolant);
            silhouetteShader.TrySetParameter("silhouetteColor", new Color(14, 18, 30).ToVector4());
            silhouetteShader.SetTexture(burnMarkTarget, 1);
            silhouetteShader.Apply();

            Vector2 drawPosition = NPC.Center - screenPos;
            Main.spriteBatch.Draw(target, drawPosition, null, Color.White, 0f, target.Size() * 0.5f, 1f, 0, 0f);

            if (NPC.IsABestiaryIconDummy)
                Main.spriteBatch.ResetToDefaultUI();
            else
                Main.spriteBatch.ResetToDefault();

            // Draw a gleam over Mars' eye as an indicator if he's rendering as a silhouette.
            float gleamInterpolant = Sqrt(Convert01To010(SilhouetteInterpolant.Squared()) + 0.001f);
            float gleamRotation = 0.4f;
            Vector2 gleamScale = new Vector2(0.3f, 0.7f);
            Vector2 gleamDrawPosition = NPC.Center - screenPos + new Vector2(-12f, -64f).RotatedBy(NPC.rotation) * NPC.scale;
            Texture2D gleam = MiscTexturesRegistry.ShineFlareTexture.Value;
            Main.spriteBatch.Draw(gleam, gleamDrawPosition, null, new Color(255, 142, 9, 0) * gleamInterpolant, gleamRotation, gleam.Size() * 0.5f, gleamScale * gleamInterpolant.Squared() * 1.6f, 0, 0f);
            Main.spriteBatch.Draw(gleam, gleamDrawPosition, null, new Color(255, 174, 40, 0) * gleamInterpolant, gleamRotation, gleam.Size() * 0.5f, gleamScale * gleamInterpolant.Squared() * 1.2f, 0, 0f);
            Main.spriteBatch.Draw(gleam, gleamDrawPosition, null, new Color(255, 250, 230, 0) * gleamInterpolant, gleamRotation, gleam.Size() * 0.5f, gleamScale * gleamInterpolant.Cubed() * 0.85f, 0, 0f);
        }

        return false;
    }

    /// <summary>
    /// Processes the rendering of all burn marks.
    /// </summary>
    public void ProcessBurnMarks(Vector2 targetSize)
    {
        BurnMarkTarget.Request((int)targetSize.X, (int)targetSize.Y, RenderTargetIdentifier, () =>
        {
            if (BurnMarks.Count <= 0)
                return;

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, null, CullOnlyScreen);

            ManagedShader burnMarkShader = ShaderManager.GetShader("NoxusBoss.BurnMarkShader");
            burnMarkShader.Apply();

            Texture2D noise = PerlinNoise;
            foreach (BurnMarkImpact mark in BurnMarks)
            {
                Vector2 renderTargetScreenPos = NPC.Center - targetSize * 0.5f;
                Vector2 renderTargetDrawPosition = NPC.Center - renderTargetScreenPos;
                float lifetimeRatio = mark.Time / (float)mark.Lifetime;
                Color color = new Color(mark.Seed, lifetimeRatio, 0f, Pow(Saturate(1f - lifetimeRatio), 0.65f));
                float rotation = mark.RelativePosition.ToRotation();

                Main.spriteBatch.Draw(noise, renderTargetDrawPosition + mark.RelativePosition.RotatedBy(NPC.rotation), null, color, rotation, noise.Size() * 0.5f, mark.Scale * 0.4f, 0, 0f);
            }

            Main.spriteBatch.End();
        });
    }

    /// <summary>
    /// Draws Mars' physics-affected ropes at the back.
    /// </summary>
    /// <param name="drawOffset">The draw offset for the ropes.</param>
    public void DrawBackRopes(Vector2 drawOffset)
    {
        Vector2 ropeTargetSize = new Vector2(1800f);
        BackRopeTarget.Request((int)ropeTargetSize.X, (int)ropeTargetSize.Y, RenderTargetIdentifier, () =>
        {
            Vector2 ropeTargetOffset = ropeTargetSize * 0.5f - NPC.Center + Main.screenPosition;
            for (int i = 0; i < Wires.Length; i++)
                Wires[i].Render(completionRatio => WireColor * InverseLerp(0.75f, 0.45f, completionRatio), false, 2f, ropeTargetOffset, NPC.IsABestiaryIconDummy);
        });

        // Not using a render target for this results in layering problems in the bestiary for some reason.
        if (BackRopeTarget.TryGetTarget(RenderTargetIdentifier, out RenderTarget2D? target) && target is not null)
        {
            Main.spriteBatch.PrepareForShaders();

            ManagedShader pixelationShader = ShaderManager.GetShader("Luminance.PixelationShader");
            pixelationShader.TrySetParameter("pixelationFactor", Vector2.One * 2f / target.Size());
            pixelationShader.Apply();

            Main.spriteBatch.Draw(target, NPC.Center + drawOffset, null, Color.White, 0f, target.Size() * 0.5f, 1f, 0, 0f);
            Main.spriteBatch.ResetToDefault();
        }
    }

    /// <summary>
    /// The width function for Mars' thruster flames.
    /// </summary>
    public float ThrusterFlameWidthFunction(float completionRatio)
    {
        float pulsation = Lerp(0.65f, 1f, Cos01(VisualsTimer * -40f + completionRatio * 10f));
        float baseWidth = NPC.scale * EasingCurves.Quadratic.Evaluate(EasingType.InOut, 18f, 2f, completionRatio);
        return baseWidth * pulsation * ThrusterStrength;
    }

    /// <summary>
    /// The color function for Mars' thruster flames.
    /// </summary>
    public Color ThrusterFlameColorFunction(float completionRatio)
    {
        float colorInterpolant = InverseLerp(0f, 0.55f, completionRatio) * 0.8f;
        Color color = ThrusterFlamePalette.SampleColor(colorInterpolant);
        return NPC.GetAlpha(color);
    }

    /// <summary>
    /// Draws Mars' thruster flames.
    /// </summary>
    public void DrawThrusterFlames(Vector2 drawOffset, out Vector2 leftThrusterPosition, out Vector2 rightThrusterPosition)
    {
        float flameReach = Lerp(13f, 16f, Cos01(VisualsTimer * 20f)) * NPC.scale * ThrusterStrength;

        Vector2 leftThrusterOffset = new Vector2(-114f, 106f).RotatedBy(NPC.rotation) * NPC.scale;
        Vector2 rightThrusterOffset = new Vector2(114f, 106f).RotatedBy(NPC.rotation) * NPC.scale;
        leftThrusterPosition = NPC.Center + leftThrusterOffset + drawOffset;
        rightThrusterPosition = NPC.Center + rightThrusterOffset + drawOffset;

        Vector2 thrusterTargetSize = new Vector2(900f);
        Vector2[] leftFlamePoints = new Vector2[12];
        leftFlamePoints[0] = thrusterTargetSize * 0.5f + leftThrusterOffset + Main.screenPosition;
        Vector2[] rightFlamePoints = new Vector2[12];
        rightFlamePoints[0] = thrusterTargetSize * 0.5f + rightThrusterOffset + Main.screenPosition;

        ThrusterTarget.Request((int)thrusterTargetSize.X, (int)thrusterTargetSize.Y, RenderTargetIdentifier, () =>
        {
            float flameTurnAnglePerIncremnet = Abs(NPC.rotation) * NPC.velocity.X.NonZeroSign() * -0.11f;
            for (int i = 1; i < leftFlamePoints.Length; i++)
            {
                float angleOffset = i * flameTurnAnglePerIncremnet + NPC.rotation + 0.39f;
                leftFlamePoints[i] = leftFlamePoints[i - 1] + Vector2.UnitY.RotatedBy(angleOffset) * flameReach;

                angleOffset = i * flameTurnAnglePerIncremnet + NPC.rotation - 0.39f;
                rightFlamePoints[i] = rightFlamePoints[i - 1] + Vector2.UnitY.RotatedBy(angleOffset) * flameReach;
            }

            ManagedShader thrusterShader = ShaderManager.GetShader("NoxusBoss.MarsThrusterFlameShader");
            thrusterShader.TrySetParameter("localTime", NPC.whoAmI * 0.31f + VisualsTimer);
            thrusterShader.SetTexture(WavyBlotchNoiseDetailed, 1, SamplerState.LinearWrap);

            PrimitiveSettings settings = new PrimitiveSettings(ThrusterFlameWidthFunction, ThrusterFlameColorFunction, null, Shader: thrusterShader, UseUnscaledMatrix: true,
                ProjectionAreaWidth: Main.instance.GraphicsDevice.Viewport.Width, ProjectionAreaHeight: Main.instance.GraphicsDevice.Viewport.Height);

            PrimitiveRenderer.RenderTrail(leftFlamePoints, settings, 40);
            PrimitiveRenderer.RenderTrail(rightFlamePoints, settings, 40);
        });

        if (ThrusterTarget.TryGetTarget(RenderTargetIdentifier, out RenderTarget2D? target) && target is not null)
        {
            DrawWithPixelation(() =>
            {
                Main.spriteBatch.Draw(target, NPC.Center + drawOffset + Main.screenPosition - Main.screenLastPosition, null, Color.White, 0f, target.Size() * 0.5f, 1f, 0, 0f);
            }, Vector2.One * 2f / target.Size());
        }
    }

    /// <summary>
    /// Draws Mars' railgun cannon on his left arm.
    /// </summary>
    public void DrawRailgunCannon(Vector2 screenPosition)
    {
        Texture2D railgun = GennedAssets.Textures.Mars.RailgunCannon.Value;
        Texture2D railgunGlowmask = GennedAssets.Textures.Mars.RailgunCannonGlowmask.Value;
        Vector2 drawPosition = LeftHandPosition - screenPosition;
        Color railGunColor = Lighting.GetColor(LeftHandPosition.ToTileCoordinates());

        int cannonDirection = Cos(RailgunCannonAngle).NonZeroSign();
        float cannonAngle = RailgunCannonAngle;
        SpriteEffects direction = SpriteEffects.None;
        if (cannonDirection == -1)
        {
            direction = SpriteEffects.FlipHorizontally;
            cannonAngle += Pi;
        }

        DrawArm(screenPosition, LeftHandPosition, -1);

        Rectangle leftFrame = railgun.Frame(1, 2, 0, 0);
        Main.spriteBatch.Draw(railgun, drawPosition, leftFrame, NPC.GetAlpha(railGunColor) * (1f - AltCannonVisualInterpolant), cannonAngle, leftFrame.Size() * 0.5f, NPC.scale, direction, 0f);
        Main.spriteBatch.Draw(railgunGlowmask, drawPosition, leftFrame, NPC.GetAlpha(GlowmaskColor) * (1f - AltCannonVisualInterpolant), cannonAngle, leftFrame.Size() * 0.5f, NPC.scale, direction, 0f);

        Rectangle rightFrame = railgun.Frame(1, 2, 0, 1);
        Main.spriteBatch.Draw(railgun, drawPosition, rightFrame, NPC.GetAlpha(railGunColor) * AltCannonVisualInterpolant, cannonAngle, leftFrame.Size() * 0.5f, NPC.scale, direction, 0f);
        Main.spriteBatch.Draw(railgunGlowmask, drawPosition, rightFrame, NPC.GetAlpha(GlowmaskColor) * AltCannonVisualInterpolant, cannonAngle, leftFrame.Size() * 0.5f, NPC.scale, direction, 0f);
    }

    /// <summary>
    /// Draws mars' railgun cannon telegraph.
    /// </summary>
    public void DrawRailgunCannonTelegraphAndBeam()
    {
        int cannonDirection = Cos(RailgunCannonAngle).NonZeroSign();
        float cannonAngle = RailgunCannonAngle;
        if (cannonDirection == -1)
            cannonAngle += Pi;

        Vector2 telegraphStart = LeftHandPosition + new Vector2(cannonDirection * 10f, 39f).RotatedBy(cannonAngle) * NPC.scale;
        Vector2 telegraphEnd = telegraphStart + RailgunCannonAngle.ToRotationVector2() * 4000f;

        // Make the telegraph be stopped by the forcefield.
        telegraphEnd = AttemptForcefieldIntersection(telegraphStart, telegraphEnd);

        // Draw the line telegraph.
        Main.spriteBatch.DrawLineBetter(telegraphStart, telegraphEnd, RailgunCannonTelegraphColor * RailgunCannonTelegraphOpacity, 2f);

        // Draw all laserbeams.
        int cannonID = ModContent.ProjectileType<RailGunCannonDeathray>();
        foreach (Projectile projectile in Main.ActiveProjectiles)
        {
            if (projectile.type == cannonID)
                Main.instance.DrawProj(projectile.whoAmI);
        }
    }

    /// <summary>
    /// Draws Mars' unstable energy cannon on his right arm.
    /// </summary>
    public void DrawUnstableEnergyCannon(Vector2 screenPosition)
    {
        Texture2D cannon = GennedAssets.Textures.Mars.UnstableEnergyCannon.Value;
        Texture2D cannonGlowmask = GennedAssets.Textures.Mars.UnstableEnergyCannonGlowmask.Value;
        Rectangle frame = cannon.Frame(1, 2, 0, EnergyCannonChainsawActive.ToInt());
        Vector2 drawPosition = RightHandPosition - screenPosition - Vector2.UnitY.RotatedBy(NPC.rotation) * NPC.scale * 16f;
        Color railGunColor = Lighting.GetColor(RightHandPosition.ToTileCoordinates());

        int cannonDirection = Cos(EnergyCannonAngle).NonZeroSign();
        float cannonAngle = EnergyCannonAngle;
        SpriteEffects direction = SpriteEffects.None;
        Vector2 origin = Vector2.One * 0.5f;
        if (cannonDirection == -1)
        {
            direction = SpriteEffects.FlipHorizontally;
            cannonAngle += Pi;
        }

        DrawArm(screenPosition, RightHandPosition, 1);

        Vector3[] gradient = new Vector3[]
        {
            new Color(255, 0, 0).ToVector3(),
            new Color(255, 0, 70).ToVector3(),
            new Color(255, 255, 255).ToVector3(),
        };

        ResetSpriteBatch(SpriteSortMode.Immediate, BlendState.AlphaBlend);
        ManagedShader chainshawShader = ShaderManager.GetShader("NoxusBoss.MarsChainsawShader");
        chainshawShader.TrySetParameter("gradient", gradient);
        chainshawShader.TrySetParameter("gradientCount", gradient.Length);
        chainshawShader.TrySetParameter("scrollTime", ChainsawVisualsTimer * 3.7f);
        chainshawShader.TrySetParameter("fadeToFastScrollInterpolant", ChainsawActivationInterpolant);
        chainshawShader.SetTexture(WavyBlotchNoise, 1, SamplerState.LinearWrap);
        chainshawShader.SetTexture(GennedAssets.Textures.Mars.UnstableEnergyCannonScrollMap.Value, 2, SamplerState.PointClamp);
        chainshawShader.SetTexture(GennedAssets.Textures.Mars.UnstableEnergyCannonScrollMapBlurred.Value, 3, SamplerState.LinearClamp);
        chainshawShader.Apply();

        Main.spriteBatch.Draw(cannon, drawPosition, frame, NPC.GetAlpha(railGunColor), cannonAngle, frame.Size() * origin, NPC.scale, direction, 0f);
        Main.spriteBatch.Draw(cannonGlowmask, drawPosition, frame, NPC.GetAlpha(GlowmaskColor), cannonAngle, frame.Size() * origin, NPC.scale, direction, 0f);
        ResetSpriteBatch(SpriteSortMode.Deferred, BlendState.AlphaBlend);
    }

    public void DrawArm(Vector2 screenPosition, Vector2 handPosition, int armSide)
    {
        Vector2 connectorStart = NPC.Center + new Vector2(armSide * 84f, -26f).RotatedBy(NPC.rotation) * NPC.scale;
        Vector2 armStart = connectorStart + NPC.scale * new Vector2(armSide * 32f, -6f).RotatedBy(NPC.rotation);

        bool flip = armSide == -1;
        float elbowReorientInterpolant = InverseLerp(0f, -90f, handPosition.Y - NPC.Center.Y);
        Vector2 elbowPosition = CalculateElbowPosition(connectorStart, handPosition, ArmLength, ForearmLength, flip);
        Vector2 altElbowPosition = CalculateElbowPosition(connectorStart, handPosition, ArmLength, ForearmLength, !flip);
        elbowPosition = Vector2.SmoothStep(elbowPosition, altElbowPosition, elbowReorientInterpolant);
        elbowPosition = Vector2.Lerp(elbowPosition, connectorStart, Saturate(Sin((elbowPosition - armStart).ToRotation())) * 0.25f);

        Texture2D armTexture = GennedAssets.Textures.Mars.Arm.Value;
        Texture2D armGlowmask = GennedAssets.Textures.Mars.ArmGlowmask.Value;
        Rectangle armFrame = armTexture.Frame();
        Vector2 armOrigin = armFrame.Size() * new Vector2(0.81f, 0.66f);
        float armRotation = (elbowPosition - armStart).ToRotation() + Pi;
        if (armSide == 1)
        {
            armRotation += Pi;
            armOrigin.X = armTexture.Width - armOrigin.X;
        }
        Color armColor = NPC.GetAlpha(Lighting.GetColor(armStart.ToTileCoordinates()));
        Main.spriteBatch.Draw(armTexture, armStart - screenPosition, armFrame, armColor, armRotation, armOrigin, NPC.scale, armSide.ToSpriteDirection() ^ SpriteEffects.FlipHorizontally, 0f);
        Main.spriteBatch.Draw(armGlowmask, armStart - screenPosition, armFrame, GlowmaskColor, armRotation, armOrigin, NPC.scale, armSide.ToSpriteDirection() ^ SpriteEffects.FlipHorizontally, 0f);

        Texture2D forearmTexture = GennedAssets.Textures.Mars.Forearm.Value;
        Texture2D forearmGlowmask = GennedAssets.Textures.Mars.ForearmGlowmask.Value;
        Rectangle forearmFrame = forearmTexture.Frame();
        Color forearmColor = NPC.GetAlpha(Lighting.GetColor(elbowPosition.ToTileCoordinates()));
        Vector2 forearmOrigin = forearmFrame.Size() * new Vector2(0.81f, 0.66f);
        float forearmRotation = (handPosition - elbowPosition).ToRotation() + Pi;
        if (armSide == 1)
        {
            forearmRotation += Pi;
            forearmOrigin.X = forearmTexture.Width - forearmOrigin.X;
        }
        Main.spriteBatch.Draw(forearmTexture, elbowPosition - screenPosition, forearmFrame, forearmColor, forearmRotation, forearmOrigin, NPC.scale, armSide.ToSpriteDirection() ^ SpriteEffects.FlipHorizontally, 0f);
        Main.spriteBatch.Draw(forearmGlowmask, elbowPosition - screenPosition, forearmFrame, GlowmaskColor, forearmRotation, forearmOrigin, NPC.scale, armSide.ToSpriteDirection() ^ SpriteEffects.FlipHorizontally, 0f);
    }
}
