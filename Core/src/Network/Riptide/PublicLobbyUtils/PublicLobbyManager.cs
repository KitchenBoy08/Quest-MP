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

namespace LabFusion.Network
{
    public static class PublicLobbyManager
    {
        public static MenuCategory publicLobbyCategory;

        public static void CreatePublicLobby()
        {
            if (RiptideNetworkLayer.currentclient.IsConnected)
            {
                RiptideNetworkLayer.currentclient.Disconnect();
            }

            Riptide.Message createLobby = Riptide.Message.Create(Riptide.MessageSendMode.Reliable, 19);

            if (FusionPreferences.ClientSettings.TideServerName.GetValue() != string.Empty)
                createLobby.AddString(FusionPreferences.ClientSettings.TideServerName.GetValue());
            else if (RiptideNetworkLayer.RiptideUsername != string.Empty)
                createLobby.AddString(FusionPreferences.ClientSettings.Nickname.GetValue() + "'s Server");
            else
                createLobby.AddString("UNKNOWN USER's Server");
            createLobby.AddString($"{LabFusion.FusionVersion.versionMajor}.{LabFusion.FusionVersion.versionMinor}.{LabFusion.FusionVersion.versionPatch}");

            createLobby.AddInt((int)FusionPreferences.LocalServerSettings.Privacy.GetValue());
            createLobby.AddByte(FusionPreferences.LocalServerSettings.MaxPlayers.GetValue());
            createLobby.AddBool(FusionPreferences.LocalServerSettings.AllowQuestUsers.GetValue());
            createLobby.AddBool(FusionPreferences.LocalServerSettings.AllowPCUsers.GetValue());

            createLobby.AddString(FusionSceneManager.Level.name);

            RiptideNetworkLayer.currentclient.Connect(RiptideNetworkLayer.PublicLobbyHost, 5, 0, createLobby);
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
                RiptideNetworkLayer.publicLobbyClient.Connect(RiptideNetworkLayer.PublicLobbyHost, 5, 0, request);
            } else
            {
                RiptideNetworkLayer.publicLobbyClient.Send(request);
            }
        }

        [MessageHandler((ushort)RiptideMessageTypes.LobbyInfo)]
        public static void HandleLobbyInfo(Message message)
        {
            if (publicLobbyCategory == null)
                return;

            int ServerID = message.GetInt();

            ushort hostID = message.GetUShort();

            string LobbyName = message.GetString();
            string LobbyVersion = message.GetString();
            int PlayerCount = message.GetInt();

            ServerPrivacy Privacy = (ServerPrivacy)message.GetInt();
            int MaxPlayers = message.GetInt();
            bool AllowQuestUsers = message.GetBool();
            bool AllowPCUsers = message.GetBool();

            string LevelName = message.GetString();

            if (Privacy != ServerPrivacy.PUBLIC)
                return;

            var lobby = publicLobbyCategory.CreateCategory($"{LobbyName}\n({PlayerCount}/{MaxPlayers})", Color.white);
            lobby.CreateFunctionElement("Join Server", Color.green, () =>
            {
                if (RiptideNetworkLayer.currentclient.IsConnected)
                    RiptideNetworkLayer.currentclient.Disconnect();

                RiptideNetworkLayer.currentPublicHostID = (short)hostID;
                Message joinRequest = Message.Create(MessageSendMode.Reliable, 10);
                joinRequest.AddInt(ServerID);

                RiptideNetworkLayer.currentclient.Connect(RiptideNetworkLayer.PublicLobbyHost, 5, 0, joinRequest);
            });

            var lobbyInfo = lobby.CreateSubPanel("Lobby Info", Color.white);
            lobbyInfo.CreateFunctionElement($"Lobby ID: {ServerID}", Color.white, null);
            lobbyInfo.CreateFunctionElement($"Lobby Version: {LobbyVersion}", Color.white, null);
            lobbyInfo.CreateFunctionElement($"Player Count: {PlayerCount}", Color.white, null);

            var gameInfo = lobby.CreateSubPanel("Game Info", Color.white);
            gameInfo.CreateFunctionElement($"Level Name: {LevelName}", Color.white, null);
        }
    }
}
