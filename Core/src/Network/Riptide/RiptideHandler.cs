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

        public static byte[] FusionMessagetoBytes (FusionMessage fusionMessage)
        {
            byte[] bytes = new byte[fusionMessage.Length];
            try
            {
                for (int i = 0; i < fusionMessage.Length; i++)
                {
                     unsafe {bytes[i] = Marshal.ReadByte((IntPtr)fusionMessage.Buffer, i); }
                }
            }
            catch (Exception e)
            {
                FusionLogger.Error($"Failed to convert message into bytes: {e}");
            }
            return bytes;
        }

        public static Riptide.Message PrepareMessage(FusionMessage fusionMessage, NetworkChannel channel)
        {
            //Id is always 0 because a fusion message sent from riptide will always be in bytes
            Riptide.Message message = Riptide.Message.Create(RiptideHandler.ConvertToSendMode(channel), 0);
            message.AddBytes(FusionMessagetoBytes(fusionMessage));
            return message;
        }
    }
    
}