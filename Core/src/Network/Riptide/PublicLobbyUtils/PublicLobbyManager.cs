using BoneLib.BoneMenu.Elements;
using LabFusion.Network;
using LabFusion.Preferences;
using LabFusion.Senders;
using LabFusion.Utilities;
using Riptide;
using System;
using System.Collections.Generic;
using System.Configuration.Internal;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking.Match;

namespace LabFusion.Core.src.Network.Riptide
{
    public static class PublicLobbyManager
    {
        public static MenuCategory publicLobbyCategory;

        public static void CreateLobby()
        {
            if (RiptideNetworkLayer.currentclient.IsConnected)
                RiptideNetworkLayer.currentclient.Disconnect();

            RiptideNetworkLayer.currentclient.Connected += OnCreateLobby;
            RiptideNetworkLayer.currentclient.Connect(FusionPreferences.ClientSettings.PublicLobbyIP.GetValue() + ":7676");

            Message lobbyCreate = Message.Create(MessageSendMode.Reliable, 14);

            if (FusionPreferences.ClientSettings.TideServerName.GetValue() != "")
                lobbyCreate.AddString(FusionPreferences.ClientSettings.TideServerName.GetValue());
            else if (FusionPreferences.ClientSettings.Nickname.GetValue() != null)
                lobbyCreate.AddString(FusionPreferences.ClientSettings.Nickname.GetValue() + "'s Server");
            else
                lobbyCreate.AddString("UNKOWN USER's Server");
        }

        private static void OnCreateLobby(object sender, EventArgs e)
        {
            RiptideNetworkLayer.isHost = true;

            InternalServerHelpers.OnStartServer();
        }

        public static void RequestLobbies(MenuCategory category)
        {
            publicLobbyCategory = category;

            Message request = Message.Create(MessageSendMode.Reliable, 0);
            request.AddString("LobbyRequest");

            RiptideNetworkLayer.publicLobbyClient.Send(request);

            if (!RiptideNetworkLayer.publicLobbyClient.IsConnected)
            {
                RiptideNetworkLayer.publicLobbyClient.Connect(FusionPreferences.ClientSettings.PublicLobbyIP.GetValue() + ":7676", 5, 0, request);
            } else
            {
                RiptideNetworkLayer.publicLobbyClient.Send(request);
            }
        }

        [MessageHandler(20)]
        public static void HandleLobbyInfo(Message message)
        {
            if (publicLobbyCategory == null)
                return;

            int ServerID = message.GetInt();

            ushort hostID = message.GetUShort();

            string LobbyName = message.GetString();
            string LobbyVersion = message.GetString();
            int PlayerCount = message.GetInt();

            bool NametagsEnabled = message.GetBool();
            ServerPrivacy Privacy = (ServerPrivacy)message.GetInt();
            TimeScaleMode TimeScaleMode = (TimeScaleMode)message.GetInt();
            int MaxPlayers = message.GetInt();
            bool VoicechatEnabled = message.GetBool();
            bool AllowQuestUsers = message.GetBool();
            bool AllowPCUsers = message.GetBool();

            string LevelName = message.GetString();

            if (Privacy != ServerPrivacy.PUBLIC)
                return;

            var lobby = publicLobbyCategory.CreateCategory($"{LobbyName} ({PlayerCount}/{MaxPlayers})", Color.white);
            lobby.CreateFunctionElement("Join Server", Color.green, () =>
            {
                if (RiptideNetworkLayer.currentclient.IsConnected)
                    RiptideNetworkLayer.currentclient.Disconnect();

                RiptideNetworkLayer.currentPublicHostID = (short)hostID;
                Message joinRequest = Message.Create(MessageSendMode.Reliable, 10);
                joinRequest.AddInt(ServerID);

                RiptideNetworkLayer.currentclient.Connect(FusionPreferences.ClientSettings.PublicLobbyIP.GetValue() + ":7676", 5, 0, joinRequest);
            });

            var lobbyInfo = lobby.CreateSubPanel("Lobby Info", Color.white);
            lobbyInfo.CreateFunctionElement($"Lobby ID: {ServerID}", Color.white, null);
            lobbyInfo.CreateFunctionElement($"Lobby Version: {LobbyVersion}", Color.white, null);
            lobbyInfo.CreateFunctionElement($"Player Count: {PlayerCount}", Color.white, null);

            var gameInfo = lobby.CreateSubPanel("Game Info", Color.white);
            gameInfo.CreateFunctionElement($"VC Enabled: {VoicechatEnabled}", Color.white, null);
            gameInfo.CreateFunctionElement($"Level Name: {LevelName}", Color.white, null);
        }

        [MessageHandler(22)]
        public static void HandleJoinLobbies(Message message)
        {
            RiptideNetworkLayer.currentPublicLobbyID = message.GetInt();

            RiptideNetworkLayer.CurrentServerType.SetType(Enums.ServerTypes.PUBLIC);

            ConnectionSender.SendConnectionRequest();
        }
    }
}
