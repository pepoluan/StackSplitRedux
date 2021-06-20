﻿using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace StackSplitRedux.MenuHandlers
    {
    class BuyAction : ShopAction
        {
        /// <summary>Constructs an instance.</summary>
        /// <param name="menu">The native shop menu.</param>
        /// <param name="item">The item to buy.</param>
        public BuyAction(ShopMenu menu, ISalable item)
            : base(menu, item) {
            // Default amount
            this.Amount = Mod.Config.DefaultShopAmount;
            }

        /// <summary>Verifies the conditions to perform te action.</summary>
        public override bool CanPerformAction() {
            var held = Mod.Reflection.GetField<Item>(this.NativeShopMenu, "heldItem").GetValue();
            int currentMonies = ShopMenu.getPlayerCurrencyAmount(Game1.player, this.ShopCurrencyType);

            return
                this.ClickedItem is Item chosen          // not null
                && (
                    held == null
                    || (chosen.canStackWith(held) && held.Stack < held.maximumStackSize())  // Holding the same item and not hold max stack
                    )
                && chosen.canStackWith(chosen)           // Item type is stackable
                && currentMonies >= chosen.salePrice()   // Can afford
                ;
            }

        /// <summary>Does the action.</summary>
        /// <param name="amount">Number of items.</param>
        /// <param name="clickLocation">Where the player clicked.</param>
        public override void PerformAction(int amount, Point clickLocation) {
            var chosen = this.ClickedItem;
            var nativeMenu = this.NativeShopMenu;

            var heldItem = Mod.Reflection.GetField<Item>(nativeMenu, "heldItem").GetValue();
            var priceAndStockField = Mod.Reflection.GetField<Dictionary<ISalable, int[]>>(nativeMenu, "itemPriceAndStock");
            var priceAndStockMap = priceAndStockField.GetValue();
            Debug.Assert(priceAndStockMap.ContainsKey(chosen));

            // Calculate the number to purchase
            int numInStock = priceAndStockMap[chosen][1];
            int itemPrice = priceAndStockMap[chosen][0];
            int currentMonies = ShopMenu.getPlayerCurrencyAmount(Game1.player, this.ShopCurrencyType);
            amount = Math.Min(Math.Min(amount, currentMonies / itemPrice), Math.Min(numInStock, chosen.maximumStackSize()));

            // If we couldn't grab all that we wanted then only subtract the amount we were able to grab
            int numHeld = heldItem?.Stack ?? 0;
            int overflow = Math.Max(numHeld + amount - chosen.maximumStackSize(), 0);
            amount -= overflow;

            Log.TraceIfD($"Attempting to purchase {amount} of {chosen.Name} for {itemPrice * amount}");

            if (amount <= 0) {
                Log.TraceIfD("amount <= 0, purchase aborted");
                return;
                }
            Log.TraceIfD($"Purchase of {amount} {chosen.Name} successful");

            // Try to purchase the item - method returns true if it should be removed from the shop since there's no more.
            var purchaseMethod = Mod.Reflection.GetMethod(nativeMenu, "tryToPurchaseItem");
            int index = BuyAction.GetClickedItemIndex(nativeMenu, clickLocation);
            if (purchaseMethod.Invoke<bool>(chosen, heldItem, amount, clickLocation.X, clickLocation.Y, index)) {
                // remove the purchased item from the stock etc.
                priceAndStockMap.Remove(chosen);
                priceAndStockField.SetValue(priceAndStockMap);
                var itemsForSaleField = Mod.Reflection.GetField<List<ISalable>>(nativeMenu, "forSale");
                var itemsForSale = itemsForSaleField.GetValue();
                itemsForSale.Remove(chosen);
                itemsForSaleField.SetValue(itemsForSale);
                }
            }

        /// <summary>Helper method getting which item in the shop was clicked.</summary>
        /// <param name="shopMenu">Native shop menu.</param>
        /// <param name="p">Mouse location.</param>
        /// <returns>The clicked item or null if none was clicked.</returns>
        public static ISalable GetClickedShopItem(ShopMenu shopMenu, Point p) {
            var itemsForSale = Mod.Reflection.GetField<List<ISalable>>(shopMenu, "forSale").GetValue();
            int index = GetClickedItemIndex(shopMenu, p);
            Debug.Assert(index < itemsForSale.Count);
            return index >= 0 ? itemsForSale[index] : null;
            }

        /// <summary>Gets the index of the clicked shop item. This index corresponds to the list of buttons and list of items.</summary>
        /// <param name="shopMenu">Native shop menu.</param>
        /// <param name="p">Mouse location.</param>
        /// <returns>The clicked item or null if none was clicked.</returns>
        public static int GetClickedItemIndex(ShopMenu shopMenu, Point p) {
            int currentItemIndex = Mod.Reflection.GetField<int>(shopMenu, "currentItemIndex").GetValue();
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
