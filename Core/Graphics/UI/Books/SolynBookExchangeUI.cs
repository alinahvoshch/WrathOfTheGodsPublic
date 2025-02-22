using Luminance.Common.Easings;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.Tiles.SolynCampsite;
using NoxusBoss.Content.Tiles.TileEntities;
using NoxusBoss.Core.Autoloaders.SolynBooks;
using NoxusBoss.Core.GlobalInstances;
using NoxusBoss.Core.Graphics.FastParticleSystems;
using NoxusBoss.Core.Netcode;
using NoxusBoss.Core.Netcode.Packets;
using ReLogic.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace NoxusBoss.Core.Graphics.UI.Books;

public class SolynBookExchangeUI : ModSystem
{
    /// <summary>
    /// How much the rewards panel has unrolled, as a 0-1 interpolant.
    /// </summary>
    public float RewardsPanelUnrollInterpolant
    {
        get;
        set;
    }

    /// <summary>
    /// The horizontal coverage of the rewards panel.
    /// </summary>
    public float RewardsPanelHorizontalCoverage
    {
        get;
        set;
    }

    /// <summary>
    /// The tile entity that's currently visible.
    /// </summary>
    public TESolynTent? VisibleTileEntity
    {
        get;
        set;
    }

    /// <summary>
    /// The set of pulse interpolants for each book slot.
    /// </summary>
    public Dictionary<int, float> NewPulseInterpolants
    {
        get;
        private set;
    } = new Dictionary<int, float>();

    /// <summary>
    /// The maximum amount of book slots that can exist in a single row before a new row must be created.
    /// </summary>
    public static int MaxHorizontalSlots => 8;

    /// <summary>
    /// The amount of padding space between slots in the UI.
    /// </summary>
    public static float SpacingBetweenSlots => 6f;

    /// <summary>
    /// The overall scale factor of the UI.
    /// </summary>
    public static float UIScale => 0.7f;

    /// <summary>
    /// The amount of padding space on the edges of the UI.
    /// </summary>
    public static Vector2 EdgePadding => Vector2.One * 8f;

    /// <summary>
    /// The particle responsible for shiny effects on newly redeemed books.
    /// </summary>
    public static FastParticleSystem BubbleParticleSystem
    {
        get;
        private set;
    } = FastParticleSystemManager.CreateNew(256, PrepareBubbleRendering, ExtraParticleUpdates);

    public override void OnWorldLoad() => Reset();

    public override void OnWorldUnload() => Reset();

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int inventoryIndex = layers.FindIndex(layer => layer.Name == "Vanilla: Inventory");
        if (inventoryIndex == -1)
            return;

        layers.Insert(inventoryIndex, new LegacyGameInterfaceLayer("Wrath of the Gods: Solyn Book Exchange UI", () =>
        {
            RenderUI();
            return true;
        }, InterfaceScaleType.Game));
    }

    /// <summary>
    /// Resets all local UI variables.
    /// </summary>
    public void Reset()
    {
        RewardsPanelUnrollInterpolant = 0f;
        RewardsPanelHorizontalCoverage = 0f;
        NewPulseInterpolants.Clear();
    }

    private void RenderUI()
    {
        if (VisibleTileEntity is null)
        {
            Reset();
            return;
        }

        // Make sure the tile didn't get broken.
        Point16 tilePosition = VisibleTileEntity.Position;
        TESolynTent? tileEntity = FindTileEntity<TESolynTent>(tilePosition.X, tilePosition.Y, SolynTent.Width, SolynTent.Height);
        if (tileEntity is null || Framing.GetTileSafely(tilePosition.X, tilePosition.Y).TileType != ModContent.TileType<SolynTent>())
        {
            VisibleTileEntity = null;
            return;
        }

        float opacity = VisibleTileEntity.UIAppearanceInterpolant;
        if (opacity <= 0.1f)
            return;

        Vector2 drawBottom = tilePosition.ToVector2() * 16f + new Vector2(SolynTent.Width * 8f, -54f) - Main.screenPosition;
        List<AutoloadableSolynBook> books = SolynBookExchangeRegistry.ObtainableBooks;
        Vector2 bookSlotsTop = RenderBookSlots(books, drawBottom, opacity);
        RenderCompletionBar(books, drawBottom, opacity);
        RenderRewardsSlots(bookSlotsTop + Vector2.UnitX * UIScale * 256f, opacity);

        if (BubbleParticleSystem.particles.Any(p => p.Active))
        {
            Main.spriteBatch.ResetToDefault();
            BubbleParticleSystem.UpdateAll();
            BubbleParticleSystem.RenderAll();
            Main.spriteBatch.ResetToDefault();
        }
    }

    private Vector2 RenderBookSlots(List<AutoloadableSolynBook> books, Vector2 drawBottom, float opacity)
    {
        int totalSlots = books.Count;
        int totalRows = (int)Ceiling(totalSlots / (float)MaxHorizontalSlots);
        float expand = EasingCurves.Cubic.Evaluate(EasingType.InOut, Sqrt(InverseLerp(0f, 0.6f, opacity)));
        Vector2 scale = new Vector2(expand, 1f) * UIScale;
        Texture2D slotBackground = TextureAssets.InventoryBack.Value;

        Vector2 slotCoverage = (slotBackground.Width + SpacingBetweenSlots) * scale;
        Vector2 backgroundSize = slotCoverage * new Vector2(MaxHorizontalSlots, totalRows) + EdgePadding;
        Rectangle drawArea = new Rectangle((int)(drawBottom.X - backgroundSize.X * 0.5f), (int)(drawBottom.Y - backgroundSize.Y), (int)backgroundSize.X, (int)backgroundSize.Y);
        Utils.DrawInvBG(Main.spriteBatch, drawArea, new Color(23, 25, 81) * opacity);

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

        AutoloadableSolynBook? redemmedBook = HandleBookPlacement(books, drawArea);

        Vector2 topLeft = drawArea.TopLeft() + EdgePadding;
        for (int i = 0; i < totalSlots; i++)
        {
            if (!NewPulseInterpolants.ContainsKey(i))
                NewPulseInterpolants[i] = 0f;

            float x = i % MaxHorizontalSlots;
            float y = i / MaxHorizontalSlots;
            bool lastRow = y == totalRows - 1;
            AutoloadableSolynBook book = books[i];

            // Center icons on the last row.
            if (lastRow)
            {
                int totalSlotsInLastRow = totalSlots % MaxHorizontalSlots;
                if (totalSlotsInLastRow >= 1)
                    x += (MaxHorizontalSlots - totalSlotsInLastRow) * 0.5f;
            }

            // Render the background.
            Vector2 iconTopLeft = topLeft + new Vector2(x, y) * slotCoverage;
            Vector2 iconCenter = iconTopLeft + slotBackground.Size() * scale * 0.5f;
            bool mouseOverIcon = Utils.CenteredRectangle(iconCenter, slotBackground.Size() * scale).Contains(Main.MouseScreen.ToPoint());
            Main.spriteBatch.Draw(slotBackground, iconTopLeft, null, Color.White * opacity, 0f, Vector2.Zero, scale, 0, 0f);

            // Turn the book into a silhouette if it has not been obtained yet.
            bool bookIsObtained = SolynBookExchangeRegistry.RedeemedBooks.Contains(book.Name);
            Color bookColor = Color.White;
            if (!bookIsObtained)
            {
                bookColor = new Color(10, 11, 25) * 0.3f;
                ManagedShader silhouetteShader = ShaderManager.GetShader("NoxusBoss.SilhouetteShader");
                silhouetteShader.Apply();
            }

            if (mouseOverIcon)
            {
                if (NewPulseInterpolants[i] >= 1f)
                    NewPulseInterpolants[i] = 0f;

                Item clone = book.Item.Clone();
                clone.Wrath().UnobtainedSolynBook = !bookIsObtained;
                clone.Wrath().SlotInBookshelfUI = true;
                ItemSlot.MouseHover([clone]);
            }

            // Create bubbles if a book was just redeemed.
            if (book.Name == (redemmedBook?.Name ?? string.Empty))
            {
                for (int j = 0; j < 10; j++)
                {
                    Vector2 bubbleVelocity = -Vector2.UnitY.RotatedByRandom(0.5f) * Main.rand.NextFloat(2f, 3f);
                    BubbleParticleSystem.CreateNew(iconCenter + Main.rand.NextVector2Circular(30f, 30f) * UIScale + Main.screenPosition, bubbleVelocity, Vector2.One * Main.rand.NextFloat(5f, 8f), Color.White);
                }
                NewPulseInterpolants[i] = 0.01f;
            }

            // Render the item.
            if (NewPulseInterpolants[i] > 0f && NewPulseInterpolants[i] < 1f)
            {
                NewPulseInterpolants[i] += 0.021f;
                float pulse = Convert01To010(NewPulseInterpolants[i].Squared());
                for (int j = 0; j < 25; j++)
                    ItemSlot.DrawItemIcon(book.Item, 0, Main.spriteBatch, iconCenter + (TwoPi * j / 25f).ToRotationVector2() * pulse * 2.5f, scale.X, scale.X * 50f, new Color(255, 124, 23, 0) * opacity);
            }
            ItemSlot.DrawItemIcon(book.Item, 0, Main.spriteBatch, iconCenter, scale.X, scale.X * 50f, bookColor * opacity);
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();

            if (NewPulseInterpolants[i] > 0f)
            {
                float newPulseScale = Lerp(0.38f, 0.42f, Cos01(Main.GlobalTimeWrappedHourly * 3f + i).Squared());
                DynamicSpriteFont font = FontAssets.DeathText.Value;
                ChatManager.DrawColorCodedString(Main.spriteBatch, font, "New!", iconCenter + scale * new Vector2(-16f, 6f), new Color(255, 209, 23) * opacity, 0f, Vector2.Zero, Vector2.One * scale * newPulseScale);
            }
        }

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

        return drawArea.Top();
    }

    private static AutoloadableSolynBook? HandleBookPlacement(List<AutoloadableSolynBook> books, Rectangle placementArea)
    {
        bool mouseOverBackground = placementArea.Contains(Main.MouseScreen.ToPoint());
        if (mouseOverBackground)
        {
            Main.LocalPlayer.mouseInterface = true;
            Main.blockMouse = true;
        }

        bool mouseItemIsBook = books.Any(b => Main.mouseItem.type == b.Type);
        if (!mouseItemIsBook)
            return null;

        bool clickingOnBackground = mouseOverBackground && Main.mouseLeft && Main.mouseLeftRelease;
        if (clickingOnBackground && Main.mouseItem.ModItem is AutoloadableSolynBook mouseBook)
        {
            bool bookAlreadyPlaced = SolynBookExchangeRegistry.RedeemedBooks.Contains(mouseBook.Name);
            if (!bookAlreadyPlaced)
            {
                SolynBookExchangeRegistry.RedeemBook(Main.LocalPlayer, mouseBook.Name);

                // Play the synth based on rarity.
                if (mouseBook.Data.Rarity <= 1)
                    SoundEngine.PlaySound(GennedAssets.Sounds.Solyn.BookSynthFirstTier);
                else if (mouseBook.Data.Rarity == 2)
                    SoundEngine.PlaySound(GennedAssets.Sounds.Solyn.BookSynthSecondTier);
                else
                    SoundEngine.PlaySound(GennedAssets.Sounds.Solyn.BookSynthThirdTier);

                // Play a random page flip sound.
                SoundEngine.PlaySound(GennedAssets.Sounds.Solyn.BookCollectPage);

                // Consume the item.
                if (Main.mouseItem.stack >= 2)
                    Main.mouseItem.stack--;
                else
                    Main.mouseItem.TurnToAir();

                return mouseBook;
            }
        }

        return null;
    }

    private static void RenderCompletionBar(List<AutoloadableSolynBook> books, Vector2 drawBottom, float opacity)
    {
        float bookObtainmentInterpolant = Saturate(SolynBookExchangeRegistry.RedeemedBooks.Count / (float)books.Count);
        float bookObtainmentPercentage = Round(bookObtainmentInterpolant * 100f, 1);
        Texture2D background = GennedAssets.Textures.SolynBookExchange.ProgressBarBack;
        Texture2D fill = GennedAssets.Textures.SolynBookExchange.ProgressBarFill;

        float expand = EasingCurves.Cubic.Evaluate(EasingType.InOut, Sqrt(InverseLerp(0.25f, 1f, opacity)));
        Vector2 scale = new Vector2(expand, Cbrt(expand));

        Vector2 drawPosition = drawBottom + Vector2.UnitY * 12f;
        Rectangle fillArea = new Rectangle(0, 0, (int)(fill.Width * bookObtainmentInterpolant), fill.Height);
        Main.spriteBatch.Draw(background, drawPosition, null, Color.White * opacity, 0f, background.Size() * 0.5f, scale, 0, 0f);
        Main.spriteBatch.Draw(fill, drawPosition, fillArea, Color.White * opacity, 0f, background.Size() * 0.5f, scale, 0, 0f);

        Vector2 textScale = Vector2.One * scale * 0.36f;
        DynamicSpriteFont font = FontAssets.DeathText.Value;
        string completionText = Language.GetText("Mods.NoxusBoss.UI.SolynBookExchange.BookObtainmentPercentage").Format(bookObtainmentPercentage);
        Vector2 textDrawPosition = drawPosition + new Vector2(font.MeasureString(completionText).X * textScale.X * -0.5f, 12f);
        ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, completionText, textDrawPosition, Color.White * opacity, 0f, Vector2.Zero, textScale);
    }

    private void RenderRewardsSlots(Vector2 drawTop, float opacity)
    {
        List<Item> unclaimedRewards = Main.LocalPlayer.GetModPlayer<SolynBookRewardsPlayer>().UnclaimedRewards;
        int totalSlots = unclaimedRewards.Count;
        if (totalSlots <= 0)
        {
            RewardsPanelUnrollInterpolant = 0f;
            return;
        }

        // Make the rewards panel unroll.
        RewardsPanelUnrollInterpolant = Saturate(RewardsPanelUnrollInterpolant + 0.023f);
        RewardsPanelHorizontalCoverage = Lerp(RewardsPanelHorizontalCoverage, totalSlots, 0.15f);

        int totalRows = (int)Ceiling(totalSlots / (float)MaxHorizontalSlots);
        float unroll = MathF.Max(0f, EasingCurves.Elastic.Evaluate(EasingType.InOut, RewardsPanelUnrollInterpolant));
        float horizontalExpand = EasingCurves.Cubic.Evaluate(EasingType.InOut, Sqrt(InverseLerp(0f, 0.6f, opacity)));
        Vector2 scale = new Vector2(horizontalExpand, 1f) * UIScale * unroll;
        Texture2D slotBackground = TextureAssets.InventoryBack.Value;

        Vector2 slotCoverage = (slotBackground.Width + SpacingBetweenSlots) * scale;
        Vector2 backgroundSize = slotCoverage * new Vector2(MathF.Min(RewardsPanelHorizontalCoverage, MaxHorizontalSlots), totalRows) + EdgePadding;
        Rectangle drawArea = new Rectangle((int)drawTop.X, (int)drawTop.Y, (int)backgroundSize.X, (int)backgroundSize.Y);
        Utils.DrawInvBG(Main.spriteBatch, drawArea, new Color(23, 25, 81) * opacity);

        DynamicSpriteFont font = FontAssets.MouseText.Value;
        Vector2 topLeft = drawArea.TopLeft() + EdgePadding;
        for (int i = 0; i < totalSlots; i++)
        {
            float x = i % MaxHorizontalSlots;
            float y = i / MaxHorizontalSlots;
            Item reward = unclaimedRewards[i];

            // Render the background.
            Vector2 iconScale = scale;
            if (i > RewardsPanelHorizontalCoverage)
                continue;
            if (i > RewardsPanelHorizontalCoverage - 1f)
                iconScale *= RewardsPanelHorizontalCoverage - i;

            Vector2 iconTopLeft = topLeft + new Vector2(x, y) * slotCoverage;
            Vector2 iconCenter = iconTopLeft + slotBackground.Size() * iconScale * 0.5f;
            bool mouseOverIcon = Utils.CenteredRectangle(iconCenter, slotBackground.Size() * iconScale).Contains(Main.MouseScreen.ToPoint());
            Main.spriteBatch.Draw(slotBackground, iconTopLeft, null, Color.White * opacity, 0f, Vector2.Zero, iconScale, 0, 0f);

            if (mouseOverIcon)
            {
                if (Utils.PressingShift(Main.keyState))
                    Main.cursorOverride = 7;

                if (Main.mouseLeft && Main.mouseLeftRelease)
                {
                    bool rewardWasClaimed = false;
                    if (Utils.PressingShift(Main.keyState) && Main.LocalPlayer.ItemSpace(reward).CanTakeItemToPersonalInventory)
                    {
                        SoundEngine.PlaySound(SoundID.Grab);
                        Main.LocalPlayer.QuickSpawnItem(new EntitySource_WorldEvent(), reward.Clone(), reward.stack);
                        unclaimedRewards.Remove(reward);
                        rewardWasClaimed = true;
                    }
                    else if (Main.mouseItem.IsAir)
                    {
                        SoundEngine.PlaySound(SoundID.Grab);
                        Main.mouseItem = reward.Clone();
                        unclaimedRewards.Remove(reward);
                        rewardWasClaimed = true;
                    }
                    if (rewardWasClaimed)
                        PacketManager.SendPacket<SolynBookRewardPacket>(Main.myPlayer);

                    return;
                }
                ItemSlot.MouseHover([reward]);
            }

            // Render the item.
            float itemScale = MathF.Min(iconScale.X, iconScale.Y);
            ItemSlot.DrawItemIcon(reward, 0, Main.spriteBatch, iconCenter, itemScale, itemScale * 50f, Color.White * opacity);

            ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, reward.stack.ToString(), iconCenter - Vector2.UnitX * iconScale * 16f, Color.White * opacity, Color.Black * opacity, 0f, Vector2.Zero, Vector2.One * iconScale);
        }

        bool mouseOverBackground = drawArea.Contains(Main.MouseScreen.ToPoint());
        if (mouseOverBackground)
        {
            Main.LocalPlayer.mouseInterface = true;
            Main.blockMouse = true;
        }
    }

    private static void PrepareBubbleRendering()
    {
        Matrix world = Matrix.CreateTranslation(-Main.screenPosition.X, -Main.screenPosition.Y, 0f);
        Matrix projection = Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, -100f, 100f);

        Texture2D bubble = GennedAssets.Textures.SolynBookExchange.NewBubbleParticle.Value;
        ManagedShader overlayShader = ShaderManager.GetShader("NoxusBoss.BasicPrimitiveOverlayShader");
        overlayShader.TrySetParameter("uWorldViewProjection", world * Main.GameViewMatrix.TransformationMatrix * projection);
        overlayShader.SetTexture(bubble, 1, SamplerState.LinearClamp);
        overlayShader.Apply();
    }

    private static void ExtraParticleUpdates(ref FastParticle particle)
    {
        particle.Size *= 0.975f;
        particle.Velocity.X *= 0.97f;
        particle.Velocity.Y *= 0.93f;

        if (particle.Time >= 150)
            particle.Active = false;
    }
}
