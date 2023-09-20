// Originally used for BoneLib
// https://github.com/yowchap/BoneLib/blob/main/BoneLib/BoneLibUpdater/Main.cs

using MelonLoader;

using System;
using System.IO;
using System.Reflection;

using static MelonLoader.MelonLogger;

namespace TideFusionUpdater
{
    public struct TideUpdaterVersion
    {
        public const byte versionMajor = 1;
        public const byte versionMinor = 0;
        public const short versionPatch = 0;
    }

    public class TideUpdaterPlugin : MelonPlugin
    {
        public const string Name = "TideFusion Updater";
        public const string Author = "KitchenBoy08";
        public static readonly Version Version = new Version(TideUpdaterVersion.versionMajor, TideUpdaterVersion.versionMinor, TideUpdaterVersion.versionPatch);

        public static TideUpdaterPlugin Instance { get; private set; }
        public static Instance Logger { get; private set; }
        public static Assembly UpdaterAssembly { get; private set; }

        private MelonPreferences_Category _prefCategory = MelonPreferences.CreateCategory("TideFusionUpdater");
        private MelonPreferences_Entry<bool> _offlineModePref;

        public bool IsOffline => _offlineModePref.Value;

        public const string ModName = "LabFusion";
        public const string PluginName = "TideFusionUpdater";
        public const string FileExtension = ".dll";

        public static readonly string ModAssemblyPath = Path.Combine(MelonHandler.ModsDirectory, $"{ModName}{FileExtension}");
        public static readonly string PluginAssemblyPath = Path.Combine(MelonHandler.PluginsDirectory, $"{PluginName}{FileExtension}");

        public override void OnPreInitialization()
        {
            Instance = this;
            Logger = LoggerInstance;
            UpdaterAssembly = MelonAssembly.Assembly;

            _offlineModePref = _prefCategory.CreateEntry("OfflineMode", false);
            _prefCategory.SaveToFile(false);

            LoggerInstance.Msg(IsOffline ? ConsoleColor.Yellow : ConsoleColor.Green, IsOffline ? "Tide Auto-Updater is OFFLINE." : "Tide Auto-Updater is ONLINE.");

            if (IsOffline) {
                if (!File.Exists(ModAssemblyPath)) {
                    LoggerInstance.Warning($"{ModName}{FileExtension} was not found in the Mods folder!");
                    LoggerInstance.Warning("Download it from the Github or switch to ONLINE mode.");
                    LoggerInstance.Warning("https://github.com/KitchenBoy08/TideFusion/releases");
                }
            }
            else {
                Updater.UpdateMod();
            }
        }

        public override void OnApplicationQuit() {
            Updater.UpdatePlugin();
        }
    }
}