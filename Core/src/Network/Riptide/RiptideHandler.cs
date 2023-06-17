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

namespace LabFusion.Network
{
    class RiptideHandler
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
    }
    
}