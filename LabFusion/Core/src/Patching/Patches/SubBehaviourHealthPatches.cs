using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using LabFusion.NativeStructs;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Syncables;
using LabFusion.Utilities;
using MelonLoader;

using Il2CppPuppetMasta;

using Il2CppSLZ;
using MelonLoader.NativeUtils;
using Il2CppSLZ.Combat;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(SubBehaviourHealth))]
    public static class SubBehaviourHealthPatches
    {
        [HarmonyPatch(nameof(SubBehaviourHealth.TakeDamage))]
        [HarmonyPrefix]
        public static bool TakeDamage(SubBehaviourHealth __instance, int m, Attack attack)
        {
            try
            {
                if (NetworkInfo.HasServer)
                {
                    if (PuppetMasterExtender.Cache.TryGet(__instance.behaviour.puppetMaster, out var syncable) && !syncable.IsOwner())
                    {
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
#if DEBUG
                FusionLogger.LogException("executing patch SubBehaviourHealth.TakeDamage", e);
#endif
            }

            return true;
        }
    }
}
