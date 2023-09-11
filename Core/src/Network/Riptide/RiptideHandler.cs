using System;
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
                    sendMode = MessageSendMode.Unreliable;
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

        public static Message PrepareMessage(FusionMessage fusionMessage, NetworkChannel channel, SendTypes sendType, ushort playerID = 0)
        {
            var message = Message.Create(ConvertToSendMode(channel), 0); // Create the message

            message.Release(); // Make sure the message is empty before adding bytes

            message.AddBytes(FusionMessageToBytes(fusionMessage)); // Add bytes

            message.AddInt((int)sendType);

            message.AddUShort(playerID);

            return message;
        }

        [MessageHandler(0)]
        public static void HandleSomeMessageFromServer(Message message)
        {
            unsafe
            {
                int messageLength = message.GetBytes().Length;

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
                int messageLength = message.GetBytes().Length;

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
            else if (message.GetInt() == (int)ServerTypes.P2P)
            {
                RiptideNetworkLayer.CurrentServerType.SetType(ServerTypes.P2P);
                RiptideNetworkLayer.currentclient.Disconnected += RiptideNetworkLayer.OnClientDisconnect;
                // Update player ID here since it's determined on the Riptide Client ID
                PlayerIdManager.SetLongId(RiptideNetworkLayer.currentclient.Id);
                PlayerIdManager.SetUsername($"Riptide Enjoyer");
                ConnectionSender.SendConnectionRequest();
                RiptideNetworkLayer.OnUpdateRiptideLobby();
                return;
            } else
            {
                FusionLogger.Error("Server Response was incorrect!");
            }
        }

        [MessageHandler(1)]
        private static void HandleClientRequest(ushort riptideID, Message message)
        {
            if (message.GetString() == "RequestServerType")
            {
                Riptide.Message sent = Riptide.Message.Create(MessageSendMode.Reliable, 0);
                sent.AddInt((int)ServerTypes.P2P);
                RiptideNetworkLayer.currentserver.TryGetClient(riptideID, out Connection client);
                RiptideNetworkLayer.currentserver.Send(sent, client);
                return;
            }
        }
    }
}