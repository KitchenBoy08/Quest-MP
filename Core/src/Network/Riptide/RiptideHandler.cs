using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using LabFusion.Core.src.Network.Riptide.Enums;
using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.Senders;
using LabFusion.Utilities;
using Riptide;

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

        public static Message PrepareMessage(FusionMessage fusionMessage, NetworkChannel channel, ushort messageChannel, ushort playerID = 0)
        {
            if (RiptideNetworkLayer.CurrentServerType.GetType() == ServerTypes.P2P)
            {
                var message = Message.Create(ConvertToSendMode(channel), 0); // Create the message

                message.Release(); // Make sure the message is empty before adding bytes

                message.AddBytes(FusionMessageToBytes(fusionMessage)); // Add bytes

                return message;
            } else
            {
                var message = Message.Create(ConvertToSendMode(channel), messageChannel); // Create the message

                message.Release(); // Make sure the message is empty before adding bytes

                if (playerID != 0)
                {
                    message.AddBytes(AddPlayerIDToBytes(FusionMessageToBytes(fusionMessage), (byte)playerID)); // Add bytes
                } else
                {
                    message.AddBytes(FusionMessageToBytes(fusionMessage)); // Add bytes
                }

                return message;
            }
        }

        [MessageHandler(0)]
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

        [MessageHandler(0)]
        private static void HandleSomeMessageFromClient(ushort riptideID, Message message)
        {
            unsafe
            {
                int messageLength = message.WrittenLength;

                byte[] buffer = message.GetBytes();
                fixed (byte* messageBuffer = buffer)
                {
                    FusionMessageHandler.ReadMessage(messageBuffer, messageLength, true);
                } 
            }
        }

        [MessageHandler(1)]
        private static void HandleServerResponse(Message message)
        {
            if (message.GetInt() == (int)ServerTypes.DEDICATED)
            {
                RiptideNetworkLayer.CurrentServerType.SetType(ServerTypes.DEDICATED);
                if (RiptideNetworkLayer.currentclient.Id == 1)
                {
                    RiptideNetworkLayer.isHost = true;

                    RiptideNetworkLayer.currentclient.Disconnected += RiptideNetworkLayer.OnClientDisconnect;
                    // Update player ID here since it's determined on the Riptide Client ID
                    PlayerIdManager.SetLongId(RiptideNetworkLayer.currentclient.Id);
                    PlayerIdManager.SetUsername($"Riptide Enjoyer");

                    InternalServerHelpers.OnStartServer();

                    RiptideNetworkLayer.OnUpdateRiptideLobby();

                    FusionSceneManager.HookOnDelayedLevelLoad(() => {
                        PermissionList.SetPermission(PlayerIdManager.LocalLongId, PlayerIdManager.LocalUsername, PermissionLevel.DEFAULT);
                    });
                }
                else
                {
                    RiptideNetworkLayer.isHost = false;

                    RiptideNetworkLayer.currentclient.Disconnected += RiptideNetworkLayer.OnClientDisconnect;
                    // Update player ID here since it's determined on the Riptide Client ID
                    PlayerIdManager.SetLongId(RiptideNetworkLayer.currentclient.Id);
                    PlayerIdManager.SetUsername($"Riptide Enjoyer");

                    ConnectionSender.SendConnectionRequest();
                }
            }
            else
            {
                RiptideNetworkLayer.isHost = false;

                RiptideNetworkLayer.CurrentServerType.SetType(ServerTypes.P2P);
                RiptideNetworkLayer.currentclient.Disconnected += RiptideNetworkLayer.OnClientDisconnect;
                // Update player ID here since it's determined on the Riptide Client ID
                PlayerIdManager.SetLongId(RiptideNetworkLayer.currentclient.Id);
                PlayerIdManager.SetUsername($"Riptide Enjoyer");

                ConnectionSender.SendConnectionRequest();
            }
        }

        [MessageHandler(1)]
        private static void HandleClientRequest(ushort riptideID, Message message)
        {
            RiptideNetworkLayer.currentserver.TryGetClient(riptideID, out Connection client);

            if (message.GetString() == "RequestServerType")
            {
                Riptide.Message sent = Riptide.Message.Create(MessageSendMode.Unreliable, 1);
                sent.AddInt((int)ServerTypes.P2P);
                RiptideNetworkLayer.currentserver.Send(sent, client);
            }
        }

        private static byte[] AddPlayerIDToBytes(byte[] bytes, byte playerID)
        {
            List<byte> byteList = bytes.ToList();

            byteList.Add(playerID);

            return byteList.ToArray();
        }
    }
}