using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.SDK.Achievements;
using LabFusion.SDK.Gamemodes;
using LabFusion.Senders;
using LabFusion.Utilities;
using Riptide;
using Riptide.Transports;
using SLZ.Bonelab;
using SLZ.Marrow.SceneStreaming;
using Steamworks.Data;
using static LabFusion.Network.RiptideNetworkLayer;

namespace LabFusion.Network
{
    public static class FusionMessageReader
    {
        // Handle Messages from the Server
        [MessageHandler((ushort)RiptideMessageTypes.FusionMessage)]
        public static void HandleSomeMessageFromServer(Message message)
        {
            unsafe
            {
                int messageLength = message.WrittenLength;

                byte[] buffer = message.GetBytes();
                fixed (byte* messageBuffer = buffer)
                {
                    FusionMessageHandler.ReadMessage(messageBuffer, messageLength);
                }
            }
        }

        // Handle Messages from a Client
        [MessageHandler((ushort)RiptideMessageTypes.FusionMessage)]
        private static void HandleSomeMessageFromClient(ushort riptideID, Message message)
        {
            unsafe
            {
                int messageLength = message.WrittenLength;

                byte[] buffer = message.GetBytes();
                fixed (byte* messageBuffer = buffer)
                {
                    FusionMessageHandler.ReadMessage(messageBuffer, messageLength);
                }
            }
        }
    }
}
