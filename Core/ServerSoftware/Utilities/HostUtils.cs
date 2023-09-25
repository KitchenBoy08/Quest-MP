using Riptide;
using Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerSoftware.Utilities
{
    public static class HostUtils
    {
        public static void TrySetHost(Connection client)
        {
            Message message = Message.Create(MessageSendMode.Reliable, 11);
            message.Release();

            message.AddByte(1);

            ServerClass.currentserver.Send(message, client);
        }

        [MessageHandler(11)]
        public static void HandleHostResponse(ushort riptideID, Message message)
        {
            bool answer = message.GetBool();

            if (answer)
            {
                if (ServerClass.currentserver.TryGetClient(riptideID, out Connection client))
                {
                    ServerClass.host = client;
                    ServerClass.UpdateWindow("Obtained new host!");
                } else
                {
                    ServerClass.RestartServer();
                }
            }
        }
    }
}