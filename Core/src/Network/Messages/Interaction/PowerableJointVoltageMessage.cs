﻿using LabFusion.Data;
using LabFusion.Syncables;
using LabFusion.Patching;

namespace LabFusion.Network
{
    public class PowerableJointVoltageData : IFusionSerializable
    {
        public const int Size = sizeof(byte) + sizeof(ushort) + sizeof(float);

        public byte smallId;
        public ushort syncId;
        public float voltage;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write(syncId);
            writer.Write(voltage);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            syncId = reader.ReadUInt16();
            voltage = reader.ReadSingle();
        }

        public static PowerableJointVoltageData Create(byte smallId, ushort syncId, float voltage)
        {
            return new PowerableJointVoltageData()
            {
                smallId = smallId,
                syncId = syncId,
                voltage = voltage,
            };
        }
    }

    [Net.DelayWhileTargetLoading]
    public class PowerableJointVoltageMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.PowerableJointVoltage;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using FusionReader reader = FusionReader.Create(bytes);
            var data = reader.ReadFusionSerializable<PowerableJointVoltageData>();
            // Send message to other clients if server
            if (NetworkInfo.IsServer && isServerHandled)
            {
                using var message = FusionMessage.Create(Tag.Value, bytes);
                MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);
            }
            else
            {
                if (SyncManager.TryGetSyncable<PropSyncable>(data.syncId, out var syncable) && syncable.TryGetExtender<PowerableJointExtender>(out var extender))
                {
                    PowerableJointPatches.IgnorePatches = true;
                    extender.Component.SETJOINT(data.voltage);
                    PowerableJointPatches.IgnorePatches = false;
                }
            }
        }
    }
}
