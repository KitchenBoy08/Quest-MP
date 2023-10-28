using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Riptide;
using PublicLobbyHost;

namespace PublicLobbyHost
{
    public static class PacketHandlers
    {
        private static void BroadcastToLobby(ushort id, Message message)
        {
            foreach (var client in Program.GetPublicLobby(id).clientIDs)
            {
                Program.mainHost.Send(message, client);
            }
        }

        [MessageHandler(0)]
        public static void HandleLobbyRequest(ushort riptideID, Message message)
        {
            Program.mainHost.TryGetClient(riptideID, out Connection client);

            foreach (var lobby in Program.lobbies)
            {
                Message lobbyData = Message.Create(MessageSendMode.Reliable, 20);

                lobbyData.AddInt(lobby.ServerID);

                lobbyData.AddUShort(lobby.hostID);

                lobbyData.AddString(lobby.LobbyName);
                lobbyData.AddString(lobby.LobbyVersion);
                lobbyData.AddInt(lobby.PlayerCount);

                lobbyData.AddInt(lobby.Privacy);
                lobbyData.AddInt(lobby.MaxPlayers);
                lobbyData.AddBool(lobby.AllowQuestUsers);
                lobbyData.AddBool(lobby.AllowPCUsers);

                lobbyData.AddString(lobby.LevelName);

                Program.mainHost.Send(lobbyData, client.Id);
            }
        
        }

        [MessageHandler(19)]
        public static void HandleCreateLobby(ushort riptideID, Message message)
        {
            Program.mainHost.TryGetClient(riptideID, out Connection client);

            PublicLobby newLobby = new PublicLobby();
            newLobby.ServerID = Program.lobbies.Count;

            newLobby.hostID = riptideID;

            newLobby.LobbyName = message.GetString();
            newLobby.LobbyVersion = message.GetString();
            newLobby.PlayerCount = 1;

            newLobby.Privacy = message.GetInt();
            newLobby.MaxPlayers = message.GetByte();
            newLobby.AllowQuestUsers = message.GetBool();
            newLobby.AllowPCUsers = message.GetBool();

            newLobby.LevelName = message.GetString();

            Program.lobbies.Add(newLobby);

            Message createServer = Message.Create(MessageSendMode.Reliable, 19);
            createServer.AddByte(0);
            Program.mainHost.Send(createServer, client);
        }

        [MessageHandler(10)]
        public static void HandleJoinLobby(ushort riptideID, Message message)
        {
            Program.mainHost.TryGetClient(riptideID, out Connection client);

            int serverID = message.GetInt();

            Program.lobbies[serverID].clientIDs.Add(client.Id);

            Message joinAccept = Message.Create(MessageSendMode.Reliable, 22);
            joinAccept.AddInt(serverID);

            Program.mainHost.Send(joinAccept, client.Id);
        }

        [MessageHandler(1)]
        public static void HandleClientRequest(ushort riptideID, Message message)
        {
            Program.mainHost.TryGetClient(riptideID, out Connection client);

            if (message.GetString() == "RequestServerType")
            {
                Riptide.Message sent = Riptide.Message.Create(MessageSendMode.Reliable, 1);
                sent.AddInt(3);

                Program.mainHost.Send(sent, client.Id);
            }
        }

        [MessageHandler(3)]
        public static void HandleBroadcast(ushort riptideID, Message message)
        {
            byte[] bytes = message.GetBytes();
            ushort id = message.GetUShort();

            Message sent = Message.Create(message.SendMode, 0);
            sent.Release();

            sent.AddBytes(bytes);
            sent.AddBool(false);

            if (id == 0)
                BroadcastToLobby(id, sent);
            else
                Program.mainHost.Send(sent, id);

        }

        [MessageHandler(4)]
        public static void HandleSendToServer(ushort riptideID, Message message)
        {
            if (Program.GetPublicLobby(riptideID) != null)
            {
                byte[] bytes = message.GetBytes();
                ushort id = message.GetUShort();

                Message sent = Message.Create(message.SendMode, 0);
                sent.Release();
                sent.AddBytes(bytes);

                Program.mainHost.Send(sent, id);
            }
        }

        [MessageHandler(5)]
        public static void HandleSendFromServer(ushort riptideID, Message message)
        {
            byte[] bytes = message.GetBytes();
            ushort id = message.GetUShort();

            Message sent = Message.Create(message.SendMode, 0);
            sent.Release();
            sent.AddBytes(bytes);

            Program.mainHost.Send(sent, id);
        }
    }
}
