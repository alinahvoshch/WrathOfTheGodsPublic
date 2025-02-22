using Luminance.Assets;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.Items.Pets;
using NoxusBoss.Core.Graphics.RenderTargets;
using NoxusBoss.Core.Graphics.SpecificEffectManagers;
using NoxusBoss.Core.Physics.InverseKinematics;
using NoxusBoss.Core.World.GameScenes.OldDukeDeath;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Projectiles.Pets;

public class CuriousHand : ModProjectile
{
    public enum HandAIState
    {
        IdleMovement,
        StealUnfortunateNearbyEnemy
    }

    /// <summary>
    /// A general purpose AI timer for the current state.
    /// </summary>
    public int AITimer
    {
        get;
        set;
    }

    public bool TargetIsGripped
    {
        get;
        set;
    }

    /// <summary>
    /// The current state of this hand.
    /// </summary>
    public HandAIState CurrentState
    {
        get => (HandAIState)Projectile.ai[0];
        set => Projectile.ai[0] = (int)value;
    }

    /// <summary>
    /// The index of the NPC this hand has decided to steal.
    /// </summary>
    public int NPCIndexToSteal
    {
        get => (int)Projectile.ai[1];
        set => Projectile.ai[1] = value;
    }

    public ref float ArmReach => ref Projectile.ai[2];

    /// <summary>
    /// The owner of this hand.
    /// </summary>
    public Player Owner => Main.player[Projectile.owner];

    /// <summary>
    /// The grip angle of finger A.
    /// </summary>
    public float GripAngleA;

    /// <summary>
    /// The grip angle of finger B.
    /// </summary>
    public float GripAngleB;

    /// <summary>
    /// The grip angle of finger C.
    /// </summary>
    public float GripAngleC;

    /// <summary>
    /// The animation completion of finger A. Used when making the finger step forward via inverse kinematics calculations.
    /// </summary>
    public float FingerAnimationCompletionA;

    /// <summary>
    /// The animation starting point for finger A. Used when making the finger step forward via inverse kinematics calculations.
    /// </summary>
    public Vector2 FingerAnimationStartA;

    /// <summary>
    /// The reach position for finger A. This is what inverse kinematics calculations will attempt to reach towards.
    /// </summary>
    public Vector2 FingerPositionA;

    /// <summary>
    /// The animation completion of finger B. Used when making the finger step forward via inverse kinematics calculations.
    /// </summary>
    public float FingerAnimationCompletionB;

    /// <summary>
    /// The animation starting point for finger B. Used when making the finger step forward via inverse kinematics calculations.
    /// </summary>
    public Vector2 FingerAnimationStartB;

    /// <summary>
    /// The reach position for finger B. This is what inverse kinematics calculations will attempt to reach towards.
    /// </summary>
    public Vector2 FingerPositionB;

    /// <summary>
    /// The animation completion of finger C. Used when making the finger step forward via inverse kinematics calculations.
    /// </summary>
    public float FingerAnimationCompletionC;

    /// <summary>
    /// The reach position for finger C. This is what inverse kinematics calculations will attempt to reach towards.
    /// </summary>
    public Vector2 FingerAnimationStartC;

    /// <summary>
    /// The reach position for finger C. This is what inverse kinematics calculations will attempt to reach towards.
    /// </summary>
    public Vector2 FingerPositionC;

    /// <summary>
    /// The render target that holds the contents of this hand and its rift.
    /// </summary>
    /// 
    /// <remarks>
    /// This is necessary to ensure that critter shampoo can be used to apply dyes to the hand.
    /// </remarks>
    public static InstancedRequestableTarget ArmTarget
    {
        get;
        private set;
    }

    public static int[] NPCIDsToSteal => [NPCID.BlueSlime, NPCID.Zombie, NPCID.EyeofCthulhu, NPCID.MoonLordHead, FUCKYOUOLDDUKESystem.OldDukeID];

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetStaticDefaults()
    {
        Main.projPet[Type] = true;
        ProjectileID.Sets.CharacterPreviewAnimations[Type] = ProjectileID.Sets.SimpleLoop(0, 1);

        if (Main.netMode != NetmodeID.Server)
        {
            ArmTarget = new InstancedRequestableTarget();
            Main.ContentThatNeedsRenderTargets.Add(ArmTarget);
        }
    }

    public override void SetDefaults()
    {
        Projectile.width = 982;
        Projectile.height = 982;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 90000;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.netImportant = true;
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(AITimer);

        writer.Write(FingerAnimationCompletionA);
        writer.WriteVector2(FingerAnimationStartA);
        writer.WriteVector2(FingerPositionA);

        writer.Write(FingerAnimationCompletionB);
        writer.WriteVector2(FingerAnimationStartB);
        writer.WriteVector2(FingerPositionB);

        writer.Write(FingerAnimationCompletionC);
        writer.WriteVector2(FingerAnimationStartC);
        writer.WriteVector2(FingerPositionC);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        AITimer = reader.ReadInt32();

        FingerAnimationCompletionA = reader.ReadSingle();
        FingerAnimationStartA = reader.ReadVector2();
        FingerPositionA = reader.ReadVector2();

        FingerAnimationCompletionB = reader.ReadSingle();
        FingerAnimationStartB = reader.ReadVector2();
        FingerPositionB = reader.ReadVector2();

        FingerAnimationCompletionC = reader.ReadSingle();
        FingerAnimationStartC = reader.ReadVector2();
        FingerPositionC = reader.ReadVector2();
    }

    public override void AI()
    {
        CheckActive();

        TargetIsGripped = false;
        GripAngleA = GripAngleA.AngleLerp(0f, 0.1f);
        GripAngleB = GripAngleB.AngleLerp(0f, 0.1f);
        GripAngleC = GripAngleC.AngleLerp(Projectile.spriteDirection * 0.32f, 0.1f);

        switch (CurrentState)
        {
            case HandAIState.IdleMovement:
                DoBehavior_IdleMovement();
                break;
            case HandAIState.StealUnfortunateNearbyEnemy:
                DoBehavior_StealUnfortunateNearbyEnemy();
                break;
        }

        if (NPCIndexToSteal >= 0 && !Main.npc[NPCIndexToSteal].active)
        {
            NPCIndexToSteal = -1;
            Projectile.netUpdate = true;
        }

        AITimer++;
    }

    public void CheckActive()
    {
        // Keep the projectile from disappearing as long as the player isn't dead and has the pet buff.
        if (!Owner.dead && Owner.HasBuff(OblivionChime.BuffID))
            Projectile.timeLeft = 2;
    }

    /// <summary>
    /// Performs the hand's idle movement state, having it walk faithfully behind its owner.
    /// </summary>
    public void DoBehavior_IdleMovement()
    {
        float height = 424f;
        float idealRotation = Clamp((Owner.Center.X - Projectile.Center.X) * -0.023f, -0.2f, 0.2f);
        Vector2 baseDestination = Owner.Center + new Vector2(-Owner.direction * 250f, -30f);

        int tries = 0;
        while (!Collision.CanHitLine(baseDestination - Vector2.UnitX * 250f, 500, 1, Owner.Center, 1, 1))
        {
            baseDestination += Vector2.UnitX * Owner.direction * 16f;

            tries++;
            if (tries >= 50)
                break;
        }

        Vector2 leftGround = FindGroundVertical((baseDestination - Vector2.UnitX * 140f).ToTileCoordinates()).ToWorldCoordinates();
        Vector2 rightGround = FindGroundVertical((baseDestination + Vector2.UnitX * 140f).ToTileCoordinates()).ToWorldCoordinates();

        Vector2 hoverDestination = (leftGround + rightGround) * 0.5f - Vector2.UnitY * height;
        Vector2 idealFingerA = Projectile.Center + new Vector2(Owner.direction == 1 ? 205f : -190f, height);
        Vector2 idealFingerB = Projectile.Center + new Vector2(Owner.direction == 1 ? 105f : -110f, height);
        Vector2 idealFingerC = Projectile.Center + new Vector2(Owner.direction == 1 ? 270f : 90f, height);
        if (!hoverDestination.WithinRange(baseDestination, 640f))
        {
            idealFingerA.X = Lerp(idealFingerA.X, Projectile.Center.X, 0.85f);
            idealFingerB.X = Lerp(idealFingerB.X, Projectile.Center.X, 0.85f);
            idealFingerC = FingerPositionC;
            idealRotation *= 2.4f;
            hoverDestination = baseDestination;
        }

        if (!Projectile.WithinRange(Owner.Center, 3000f))
        {
            Projectile.Center = Owner.Center - Vector2.UnitX * Owner.direction * 300f;
            Projectile.velocity *= 0.05f;
            Projectile.netUpdate = true;
        }

        Projectile.SmoothFlyNear(hoverDestination, 0.05f, 0.94f);

        float moveDistanceThreshold = 120f;
        if (Projectile.velocity.Length() <= 1.8f)
            moveDistanceThreshold = 50f;

        // Move finger A.
        idealFingerA = FindGroundVertical(idealFingerA.ToTileCoordinates()).ToWorldCoordinates(8f, -2f);
        MoveFinger(ref FingerAnimationCompletionA, ref FingerPositionA, ref FingerAnimationStartA, idealFingerA, moveDistanceThreshold);

        // Move finger B.
        idealFingerB = FindGroundVertical(idealFingerB.ToTileCoordinates()).ToWorldCoordinates(8f, -2f);
        MoveFinger(ref FingerAnimationCompletionB, ref FingerPositionB, ref FingerAnimationStartB, idealFingerB, moveDistanceThreshold);

        // Move finger C.
        FingerPositionC = Vector2.Lerp(FingerPositionC, idealFingerC, 0.15f);

        Projectile.rotation = Projectile.rotation.AngleLerp(idealRotation, 0.03f);
        Projectile.spriteDirection = -Owner.direction;

        ArmReach = Lerp(ArmReach, 630f, 0.15f);

        NPCIndexToSteal = NPC.FindFirstNPC(Main.rand.Next(NPCIDsToSteal));
        if (NPCIndexToSteal >= 0 && Projectile.WithinRange(Main.npc[NPCIndexToSteal].Center, 1020f))
        {
            bool rightBelowHand = Distance(Main.npc[NPCIndexToSteal].Center.X, Projectile.Center.X) <= 100f && Main.npc[NPCIndexToSteal].Center.Y > Projectile.Center.Y;
            if (Main.rand.NextBool(900) || rightBelowHand)
                SwitchAIState(HandAIState.StealUnfortunateNearbyEnemy);
        }
    }

    public void DoBehavior_StealUnfortunateNearbyEnemy()
    {
        Projectile.rotation = Projectile.rotation.AngleLerp(0f, 0.1f);

        bool takeIntoPortal = AITimer >= 45;
        if (!takeIntoPortal)
        {
            ArmReach = Lerp(ArmReach, 375f, 0.1f);
        }
        else
        {
            ArmReach = Lerp(ArmReach, 1200f, 0.075f);
            TargetIsGripped = true;

            if (ArmReach >= 1180f && NPCIndexToSteal >= 0)
            {
                if (Main.npc[NPCIndexToSteal].type == NPCID.MoonLordHead)
                    TotalScreenOverlaySystem.OverlayInterpolant = 1.2f;
                Main.npc[NPCIndexToSteal].active = false;
            }
        }

        if (NPCIndexToSteal >= 0 && NPCIDsToSteal.Contains(Main.npc[NPCIndexToSteal].type))
        {
            NPC target = Main.npc[NPCIndexToSteal];

            float gripInterpolant = InverseLerp(0f, 32f, AITimer).Cubed() * (Projectile.spriteDirection == 1 ? -0.45f : 1f);
            GripAngleA = gripInterpolant * 0.5f;
            GripAngleB = gripInterpolant * 0.5f;
            GripAngleC = gripInterpolant * -0.4f;

            if (!takeIntoPortal)
            {
                float flySpeedInterpolant = InverseLerp(0f, 25f, AITimer) * 0.25f;
                Projectile.SmoothFlyNear(target.Center + new Vector2(Projectile.spriteDirection * -64f, -460f), flySpeedInterpolant, 1f - flySpeedInterpolant * 1.6f);
            }
            else
            {
                target.velocity.X = 0f;
                target.position.Y = Projectile.Center.Y + MathF.Max(936f - ArmReach, 0f) - target.height;
                Projectile.velocity *= 0.7f;
            }

            target.hide = TargetIsGripped;

            Vector2 basePosition = Projectile.Center + Vector2.UnitY * 800f;
            FingerPositionA = basePosition;
            FingerPositionB = basePosition;
            FingerPositionC = basePosition;
        }
        else
            SwitchAIState(HandAIState.IdleMovement);
    }

    public void SwitchAIState(HandAIState nextState)
    {
        CurrentState = nextState;
        AITimer = 0;

        FingerAnimationStartA = FingerPositionA;
        FingerAnimationCompletionA = 0f;

        FingerAnimationStartB = FingerPositionB;
        FingerAnimationCompletionB = 0f;

        FingerAnimationStartC = FingerPositionC;
        FingerAnimationCompletionC = 0f;
        Projectile.netUpdate = true;
    }

    public void MoveFinger(ref float animationCompletion, ref Vector2 currentPosition, ref Vector2 start, Vector2 end, float moveDistanceThreshold = 140f)
    {
        bool fingerAStarted = FingerAnimationCompletionA > 0f && FingerAnimationCompletionA < 0.48f;
        bool fingerBStarted = FingerAnimationCompletionB > 0f && FingerAnimationCompletionB < 0.48f;
        bool fingerCStarted = FingerAnimationCompletionC > 0f && FingerAnimationCompletionC < 0.48f;
        bool animationJustStarted = fingerAStarted || fingerBStarted || fingerCStarted;
        if (animationCompletion <= 0f && !currentPosition.WithinRange(end, moveDistanceThreshold) && !animationJustStarted)
        {
            start = currentPosition;
            animationCompletion = 0.03f;
        }

        if (animationCompletion >= 0.03f)
        {
            animationCompletion = Saturate(animationCompletion + 0.067f);
            currentPosition = Vector2.SmoothStep(start, end, InverseLerp(0.1f, 0.9f, animationCompletion));
            currentPosition -= Vector2.UnitY * Convert01To010(animationCompletion) * 70f;

            if (animationCompletion >= 1f)
                animationCompletion = 0f;
        }

        if (currentPosition.X >= 50f && currentPosition.X <= Main.maxTilesX * 16f - 50f)
            currentPosition.Y = Lerp(currentPosition.Y, FindGroundVertical(currentPosition.ToTileCoordinates()).ToWorldCoordinates(8f, 16f).Y, 0.075f);
    }

    public void RenderRift()
    {
        Main.spriteBatch.PrepareForShaders();

        float squish = 0.55f;
        Color color = new Color(77, 0, 2);
        Color edgeColor = new Color(1f, 0.08f, 0.08f);

        Texture2D innerRiftTexture = GennedAssets.Textures.FirstPhaseForm.RiftInnerTexture.Value;
        Vector2 textureArea = Projectile.Size * new Vector2(1f, 1f - squish) / innerRiftTexture.Size();

        ManagedShader riftShader = ShaderManager.GetShader("NoxusBoss.DarkPortalShader");
        riftShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly * 0.1f + Projectile.identity * 0.75f);
        riftShader.TrySetParameter("baseCutoffRadius", 0.1f);
        riftShader.TrySetParameter("swirlOutwardnessExponent", 0.151f);
        riftShader.TrySetParameter("swirlOutwardnessFactor", 3.67f);
        riftShader.TrySetParameter("vanishInterpolant", InverseLerp(1f, 0f, Projectile.scale - Projectile.identity / 13f % 0.2f));
        riftShader.TrySetParameter("edgeColor", edgeColor.ToVector4());
        riftShader.TrySetParameter("edgeColorBias", 0f);
        riftShader.SetTexture(WavyBlotchNoise, 1, SamplerState.AnisotropicWrap);
        riftShader.SetTexture(WavyBlotchNoise, 2, SamplerState.AnisotropicWrap);
        riftShader.Apply();

        Main.spriteBatch.Draw(innerRiftTexture, Projectile.Center - Main.screenPosition, null, color, Projectile.rotation, innerRiftTexture.Size() * 0.5f, textureArea, 0, 0f);

        Main.spriteBatch.ResetToDefault();
    }

    public void ForwardKinematics(float scale, Color color, Vector2 start, Vector2[] origins, Texture2D[] textures, Vector2[] offsets, float[] rotationOffsets, Vector2[]? manualOffsets = null)
    {
        SpriteEffects direction = Projectile.spriteDirection.ToSpriteDirection();
        Vector2[] drawPositions = new Vector2[textures.Length];
        Vector2 currentDrawPosition = start;
        for (int i = 0; i < textures.Length; i++)
        {
            drawPositions[i] = currentDrawPosition + (manualOffsets?[i] ?? Vector2.Zero);
            currentDrawPosition += offsets[i];
        }

        for (int i = 0; i < textures.Length; i++)
        {
            float rotation = offsets[i].ToRotation() - rotationOffsets[i] - PiOver2;
            Texture2D texture = textures[i];
            Vector2 origin = origins[i];
            if (direction == SpriteEffects.FlipHorizontally)
                origin.X = 1f - origin.X;

            Main.spriteBatch.Draw(texture, drawPositions[i], null, color, rotation, origin * texture.Size(), scale, direction, 0f);
        }
    }

    /// <summary>
    /// Renders the hand, including its individual digits.
    /// </summary>
    public void RenderHand()
    {
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);

        ManagedShader overlayShader = ShaderManager.GetShader("NoxusBoss.CuriousHandShader");
        overlayShader.TrySetParameter("lineCenter", Projectile.Center - Main.screenPosition);
        overlayShader.TrySetParameter("lineDirection", (Projectile.rotation + PiOver2).ToRotationVector2());
        overlayShader.Apply();

        Texture2D arm = GennedAssets.Textures.SecondPhaseForm.FrontForearmLeft.Value;
        float generalScale = Projectile.scale * 0.8f;
        float armRotation = Projectile.rotation;
        SpriteEffects direction = Projectile.spriteDirection.ToSpriteDirection();

        Vector2 armStart = Projectile.Center - Main.screenPosition + new Vector2(200f, -ArmReach).RotatedBy(armRotation) * generalScale;
        Vector2 handPosition = armStart + new Vector2(-190f, 578f).RotatedBy(armRotation) * generalScale;

        // Draw the arm.
        Vector2 armOrigin = arm.Size() * new Vector2(0.83f, 0.15f);
        Main.spriteBatch.Draw(arm, armStart, null, Color.White, armRotation, armOrigin, generalScale, direction, 0f);

        RenderFingerA(handPosition, generalScale, armRotation);

        // Draw the hand.
        Texture2D leftHand = GennedAssets.Textures.Hands.FrontHandLeft;
        Vector2 handOrigin = leftHand.Size() * new Vector2(0.5f, 0f);
        Main.spriteBatch.Draw(leftHand, handPosition, null, Color.White, armRotation, handOrigin, generalScale, direction, 0f);

        RenderFingerC(handPosition, generalScale, armRotation);
        RenderFingerB(handPosition, generalScale, armRotation);

        Main.spriteBatch.End();
    }

    private void RenderFingerA(Vector2 handPosition, float generalScale, float armRotation)
    {
        float digitAScale = generalScale * 0.95f;
        Vector2 digitAStart = handPosition + new Vector2(Projectile.spriteDirection * -70f, 220f).RotatedBy(armRotation) * Projectile.scale;
        Texture2D digitA1 = GennedAssets.Textures.Hands.FrontHandLeftFinger1Digit1;
        Texture2D digitA2 = GennedAssets.Textures.Hands.FrontHandLeftFinger1Digit2;
        Texture2D digitA3 = GennedAssets.Textures.Hands.FrontHandLeftFinger1Digit3;
        KinematicChain digitA = new KinematicChain(digitA1.Size().Length() * digitAScale * 0.707f, digitA2.Size().Length() * digitAScale * 0.707f, digitA3.Size().Length() * digitAScale * 0.707f)
        {
            StartingPoint = digitAStart
        };
        digitA[1].Constraints.Add(new UpwardOnlyConstraint(new Vector2(-Projectile.spriteDirection, 0f)));

        digitA.Update(FingerPositionA - Main.screenPosition);
        ForwardKinematics(digitAScale, Color.White, digitAStart,
            [
                new(0.5f, 0f),
                new(0.55f, 0.1f),
                new(0f, 0.1f),
            ], [digitA1, digitA2, digitA3],
            [digitA[0].Offset.RotatedBy(GripAngleA), digitA[1].Offset.RotatedBy(GripAngleA * 2f), digitA[2].Offset.RotatedBy(GripAngleA * 3f)],
            [0f, 0f, -0.3f]);
    }

    private void RenderFingerB(Vector2 handPosition, float generalScale, float armRotation)
    {
        float digitBScale = generalScale * 0.785f;
        Vector2 digitBStart = handPosition + new Vector2(Projectile.spriteDirection * -4f, 250f).RotatedBy(armRotation) * Projectile.scale;
        Texture2D digitB1 = GennedAssets.Textures.Hands.FrontHandLeftFinger2Digit1;
        Texture2D digitB2 = GennedAssets.Textures.Hands.FrontHandLeftFinger2Digit2;
        Texture2D digitB3 = GennedAssets.Textures.Hands.FrontHandLeftFinger2Digit3;
        KinematicChain digitB = new KinematicChain(digitB1.Size().Length() * digitBScale * 0.707f, digitB2.Size().Length() * digitBScale * 0.707f, digitB3.Size().Length() * digitBScale * 0.65f)
        {
            StartingPoint = digitBStart
        };
        digitB[0].Constraints.Add(new UpwardOnlyConstraint(new Vector2(-Projectile.spriteDirection, 0.3f)));
        digitB[1].Constraints.Add(new UpwardOnlyConstraint(new Vector2(-Projectile.spriteDirection, 0f)));

        digitB.Update(FingerPositionB - Main.screenPosition);
        ForwardKinematics(digitBScale, Color.White, digitBStart,
            [
                new(0.5f, 0f),
                new(0.5f, 0.15f),
                new(0f, 0f),
            ], [digitB1, digitB2, digitB3],
            [digitB[0].Offset.RotatedBy(GripAngleB), digitB[1].Offset.RotatedBy(GripAngleB * 2f), digitB[2].Offset.RotatedBy(GripAngleB * 3f)],
            [0f, 0f, -0.1f]);
    }

    private void RenderFingerC(Vector2 handPosition, float generalScale, float armRotation)
    {
        float digitCScale = generalScale * 1.05f;
        Vector2 digitCStart = handPosition + new Vector2(Projectile.spriteDirection * 64f, 170f).RotatedBy(armRotation) * Projectile.scale;
        Texture2D digitC1 = GennedAssets.Textures.Hands.FrontHandLeftFinger3Digit1;
        Texture2D digitC2 = GennedAssets.Textures.Hands.FrontHandLeftFinger3Digit2;
        KinematicChain digitC = new KinematicChain(digitC1.Size().Length() * digitCScale * 0.707f, digitC2.Size().Length() * digitCScale * 0.707f)
        {
            StartingPoint = digitCStart
        };

        digitC.Update(FingerPositionC - Main.screenPosition);
        ForwardKinematics(digitCScale, Color.White, digitCStart,
            [
                new(0f, 0f),
                new(0f, 0.1f)
            ], [digitC1, digitC2],
            [digitC[0].Offset.RotatedBy(GripAngleC) * 1.15f, digitC[1].Offset.RotatedBy(GripAngleC * 3f)],
            [-0.58f, (Projectile.spriteDirection == 1 ? 0f : 1.25f) - 0.97f], new Vector2[]
            {
                Vector2.Zero,
                Projectile.spriteDirection == 1 ? Vector2.Zero : (digitC[0].Rotation + GripAngleC).ToRotationVector2() * Projectile.scale * -80f + (digitC[0].Rotation + GripAngleC + PiOver2).ToRotationVector2() * Projectile.scale * 100f
            });
    }

    public override bool PreDraw(ref Color lightColor)
    {
        if (TargetIsGripped && NPCIndexToSteal >= 0)
            Main.instance.DrawNPC(NPCIndexToSteal, false);

        ArmTarget.Request(Main.screenWidth, Main.screenHeight, Projectile.whoAmI, RenderHand);
        if (!ArmTarget.TryGetTarget(Projectile.whoAmI, out RenderTarget2D? target) || target is null)
            return false;

        Color color = Color.White;
        if (!Main.gameMenu)
            color = Projectile.GetAlpha(color);

        RenderRift();

        Main.spriteBatch.PrepareForShaders();

        float blurOffset = 0.00091f;
        float[] blurWeights = new float[7];
        for (int i = 0; i < blurWeights.Length; i++)
            blurWeights[i] = GaussianDistribution(i - blurWeights.Length * 0.5f, 1.1f) / 7f;

        ManagedShader overlayShader = ShaderManager.GetShader("NoxusBoss.CuriousHandGeneralOverlayShader");
        overlayShader.TrySetParameter("glowInterpolant", 0.2f);
        overlayShader.TrySetParameter("blurWeights", blurWeights);
        overlayShader.TrySetParameter("blurOffset", blurOffset);
        overlayShader.TrySetParameter("glowColor", new Color(0.341f, 0.059f, 0.043f).ToVector3());
        overlayShader.Apply();

        Vector2 drawPosition = Main.screenLastPosition - Main.screenPosition;
        if (Main.gameMenu)
        {
            drawPosition = new Vector2(-150f, 95f);
            Projectile.spriteDirection = -1;
            Projectile.scale = 0.4f;
            ArmReach = 630f;
            Projectile.rotation = -0.5f;
            GripAngleA = 0.5f;
            GripAngleB = 0.5f;
        }

        Main.EntitySpriteDraw(target, drawPosition, null, color, 0f, Vector2.Zero, 1f, 0);

        Main.spriteBatch.ResetToDefault();

        return false;
    }
}
