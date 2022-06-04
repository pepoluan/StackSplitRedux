using System;
using StackSplitRedux.MenuHandlers;

using Microsoft.Xna.Framework;

using StardewModdingAPI;

using StardewValley;
using StardewValley.Menus;

namespace StackSplitRedux
    {
    public class API : IStackSplitAPI
        {
        public readonly static string Version = "0.9.0";

        internal static StackSplit StackSplitRedux;

        public API(StackSplit mod) {
            StackSplitRedux = mod;
            }

        public bool TryRegisterMenu(Type menuType) {
            if (HandlerMapping.TryGetHandler(menuType, out IMenuHandler handler)) {
                HandlerMapping.Add(menuType, handler);
                Log.Debug($"API: Registered {menuType}, handled by {handler.GetType()}");
                return true;
                }
            Log.Error($"API: Don't know how to handle {menuType}, not registered!");
            return false;
            }

        private static Func<IClickableMenu, O> CastFunc<T, O>(Func<T, O> func) where T : IClickableMenu where O : class {
            return menu => menu is T tobj ? func(tobj) : null;
        }

        private static Func<IClickableMenu,A,O> CastFunc<T,A,O>(Func<T, A, O> func) where T : IClickableMenu where O : class {
            return (menu, other) => menu is T tobj ? func(tobj, other) : null;
            }

        public bool RegisterBasicMenu<T>(
            Func<T, InventoryMenu> inventoryGetter,
            Func<T, IReflectedField<Item>> hoveredItemFieldGetter,
            Func<T, IReflectedField<Item>> heldItemFieldGetter,
            Func<T, Point, Tuple<int, Action<bool, int>>> stackChecker
        ) where T : IClickableMenu {
            return RegisterBasicMenu(
                typeof(T),
                CastFunc(inventoryGetter),
                CastFunc(hoveredItemFieldGetter),
                CastFunc(heldItemFieldGetter),
                CastFunc(stackChecker)
            );

            }

        public bool RegisterBasicMenu(
            Type menuType,
            Func<IClickableMenu, InventoryMenu> inventoryGetter,
            Func<IClickableMenu, IReflectedField<Item>> hoveredItemFieldGetter,
            Func<IClickableMenu, IReflectedField<Item>> heldItemFieldGetter,
            Func<IClickableMenu, Point, Tuple<int, Action<bool, int>>> stackChecker
        ) {
            ModdedAdapterMapping.Add(
                menuType,
                new ModdedMenuAdapter(inventoryGetter, hoveredItemFieldGetter, heldItemFieldGetter, stackChecker)
                );

            HandlerMapping.Add(menuType, typeof(ModdedMenuHandler));

            Log.Debug($"API: Registered {menuType} with custom handlers.");

            return true;
            }
        }
    }
