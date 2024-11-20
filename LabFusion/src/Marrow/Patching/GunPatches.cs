﻿using HarmonyLib;

using LabFusion.Network;
using LabFusion.Utilities;
using LabFusion.Player;
using LabFusion.Entities;

using Il2CppSLZ.Marrow;

namespace LabFusion.Marrow.Patching;

[HarmonyPatch(typeof(Gun))]
public static class GunPatches
{
    public static bool IgnorePatches { get; set; } = false;

    [HarmonyPatch(nameof(Gun.OnTriggerGripAttached))]
    [HarmonyPostfix]
    public static void OnTriggerGripAttached(Gun __instance, Hand hand)
    {
        // Make sure we're in a server first
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        // If this isn't our RigManager, then we should provide the Gun with the NetworkAmmoInventory instead
        if (!hand.manager.IsLocalPlayer())
        {
            __instance._ammoInventory = NetworkGunManager.NetworkAmmoInventory;
        }
    }

    [HarmonyPatch(nameof(Gun.Fire))]
    [HarmonyPrefix]
    public static bool Fire(Gun __instance)
    {
        if (IgnorePatches)
        {
            return true;
        }

        if (!NetworkInfo.HasServer)
        {
            return true;
        }

        var grip = __instance.triggerGrip;

        if (grip == null)
        {
            return true;
        }

        var hand = grip.GetHand();

        if (hand == null) 
        {
            return true;
        }

        var manager = hand.manager;

        bool isPlayerRep = NetworkPlayerManager.HasExternalPlayer(manager);

        if (isPlayerRep && __instance.cartridgeState == Gun.CartridgeStates.UNSPENT)
        {
            return false;
        }

        var health = manager.health.TryCast<Player_Health>();

        bool isDead = health.deathIsImminent;

        if (isDead)
        {
            return false;
        }

        return true;
    }

    [HarmonyPatch(nameof(Gun.OnFire))]
    [HarmonyPrefix]
    public static void OnFire(Gun __instance)
    {
        if (IgnorePatches)
        {
            return;
        }

        if (!NetworkInfo.HasServer)
        {
            return;
        }

        var gunEntity = GunExtender.Cache.Get(__instance);

        if (gunEntity == null)
        {
            return;
        }

        var gunExtender = gunEntity.GetExtender<GunExtender>();

        try
        {
            // Make sure this is being grabbed by our main player
            if (__instance.triggerGrip && __instance.triggerGrip.attachedHands.Find((Il2CppSystem.Predicate<Hand>)((h) => h.manager.IsLocalPlayer())))
            {
                using var writer = FusionWriter.Create(GunShotData.Size);
                var ammoCount = __instance._magState != null ? (byte)__instance._magState.AmmoCount : (byte)0;

                var data = GunShotData.Create(PlayerIdManager.LocalSmallId, ammoCount, gunEntity.Id, (byte)gunExtender.GetIndex(__instance).Value);
                writer.Write(data);

                using var message = FusionMessage.Create(NativeMessageTag.GunShot, writer);
                MessageSender.SendToServer(NetworkChannel.Reliable, message);
            }
        }
        catch (Exception e)
        {
            FusionLogger.LogException("patching Gun.OnFire", e);
        }
    }
}