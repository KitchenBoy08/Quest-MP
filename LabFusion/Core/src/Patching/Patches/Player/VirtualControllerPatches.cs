using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BoneLib;
using HarmonyLib;

using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.NativeStructs;

using MelonLoader;

using Il2CppSLZ;
using Il2CppSLZ.Combat;
using Il2CppSLZ.Interaction;
using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Utilities;

using UnityEngine;
using LabFusion.Utilities;
using LabFusion.Data;
using MelonLoader.NativeUtils;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(VirtualController))]
    public static class VirtualControllerPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(VirtualController.CheckHandDesync))]
        public static bool CheckHandDesync(HandGripPair pair, SimpleTransform contHandle, SimpleTransform rigHandle, ref bool __result)
        {
            try
            {
                if (NetworkInfo.HasServer)
                {
                    if (PlayerRepManager.HasPlayerId(pair.hand.manager))
                    {
                        __result = false;
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
#if DEBUG
                FusionLogger.LogException("executing patch VirtualController.CheckHandDesync", e);
#endif
            }

            return true;
        }
    }
}
