using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;

using SLZ.Bonelab;
using SLZ.Marrow.SceneStreaming;
using SLZ.Marrow.Warehouse;

using UnityEngine;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(DoorPortalController), "OnTriggerEnter")]
    public class DoorPortalPatch {
        public static bool Prefix(Collider other) {
            if (other.CompareTag("Player")) {
                return TriggerUtilities.IsMainRig(other);
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(SceneStreamer))]
    public class SceneLoadPatch {
        public static bool IgnorePatches = false;

        [HarmonyPatch(nameof(SceneStreamer.Reload))]
        [HarmonyPrefix]
        public static bool Reload() {
            // TideFusion Specific
            FusionPermissions.FetchPermissionLevel(PlayerIdManager.LocalLongId, out PermissionLevel permLevel, out Color colorLevel);

            // Check if we need to exit early
            if (!IgnorePatches && NetworkInfo.HasServer && !NetworkInfo.IsServer || RiptideNetworkLayer.CurrentServerType.GetType() == ServerTypes.DEDICATED && permLevel != PermissionLevel.OWNER && NetworkInfo.IsServer) {
                return false;
            }

            return true;
        }

        [HarmonyPatch(nameof(SceneStreamer.Load), typeof(string), typeof(string))]
        [HarmonyPrefix]
        public static bool StringLoad(string levelBarcode, string loadLevelBarcode = "") {
            // TideFusion Specific
            FusionPermissions.FetchPermissionLevel(PlayerIdManager.LocalLongId, out PermissionLevel permLevel, out Color colorLevel);

            // Check if we need to exit early
            if (!IgnorePatches && NetworkInfo.HasServer && !NetworkInfo.IsServer || RiptideNetworkLayer.CurrentServerType.GetType() == ServerTypes.DEDICATED && permLevel != PermissionLevel.OWNER && NetworkInfo.IsServer) {
                return false;
            }

            return true;
        }

        [HarmonyPatch(nameof(SceneStreamer.Load), typeof(LevelCrateReference), typeof(LevelCrateReference))]
        [HarmonyPrefix]
        public static bool CrateLoad(LevelCrateReference level, LevelCrateReference loadLevel) {
            try {
                // TideFusion Specific
                FusionPermissions.FetchPermissionLevel(PlayerIdManager.LocalLongId, out PermissionLevel permLevel, out Color colorLevel);

                // Check if we need to exit early
                if (!IgnorePatches && NetworkInfo.HasServer && !NetworkInfo.IsServer || RiptideNetworkLayer.CurrentServerType.GetType() == ServerTypes.DEDICATED && permLevel != PermissionLevel.OWNER && NetworkInfo.IsServer) {
                    return false;
                }
            }

            catch (Exception e)
            {
#if DEBUG
                FusionLogger.LogException("to execute patch SceneStreamer.Load", e);
#endif
            }

            return true;
        }
    }
}
