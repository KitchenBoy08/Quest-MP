using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using Il2CppSLZ.Props;

using UnityEngine;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(DevManipulatorGun))]
    public static class DevManipulatorPatches
    {
        [HarmonyPatch(nameof(DevManipulatorGun.Awake))]
        [HarmonyPostfix]
        public static void Awake(DevManipulatorGun __instance)
        {
            // Add players to layer mask
            var job = __instance.gravityManipulator;
            job.pullableLayerMask |= (1 << LayerMask.NameToLayer("Player"));
        }
    }
}
