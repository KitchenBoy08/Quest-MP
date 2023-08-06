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
using UnityEngine.Rendering.Universal;
using LabFusion.Core.src.BoneMenu;
using System.Net;
using System.Runtime.InteropServices;
using Il2CppSystem;
using LabFusion.MonoBehaviours;

namespace LabFusion.Network
{
    public class RiptideNetworkLayer : NetworkLayer
    {
        internal override string Title => "Riptide";

        // TODO: VC
        public static GameObject voiceObject;
        public static UnityVoice voiceScript;

        // AsyncCallbacks are bad!
        // In Unity/Melonloader, they can cause random crashes, especially when making a lot of calls
        public const bool AsyncCallbacks = false;
        public static Server currentserver { get; set; }
        public static Client currentclient { get; set; }
        public static string publicIp;

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
            IPGetter.GetExternalIP(OnExternalIPAddressRetrieved);
            PlayerIdManager.SetUsername("Riptide Enjoyer");

            FusionLogger.Log("Initialized Riptide Layer");
        }

        internal override void OnLateInitializeLayer()
        {
            PlayerIdManager.SetUsername("Riptide Enjoyer");

            HookRiptideEvents();
        }

        private void OnExternalIPAddressRetrieved(string ipAddress)
        {
            if (!string.IsNullOrEmpty(ipAddress))
            {
#if DEBUG
                Debug.Log($"External IP address: {ipAddress}");
#endif
                publicIp = ipAddress;
            }
            else
            {
                Debug.LogError("Failed to retrieve external IP address.");
            }
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
            string Username = ("Riptide Enjoyer " + userId);
            return Username;
        }

        internal override bool IsFriend(ulong userId) {
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

        internal override void OnVoiceBytesReceived(PlayerId id, byte[] bytes)
        {
            // If we are deafened, no need to deal with voice chat
            if (VoiceHelper.IsDeafened)
                return;

            var handler = VoiceManager.GetVoiceHandler(id);
            handler?.OnVoiceBytesReceived(bytes);
        }

        public static byte[] AudioClipToByteArray(AudioClip audioClip)
        {
            // Get the audio data from the AudioClip.
            float[] samples = new float[audioClip.samples * audioClip.channels];
            audioClip.GetData(samples, 0);

            // Convert the audio data (float[]) to bytes.
            byte[] byteArray = new byte[samples.Length * 4]; // 4 bytes per float (32-bit float).
            for (int i = 0; i < samples.Length; i++)
            {
                byte[] temp = System.BitConverter.GetBytes(samples[i]);
                for (int j = 0; j < temp.Length; j++)
                {
                    byteArray[i * 4 + j] = temp[j];
                }
            }

            return byteArray;
        }

        internal override void OnVoiceChatUpdate()
        {
            /*if (voiceObject != null)
            {
                if (voiceScript != null)
                {
                    if (NetworkInfo.HasServer)
                    {
                        if (VoiceHelper.IsVoiceEnabled)
                        {
                            if (!voiceScript.IsMicrophoneRecording())
                                voiceScript.StartRecording();

                            if (VoiceHelper.IsVoiceEnabled && voiceScript.GetVoiceData() != null)
                            {
                                byte[] voiceData = voiceScript.GetVoiceData();

                                if (voiceData != null)
                                {
                                    PlayerSender.SendPlayerVoiceChat(voiceData, true);
                                    FusionLogger.Log($"Sent voice bytes length: {voiceData.Length}");
                                } else
                                {
                                    FusionLogger.Log("Voice Bytes are null!");
                                }
                            }

                            // Update the manager
                            VoiceManager.Update();
                        }
                    }
                    else
                    {
                        voiceScript.StartRecording();
                    }
                } else
                {
                    FusionLogger.Error("Tried to update Voice Chat, but voiceScript is null!");
                }
            } else
            {
                FusionLogger.Error("Tried to update Voice Chat, but voiceObject is null!");
            }*/
        }

        internal override void StartServer() {
            currentclient = new Client();
            currentserver = new Server();

            currentclient.Connected += OnStarted;
            currentserver.Start(7777, 10);

            currentclient.Connect("127.0.0.1:7777");
        }

        private void OnStarted(object sender, System.EventArgs e) {
#if DEBUG
            FusionLogger.Log("SERVER START HOOKED");
#endif

            // Update player ID here since it's determined on the Riptide Client ID
            PlayerIdManager.SetLongId(currentclient.Id);
            PlayerIdManager.SetUsername($"Riptide Enjoyer {currentclient.Id}");

            _isServerActive = true;
            _isConnectionActive = true;

            OnUpdateRiptideLobby();

            // Call server setup
            InternalServerHelpers.OnStartServer();

            currentserver.ClientDisconnected += OnClientDisconnect;

            currentclient.Connected -= OnStarted;
        }

        private bool isConnecting;
        public void ConnectToServer(string ip) {
            if (!isConnecting)
            {
                currentclient.Connected += OnConnect;
            }

            isConnecting = true;
            // Leave existing server
            if (IsClient || IsServer)
                Disconnect();

            currentclient.Connect(ip + ":7777");
        }

        private void OnConnect(object sender, System.EventArgs e) {
#if DEBUG
            FusionLogger.Log("SERVER CONNECT HOOKED");
#endif
            isConnecting = false;

            currentclient.Disconnected += OnClientDisconnect;
            // Update player ID here since it's determined on the Riptide Client ID
            PlayerIdManager.SetLongId(currentclient.Id);
            PlayerIdManager.SetUsername($"Riptide Enjoyer {currentclient.Id}");

            _isServerActive = false;
            _isConnectionActive = true;

            ConnectionSender.SendConnectionRequest();

            OnUpdateRiptideLobby();

            currentclient.Connected -= OnConnect;
        }

        private void OnClientDisconnect(object sender, Riptide.DisconnectedEventArgs disconnect)
        {
            Disconnect();

            currentclient.Disconnected -= OnClientDisconnect;
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
            MultiplayerHooking.OnMainSceneInitialized += OnMainScene;
            MultiplayerHooking.OnMainSceneInitialized += OnUpdateRiptideLobby;
            GamemodeManager.OnGamemodeChanged += OnGamemodeChanged;
            MultiplayerHooking.OnPlayerJoin += OnUserJoin;
            MultiplayerHooking.OnPlayerLeave += OnPlayerLeave;
            MultiplayerHooking.OnServerSettingsChanged += OnUpdateRiptideLobby;
            MultiplayerHooking.OnDisconnect += OnDisconnect;
        }

        private void OnMainScene()
        {
            if (voiceObject == null)
            {
                voiceObject = new GameObject("UnityVoice");
                voiceScript = voiceObject.AddComponent<UnityVoice>();
                UnityEngine.Object.DontDestroyOnLoad(voiceObject);
                UnityEngine.Object.DontDestroyOnLoad(voiceScript);
            }
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
            MultiplayerHooking.OnMainSceneInitialized -= OnMainScene;
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

            BoneMenuCreator.PopulateServerInfo(category);
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
            string encodedIP = IPSafety.IPSafety.EncodeIPAddress(publicIp);

            Clipboard.SetText(encodedIP);
        }

        private void OnUpdateCreateServerText() {
            if (currentclient.IsConnected)
                _createServerElement.SetName("Disconnect");
            else if (currentclient.IsConnected == false)
                _createServerElement.SetName("Create Server");
        }
        public static FunctionElement _joinCodeElement;
        private FunctionElement _targetServerElement;
        private void CreateManualJoiningMenu(MenuCategory category) {
            if (!HelperMethods.IsAndroid()) {
                category.CreateFunctionElement("Join Server", Color.green, OnClickJoinServer);
                _targetServerElement = category.CreateFunctionElement($"Server ID: {_targetServerIP}", Color.white, null);
                category.CreateFunctionElement("Paste Server ID from Clipboard", Color.white, OnPasteServerIP);

                KeyboardCreator.CreateKeyboard(category, FusionPreferences.ClientSettings.ServerCode);
            }
            else {
                if (FusionPreferences.ClientSettings.ServerCode == "PASTE SERVER CODE HERE") {
                    category.CreateFunctionElement("ERROR: CLICK ME", Color.red, OnClickCodeError);
                }
                else {
                    _joinCodeElement = category.CreateFunctionElement($"Join Server Code: {FusionPreferences.ClientSettings.ServerCode.GetValue()}", Color.green, OnClickJoinServer);
                }
                KeyboardCreator.CreateKeyboard(category, FusionPreferences.ClientSettings.ServerCode);
            }
        }
        public static void OnSetCode()
        {
            _joinCodeElement.SetName($"Join Server Code: {FusionPreferences.ClientSettings.ServerCode.GetValue()}");
        }

        private void OnClickJoinServer() {
            string code = FusionPreferences.ClientSettings.ServerCode;
            if (code.Contains(".")) {
                ConnectToServer(code);
            }
            else {
                string decodedIp = IPSafety.IPSafety.DecodeIPAddress(code);
                ConnectToServer(decodedIp);
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
                        FusionPreferences.ClientSettings.ServerCode.SetValue(serverCode);
                        _targetServerIP = serverCode;
                        _targetServerElement.SetName($"Server ID: {serverCode}");
                    }
                    else {
                        string decodedIP = IPSafety.IPSafety.DecodeIPAddress(serverCode);
                        _targetServerIP = decodedIP;
                        string encodedIP = IPSafety.IPSafety.EncodeIPAddress(decodedIP);
                        FusionPreferences.ClientSettings.ServerCode.SetValue(encodedIP);
                        _targetServerElement.SetName($"Server ID: {encodedIP}");
                    }
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
            string ip = publicIp;
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

        internal override bool CheckSupported()
        {
            return true;
        }

        internal override bool CheckValidation()
        {
            return true;
        }
    }
}