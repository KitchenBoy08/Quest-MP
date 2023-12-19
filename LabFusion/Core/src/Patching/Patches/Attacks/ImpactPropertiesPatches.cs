using System;
using System.Runtime.InteropServices;
using System.Collections;

using HarmonyLib;

using LabFusion.Network;
using LabFusion.NativeStructs;
using LabFusion.Utilities;
using LabFusion.Data;
using LabFusion.Senders;

using MelonLoader;

using Il2CppSLZ.Combat;
using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.AI;

using UnityEngine;
using MelonLoader.NativeUtils;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(ImpactProperties))]
    public static class ImpactPropertiesPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(ImpactProperties.ReceiveAttack))]
        public static bool ReceiveAttack(Attack attack)
        {
            try
            {
                if (NetworkInfo.HasServer)
                {
                    unsafe
                    {
                        Collider collider = attack.collider;
                        TriggerRefProxy proxy = attack.proxy;

                        // Check if this was a bullet attack + it was us who shot the bullet
                        if (proxy == RigData.RigReferences.Proxy && attack.attackType == AttackType.Piercing)
                        {
                            var rb = collider.attachedRigidbody;
                            if (!rb)
                                return false;

                            ImpactUtilities.OnHitRigidbody(rb);
                        }
                    }
                }
            }
            catch (Exception e)
            {
#if DEBUG
                FusionLogger.LogException("executing patch ImpactProperties.ReceiveAttack", e);
#endif
            }

            return true;
        }

        private static IEnumerator CoWaitAndSync(Rigidbody rb)
        {
            for (var i = 0; i < 4; i++)
                yield return null;

            PropSender.SendPropCreation(rb.gameObject);
        }
    }
}
