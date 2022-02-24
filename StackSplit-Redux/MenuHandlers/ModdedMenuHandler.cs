using StardewValley.Menus;
using StackSplitRedux.UI;

namespace StackSplitRedux.MenuHandlers
    {
    public class ModdedMenuHandler : BaseMenuHandler<IClickableMenu>
        {

        private readonly ModdedPageHandler ModdedPageHandler = new();

        public ModdedMenuHandler()
            : base() {
            }

        public override void Open(IClickableMenu menu) {
            base.Open(menu);
            this.ModdedPageHandler.Open(menu, this.NativeMenu, this.InvHandler);
            }

        public override void Close() {
            base.Close();
            this.ModdedPageHandler.Close();
            }

        protected override EInputHandled InventoryClicked() {
            var handled = this.ModdedPageHandler.InventoryClicked(out int stackAmount);
            if (handled != EInputHandled.NotHandled)
                this.SplitMenu = new StackSplitMenu(OnStackAmountReceived, stackAmount);
            return handled;
            }

        protected override EInputHandled OpenSplitMenu() {
            var handled = this.ModdedPageHandler.OpenSplitMenu(out int stackAmount);
            if (handled != EInputHandled.NotHandled)
                this.SplitMenu = new StackSplitMenu(OnStackAmountReceived, stackAmount);
            return handled;
            }

        protected override void OnStackAmountReceived(string s) {
            if (int.TryParse(s, out int amount)) {
                this.ModdedPageHandler.OnStackAmountEntered(amount);
                }
            base.OnStackAmountReceived(s);
            }

        protected override void InitInventory() {
            // Do nothing: ModdedPageHandler.Open will init the inventory.
            }

        }
    }
