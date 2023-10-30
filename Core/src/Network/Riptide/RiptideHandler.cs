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
    public static class RiptideHandler
    {
        public static MessageSendMode ConvertToSendMode (NetworkChannel channel)
        {
            MessageSendMode sendMode;
            switch (channel)
            {
                case NetworkChannel.Unreliable:
                default:
                    sendMode = MessageSendMode.Unreliable;
                    break;
                case NetworkChannel.VoiceChat:
                    sendMode = MessageSendMode.Unreliable;
                    break;
                case NetworkChannel.Reliable:
                    sendMode = MessageSendMode.Reliable;
                    break;
            }
            return sendMode;
        }

        public unsafe static byte[] FusionMessageToBytes (FusionMessage fusionMessage)
        {
            int length = fusionMessage.Length;
            byte* buffer = fusionMessage.Buffer;

            byte[] bytes = ConvertBytePointerToByteArray(buffer, length);

            return bytes;
        }

        public unsafe static byte[] ConvertBytePointerToByteArray(byte* bytePtr, int length)
        {
            // Create a new managed byte array
            byte[] byteArray = new byte[length];

            // Copy the data from the byte pointer to the byte array using Marshal.Copy
            Marshal.Copy((IntPtr)bytePtr, byteArray, 0, length);

            return byteArray;
        }

        public static Message PrepareMessage(FusionMessage fusionMessage, NetworkChannel channel, ushort messageChannel, short playerID = -1)
        {
            if (CurrentServerType.GetType() == ServerTypes.P2P)
            {
                var message = Message.Create(ConvertToSendMode(channel), (ushort)RiptideMessageTypes.FusionMessage); // Create the message

                message.Release(); // Make sure the message is empty before adding bytes

                message.AddBytes(FusionMessageToBytes(fusionMessage)); // Add bytes

                return message;
            } else
            {
                var message = Message.Create(ConvertToSendMode(channel), messageChannel); // Create the message

                message.Release(); // Make sure the message is empty before adding bytes

                message.AddBytes(FusionMessageToBytes(fusionMessage));

                if (playerID != -1)
                {
                    message.AddUShort((ushort)playerID);
                }
                return message;
            }
        }
    }
}