namespace StackSplitRedux
    {
    internal class Mod : StardewModdingAPI.Mod
        {
        internal static StardewModdingAPI.Mod Instance;
        internal static StardewModdingAPI.ITranslationHelper I18n { get => Instance.Helper.Translation; }
        internal static StardewModdingAPI.IReflectionHelper Reflection { get => Instance.Helper.Reflection; }
        internal static StardewModdingAPI.IInputHelper Input { get => Instance.Helper.Input; }
        internal static StardewModdingAPI.Events.IModEvents Events { get => Instance.Helper.Events; }
        internal static StardewModdingAPI.IModRegistry Registry { get => Instance.Helper.ModRegistry; }

        private static StackSplit StackSplitRedux;

        private const string OLD_STACKSPLITX = "tstaples.StackSplitX";

        public override void Entry(StardewModdingAPI.IModHelper helper) {
            Mod.Instance = this;

            if (DetectConflict()) return;

            Log.Info($"{this.ModManifest.UniqueID} version {typeof(Mod).Assembly.GetName().Version} (API version {API.Version}) is loading...");
            Mod.StackSplitRedux = new();
            }

        public override object GetApi() {
            return new API(Mod.StackSplitRedux);
            }

        public bool DetectConflict() {
            if (this.Helper.ModRegistry.IsLoaded(OLD_STACKSPLITX)) {
                Log.Error("Old StackSplitX mod detected!");
                Log.Error("Will abort loading this mod to prevent conflict/crashes!");
                return true;
                }
            return false;
            }
        }
    }
