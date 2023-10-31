using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Riptide;
using PublicLobbyHost.Utilities;

namespace PublicLobbyHost
{
    public static class PacketHandlers
    {
        private static void BroadcastToLobby(ushort hostId, Message message)
        {
            foreach (var client in PublicLobbyHost.GetPublicLobby(hostId).clientIDs)
            {
                PublicLobbyHost.mainHost.Send(message, client);
            }
        }

        [MessageHandler((ushort)RiptideMessageTypes.RequestLobbies)]
        public static void HandleLobbyRequest(ushort riptideID, Message message)
        {
            PublicLobbyHost.mainHost.TryGetClient(riptideID, out Connection client);

            foreach (var lobby in PublicLobbyHost.lobbies)
            {
                Message lobbyData = Message.Create(MessageSendMode.Reliable, (ushort)RiptideMessageTypes.LobbyInfo);

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

                PublicLobbyHost.mainHost.Send(lobbyData, client.Id);
            }
        
        }

        [MessageHandler((ushort)RiptideMessageTypes.CreateLobby)]
        public static void HandleCreateLobby(ushort riptideID, Message message)
        {
            PublicLobbyHost.mainHost.TryGetClient(riptideID, out Connection client);

            PublicLobby newLobby = new PublicLobby();
            newLobby.ServerID = PublicLobbyHost.lobbies.Count;

            newLobby.hostID = riptideID;

            newLobby.LobbyName = message.GetString();
            newLobby.LobbyVersion = message.GetString();
            newLobby.PlayerCount = 1;

            newLobby.Privacy = message.GetInt();
            newLobby.MaxPlayers = message.GetByte();
            newLobby.AllowQuestUsers = message.GetBool();
            newLobby.AllowPCUsers = message.GetBool();

            newLobby.LevelName = message.GetString();

            PublicLobbyHost.lobbies.Add(newLobby);

            Message createServer = Message.Create(MessageSendMode.Reliable, 19);
            createServer.AddByte(0);
            PublicLobbyHost.mainHost.Send(createServer, client);
        }

        [MessageHandler((ushort)RiptideMessageTypes.JoinLobby)]
        public static void HandleJoinLobby(ushort riptideID, Message message)
        {
            PublicLobbyHost.mainHost.TryGetClient(riptideID, out Connection client);

            int serverID = message.GetInt();

            PublicLobbyHost.lobbies[serverID].clientIDs.Add(client.Id);

            Message joinAccept = Message.Create(MessageSendMode.Reliable, 22);
            joinAccept.AddInt(serverID);

            PublicLobbyHost.mainHost.Send(joinAccept, client.Id);
        }

        [MessageHandler((ushort)RiptideMessageTypes.UpdateLobbyInfo)]
        public static void HandleUpdateLobby(ushort riptideID, Message message)
        {
            var lobby = PublicLobbyHost.GetPublicLobby(riptideID);
            
            int privacy = message.GetInt();
            byte maxPlayers = message.GetByte();
            int playerCount = message.GetInt();
            bool allowQuestPlayers = message.GetBool();
            bool allowPcPlayers = message.GetBool();

            string levelName = message.GetString();

            lobby.Privacy = privacy;
            lobby.MaxPlayers = maxPlayers;
            lobby.PlayerCount = playerCount;
            lobby.AllowQuestUsers = allowQuestPlayers;
            lobby.AllowPCUsers = allowPcPlayers;
            lobby.LevelName = levelName;

            PublicLobbyHost.UpdateLobby(lobby);
        }

        [MessageHandler((ushort)RiptideMessageTypes.ServerType)]
        public static void HandleClientRequest(ushort riptideID, Message message)
        {
            PublicLobbyHost.mainHost.TryGetClient(riptideID, out Connection client);

            Riptide.Message sent = Riptide.Message.Create(MessageSendMode.Reliable, (ushort)RiptideMessageTypes.ServerType);
            sent.AddInt(3);

            PublicLobbyHost.mainHost.Send(sent, client.Id);
        }

        [MessageHandler((ushort)RiptideMessageTypes.Broadcast)]
        public static void HandleBroadcast(ushort riptideID, Message message)
        {
            byte[] bytes = message.GetBytes();
            ushort id = message.GetUShort();

            Message sent = Message.Create(message.SendMode, RiptideMessageTypes.FusionMessage);
            sent.Release();

            sent.AddBytes(bytes);
            sent.AddBool(false);

            if (id == 0)
                BroadcastToLobby(id, sent);
            else
                PublicLobbyHost.mainHost.Send(sent, id);

        }

        [MessageHandler((ushort)RiptideMessageTypes.SendToServer)]
        public static void HandleSendToServer(ushort riptideID, Message message)
        {
            if (PublicLobbyHost.GetPublicLobby(riptideID) != null)
            {
                byte[] bytes = message.GetBytes();
                ushort id = message.GetUShort();

                Message sent = Message.Create(message.SendMode, RiptideMessageTypes.FusionMessage);
                sent.Release();
                sent.AddBytes(bytes);

                PublicLobbyHost.mainHost.Send(sent, id);
            }
        }

        [MessageHandler((ushort)RiptideMessageTypes.SendFromServer)]
        public static void HandleSendFromServer(ushort riptideID, Message message)
        {
            byte[] bytes = message.GetBytes();
            ushort id = message.GetUShort();

            Message sent = Message.Create(message.SendMode, RiptideMessageTypes.FusionMessage);
            sent.Release();
            sent.AddBytes(bytes);

            PublicLobbyHost.mainHost.Send(sent, id);
        }
    }
}
