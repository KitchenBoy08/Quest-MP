using Il2CppSystem;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using Il2CppSLZ.UI;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Utilities;

namespace LabFusion.Patching
{

    [HarmonyPatch(typeof(PopUpMenuView), nameof(PopUpMenuView.AddDevMenu))]
    public static class AddDevMenuPatch
    {
        public static void Prefix(PopUpMenuView __instance, ref Il2CppSystem.Action spawnDelegate)
        {
            spawnDelegate += (Il2CppSystem.Action)(() => { OnSpawnDelegate(__instance); });
        }

        public static void OnSpawnDelegate(PopUpMenuView __instance)
        {
            if (NetworkInfo.HasServer && !NetworkInfo.IsServer && RigData.RigReferences.RigManager && RigData.RigReferences.RigManager.uiRig.popUpMenu == __instance)
            {
                var transform = new SerializedTransform(__instance.radialPageView.transform);
                PooleeUtilities.RequestSpawn(__instance.crate_SpawnGun.Barcode, transform);
                PooleeUtilities.RequestSpawn(__instance.crate_Nimbus.Barcode, transform);
            }
        }
    }
}
