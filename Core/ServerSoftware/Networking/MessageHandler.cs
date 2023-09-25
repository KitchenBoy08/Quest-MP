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

            Message sent = Message.Create(message.SendMode, 0);
            sent.Release();

            sent.AddBytes(bytes);

            ServerClass.currentserver.Send(sent, ServerClass.host);
            Console.WriteLine("SentToServer");
        }

        [MessageHandler(3)]
        public static void HandleSendToAll(ushort riptideID, Message message)
        {
            byte[] bytes = message.GetBytes();

            Message sent = Message.Create(message.SendMode, 0);
            sent.Release();

            sent.AddBytes(bytes);

            ServerClass.currentserver.SendToAll(sent);
        }

        [MessageHandler(4)]
        public static void HandleSendFromServer(ushort riptideID, Message message)
        {
            byte[] bytes = message.GetBytes();
            ushort playerID = message.GetUShort();
            
            Message sentFromServer = Message.Create(message.SendMode, 0);
            sentFromServer.Release();

            sentFromServer.AddBytes(bytes);

            ServerClass.currentserver.Send(sentFromServer, playerID);
            Console.WriteLine($"Sent to Client: {playerID}");
        }

        [MessageHandler(1)]
        public static void HandleClientRequest(ushort riptideID, Message message)
        {
            ServerClass.currentserver.TryGetClient(riptideID, out Connection client);

            if (message.GetString() == "RequestServerType")
            {
                Riptide.Message sent = Riptide.Message.Create(MessageSendMode.Reliable, 1);
                sent.AddInt((int)ServerTypes.DEDICATED);
                ServerClass.currentserver.Send(sent, client);
            }
        }
    }
}