using StardewModdingAPI;

namespace StackSplitRedux
    {
    public static class Log
        {
        public static void Error(string msg) => Mod.Instance.Monitor.Log(msg, LogLevel.Error);
        public static void Warn(string msg) => Mod.Instance.Monitor.Log(msg, LogLevel.Warn);
        public static void Info(string msg) => Mod.Instance.Monitor.Log(msg, LogLevel.Info);
        public static void Debug(string msg) => Mod.Instance.Monitor.Log(msg, LogLevel.Debug);
        public static void Trace(string msg) => Mod.Instance.Monitor.Log(msg, LogLevel.Trace);
        public static void TraceIfD(string msg) {
#if DEBUG
            Trace(msg);
#endif
            }
        }
    }
