using Riptide;
using Server.Enums;
using ServerSoftware;

namespace Server.Networking
{
    public static class MessageHandler
    {
        [MessageHandler((ushort)RiptideMessageTypes.SendToServer)]
        public static void HandleSendToServer(ushort riptideID, Message message)
        {
            byte[] bytes = message.GetBytes();

            Message sent = Message.Create(message.SendMode, (ushort)RiptideMessageTypes.FusionMessage);
            sent.Release();

            sent.AddBytes(bytes);
            sent.AddBool(true);

            if (ServerClass.currentserver.TryGetClient((ushort)ServerClass.hostID, out Connection client))
                ServerClass.currentserver.Send(sent, (ushort)ServerClass.hostID);
            else
                ServerClass.UpdateWindow("Failed to SendToHost");
        }

        [MessageHandler((ushort)RiptideMessageTypes.Broadcast)]
        public static void HandleBroadcast(ushort riptideID, Message message)
        {
            byte[] bytes = message.GetBytes();

            Message sent = Message.Create(message.SendMode, (ushort)RiptideMessageTypes.FusionMessage);
            sent.Release();

            sent.AddBytes(bytes);
            sent.AddBool(false);

            ServerClass.currentserver.SendToAll(sent);
        }

        [MessageHandler((ushort)RiptideMessageTypes.SendFromServer)]
        public static void HandleSendFromServer(ushort riptideID, Message message)
        {
            byte[] bytes = message.GetBytes();
            ushort playerID = message.GetUShort();
            
            Message sent = Message.Create(message.SendMode, (ushort)RiptideMessageTypes.FusionMessage);
            sent.Release();

            sent.AddBytes(bytes);
            sent.AddBool(false);

            ServerClass.currentserver.Send(sent, playerID);
        }

        [MessageHandler((ushort)RiptideMessageTypes.ServerType)]
        public static void HandleClientRequest(ushort riptideID, Message message)
        {
            if (ServerClass.currentserver.TryGetClient(riptideID, out Connection client))
            {
                if (message.GetString() == "RequestServerType")
                {
                    Riptide.Message sent = Riptide.Message.Create(MessageSendMode.Reliable, RiptideMessageTypes.ServerType);
                    sent.AddInt((int)ServerTypes.DEDICATED);
                    sent.AddString(ServerClass.currentLevelBarcode);
                    sent.AddString(ServerClass.currentLevelName);
                    ServerClass.currentserver.Send(sent, client);
                }
            }
        }

        [MessageHandler((ushort)RiptideMessageTypes.LevelInfo)]
        public static void HandleLevelInfo(ushort riptideID, Message message)
        {
            string levelBarcode = message.GetString();
            string levelName = message.GetString();
            ServerClass.currentLevelBarcode = levelBarcode;
            ServerClass.currentLevelName = levelName;
            ServerClass.UpdateWindow($"Loaded new level with Title: \n{levelName}");
        }

        [MessageHandler((ushort)RiptideMessageTypes.Notification)]
        public static void HandleClientNotification(ushort riptideID, Message message)
        {
            string notif = message.GetString();
            ServerClass.UpdateWindow(notif);
        }
    }
}