using System;
using System.Runtime.InteropServices;

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

        //Add here some Bytes to Fusion Message thing, it'll be needed.

        public static Message PrepareMessage(FusionMessage fusionMessage, NetworkChannel channel)
        {
            var message = Message.Create(ConvertToSendMode(channel), 0); // Create the message

            message.Release(); // Make sure the message is empty before adding bytes

            message.AddBytes(FusionMessageToBytes(fusionMessage)); // Add bytes
            return message;
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
                    FusionMessageHandler.ReadMessage(messageBuffer, messageLength, false);
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
    }
}