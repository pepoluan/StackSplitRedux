using System;
using System.Collections.Generic;
using StackSplitRedux.MenuHandlers;
using StardewModdingAPI;

namespace StackSplitRedux
    {
    internal static class ModdedAdapterMapping
        {
        private static readonly Dictionary<Type, IModdedMenuAdapter> AdapterByType = new();

        internal static void Add(Type menuType, IModdedMenuAdapter adapter) {
            if (AdapterByType.ContainsKey(menuType))
                Log.Warn($"Redefining adapter for {menuType}");
            AdapterByType[menuType] = adapter;
            }

        internal static bool TryGetAdapter(Type menuType, out IModdedMenuAdapter adapter) {
            if (AdapterByType.TryGetValue(menuType, out adapter)) return true;
            foreach (var kvp in AdapterByType) {
                if (menuType.IsSubclassOf(kvp.Key)) {
                    adapter = kvp.Value;
                    return true;
                    }
                }
            return false;
            }

        }

    }
