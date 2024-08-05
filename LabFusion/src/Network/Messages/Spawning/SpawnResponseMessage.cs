﻿using LabFusion.Data;
using LabFusion.Player;
using LabFusion.Utilities;

using Il2CppSLZ.Marrow.Pool;
using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Interaction;

using LabFusion.Extensions;

using LabFusion.Exceptions;
using LabFusion.Senders;
using LabFusion.RPC;
using LabFusion.Marrow;
using LabFusion.Entities;
using LabFusion.Downloading;
using LabFusion.Downloading.ModIO;

namespace LabFusion.Network;

public class SpawnResponseData : IFusionSerializable
{
    public const int DefaultSize = sizeof(byte) * 2 + sizeof(ushort) + SerializedTransform.Size;

    public byte owner;
    public string barcode;
    public ushort entityId;

    public SerializedTransform serializedTransform;

    public uint trackerId;

    public static int GetSize(string barcode)
    {
        return DefaultSize + barcode.GetSize();
    }

    public void Serialize(FusionWriter writer)
    {
        writer.Write(owner);
        writer.Write(barcode);
        writer.Write(entityId);
        writer.Write(serializedTransform);

        writer.Write(trackerId);
    }

    public void Deserialize(FusionReader reader)
    {
        owner = reader.ReadByte();
        barcode = reader.ReadString();
        entityId = reader.ReadUInt16();
        serializedTransform = reader.ReadFusionSerializable<SerializedTransform>();

        trackerId = reader.ReadUInt32();
    }

    public static SpawnResponseData Create(byte owner, string barcode, ushort entityId, SerializedTransform serializedTransform, uint trackerId = 0)
    {
        return new SpawnResponseData()
        {
            owner = owner,
            barcode = barcode,
            entityId = entityId,
            serializedTransform = serializedTransform,
            trackerId = trackerId,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class SpawnResponseMessage : FusionMessageHandler
{
    public override byte Tag => NativeMessageTag.SpawnResponse;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        if (isServerHandled)
        {
            throw new ExpectedClientException();
        }

        using var reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<SpawnResponseData>();

        byte owner = data.owner;
        string barcode = data.barcode;
        ushort entityId = data.entityId;
        var trackerId = data.trackerId;

        bool hasCrate = CrateFilterer.HasCrate<SpawnableCrate>(new(barcode));

        if (!hasCrate)
        {
            // TODO: implement
            bool shouldDownload = true;

            // Check if we should download the mod (it's not blacklisted, mod downloading disabled, etc.)
            if (!shouldDownload)
            {
                return;
            }

            NetworkModRequester.RequestMod(new NetworkModRequester.ModRequestInfo()
            {
                target = owner,
                barcode = barcode,
                modCallback = OnModInfoReceived,
            });

            void OnModInfoReceived(NetworkModRequester.ModCallbackInfo info)
            {
                if (!info.hasFile)
                {
                    return;
                }

                ModIODownloader.EnqueueDownload(new ModTransaction()
                {
                    modFile = info.modFile,
                    temporary = true,
                    callback = OnModDownloaded,
                });
            }

            void OnModDownloaded(DownloadCallbackInfo info)
            {
                if (info.result == ModResult.FAILED)
                {
                    FusionLogger.Warn($"Failed downloading spawnable {barcode}!");
                    return;
                }

                BeginSpawn();
            }

            return;
        }

        BeginSpawn();

        void BeginSpawn()
        {
            var crateRef = new SpawnableCrateReference(barcode);

            var spawnable = new Spawnable()
            {
                crateRef = crateRef,
                policyData = null
            };

            AssetSpawner.Register(spawnable);

            void OnPooleeSpawned(Poolee go)
            {
                OnSpawnFinished(owner, barcode, entityId, go, trackerId);
            }

            SafeAssetSpawner.Spawn(spawnable, data.serializedTransform.position, data.serializedTransform.rotation, OnPooleeSpawned);
        }
    }

    public static void OnSpawnFinished(byte owner, string barcode, ushort entityId, Poolee poolee, uint trackerId = 0)
    {
        // The poolee will never be null, so we don't have to check for it
        // Only case where it could be null is the object not spawning, but the spawn callback only executes when it exists
        var go = poolee.gameObject;

        // Remove the existing entity on this poolee if it exists
        if (PooleeExtender.Cache.TryGet(poolee, out var conflictingEntity))
        {
            FusionLogger.Warn($"Unregistered entity {conflictingEntity.Id} on poolee {poolee.name} due to conflicting id.");

            NetworkEntityManager.IdManager.UnregisterEntity(conflictingEntity);
        }

        NetworkEntity newEntity = null;

        // Get the marrow entity on the spawned object
        var marrowEntity = MarrowEntity.Cache.Get(go);

        // Make sure we have a marrow entity before creating a prop
        if (marrowEntity != null)
        {
            // Create a network entity
            newEntity = new();
            newEntity.SetOwner(PlayerIdManager.GetPlayerId(owner));

            // Setup a network prop
            NetworkProp newProp = new(newEntity, marrowEntity);

            // Register this entity
            NetworkEntityManager.IdManager.RegisterEntity(entityId, newEntity);

            // Insert the catchup hook for future users
            newEntity.OnEntityCatchup += (entity, player) =>
            {
                SpawnSender.SendCatchupSpawn(owner, barcode, entityId, new SerializedTransform(go.transform), player);
            };
        }

        // Invoke spawn callback
        if (owner == PlayerIdManager.LocalSmallId)
        {
            NetworkAssetSpawner.OnSpawnComplete(trackerId, new NetworkAssetSpawner.SpawnCallbackInfo()
            {
                spawned = go,
                entity = newEntity,
            });
        }
    }
}