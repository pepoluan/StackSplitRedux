using System;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Menus;
using StardewModdingAPI;

namespace StackSplitRedux.MenuHandlers
    {
    public interface IModdedMenuAdapter
        {

        InventoryMenu GetInventoryMenu(IClickableMenu menu);

        IReflectedField<Item> GetHoverItemField(IClickableMenu menu);

        IReflectedField<Item> GetHeldItemField(IClickableMenu menu);

        Tuple<int, Action<bool, int>> GetStack(IClickableMenu menu, Point mouse);

        }
    }
