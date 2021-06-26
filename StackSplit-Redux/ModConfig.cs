using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackSplitRedux
    {
    public class ModConfig
        {
        public int DefaultCraftingAmount { get; set; } = 1;
        public int DefaultShopAmount { get; set; } = 5;
        public bool DebuggingMode { get; set; } = false;
        }

    /// <summary>
    /// This class containe "tunables" that should not be user-editable
    /// </summary>
    /// <remarks>Gathered here so we can tune the mod from one place, instead of hunting down config knobs everywhere</remarks>
    internal static class StaticConfig
        {
        internal readonly static SButton[] ModifierKeys = new[] { SButton.LeftShift, SButton.RightShift };
        internal readonly static int SplitMenuOpenDelayTicks = 2;
        internal readonly static string[] ConflictingMods = new [] { "tstaples.StackSplitX" };
        }
    }
