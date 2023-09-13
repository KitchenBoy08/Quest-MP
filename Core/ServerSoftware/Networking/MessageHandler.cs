using Riptide;
using Server.Enums;

namespace Server.Networking
{
    public static class MessageHandler
    {
        [MessageHandler(2)]
        public static void HandleSendToServer(ushort riptideID, Message message)
        {
            byte[] bytes = message.GetBytes();
            Message sent = Message.Create(MessageSendMode.Unreliable, 0);
            sent.AddBytes(bytes);

            ServerClass.currentserver.Send(sent, ServerClass.host);
            Console.WriteLine("Handling SendToServer");
            Console.WriteLine($"Sent {bytes.Length}");
        }

        [MessageHandler(3)]
        public static void HandleSendToAll(ushort riptideID, Message message)
        {
            byte[] bytes = message.GetBytes();
            Message sent = Message.Create(MessageSendMode.Unreliable, 0);
            sent.AddBytes(bytes);

            ServerClass.currentserver.SendToAll(sent);
            Console.WriteLine("Handling SendToAll");
            Console.WriteLine($"Sent {bytes.Length}");
        }

        [MessageHandler(4)]
        public static void HandleSendFromServer(ushort riptideID, Message message)
        {
            byte[] bytes = message.GetBytes();
            ushort playerID = bytes[bytes.Count() - 1];

            Message sentFromServer = Message.Create(MessageSendMode.Unreliable, 0);
            sentFromServer.Release();

            List<byte> bytesToAdd = bytes.ToList();
            bytesToAdd.RemoveAt(bytes.Count() - 1);

            sentFromServer.AddBytes(bytesToAdd.ToArray());

            Server.ServerClass.currentserver.Send(sentFromServer, playerID);
            Console.WriteLine("Handled SendFromServer");
            Console.WriteLine($"Sent {bytes.Length}");
        }

        [MessageHandler(1)]
        private static void HandleClientRequest(ushort riptideID, Message message)
        {
            Server.ServerClass.currentserver.TryGetClient(riptideID, out Connection client);

            if (message.GetString() == "RequestServerType")
            {
                Riptide.Message sent = Riptide.Message.Create(MessageSendMode.Unreliable, 1);
                sent.AddInt((int)ServerTypes.DEDICATED);
                Server.ServerClass.currentserver.Send(sent, client);
                Console.WriteLine("Sending Server Type");
                return;
            }
        }
    }
}