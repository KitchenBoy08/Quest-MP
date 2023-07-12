﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BoneLib.BoneMenu;
using BoneLib.BoneMenu.Elements;

using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Preferences;

using SLZ.Rig;

using Riptide;
using Riptide.Transports;
using Riptide.Utils;

using UnityEngine;

using Color = UnityEngine.Color;

using MelonLoader;

using System.Windows.Forms;

using LabFusion.Senders;
using LabFusion.BoneMenu;

using System.IO;

using UnhollowerBaseLib;
using LabFusion.SDK.Gamemodes;
using Steamworks;
using BoneLib;
using System.Windows.Forms.DataVisualization.Charting;

namespace LabFusion.Network
{
    public class RiptideNetworkLayer : NetworkLayer
    {

        private FunctionElement _createServerElement;

        protected string _targetServerIP;

        // AsyncCallbacks are bad!
        // In Unity/Melonloader, they can cause random crashes, especially when making a lot of calls
        public const bool AsyncCallbacks = false;
        Server currentserver { get; set; }
        Client currentclient { get; set; }

        /// <summary>
        /// Returns true if this layer is hosting a server.
        /// </summary>
        internal override bool IsServer => _isServerActive;


        /// <summary>
        /// Returns true if this layer is a client inside of a server (still returns true if this is the host!)
        /// </summary>
        internal override bool IsClient => _isConnectionActive;

        protected bool _isServerActive = false;
        protected bool _isConnectionActive = false;

        /// <summary>
        /// Returns true if the networking solution allows the server to send messages to the host (Actual Server Logic vs P2P).
        /// </summary>
        /// Riptide should be able to, consider removing this since it's already true in the inherited class
        internal override bool ServerCanSendToHost => true;

        /// <summary>
        /// Returns the current active lobby.
        /// </summary>

        /// <summary>
        /// Starts the server.
        /// </summary>
        internal override void StartServer()
        {
            currentclient.Connected += OnStarted;
            currentserver.Start(7777, 10);

            currentclient.Connect("127.0.0.1:7777");
        }

        private void OnStarted(object sender, EventArgs e)
        {
            FusionLogger.Log("SERVER START HOOKED");

            //Update player id here just to be safe
            PlayerIdManager.SetLongId(currentclient.Id);

            ConnectionSender.SendConnectionRequest();

            // Call server setup
            InternalServerHelpers.OnStartServer();
            _isServerActive = true;
            _isConnectionActive = true;

            OnUpdateRiptideLobby();
            currentclient.Connected -= OnStarted;
        }

        /// <summary>
        /// Disconnects the client from the connection and/or server.
        /// </summary>
        internal override void Disconnect(string reason = "")
        {
            currentclient.Disconnect();
            if (IsServer)
            {
                currentserver.Stop();
            }

            InternalServerHelpers.OnDisconnect(reason);
            OnUpdateRiptideLobby();

            _isServerActive = false;
            _isConnectionActive = false;

            FusionLogger.Log($"Disconnected from server because: {reason}");
        }

        

        /// <summary>
        /// Returns the username of the player with id userId.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// This should maybe return a username determined from a Melonpreference or oculus platform, sent over the net
        /// (Not in this method, it should be done upon connection)
        internal override string GetUsername(ulong userId)
        {
            // Find a way to get nickname, this will do for testing
            string Username = ("Player" + userId);
            return Username;
        }

        /// <summary>
        /// Returns true if this is a friend (ex. steam friends).
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        internal override bool IsFriend(ulong userId)
        {
            // Currently there's no Friend system in Place and probably isn't needed, so we always return false
            return false;
        }

        /// <summary>
        /// Sends the message to the specified user if this is a server.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        internal override void SendFromServer(byte userId, NetworkChannel channel, FusionMessage message)
        {
            var id = PlayerIdManager.GetPlayerId(userId);
            if (id != null)
            {
                ushort riptideid = (ushort)id;
                SendFromServer(riptideid, channel, message);
#if DEBUG
                FusionLogger.Log("SentFromServerMethod1");
#endif
            }

        }

        /// <summary>
        /// Sends the message to the specified user if this is a server.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        internal override void SendFromServer(ulong userId, NetworkChannel channel, FusionMessage message)
        {
            if (IsServer)
            {
                Riptide.Message riptideMessage = RiptideHandler.PrepareMessage(message, channel);
                // This should determine user riptide id from fusion player metadata
                ushort riptideid = (ushort)userId;
                currentserver.Send(riptideMessage, riptideid);
#if DEBUG
                FusionLogger.Log("SentFromServerMethod2");
#endif
            }
        }

        /// <summary>
        /// Sends the message to the dedicated server.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        internal override void SendToServer(NetworkChannel channel, FusionMessage message)
        {
            Riptide.Message riptidemessage = RiptideHandler.PrepareMessage(message, channel);
            currentclient.Send(riptidemessage);
        }

        /// <summary>
        /// Sends the message to the server if this is a client. Sends to all clients if this is a server.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        internal override void BroadcastMessage(NetworkChannel channel, FusionMessage message)
        {
            Riptide.Message riptidemessage = RiptideHandler.PrepareMessage(message, channel);
            if (!IsServer)
            {
                currentclient.Send(riptidemessage);
            }
            else
            {
                currentserver.SendToAll(riptidemessage);
            }

        }

        /// <summary>
        /// If this is a server, sends this message back to all users except for the provided id.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        internal override void BroadcastMessageExcept(byte userId, NetworkChannel channel, FusionMessage message, bool ignoreHost = true)
        {
            for (var i = 0; i < PlayerIdManager.PlayerIds.Count; i++)
            {
                var id = PlayerIdManager.PlayerIds[i];

                if (id.SmallId != userId && (id.SmallId != 0 || !ignoreHost))
                    SendFromServer(id.SmallId, channel, message);
            }
        }

        internal override void OnInitializeLayer()
        {

            // Initialize RiptideLogger
#if DEBUG
            RiptideLogger.Initialize(MelonLogger.Msg, MelonLogger.Msg, MelonLogger.Warning, MelonLogger.Error, false);
#endif

            // Initialize currentclient only if it is null
            if (currentclient == null)
            {
                currentclient = new Client();
            }
            if (currentserver == null)
            {
                currentserver = new Server();
            }

            PlayerIdManager.SetLongId(currentclient.Id);

            if (currentclient.Id == 0)
            {
                FusionLogger.Warn("Player Long Id is 0 and something is probably wrong");
            }
            else
            {
                FusionLogger.Log($"Player Long Id is {currentclient.Id}");
            }

            if (FusionPreferences.ClientSettings.Nickname != null)
            {
                PlayerIdManager.SetUsername(FusionPreferences.ClientSettings.Nickname);
            }
            else
            {
                PlayerIdManager.SetUsername("Player " + currentclient.Id);
            }

            FusionLogger.Log("Initialized Riptide Layer");
        }

        // Probably nothing to do here
        internal override void OnLateInitializeLayer() {
            HookRiptideEvents();
        }

        internal override void OnCleanupLayer()
        {
            Disconnect();

            UnHookRiptideEvents();
            // Clean up lobbies here once that is implemented 
        }

        internal override void OnUpdateLayer()
        {
            if (currentserver != null)
            {
                currentserver.Update();
            }
            currentclient.Update();
        }

        internal override void OnLateUpdateLayer() { }

        internal override void OnGUILayer() { }

        internal override void OnVoiceChatUpdate() { }

        internal override void OnVoiceBytesReceived(PlayerId id, byte[] bytes) { }

        internal override void OnUserJoin(PlayerId id) {
            OnUpdateRiptideLobby();
        }

        public void ConnectToServer(string ip)
        {
            currentclient.Connected += OnConnect;
            // Leave existing server
            if (IsClient || IsServer)
                Disconnect();

            currentclient.Connect(ip + ":7777");
        }

        private void OnConnect(object sender, EventArgs e)
        {
            _isServerActive = false;
            _isConnectionActive = true;

            //Update player id here just to be safe
            PlayerIdManager.SetLongId(currentclient.Id);

            ConnectionSender.SendConnectionRequest();

            OnUpdateRiptideLobby();
            
            currentclient.Connected -= OnConnect;
        }

        private void UnHookRiptideEvents()
        {
            // Remove server hooks
            MultiplayerHooking.OnMainSceneInitialized -= OnUpdateRiptideLobby;
            GamemodeManager.OnGamemodeChanged -= OnGamemodeChanged;
            MultiplayerHooking.OnPlayerJoin -= OnPlayerJoin;
            MultiplayerHooking.OnPlayerLeave -= OnPlayerLeave;
            MultiplayerHooking.OnServerSettingsChanged -= OnUpdateRiptideLobby;
        }

        private void HookRiptideEvents()
        {
            // Add server hooks
            MultiplayerHooking.OnMainSceneInitialized += OnUpdateRiptideLobby;
            GamemodeManager.OnGamemodeChanged += OnGamemodeChanged;
            MultiplayerHooking.OnPlayerJoin += OnUserJoin;
            MultiplayerHooking.OnPlayerLeave += OnPlayerLeave;
            MultiplayerHooking.OnServerSettingsChanged += OnUpdateRiptideLobby;
        }

        private void OnPlayerLeave(PlayerId id)
        {
            /*
            RiptideVoiceIdentifier.RemoveVoiceIdentifier(id);
            */
            OnUpdateRiptideLobby();
        }

        private void OnPlayerJoin(PlayerId id)
        {
            /*
            if (!id.IsSelf)
                RiptideVoiceIdentifier.GetVoiceIdentifier(id);
            */
            OnUpdateRiptideLobby();
        }

        private void OnUpdateRiptideLobby()
        {
            // Update bonemenu items
            OnUpdateCreateServerText();
        }
        private void OnUpdateCreateServerText()
        {
            if (IsClient)
                _createServerElement.SetName("Disconnect from Server");
            else if (IsClient == false)
                _createServerElement.SetName("Create Server");
        }

        private void OnGamemodeChanged(Gamemode gamemode)
        {
            OnUpdateRiptideLobby();
        }

        internal override void OnSetupBoneMenu(MenuCategory category)
        {
            // Create the basic options
            CreateMatchmakingMenu(category);
            BoneMenuCreator.CreateGamemodesMenu(category);
            BoneMenuCreator.CreateSettingsMenu(category);
            BoneMenuCreator.CreateNotificationsMenu(category);
            BoneMenuCreator.CreateBanListMenu(category);

#if DEBUG
            // Debug only (dev tools)
            BoneMenuCreator.CreateDebugMenu(category);
#endif
        }

        // Matchmaking menu
        private MenuCategory _serverInfoCategory;
        private MenuCategory _manualJoiningCategory;

        private void CreateMatchmakingMenu(MenuCategory category)
        {
            // Root category
            var matchmaking = category.CreateCategory("Matchmaking", Color.red);

            // Server making
            _serverInfoCategory = matchmaking.CreateCategory("Server Info", Color.white);
            CreateServerInfoMenu(_serverInfoCategory);

            // Manual joining
            _manualJoiningCategory = matchmaking.CreateCategory("Manual Joining", Color.white);
            CreateManualJoiningMenu(_manualJoiningCategory);
        }

        private FunctionElement _targetServerElement;
        private void CreateManualJoiningMenu(MenuCategory category)
        {
            if (!HelperMethods.IsAndroid())
            {
                category.CreateFunctionElement("Join Server", Color.green, OnClickJoinServer);
                _targetServerElement = category.CreateFunctionElement($"Server ID: {_targetServerIP}", Color.white, null);
                category.CreateFunctionElement("Paste Server ID from Clipboard", Color.white, OnPasteServerIP);
            }
            else
            {
                if (FusionPreferences.ClientSettings.ServerCode == "PASTE SERVER CODE HERE")
                {
                    category.CreateFunctionElement("ERROR: CLICK ME", Color.red, OnClickCodeError);
                } else
                {
                    category.CreateFunctionElement($"Join Server Code: {FusionPreferences.ClientSettings.ServerCode}", Color.green, OnClickJoinServer);
                }
            }
        }

        private void OnClickCodeError()
        {
            FusionNotifier.Send(new FusionNotification()
            {
                title = "Code is Null",
                showTitleOnPopup = true,
                isMenuItem = false,
                isPopup = true,
                message = $"No server code has been put in FusionPreferences!",
                popupLength = 5f,
            });
        }

        private void OnPasteServerIP()
        {
            if (!HelperMethods.IsAndroid())
            {
                if (!Clipboard.ContainsText())
                {
                    return;
                }
                else
                {
                    string serverCode = Clipboard.GetText();

                    if (serverCode.Contains("."))
                    {
                        _targetServerIP = serverCode;
                        _targetServerElement.SetName($"Server ID: {serverCode}");
                    }
                    else
                    {
                        string decodedIP = IPSafety.IPSafety.DecodeIPAddress(serverCode);
                        _targetServerIP = decodedIP;
                        string encodedIP = IPSafety.IPSafety.EncodeIPAddress(decodedIP);
                        _targetServerElement.SetName($"Server ID: {encodedIP}");
                    }
                }
            } else
            {

                string serverCode = FusionPreferences.ClientSettings.ServerCode;

                if (serverCode.Contains("."))
                {
                    _targetServerIP = serverCode;
                } else if (serverCode == "PASTE SERVER CODE HERE")
                {
                    FusionNotifier.Send(new FusionNotification()
                    {
                        title = "Code is Null",
                        showTitleOnPopup = true,
                        isMenuItem = false,
                        isPopup = true,
                        message = $"No server code has been put in FusionPreferences!",
                        popupLength = 5f,
                    });
                }
                {
                    string decodedIP = IPSafety.IPSafety.DecodeIPAddress(serverCode);
                    _targetServerIP = decodedIP;
                }
            }
        }

        private void CreateServerInfoMenu(MenuCategory category)
        {
            _createServerElement = category.CreateFunctionElement("Create Server", Color.white, OnClickCreateServer);
            if (!HelperMethods.IsAndroid())
            {
                category.CreateFunctionElement("Copy Server Code to Clipboard", Color.white, OnCopyServerCode);
            }
            category.CreateFunctionElement("Display Server Code", Color.white, OnDisplayServerCode);

            BoneMenuCreator.CreatePlayerListMenu(category);
            BoneMenuCreator.CreateAdminActionsMenu(category);
        }

        private void OnDisplayServerCode()
        {
            string ip = IPSafety.IPSafety.GetPublicIP();
            string encodedIP = IPSafety.IPSafety.EncodeIPAddress(ip);

            FusionNotifier.Send(new FusionNotification()
            {
                title = "Server Code",
                showTitleOnPopup = true,
                isMenuItem = false,
                isPopup = true,
                message = $"Code: {encodedIP}",
                popupLength = 20f,
            });
        }

        private void OnCopyServerCode()
        {
            string ip = IPSafety.IPSafety.GetPublicIP();
            string encodedIP = IPSafety.IPSafety.EncodeIPAddress(ip);

            Clipboard.SetText(encodedIP);
        }

        private void OnClickJoinServer()
        {
            if (!HelperMethods.IsAndroid())
            {
                ConnectToServer(_targetServerIP);
            }
        }

        private void OnClickCreateServer()
        {
            // Is a server already running? Disconnect then create server.
            if (IsClient)
            {
                Disconnect();
            } else
            {
                StartServer();
            }

        }
    }
}
