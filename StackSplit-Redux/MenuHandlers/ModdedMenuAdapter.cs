using System;

using Microsoft.Xna.Framework;

using StardewModdingAPI;

using StardewValley;
using StardewValley.Menus;

namespace StackSplitRedux.MenuHandlers
    {
    public class ModdedMenuAdapter : IModdedMenuAdapter
        {

        private readonly Func<IClickableMenu, InventoryMenu> InventoryGetter;
        private readonly Func<IClickableMenu, IReflectedField<Item>> HoverItemFieldGetter;
        private readonly Func<IClickableMenu, IReflectedField<Item>> HeldItemFieldGetter;
        private readonly Func<IClickableMenu, Point, Tuple<int, Action<bool, int>>> StackChecker;

        public ModdedMenuAdapter(
            Func<IClickableMenu, InventoryMenu> inventoryGetter,
            Func<IClickableMenu, IReflectedField<Item>> hoverItemFieldGetter,
            Func<IClickableMenu, IReflectedField<Item>> heldItemFieldGetter,
            Func<IClickableMenu, Point, Tuple<int, Action<bool, int>>> stackChecker)
            {
            this.InventoryGetter = inventoryGetter;
            this.HoverItemFieldGetter = hoverItemFieldGetter;
            this.HeldItemFieldGetter = heldItemFieldGetter;
            this.StackChecker = stackChecker;
            }

        public IReflectedField<Item> GetHoverItemField(IClickableMenu menu) {
            return HoverItemFieldGetter?.Invoke(menu);
            }

        public IReflectedField<Item> GetHeldItemField(IClickableMenu menu) {
            return HeldItemFieldGetter?.Invoke(menu);
            }

        public InventoryMenu GetInventoryMenu(IClickableMenu menu) {
            return InventoryGetter?.Invoke(menu);
            }

        public Tuple<int, Action<bool, int>> GetStack(IClickableMenu menu, Point mouse) {
            return StackChecker?.Invoke(menu, mouse);
            }
        }
    }
