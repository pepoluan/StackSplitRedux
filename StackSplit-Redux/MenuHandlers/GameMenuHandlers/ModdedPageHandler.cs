using System;

using Microsoft.Xna.Framework;

using StardewValley;
using StardewValley.Menus;


namespace StackSplitRedux.MenuHandlers
    {
    public class ModdedPageHandler : GameMenuPageHandler<IClickableMenu>
        {

        private Action<bool, int> StackCallback;

        private IModdedMenuAdapter ModdedHandler;

        private bool WasInventoryClicked = false;

        public ModdedPageHandler()
            : base() {
            }

        public override void Open(IClickableMenu menu, IClickableMenu page, InventoryHandler inventoryHandler) {

            ModdedAdapterMapping.TryGetAdapter(page.GetType(), out ModdedHandler);
            HasInventory = ModdedHandler?.GetInventoryMenu(page) != null;

            base.Open(menu, page, inventoryHandler);
            }

        public override void InitInventory() {
            try {
                var inventoryMenu = ModdedHandler?.GetInventoryMenu(MenuPage);
                var hoverItem = ModdedHandler?.GetHoverItemField(MenuPage);
                var heldItem = ModdedHandler?.GetHeldItemField(MenuPage);

                this.InventoryHandler.Init(inventoryMenu, hoverItem, heldItem);
                }
            catch (Exception ex) {
                Log.Error($"[{nameof(ModdedPageHandler)}:{MenuPage.GetType().Name}.{nameof(InitInventory)}] Failed to initialize the inventory handler:\n{ex}");
                }
            }

        public override EInputHandled InventoryClicked(out int stackAmount) {
            WasInventoryClicked = true;
            return base.InventoryClicked(out stackAmount);
            }

        public override EInputHandled OpenSplitMenu(out int stackAmount) {
            this.WasInventoryClicked = false;

            var wanted = ModdedHandler?.GetStack(MenuPage, new Point(Game1.getOldMouseX(true), Game1.getOldMouseY(true)));

            if (wanted != null) {
                stackAmount = wanted.Item1;
                StackCallback = wanted.Item2;
                return EInputHandled.Consumed;
                }

            stackAmount = 0;
            return EInputHandled.NotHandled;
            }

        public override void OnStackAmountEntered(int amount) {
            if (this.WasInventoryClicked) {
                base.OnStackAmountEntered(amount);
                return;
                }

            StackCallback?.Invoke(true, amount);
            StackCallback = null;
            }
        }
    }
