using Riptide;
using Server.Enums;
using ServerSoftware;

namespace Server.Networking
{
    public static class MessageHandler
    {
        [MessageHandler(2)]
        public static void HandleSendToServer(ushort riptideID, Message message)
        {
            byte[] bytes = message.GetBytes();

            Message sent = Message.Create(message.SendMode, 5);
            sent.Release();

            sent.AddBytes(bytes);
            sent.AddBool(true);

            if (ServerClass.currentserver.TryGetClient((ushort)ServerClass.hostID, out Connection client))
                ServerClass.currentserver.Send(sent, (ushort)ServerClass.hostID);
            else
                ServerClass.UpdateWindow("Failed to SendToHost");
        }

        [MessageHandler(3)]
        public static void HandleSendToAll(ushort riptideID, Message message)
        {
            byte[] bytes = message.GetBytes();

            Message sent = Message.Create(message.SendMode, 5);
            sent.Release();

            sent.AddBytes(bytes);
            sent.AddBool(false);

            ServerClass.currentserver.SendToAll(sent);
        }

        [MessageHandler(4)]
        public static void HandleSendFromServer(ushort riptideID, Message message)
        {
            byte[] bytes = message.GetBytes();
            ushort playerID = message.GetUShort();
            
            Message sent = Message.Create(message.SendMode, 5);
            sent.Release();

            sent.AddBytes(bytes);
            sent.AddBool(false);

            ServerClass.currentserver.Send(sent, playerID);
        }

        [MessageHandler(1)]
        public static void HandleClientRequest(ushort riptideID, Message message)
        {
            ServerClass.currentserver.TryGetClient(riptideID, out Connection client);

            if (message.GetString() == "RequestServerType")
            {
                Riptide.Message sent = Riptide.Message.Create(MessageSendMode.Reliable, 1);
                sent.AddInt((int)ServerTypes.DEDICATED);
                sent.AddString(ServerClass.currentLevelBarcode);
                ServerClass.currentserver.Send(sent, client);
            }
        }

        [MessageHandler(7)]
        public static void HandleLevelInfo(ushort riptideID, Message message)
        {
            string levelBarcode = message.GetString();
            string levelName = message.GetString();
            ServerClass.currentLevelBarcode = levelBarcode;
            ServerClass.currentLevelName = levelName;
            ServerClass.UpdateWindow($"Loaded new level with Title: \n{levelName}");
        }

    }
}