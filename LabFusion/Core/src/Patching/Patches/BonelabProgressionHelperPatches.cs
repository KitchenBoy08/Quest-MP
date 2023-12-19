using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using LabFusion.Network;
using LabFusion.Utilities;

using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.SaveData;

using UnityEngine;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(BonelabProgressionHelper))]
    public static class BonelabProgressionHelperPatches
    {
        [HarmonyPatch(nameof(BonelabProgressionHelper.RestoreInventory))]
        [HarmonyPatch(new Type[]
        {
            typeof(PlayerProgression),
            typeof(string),
            typeof(bool),
            typeof(Transform),
            typeof(Transform),
            typeof(Il2CppSystem.Func<Barcode, Barcode>),
            typeof(Il2CppStringArray)
        })]
        [HarmonyPrefix]
        public static bool RestoreInventory(PlayerProgression progression, string levelKey, bool freshLoad, Transform leftHand, Transform rightHand, Il2CppSystem.Func<Barcode, Barcode> itemFilter, Il2CppStringArray priorLevels)
        {
            // Temporary fix
            // Eventually replace with syncing spawned inventory
            if (NetworkInfo.HasServer)
                return false;

            return true;
        }
    }
}
