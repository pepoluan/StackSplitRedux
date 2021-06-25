using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;

using StardewValley;
using StardewValley.Menus;

namespace StackSplitRedux.MenuHandlers
    {
    class BuyAction : ShopAction
        {
        private readonly Guid GUID = Guid.NewGuid();

        private int? _MaxPurchasable = null;
        /// <summary>Constructs an instance.</summary>
        /// <param name="menu">The native shop menu.</param>
        /// <param name="item">The item to buy.</param>
        public BuyAction(ShopMenu menu, ISalable item)
            : base(menu, item) {
            // Default amount
            this.Amount = Mod.Config.DefaultShopAmount;
            Log.TraceIfD($"[{nameof(BuyAction)}] Instantiated for shop {menu} item {item}, GUID = {GUID}");
            }

        ~BuyAction() {
            Log.TraceIfD($"[{nameof(BuyAction)}] Finalized for GUID = {GUID}");
            }

        /// <summary>Verifies the conditions to perform the action.</summary>
        public override bool CanPerformAction() {
            var held = Mod.Reflection.GetField<Item>(this.NativeShopMenu, "heldItem").GetValue();
            int currentMonies = ShopMenu.getPlayerCurrencyAmount(Game1.player, this.ShopCurrencyType);

            return
                this.ClickedItem is Item chosen          // not null
                && chosen.canStackWith(chosen)           // Item type is stackable
                && (
                    held == null
                    || (chosen.canStackWith(held) && held.Stack < held.maximumStackSize())  // Holding the same item and not hold max stack
                    )
                && currentMonies >= chosen.salePrice()   // Can afford
                ;
            }

        /// <summary>Does the action.</summary>
        /// <param name="amount">Number of items.</param>
        /// <param name="clickLocation">Where the player clicked.</param>
        public override void PerformAction(int amount, Point clickLocation) {
            var chosen = this.ClickedItem;
            var chosen_max = chosen.maximumStackSize();
            var nativeMenu = this.NativeShopMenu;

            Log.Trace($"[{nameof(BuyAction)}.{nameof(PerformAction)}] chosen = {chosen}, nativeMenu = {nativeMenu}, ShopCurrencyType = {this.ShopCurrencyType}");

            var heldItem = Mod.Reflection.GetField<Item>(nativeMenu, "heldItem").GetValue();
            Dictionary<ISalable, int[]> priceAndStockMap = nativeMenu.itemPriceAndStock;
            Debug.Assert(priceAndStockMap.ContainsKey(chosen));

            // Using Linq here is slower by A LOT but ultimately MUCH more readable
            amount = Seq.Min(amount, GetMaxPurchasable(), chosen_max);

            // If we couldn't grab all that we wanted then only subtract the amount we were able to grab
            int numHeld = heldItem?.Stack ?? 0;
            if ((numHeld + amount) > chosen_max) amount = chosen_max - numHeld;

            if (amount <= 0) {
                Log.Trace($"[{nameof(BuyAction)}.{nameof(PerformAction)}] purchasable amount <= 0, purchase aborted");
                return;
                }

            Log.Trace($"[{nameof(BuyAction)}.{nameof(PerformAction)}] Purchasing {amount} of {chosen.Name}");

            // Try to purchase the item - method returns true if it should be removed from the shop since there's no more.
            var purchaseMethod = Mod.Reflection.GetMethod(nativeMenu, "tryToPurchaseItem");
            int index = BuyAction.GetClickedItemIndex(nativeMenu, clickLocation);
            if (purchaseMethod.Invoke<bool>(chosen, heldItem, amount, clickLocation.X, clickLocation.Y, index)) {
                Log.TraceIfD($"[{nameof(BuyAction)}.{nameof(PerformAction)}] Item is limited, reducing stock");
                // remove the purchased item from the stock etc.
                priceAndStockMap.Remove(chosen);
                nativeMenu.forSale.Remove(chosen);
                }
            }

        public int GetMaxPurchasable() {
            if (this._MaxPurchasable is null) {
                Item chosen = this.ClickedItem;
                Dictionary<ISalable, int[]> priceAndStockMap = this.NativeShopMenu.itemPriceAndStock;
                Debug.Assert(priceAndStockMap.ContainsKey(chosen));

                // Calculate the number to purchase
                int[] stockData = priceAndStockMap[chosen];
                Log.Trace($"[{nameof(BuyAction)}.{nameof(GetMaxPurchasable)}] chosen stockData = {string.Join(", ", stockData)}");
                int numInStock = stockData[1];
                int itemPrice = stockData[0];
                int currentMonies;
                if (itemPrice > 0) {  // using money
                    currentMonies = ShopMenu.getPlayerCurrencyAmount(Game1.player, this.ShopCurrencyType);
                    Log.TraceIfD($"[{nameof(BuyAction)}.{nameof(GetMaxPurchasable)}] player has {currentMonies} of currency {this.ShopCurrencyType}");
                    }
                else {  // barter system. "monies" is now the wanted barter item in [2]
                    itemPrice = stockData[3];
                    var barterItem = stockData[2];
                    currentMonies = Game1.player.getItemCount(barterItem);
                    Log.TraceIfD($"[{nameof(BuyAction)}.{nameof(GetMaxPurchasable)}] Barter system: player has {currentMonies} of item {barterItem}");
                    }
                Log.Trace($"[{nameof(BuyAction)}.{nameof(GetMaxPurchasable)}] chosen item price is {itemPrice}");
                Debug.Assert(itemPrice > 0);

                this._MaxPurchasable = Math.Min(currentMonies / itemPrice, numInStock);
                }
            return this._MaxPurchasable.Value;
            }

        /// <summary>Helper method getting which item in the shop was clicked.</summary>
        /// <param name="shopMenu">Native shop menu.</param>
        /// <param name="p">Mouse location.</param>
        /// <returns>The clicked item or null if none was clicked.</returns>
        public static ISalable GetClickedShopItem(ShopMenu shopMenu, Point p) {
            var itemsForSale = shopMenu.forSale;
            int index = GetClickedItemIndex(shopMenu, p);
            Debug.Assert(index < itemsForSale.Count);
            return index >= 0 ? itemsForSale[index] : null;
            }

        /// <summary>Gets the index of the clicked shop item. This index corresponds to the list of buttons and list of items.</summary>
        /// <param name="shopMenu">Native shop menu.</param>
        /// <param name="p">Mouse location.</param>
        /// <returns>The clicked item or null if none was clicked.</returns>
        public static int GetClickedItemIndex(ShopMenu shopMenu, Point p) {
            int currentItemIndex = shopMenu.currentItemIndex;
            int saleButtonIndex = shopMenu.forSaleButtons.FindIndex(button => button.containsPoint(p.X, p.Y));
            return saleButtonIndex > -1 ? currentItemIndex + saleButtonIndex : -1;
            }

        /// <summary>Creates an instance of the action.</summary>
        /// <param name="shopMenu">Native shop menu.</param>
        /// <param name="mouse">Mouse position.</param>
        /// <returns>The instance or null if no valid item was selected.</returns>
        public static ShopAction Create(ShopMenu shopMenu, Point mouse) {
            var item = BuyAction.GetClickedShopItem(shopMenu, mouse);
            return item != null ? new BuyAction(shopMenu, item) : null;
            }
        }
    }
