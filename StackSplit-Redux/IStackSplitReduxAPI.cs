using System;

using Microsoft.Xna.Framework;

using StardewValley;
using StardewValley.Menus;

using StardewModdingAPI;

namespace StackSplitRedux
    {
    public interface IStackSplitAPI
        {
        bool TryRegisterMenu(Type menuType);

        bool RegisterBasicMenu(
            Type menuType,
            Func<IClickableMenu, InventoryMenu> inventoryGetter,
            Func<IClickableMenu, IReflectedField<Item>> hoveredItemFieldGetter,
            Func<IClickableMenu, IReflectedField<Item>> heldItemFieldGetter,
            Func<IClickableMenu, Point, Tuple<int, Action<bool, int>>> stackChecker
        );

        }
    }
