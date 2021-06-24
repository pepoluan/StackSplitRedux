using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;

namespace StackSplitRedux.MenuHandlers
    {
    public abstract class ShopAction : IShopAction
        {
        /// <summary>Native shope menu.</summary>
        protected ShopMenu NativeShopMenu { get; private set; }

        /// <summary>Native inventory menu.</summary>
        protected InventoryMenu Inventory { get; private set; }

        /// <summary>Currency type of the shop.</summary>
        protected int ShopCurrencyType { get; private set; }

        /// <summary>The item to be bought/sold.</summary>
        protected Item ClickedItem = null;

        /// <summary>The number of items in the transaction.</summary>
        protected int Amount { get; set; } = 0;

        /// <summary>The number of items in the transaction.</summary>
        public int StackAmount => this.Amount;


        /// <summary>Constructor.</summary>
        /// <param name="menu">Native shop menu.</param>
        /// <param name="item">Clicked item that this action will act on.</param>
        public ShopAction(ShopMenu menu, ISalable item) {
            Log.TraceIfD($"[{nameof(ShopAction)}..ctor] Instantiating for menu = {menu}, item = {item}");
            this.NativeShopMenu = menu;
            this.ClickedItem = (Item)item;

            try {
                this.Inventory = this.NativeShopMenu.inventory;
                this.ShopCurrencyType = this.NativeShopMenu.currency;
                }
            catch (Exception e) {
                Log.Error($"[{nameof(ShopAction)}..ctor] Failed to get native shop data. Exception:\n{e}");
                }
            }

        ~ShopAction() {
            Log.TraceIfD($"[{nameof(ShopAction)}] Finalized for menu = {this.NativeShopMenu}, item = {this.ClickedItem}");
            }

        /// <summary>Gets the size of the stack the action is acting on.</summary>
        public int GetStackAmount() {
            return this.Amount;
            }

        /// <summary>Verifies the conditions to perform the action.</summary>
        public abstract bool CanPerformAction();

        /// <summary>Does the action.</summary>
        /// <param name="amount">Number of items.</param>
        /// <param name="clickLocation">Where the player clicked.</param>
        public abstract void PerformAction(int amount, Point clickLocation);

        /// <summary>Creates an instance of the action.</summary>
        /// <returns>The instance or null if no valid item was selected.</returns>
        public static ShopAction Create() {
            return null;
            }
        }
    }
