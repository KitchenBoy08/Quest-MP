using System;
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
using LabFusion.Patching;
using System.Drawing;
using JetBrains.Annotations;
using LabFusion.Core.src.Network.Riptide;
using static LabFusion.IPSafety.IPSafety;

namespace LabFusion.Network
{
    public class RiptideNetworkLayer : NetworkLayer
    {
        // TODO: VC
        private string chosenMic;

        // AsyncCallbacks are bad!
        // In Unity/Melonloader, they can cause random crashes, especially when making a lot of calls
        public const bool AsyncCallbacks = false;
        Server currentserver { get; set; }
        Client currentclient { get; set; }

        internal override bool IsServer => _isServerActive;

        internal override bool IsClient => _isConnectionActive;

        protected bool _isServerActive = false;
        protected bool _isConnectionActive = false;

        internal override bool ServerCanSendToHost => true;

        protected string _targetServerIP;

        internal override void OnInitializeLayer() {
            currentclient = new Client();
            currentserver = new Server();

            // Initialize RiptideLogger if in DEBUG
#if DEBUG
            RiptideLogger.Initialize(MelonLogger.Msg, MelonLogger.Msg, MelonLogger.Warning, MelonLogger.Error, false);
#endif

            FusionLogger.Log("Initialized Riptide Layer");
        }

        internal override void OnLateInitializeLayer() {
            if (HelperMethods.IsAndroid())
                PlayerIdManager.SetUsername("Riptide User (QUEST)");
            else
                PlayerIdManager.SetUsername("Riptide User (PC)");

            if (FusionPreferences.ClientSettings.Nickname.GetValue() == null)
            {
                FusionPreferences.ClientSettings.Nickname.SetValue($"Player");
            }

            HookRiptideEvents();
        }

        internal override void OnCleanupLayer() {
            Disconnect();

            UnHookRiptideEvents();
        }

        internal override void OnUpdateLayer() {
            if (currentserver != null)
            {
                currentserver.Update();
            }
            if (currentclient != null)
            {
                currentclient.Update();
            }
        }

        internal override string GetUsername(ulong userId) {
            // Find a way to get nickname, this will do for testing
            string Username = ("Player" + userId);
            return Username;
        }

        internal override bool IsFriend(ulong userId) {
            // Currently there's no Friend system in Place and probably isn't needed, so we always return false
            return false;
        }

        internal override void BroadcastMessage(NetworkChannel channel, FusionMessage message) {
            if (IsServer) {
                currentserver.SendToAll(RiptideHandler.PrepareMessage(message, channel));
            }
            else {
                currentclient.Send(RiptideHandler.PrepareMessage(message, channel));
            }
        }

        internal override void SendToServer(NetworkChannel channel, FusionMessage message) {
            currentclient.Send(RiptideHandler.PrepareMessage(message, channel));
        }

        internal override void SendFromServer(byte userId, NetworkChannel channel, FusionMessage message) {
            var id = PlayerIdManager.GetPlayerId(userId);
            if (id != null)
                SendFromServer(id.LongId, channel, message);

        }

        internal override void SendFromServer(ulong userId, NetworkChannel channel, FusionMessage message) {
            if (IsServer) {
                if (userId == PlayerIdManager.LocalLongId)
                    currentserver.Send(RiptideHandler.PrepareMessage(message, channel), (ushort)PlayerIdManager.LocalLongId);
                else if (currentserver.TryGetClient((ushort)userId, out Connection client))
                    currentserver.Send(RiptideHandler.PrepareMessage(message, channel), client);
            }
        }

        internal override void StartServer() {
            currentclient = new Client();
            currentserver = new Server();

            currentclient.Connected += OnStarted;
            currentserver.Start(7777, 10);

            currentclient.Connect("127.0.0.1:7777");
        }

        private void OnStarted(object sender, EventArgs e) {
#if DEBUG
            FusionLogger.Log("SERVER START HOOKED");
#endif

            //Update player id here just to be safe
            PlayerIdManager.SetLongId(currentclient.Id);

            _isServerActive = true;
            _isConnectionActive = true;

            OnUpdateRiptideLobby();

            // Call server setup
            InternalServerHelpers.OnStartServer();

            currentserver.ClientDisconnected += OnClientDisconnect;

            currentclient.Connected -= OnStarted;
        }

        public void ConnectToServer(string ip) {

            currentclient.Connected += OnConnect;
            // Leave existing server
            if (IsClient || IsServer)
                Disconnect();

            currentclient.Connect(ip + ":7777");
        }

        private void OnConnect(object sender, EventArgs e) {
#if DEBUG
            FusionLogger.Log("SERVER CONNECT HOOKED");
#endif
            // Update player ID here since it's determined on the Riptide Client ID
            PlayerIdManager.SetLongId(currentclient.Id);

            _isServerActive = false;
            _isConnectionActive = true;

            ConnectionSender.SendConnectionRequest();

            OnUpdateRiptideLobby();

            currentclient.Connected -= OnConnect;
        }

        internal override void Disconnect(string reason = "") {
            if (currentclient.IsConnected)
                currentclient.Disconnect();

            if (currentserver.IsRunning)
                currentserver.Stop();

            _isServerActive = false;
            _isConnectionActive = false;

            InternalServerHelpers.OnDisconnect(reason);

            OnUpdateRiptideLobby();
        }

        private void HookRiptideEvents() {
            // Add server hooks
            MultiplayerHooking.OnMainSceneInitialized += OnUpdateRiptideLobby;
            GamemodeManager.OnGamemodeChanged += OnGamemodeChanged;
            MultiplayerHooking.OnPlayerJoin += OnUserJoin;
            MultiplayerHooking.OnPlayerLeave += OnPlayerLeave;
            MultiplayerHooking.OnServerSettingsChanged += OnUpdateRiptideLobby;
            MultiplayerHooking.OnDisconnect += OnDisconnect;
        }

        private void OnGamemodeChanged(Gamemode gamemode) {
            OnUpdateRiptideLobby();
        }

        private void OnPlayerJoin(PlayerId id) {
            if (!id.IsSelf) {
                VoiceManager.GetVoiceHandler(id);
            }
            OnUpdateRiptideLobby();
        }

        private void OnPlayerLeave(PlayerId id) {
            VoiceManager.Remove(id);

            OnUpdateRiptideLobby();
        }

        private void OnDisconnect() {
            VoiceManager.RemoveAll();
        }

        private void UnHookRiptideEvents() {
            // Remove server hooks
            MultiplayerHooking.OnMainSceneInitialized -= OnUpdateRiptideLobby;
            GamemodeManager.OnGamemodeChanged -= OnGamemodeChanged;
            MultiplayerHooking.OnPlayerJoin -= OnPlayerJoin;
            MultiplayerHooking.OnPlayerLeave -= OnPlayerLeave;
            MultiplayerHooking.OnServerSettingsChanged -= OnUpdateRiptideLobby;
            MultiplayerHooking.OnDisconnect -= OnDisconnect;
        }

        private void OnUpdateRiptideLobby() {
            // Update bonemenu items
            OnUpdateCreateServerText();
        }

        internal override void OnSetupBoneMenu(MenuCategory category) {
            // Create the basic options
            CreateMatchmakingMenu(category);
            CreateMicrophoneMenu(category);
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

        private void CreateMatchmakingMenu(MenuCategory category) {
            // Root category
            var matchmaking = category.CreateCategory("Matchmaking", Color.red);

            // Server making
            _serverInfoCategory = matchmaking.CreateCategory("Server Info", Color.white);
            CreateServerInfoMenu(_serverInfoCategory);

            // Manual joining
            _manualJoiningCategory = matchmaking.CreateCategory("Manual Joining", Color.white);
            CreateManualJoiningMenu(_manualJoiningCategory);
        }

        private FunctionElement _createServerElement;

        private void CreateServerInfoMenu(MenuCategory category) {
            _createServerElement = category.CreateFunctionElement("Create Server", Color.white, OnClickCreateServer);
            if (!HelperMethods.IsAndroid()) {
                category.CreateFunctionElement("Copy Server Code to Clipboard", Color.white, OnCopyServerCode);
            }
            category.CreateFunctionElement("Display Server Code", Color.white, OnDisplayServerCode);

            BoneMenuCreator.CreatePlayerListMenu(category);
            BoneMenuCreator.CreateAdminActionsMenu(category);
        }

        private void OnClickCreateServer() {
            // Is a server already running? Disconnect then create server.
            if (currentclient.IsConnected) {
                Disconnect();
            }
            else {
                StartServer();
            }
        }

        private void OnCopyServerCode() {
            string ip = IPSafety.IPSafety.GetPublicIP();
            string encodedIP = IPSafety.IPSafety.EncodeIPAddress(ip);

            Clipboard.SetText(encodedIP);
        }

        private void OnUpdateCreateServerText() {
            if (currentclient.IsConnected)
                _createServerElement.SetName("Disconnect");
            else if (currentclient.IsConnected == false)
                _createServerElement.SetName("Create Server");
        }

        private FunctionElement _targetServerElement;
        private void CreateManualJoiningMenu(MenuCategory category) {
            if (!HelperMethods.IsAndroid()) {
                category.CreateFunctionElement("Join Server", Color.green, OnClickJoinServer);
                _targetServerElement = category.CreateFunctionElement($"Server ID: {_targetServerIP}", Color.white, null);
                category.CreateFunctionElement("Paste Server ID from Clipboard", Color.white, OnPasteServerIP);
            }
            else {
                if (FusionPreferences.ClientSettings.ServerCode == "PASTE SERVER CODE HERE") {
                    category.CreateFunctionElement("ERROR: CLICK ME", Color.red, OnClickCodeError);
                }
                else {
                    category.CreateFunctionElement($"Join Server Code: {FusionPreferences.ClientSettings.ServerCode.GetValue()}", Color.green, OnClickJoinServer);
                }
            }
        }

        private void OnClickJoinServer() {
            if (!HelperMethods.IsAndroid()) {
                ConnectToServer(_targetServerIP);
            }
            else {
                string code = FusionPreferences.ClientSettings.ServerCode;
                if (code.Contains(".")) {
                    ConnectToServer(code);
                }
                else {
                    string decodedIp = IPSafety.IPSafety.DecodeIPAddress(code);
                    ConnectToServer(decodedIp);
                }
            }
        }

        private void OnPasteServerIP() {
            if (!HelperMethods.IsAndroid()) {
                if (!Clipboard.ContainsText()) {
                    return;
                }
                else {
                    string serverCode = Clipboard.GetText();

                    if (serverCode.Contains(".")) {
                        _targetServerIP = serverCode;
                        _targetServerElement.SetName($"Server ID: {serverCode}");
                    }
                    else {
                        string decodedIP = IPSafety.IPSafety.DecodeIPAddress(serverCode);
                        _targetServerIP = decodedIP;
                        string encodedIP = IPSafety.IPSafety.EncodeIPAddress(decodedIP);
                        _targetServerElement.SetName($"Server ID: {encodedIP}");
                    }
                }
            }
            else {

                string serverCode = FusionPreferences.ClientSettings.ServerCode.GetValue();

                if (serverCode.Contains(".")) {
                    _targetServerIP = serverCode;
                }
                else if (serverCode == "PASTE SERVER CODE HERE") {
                    FusionNotifier.Send(new FusionNotification()
                    {
                        title = "Code is Null",
                        showTitleOnPopup = true,
                        isMenuItem = false,
                        isPopup = true,
                        message = $"No server code has been put in FusionPreferences!",
                        popupLength = 5f,
                    });
                } else {
                    string decodedIP = IPSafety.IPSafety.DecodeIPAddress(serverCode);
                    _targetServerIP = decodedIP;
                }
            }
        }

        private void OnClientDisconnect(object sender, ServerDisconnectedEventArgs client)
        {
            // Update the mod so it knows this user has left
            InternalServerHelpers.OnUserLeave(client.Client.Id);

            // Send disconnect notif to everyone
            ConnectionSender.SendDisconnect(client.Client.Id, client.Reason.ToString());
        }

        // For Unity's microphone stuff. Will do later
        private void CreateMicrophoneMenu(MenuCategory category)
        {
            // Root category
            var micSettings = category.CreateCategory("Microphone Settings", Color.white);

            LoadMics(micSettings);
        }

        private void LoadMics(MenuCategory category)
        {
            category.Elements.Clear();

            category.CreateFunctionElement("Refresh Microphones", Color.yellow, () => LoadMics(category));

            foreach (string mic in Microphone.devices)
            {
                category.CreateFunctionElement($"Set Mic: {Environment.NewLine} {mic}", Color.white, () => SetMicrophone(mic));
            }
        }

        private void SetMicrophone(string micName)
        {
            chosenMic = micName;
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
    }
}