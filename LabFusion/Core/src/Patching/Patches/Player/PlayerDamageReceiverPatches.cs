using System;
using System.Runtime.InteropServices;

using HarmonyLib;

using LabFusion.Network;
using LabFusion.NativeStructs;
using LabFusion.Utilities;
using LabFusion.Data;
using LabFusion.Senders;
using LabFusion.Representation;

using MelonLoader;

using UnityEngine;

using System.Collections;

using Il2CppSLZ.AI;
using Il2CppSLZ.Rig;
using Il2CppSLZ.Marrow.Data;

using Il2Cpp;
using MelonLoader.NativeUtils;
using Il2CppSLZ.Combat;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(PlayerDamageReceiver))]
    public static class PlayerDamageReceiverPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlayerDamageReceiver.ReceiveAttack))]
        public static bool ReceiveAttack(PlayerDamageReceiver __instance, Attack attack)
        {
            try
            {
                if (NetworkInfo.HasServer)
                {
                    var rm = __instance.health._rigManager;

                    // Get the attack and its shooter
                    TriggerRefProxy proxy = attack.proxy;

                    RigManager shooter = null;

                    if (proxy != null && proxy.root != null)
                    {
                        shooter = RigManager.Cache.Get(proxy.root);
                    }

                    // Make sure we have the attacker and attacked
                    if (rm != null && shooter != null)
                    {
                        // Is the attacked person us?
                        if (rm.IsSelf())
                        {
                            // Were we hit by another player?
                            if (PlayerRepManager.TryGetPlayerRep(shooter, out var rep))
                            {
                                FusionPlayer.LastAttacker = rep.PlayerId;

                                // Only allow manual bullet damage
                                if (attack.attackType == AttackType.Piercing)
                                {
                                    return false;
                                }
                            }
                            // Were we hit by ourselves?
                            else
                            {
                                FusionPlayer.LastAttacker = null;
                            }
                        }
                        // Is the attacked person another player? Did we attack them?
                        else if (PlayerRepManager.TryGetPlayerRep(rm, out var rep) && shooter.IsSelf())
                        {
                            // Send the damage over the network
                            PlayerSender.SendPlayerDamage(rep.PlayerId, attack.damage);
                            PlayerSender.SendPlayerAction(PlayerActionType.DEALT_DAMAGE_TO_OTHER_PLAYER, rep.PlayerId);
                        }
                    }
                }
            }
            catch (Exception e)
            {
#if DEBUG
                FusionLogger.LogException("executing patch PlayerDamageReceiver.ReceiveAttack", e);
#endif
            }

            return true;
        }
    }
}
