using Riptide;
using Server.Enums;

namespace Server.Networking
{
    public static class MessageHandler
    {

        [MessageHandler(0)]
        private static void HandleSomeMessageFromClient(ushort riptideID, Message message)
        {
#if DEBUG
            Console.WriteLine($"Received message from id: {riptideID}!");
#endif

            byte[] bytes = message.GetBytes();
            SendTypes tag = (SendTypes)message.GetInt();
            ushort playerID = message.GetUShort();

            switch (tag)
            {
                case SendTypes.SendFromServer:
#if DEBUG
                    Console.WriteLine("Handling SendFromServer");
#endif
                    Message sentFromServer = Message.Create(MessageSendMode.Unreliable, 0);
                    sentFromServer.Release();
                    sentFromServer.AddBytes(bytes);
                    Server.ServerClass.currentserver.Send(sentFromServer, playerID);
                    break;
                case SendTypes.SendToServer:
#if DEBUG
                    Console.WriteLine("Handling SendToServer");
#endif
                    Message sentToServer = Message.Create(MessageSendMode.Unreliable, 0);
                    sentToServer.Release();
                    sentToServer.AddBytes(bytes);
                    Server.ServerClass.currentserver.Send(message, Server.ServerClass.host);
                    break;
                case SendTypes.SendToAll:
#if DEBUG
                    Console.WriteLine("Handling SendToAll");
#endif
                    Message sentToAll = Message.Create(MessageSendMode.Unreliable, 0);
                    sentToAll.Release();
                    sentToAll.AddBytes(bytes);
                    Server.ServerClass.currentserver.SendToAll(sentToAll);
                    break;
                default:
                    Console.WriteLine("Message is not in the correct format!");
                    break;
            }
        }

        [MessageHandler(1)]
        private static void HandleClientRequest(ushort riptideID, Message message)
        {
            Server.ServerClass.currentserver.TryGetClient(riptideID, out Connection client);
#if DEBUG
            Console.WriteLine($"Received Request message from id: {client.Id}!");
#endif

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