// Originally used for BoneLib
// https://github.com/yowchap/BoneLib/blob/main/BoneLib/BoneLibUpdater/Updater.cs

using MelonLoader;

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace TideFusionUpdater
{
    internal static class Updater
    {
        private static readonly string _dataDir = Path.Combine(MelonUtils.UserDataDirectory, $"{TideUpdaterPlugin.PluginName}");
        private static readonly string _updaterAppName = "updater.exe";

        private static bool pluginNeedsUpdating = false;

        public static void UpdateMod()
        {
            // Check for local version of mod and read version if it exists
            Version localVersion = new Version(0, 0, 0);
            if (File.Exists(TideUpdaterPlugin.ModAssemblyPath))
            {
                AssemblyName localAssemblyInfo = AssemblyName.GetAssemblyName(TideUpdaterPlugin.ModAssemblyPath);
                localVersion = new Version(localAssemblyInfo.Version.Major, localAssemblyInfo.Version.Minor, localAssemblyInfo.Version.Build); // Remaking the object so there's no 4th number
                TideUpdaterPlugin.Logger.Msg($"{TideUpdaterPlugin.ModName}{TideUpdaterPlugin.FileExtension} found in Mods folder. Version: {localVersion}");
            }

            try
            {
                Directory.CreateDirectory(_dataDir);
                string updaterScriptPath = Path.Combine(_dataDir, _updaterAppName);

                Assembly assembly = TideUpdaterPlugin.UpdaterAssembly;
                string resourceName = assembly.GetManifestResourceNames().First(x => x.Contains(_updaterAppName));
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    using (FileStream fileStream = File.Create(updaterScriptPath))
                        stream.CopyTo(fileStream);
                }

                Process process = new Process();
                process.StartInfo.FileName = updaterScriptPath;
                process.StartInfo.Arguments = $"{localVersion} \"{TideUpdaterPlugin.ModAssemblyPath}\" \"{TideUpdaterPlugin.PluginAssemblyPath}\" \"false\"";
                process.Start();
                process.WaitForExit();
                ExitCode code = (ExitCode)process.ExitCode;

                switch (code)
                {
                    case ExitCode.Success:
                        TideUpdaterPlugin.Instance.LoggerInstance.Msg($"{TideUpdaterPlugin.ModName}{TideUpdaterPlugin.FileExtension} updated successfully!");
                        pluginNeedsUpdating = true;
                        break;
                    case ExitCode.UpToDate:
                        TideUpdaterPlugin.Instance.LoggerInstance.Msg($"{TideUpdaterPlugin.ModName}{TideUpdaterPlugin.FileExtension} is already up to date.");
                        break;
                    case ExitCode.Error:
                        TideUpdaterPlugin.Instance.LoggerInstance.Error($"{TideUpdaterPlugin.ModName}{TideUpdaterPlugin.FileExtension} failed to update!");
                        break;
                }
            }
            catch (Exception e)
            {
                TideUpdaterPlugin.Logger.Error($"Exception caught while running {TideUpdaterPlugin.ModName} updater!");
                TideUpdaterPlugin.Logger.Error(e.ToString());
            }
        }

        public static void UpdatePlugin()
        {
            if (pluginNeedsUpdating)
            {
                Directory.CreateDirectory(_dataDir);
                string updaterScriptPath = Path.Combine(_dataDir, _updaterAppName);

                Assembly assembly = Assembly.GetExecutingAssembly();
                string resourceName = assembly.GetManifestResourceNames().First(x => x.Contains(_updaterAppName));
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    using (FileStream fileStream = File.Create(updaterScriptPath))
                        stream.CopyTo(fileStream);
                }

                Process process = new Process();
                process.StartInfo.FileName = updaterScriptPath;
                process.StartInfo.Arguments = $"{new Version(0, 0, 0)} \"{TideUpdaterPlugin.ModAssemblyPath}\" \"{TideUpdaterPlugin.PluginAssemblyPath}\" true";
                process.Start();
            }
        }
    }

    enum ExitCode
    {
        Success = 0,
        UpToDate = 1,
        Error = 2
    }
}