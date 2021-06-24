using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace StackSplitRedux.MenuHandlers
    {
    public class SellAction : ShopAction
        {
        private readonly Guid GUID = Guid.NewGuid();

        /// <summary>Constructs an instance.</summary>
        /// <param name="menu">The native shop menu.</param>
        /// <param name="item">The item to buy.</param>
        public SellAction(ShopMenu menu, Item item)
            : base(menu, item) {
            // Default amount
            // +1 before /2 ensures we get the number rounded UP
            this.Amount = (this.ClickedItem.Stack + 1) / 2;
            Log.TraceIfD($"[{nameof(SellAction)}] Instantiated for shop {menu} item {item}, GUID = {GUID}");
            }

        ~SellAction() {
            Log.TraceIfD($"[{nameof(SellAction)}] Finalized for GUID = {GUID}");
            }

        /// <summary>Verifies the conditions to perform the action.</summary>
        public override bool CanPerformAction() {
            return (this.NativeShopMenu.highlightItemToSell(this.ClickedItem) && this.ClickedItem.Stack > 1);
            }

        /// <summary>Does the action.</summary>
        /// <param name="amount">Number of items.</param>
        /// <param name="clickLocation">Where the player clicked.</param>
        public override void PerformAction(int amount, Point clickLocation) {
            amount = Math.Min(amount, this.ClickedItem.Stack);
            this.Amount = amount; // Remove if we don't need to carry this around

            // Sell item
            int price = CalculateSalePrice(this.ClickedItem, amount);
            ShopMenu.chargePlayer(Game1.player, this.ShopCurrencyType, price);
            Log.Trace($"[{nameof(SellAction)}.{nameof(PerformAction)}] Charged player {price} for {amount} of {this.ClickedItem.Name}");

            // Update the stack amount/remove the item
            var actualInventory = this.Inventory.actualInventory;
            var index = actualInventory.IndexOf(this.ClickedItem);
            if (index >= 0 && index < actualInventory.Count) {
                int amountRemaining = this.ClickedItem.Stack - amount;
                if (amountRemaining > 0)
                    actualInventory[index].Stack = amountRemaining;
                else
                    actualInventory[index] = null;
                }

            Game1.playSound("purchaseClick");

            // The animation seems to only play when we sell 1
            if (amount == 1) {
                // Play the sell animation
                var animationsField = Mod.Reflection.GetField<List<TemporaryAnimatedSprite>>(this.NativeShopMenu, "animations");
                var animations = animationsField.GetValue();

                // Messy because it's a direct copy-paste from the source code
                Vector2 value = this.Inventory.snapToClickableComponent(clickLocation.X, clickLocation.Y);
                var startingPoint = new Point((int)value.X + 32, (int)value.Y + 32);
                var endingPoint = Game1.dayTimeMoneyBox.position + new Vector2(96f, 196f);
                animations.Add(
                    new TemporaryAnimatedSprite(
                        textureName: Game1.debrisSpriteSheet.Name, 
                        sourceRect: new Rectangle(Game1.random.Next(2) * Game1.tileSize, 256, Game1.tileSize, Game1.tileSize), 
                        animationInterval: 9999f, 
                        animationLength: 1, 
                        numberOfLoops: 999, 
                        position: value + new Vector2(32f, 32f), 
                        flicker: false, 
                        flipped: false
                        ) {
                            alphaFade = 0.025f,
                            motion = Utility.getVelocityTowardPoint(startingPoint, endingPoint, 12f),
                            acceleration = Utility.getVelocityTowardPoint(startingPoint, endingPoint, 0.5f)
                            }
                    );

                animationsField.SetValue(animations);
                }
            }

        // TODO: verify this is correct and Item.sellToShopPrice doesn't do the same thing
        /// <summary>Calculates the sale price of an item based on the algorithms used in the game source.</summary>
        /// <param name="item">Item to get the price for.</param>
        /// <param name="amount">Number being sold.</param>
        /// <returns>The sale price of the item * amount.</returns>
        private int CalculateSalePrice(Item item, int amount) {
            // Formula from ShopMenu.cs
            float sellPercentage = Mod.Reflection.GetField<float>(this.NativeShopMenu, "sellPercentage").GetValue();

            float pricef = sellPercentage * amount;
            pricef *= item is StardewValley.Object sobj
                ? sobj.sellToStorePrice()
                : item.salePrice() * 0.5f
                ;

            // Invert so we give the player money instead (shitty but it's what the game does).
            return -(int)pricef;
            }

        /// <summary>Creates an instance of the action.</summary>
        /// <param name="shopMenu">Native shop menu.</param>
        /// <param name="mouse">Mouse position.</param>
        /// <returns>The instance or null if no valid item was selected.</returns>
        public static ShopAction Create(ShopMenu shopMenu, Point mouse) {
            var inventory = shopMenu.inventory;
            var item = inventory.getItemAt(mouse.X, mouse.Y);
            return item != null ? new SellAction(shopMenu, item) : null;
            }
        }
    }
