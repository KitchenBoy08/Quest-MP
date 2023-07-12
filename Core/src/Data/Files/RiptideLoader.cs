using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using LabFusion.Utilities;
using MelonLoader;

namespace LabFusion.Data
{
    public static class RiptideLoader
    {
        public static bool IsRiptideloaded { get; private set; } = false;

        //private static IntPtr _libraryPtr;

        public static void OnLoadRiptide()
        {
            if (IsRiptideloaded) return;
            string _libPath = Path.Combine(MelonUtils.GameDirectory,"Plugins", "RiptideNetworking.dll");
            string _subLibPath = Path.Combine(MelonUtils.GameDirectory, "Plugins", "netstandard.dll");
            //check if file is present, if it is, make riptide loaded and return
            if (File.Exists(_libPath) && File.Exists(_subLibPath))
            {
                IsRiptideloaded = true;
            }
            else
            {
                File.WriteAllBytes(_libPath, EmbeddedResource.LoadFromAssembly(FusionMod.FusionAssembly, ResourcePaths.RiptidePath));
                File.WriteAllBytes(_libPath, EmbeddedResource.LoadFromAssembly(FusionMod.FusionAssembly, ResourcePaths.netstandardPath));
                FusionLogger.Error("Riptide and/or netstandard library was not present, now quitting.");
                UnityEngine.Application.Quit();
            }
                

            //if not, write it to userlibs
            //string libPath = PersistentData.GetPath($"RiptideNetworking.dll");
            
            

            
            /*_libraryPtr = DllTools.LoadLibrary(libPath);

            if ( _libraryPtr != IntPtr.Zero )
            {
                FusionLogger.Log("\"Successfully loaded RiptideNetworking.dll into the application!\"");
                IsRiptideloaded = true;
            }
            else
            {
                uint errorCode = DllTools.GetLastError();
                FusionLogger.Error($"Failed to load RiptideNetworking.dll into the application.\nError Code: {errorCode}");
            }*/
        }

        /*public static void OnFreeRiptide()
        {
            DllTools.FreeLibrary( _libraryPtr );
            IsRiptideloaded = false;
        }*/
    }
}