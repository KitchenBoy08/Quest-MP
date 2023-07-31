using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.Utilities;

using Riptide;
using Riptide.Utils;
using Riptide.Transports;

using UnityEngine;
using System.Runtime.CompilerServices;
using Steamworks.Data;
using Steamworks;
using UnityEngine.UIElements;


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

        //Add here some Bytes to Fusion Message thing, it'll be needed.
        public static Riptide.Message PrepareMessage(FusionMessage fusionMessage, NetworkChannel channel)
        {
            // Id is always 0 because a fusion message sent from riptide will always be in bytes
            Riptide.Message message = Riptide.Message.Create(ConvertToSendMode(channel), 0);
            message.Release();
            message.AddBytes(FusionMessageToBytes(fusionMessage));
            return message;
        }

        // Recieving Messages WIP
        // This needs to handle a riptide message, which is its own thing
        [MessageHandler(0)]
        public static void HandleSomeMessageFromServer(Message message)
        {
            try
            {
                unsafe
                {
                    int messageLength = message.WrittenLength;

                    byte[] buffer = message.GetBytes();
                    fixed (byte* messageBuffer = buffer)
                    {
                        FusionMessageHandler.ReadMessage(messageBuffer, messageLength, false);
                    }
                }
            }
            catch (Exception e)
            {
                FusionLogger.Error($"Failed reading message from Riptide Client with reason: {e.Message}");
            }
        }

        [MessageHandler(0)]
        private static void HandleSomeMessageFromClient(ushort riptideID, Message message)
        {
            try
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
            catch (Exception e)
            {
                FusionLogger.Error($"Failed reading message from Riptide Server with reason: {e.Message}");
            }
        }
    }
}