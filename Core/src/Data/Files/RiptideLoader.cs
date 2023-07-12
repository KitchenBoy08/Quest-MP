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
            string _libPath = Path.Combine(MelonUtils.UserLibsDirectory, "RiptideNetowrking.dll");
            //check if file is present, if it is, make riptide loaded and return
            if (File.Exists(_libPath) )
            {
                IsRiptideloaded = true;
            }
            else
            {
                File.WriteAllBytes(_libPath, EmbeddedResource.LoadFromAssembly(FusionMod.FusionAssembly, ResourcePaths.RiptidePath));
                FusionLogger.Error("Riptide library was not present, now quitting.");
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