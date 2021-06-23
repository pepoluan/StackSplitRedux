using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

using StardewValley;
using StardewValley.Menus;

using SChest = StardewValley.Objects.Chest;

namespace StackSplitRedux.MenuHandlers
    {
    public class CraftingPageHandler : GameMenuPageHandler<CraftingPage>
        {
        /// <summary>Store the click location so it's the same after the user submits the split menu even if they moved their mouse.</summary>
        private Point ClickItemLocation;

        /// <summary>Used to track if the inventory or crafting menu recieved input last.</summary>
        private bool WasInventoryClicked = false;

        /// <summary>Maximum possible amount for crafting/cooking item selected</summary>
        private int MaxAmount;

        /// <summary>Null constructor that currently only invokes the base null constructor</summary>
        public CraftingPageHandler()
            : base() {
            }

        /// <summary>Initializes the inventory using the most common variable names.</summary>
        public override void InitInventory() {
            // We need to do this explicitly because the crafting page uses a different variable name for hover item.
            var inventoryMenu = this.MenuPage.GetType().GetField("inventory").GetValue(this.MenuPage) as InventoryMenu;
            var hoveredItemField = Mod.Reflection.GetField<Item>(this.MenuPage, "hoverItem");

            this.Inventory.Init(inventoryMenu, hoveredItemField);
            }

        /// <summary>Tells the handler that the inventory was shift-clicked.</summary>
        /// <param name="stackAmount">The default stack amount to display in the split menu.</param>
        /// <returns>If the input was handled or consumed. Generally returns not handled if an invalid item was selected.</returns>
        public override EInputHandled InventoryClicked(out int stackAmount) {
            this.WasInventoryClicked = true;
            return base.InventoryClicked(out stackAmount);
            }

        /// <summary>Tells the handler that the interface recieved the hotkey input.</summary>
        /// <param name="stackAmount">The default stack amount to display in the split menu.</param>
        /// <returns>If the input was handled or consumed. Generally returns not handled if an invalid item was selected.</returns>
        public override EInputHandled OpenSplitMenu(out int stackAmount) {
            stackAmount = Mod.Config.DefaultCraftingAmount;
            this.WasInventoryClicked = false;

            var hoverRecipe = Mod.Reflection.GetField<CraftingRecipe>(this.MenuPage, "hoverRecipe").GetValue();
            var hoveredItem = hoverRecipe?.createItem();
            var heldItem = Mod.Reflection.GetField<Item>(this.MenuPage, "heldItem").GetValue();
            var cooking = Mod.Reflection.GetField<bool>(this.MenuPage, "cooking").GetValue();

            // If we're holding an item already then it must stack with the item we want to craft.
            if (hoveredItem == null || (heldItem != null && heldItem.Name != hoveredItem.Name))
                return EInputHandled.NotHandled;

            // Grab ingredients
            // Because of Workbenches + kitchen stove superpower, we need to look at many places, not just backpack
            //IEnumerable<Item> extraItems = null;
            //if (cooking) {
            //    extraItems = GetFridgeItems();
            //    }
            //else {  // Workbench + connected chests
            //    // No need to search/filter for "chests only in this location"; that should already be done when
            //    // populating _materialContainers (either by game code or by mods such as "Better Workbenches"
            //    extraItems = this.MenuPage._materialContainers?.SelectMany(chest => chest.items);
            //    }
#if DEBUG
            List<Item> extraItems = new();
            foreach (SChest chest in this.MenuPage._materialContainers) {
                Log.TraceIfD($"Engrabbening {chest} @ {chest.TileLocation} (fridge = {chest.fridge.Value})");
                extraItems.AddRange(chest.items);
                }
#else
            List<Item> extraItems = this.MenuPage._materialContainers?.SelectMany(chest => chest.items).ToList();
#endif

            // Only allow items that can actually stack
            if (!hoveredItem.canStackWith(hoveredItem) || !hoverRecipe.doesFarmerHaveIngredientsInInventory(extraItems))
                return EInputHandled.NotHandled;

            this.MaxAmount = hoverRecipe.getCraftableCount(extraItems);
            this.ClickItemLocation = new Point(Game1.getOldMouseX(true), Game1.getOldMouseY(true));
            return EInputHandled.Consumed;
            }

        ///// <summary>
        ///// Retrieve all items in fridges, minifridges, and if EternalSoap.RemoteFridgeStorage is loaded,
        ///// from ... all those connected storageboxen
        ///// </summary>
        ///// <returns></returns>
        //private IEnumerable<Item> GetFridgeItems() {
        //    List<Item> result = new();
        //    var curloc = Game1.player.currentLocation;
        //    // Careful not to merge the fridges on FarmHouse and IslandFarmHouse
        //    SLocation house;
        //    if (curloc is FarmHouse farmhouse) {
        //        result.AddRange(farmhouse.fridge.Value.items);
        //        house = farmhouse;
        //        }
        //    else if (curloc is IslandFarmHouse islafarmhouse) {
        //        result.AddRange(islafarmhouse.fridge.Value.items);
        //        house = islafarmhouse;
        //        }
        //    else {
        //        return null;
        //        }
        //    if (Mod.Registry.IsLoaded("EternalSoap.RemoteFridgeStorage")) {
        //        // code by blueberry 🍰 @rhubarb#4755 on Discord
        //        // https://discord.com/channels/137344473976799233/156109690059751424/856079674970865685
        //        Type modRFS = Type.GetType("RemoteFridgeStorage.ModEntry, RemoteFridgeStorage");
        //        object instance = modRFS.GetProperty("Instance",
        //          bindingAttr: BindingFlags.Public | BindingFlags.Static)
        //          .GetValue(null);
        //        object field = modRFS.GetField("ChestController")
        //          .GetValue(instance);
        //        HashSet<SChest> chests = field.GetType().InvokeMember("GetChests",
        //          invokeAttr: BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance,
        //          binder: null,
        //          target: field,
        //          args: null) as HashSet<SChest>;
        //        result.AddRange(chests.SelectMany(c => c.items));
        //        }
        //    else {
        //        result.AddRange(
        //            house.Objects.Values.OfType<SChest>()
        //                .Where(c => c.bigCraftable.Value && c.ParentSheetIndex == 216)  // Magic spell to select mini-fridges
        //                .SelectMany(chest => chest.items)
        //            );
        //        }
        //    return result;
        //    }

        /// <summary>Lets the handler run the logic for doing the split after the amount has been input.</summary>
        /// <param name="amount">The stack size the user requested.</param>
        public override void OnStackAmountEntered(int amount) {
            // Run regular inventory logic if that's what was clicked.
            if (this.WasInventoryClicked) {
                base.OnStackAmountEntered(amount);
                return;
                }

            int count = Math.Min(amount, this.MaxAmount);
            ISoundBank origSoundBank = Game1.soundBank;
            for (int i = 0; i < count; ++i) {
                // Only play sound for the very first RightClick, or else the sound will mix together and sounds horrible
                if (i > 0) Game1.soundBank = null;
                this.MenuPage.receiveRightClick(this.ClickItemLocation.X, this.ClickItemLocation.Y, playSound: i == 0);
                Game1.soundBank = origSoundBank;
                // NOTE: This nullify-then-restore tactic is needed because as of SDV 1.5.4, CraftingPage.receiveRightClick actually
                //       *ignores* the playSound parameter; it's supposed to pass that parameter to CraftingPage.clickCraftingRecipe,
                //       but it doesn't. So the same sound gets layered one atop another with a slight shift, resulting in an overly
                //       loud and very distorted blip.
                //       If this oversight is fixed in a future patch, we can remove the nullify-and-restore lines.
                }
            }
        }
    }
