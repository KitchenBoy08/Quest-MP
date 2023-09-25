using System.Collections.Generic;

using BoneLib.BoneMenu.Elements;

using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Preferences;
using LabFusion.MonoBehaviours;
using Il2CppSystem;

using Unity.XR;

using Riptide;
using Riptide.Utils;

using UnityEngine;

using Color = UnityEngine.Color;

using MelonLoader;

using System.Windows.Forms;

using LabFusion.Senders;
using LabFusion.BoneMenu;
using LabFusion.Core.src.BoneMenu;

using LabFusion.SDK.Gamemodes;
using BoneLib;
using System.Threading.Tasks;
using System;
using System.ServiceModel.Channels;
using System.Linq;
using System.Net.NetworkInformation;
using LabFusion.Data;
using System.Reflection;
using LabFusion.Network;
using UnityEngine.UIElements;
using System.Drawing;
using LabFusion.Core.src.Network.Riptide.Enums;
using LabFusion.Syncables;
using SLZ.Marrow.Warehouse;
using SLZ.Marrow.Pool;
using System.Windows.Forms.DataVisualization.Charting;

namespace LabFusion.Network
{
    public class RiptideNetworkLayer : NetworkLayer
    {
        public static class CurrentServerType
        {
            private static ServerTypes type;

            public static void SetType(ServerTypes type)
            {
                CurrentServerType.type = type;
            }

            public static ServerTypes GetType()
            {
                return type;
            }
        }

        internal override string Title => "Riptide";

        // AsyncCallbacks are bad!
        // In Unity/Melonloader, they can cause random crashes, especially when making a lot of calls
        public const bool AsyncCallbacks = false;
        public static Server currentserver = new();
        public static Client currentclient = new();

        public static string publicIp;
        public static bool isHost = false;

        internal override bool IsServer => currentserver.IsRunning || isHost;

        internal override bool IsClient => currentclient.IsConnected;

        private readonly RiptideVoiceManager _voiceManager = new();
        internal override IVoiceManager VoiceManager => _voiceManager;

        internal override bool ServerCanSendToHost => true;

        protected string _targetServerIP;

        internal override void OnInitializeLayer() {
            // Hooking
            currentclient.Disconnected += OnClientDisconnect;

            currentserver.TimeoutTime = 30000;
            currentserver.HeartbeatInterval = 10000;

            currentclient.TimeoutTime = 30000;
            currentclient.HeartbeatInterval = 10000;

            IPGetter.GetExternalIP(OnExternalIPAddressRetrieved);

            FusionLogger.Log("Initialized Riptide Layer");
        }

        internal override void OnLateInitializeLayer()
        {
            PlayerIdManager.SetUsername("Riptide Enjoyer");

            PortHelper.OpenPort();

            HookRiptideEvents();
        }

        private void OnExternalIPAddressRetrieved(string ipAddress)
        {
            if (!string.IsNullOrEmpty(ipAddress))
            {
                publicIp = ipAddress;
            }
            else
            {
                FusionLogger.Error("Failed to retrieve external IP address.");
            }
        }

        internal override void OnCleanupLayer() {
            Disconnect();

            UnHookRiptideEvents();

            PortHelper.ClosePort();
        }

        internal override void OnUpdateLayer() {
            currentserver.Update();
            currentclient.Update();
        }

        internal override string GetUsername(ulong userId) {
            // Find a way to get nickname, this will do for testing
            string Username = ($"Riptide Enjoyer {userId}");
            return Username;
        }

        internal override bool IsFriend(ulong userId) {
            return false;
        }

        internal override void BroadcastMessage(NetworkChannel channel, FusionMessage message) {
            if (CurrentServerType.GetType() == ServerTypes.DEDICATED)
            {
                if (isHost)
                {
                    currentclient.Send(RiptideHandler.PrepareMessage(message, channel, 3));
                }
                else
                {
                    currentclient.Send(RiptideHandler.PrepareMessage(message, channel, 2));
                }
            } else
            {
                if (IsServer)
                {
                    currentserver.SendToAll(RiptideHandler.PrepareMessage(message, channel, 0));
                }
                else
                {
                    currentclient.Send(RiptideHandler.PrepareMessage(message, channel, 0));
                }
            }
        }

        internal override void SendToServer(NetworkChannel channel, FusionMessage message) {
            if (CurrentServerType.GetType() == ServerTypes.DEDICATED)
            {
                currentclient.Send(RiptideHandler.PrepareMessage(message, channel, 2));
            } else
            {
                currentclient.Send(RiptideHandler.PrepareMessage(message, channel, 0));
            }
        }

        internal override void SendFromServer(byte userId, NetworkChannel channel, FusionMessage message) {
            var id = PlayerIdManager.GetPlayerId(userId);
            if (id != null)
                SendFromServer(id.LongId, channel, message);
        }

        internal override void SendFromServer(ulong userId, NetworkChannel channel, FusionMessage message) {
            if (CurrentServerType.GetType() == ServerTypes.DEDICATED)
            {
                if (isHost)
                {
                    currentclient.Send(RiptideHandler.PrepareMessage(message, channel, 4, (ushort)userId));
                }
            } else
            {
                if (IsServer)
                {
                    if (userId == PlayerIdManager.LocalLongId)
                        currentserver.Send(RiptideHandler.PrepareMessage(message, channel, 0), (ushort)PlayerIdManager.LocalLongId);
                    else if (currentserver.TryGetClient((ushort)userId, out Riptide.Connection client))
                        currentserver.Send(RiptideHandler.PrepareMessage(message, channel, 0), client);
                }
            }
        }

        internal override void StartServer()
        {
            // Making sure the server is fully started before calling start things
            currentclient.Connected += OnStarted;

            // Player cap is set just above Fusion's built in 255 player cap, since Fusion already has a player cap limit system
            currentserver.Start(7777, 256);

            currentclient.Connect("127.0.0.1:7777");
        }

        private void OnStarted(object sender, System.EventArgs e)
        {
            currentserver.ClientDisconnected += OnClientDisconnect;

            currentclient.Connected -= OnStarted;
#if DEBUG
            FusionLogger.Log("SERVER START HOOKED");
#endif
            CurrentServerType.SetType(ServerTypes.P2P);

            // Update player ID here since it's determined on the Riptide Client ID
            PlayerIdManager.SetLongId(currentclient.Id);

            OnUpdateRiptideLobby();

            // Call server setup
            InternalServerHelpers.OnStartServer();
        }

        private bool isConnecting;
        public void ConnectToServer(string ip) {
            if (!isConnecting)
            {
                currentclient.Connected += OnConnect;
            }

            isConnecting = true;

            // Leave existing server
            if (IsClient)
                Disconnect();

            if (ip.Contains("."))
            {
                currentclient.Connect(ip + ":7777");
            }
            else
            {
                string decodedIp = IPSafety.IPSafety.DecodeIPAddress(ip);

                currentclient.Connect(decodedIp + ":7777");
            }
        }

        private void OnConnect(object sender, System.EventArgs e) {
            currentclient.Connected -= OnConnect;
#if DEBUG
            FusionLogger.Log("SERVER CONNECT HOOKED");
#endif
            isConnecting = false;
            OnUpdateRiptideLobby();

            Riptide.Message sent = Riptide.Message.Create(MessageSendMode.Reliable, 1);
            sent.AddString("RequestServerType");
            currentclient.Send(sent);
        }

        public static void OnClientDisconnect(object sender, Riptide.DisconnectedEventArgs disconnect)
        {
            if (currentclient.IsConnected)
                currentclient.Disconnect();

            if (currentserver.IsRunning)
                currentserver.Stop();

            InternalServerHelpers.OnDisconnect(GetDisconnectReason(disconnect.Reason));

            isHost = false;

            OnUpdateRiptideLobby();
        }

        internal override void Disconnect(string reason = "") {
            if (IsClient)
                currentclient.Disconnect();

            if (IsServer)
                currentserver.Stop();

            isHost = false;

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

            // Riptide Hooks
            currentclient.ConnectionFailed += OnConnectionFail;
        }

        private void OnConnectionFail(object sender, ConnectionFailedEventArgs info)
        {
            FusionNotifier.Send(new FusionNotification()
            {
                title = "Connection Failed",
                showTitleOnPopup = true,
                isMenuItem = false,
                isPopup = true,
                message = $"Failed to connect with reason: {GetConnectionFailReason(info.Reason)}",
                popupLength = 1.5f,
            });
            FusionNotifier.Send(new FusionNotification()
            {
                title = "Connection Failed",
                showTitleOnPopup = true,
                isMenuItem = false,
                isPopup = true,
                message = $"Make sure the host has port forwarded and their server is open!",
                popupLength = 1.5f,
            });

            FusionLogger.Error($"Failed to connect to server with error: {info.Message}");
        }

        private string GetConnectionFailReason(RejectReason reason)
        {
            switch (reason)
            {
                case RejectReason.ServerFull:
                    return "Server is Full";
                case RejectReason.NoConnection:
                    return "No Connection";
                case RejectReason.Custom:
                    return "Custom Reason";
                case RejectReason.Rejected:
                    return "Connection Rejected";
                case RejectReason.AlreadyConnected:
                    return "Already Connected to Server";
                default:
                    return @"¯\_(ツ)_/¯";
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
            MultiplayerHooking.OnMainSceneInitialized -= OnUpdateRiptideLobby;
            GamemodeManager.OnGamemodeChanged -= OnGamemodeChanged;
            MultiplayerHooking.OnPlayerJoin -= OnPlayerJoin;
            MultiplayerHooking.OnPlayerLeave -= OnPlayerLeave;
            MultiplayerHooking.OnServerSettingsChanged -= OnUpdateRiptideLobby;
            MultiplayerHooking.OnDisconnect -= OnDisconnect;
        }

        public static void OnUpdateRiptideLobby() {
            // Update bonemenu items
            OnUpdateCreateServerText();
        }

        internal override void OnSetupBoneMenu(MenuCategory category) {
            // Create the basic options
            CreateMatchmakingMenu(category);
            BoneMenuCreator.CreateGamemodesMenu(category);
            CreateRiptideSettings(category);
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

            // Server List
            InitializeServerListCategory(matchmaking);
        }

        private static FunctionElement _nicknameDisplay;
        private static FunctionElement _pingDisplay;
        private void CreateRiptideSettings(MenuCategory category)
        {
            var settings = category.CreateCategory("Riptide Settings", Color.blue);

            // Create Nickname Menu
            var nicknamePanel = settings.CreateCategory("Riptide Nickname", Color.white);
            _nicknameDisplay = nicknamePanel.CreateFunctionElement($"Current Nickname: {FusionPreferences.ClientSettings.Nickname.GetValue()}", Color.white, null);

            KeyboardCreator nicknameKeyboard = new KeyboardCreator();
            nicknameKeyboard.CreateKeyboard(nicknamePanel, "Edit Nickname", FusionPreferences.ClientSettings.Nickname);

            var nicknameInfo = nicknamePanel.CreateSubPanel("Reason for Existing", Color.red);
            nicknameInfo.CreateFunctionElement("Yes, I know", Color.yellow, null);
            nicknameInfo.CreateFunctionElement("`Fusion has this built in`", Color.yellow, null);
            nicknameInfo.CreateFunctionElement("but I wanted a keyboard", Color.yellow, null);

            // Create Connection Info
            var connectionInfo = settings.CreateCategory("Connection Stuff", Color.white);

            var pingMenu = connectionInfo.CreateCategory("Ping Menu", Color.white);
            _pingDisplay = pingMenu.CreateFunctionElement("Ping:\n (REFRESH)", Color.grey, null);
            pingMenu.CreateFunctionElement("Refresh", Color.blue, () =>
            {
                if (currentclient.RTT == -1)
                {
                    _pingDisplay.SetName("Ping not calculated.");
                    return;
                }

                int ping = currentclient.RTT;
                switch (ping)
                {
                    case <= 100:
                        _pingDisplay.SetColor(Color.green);
                        _pingDisplay.SetName($"Ping:\n {ping}");
                        return;
                    case <= 200:
                        _pingDisplay.SetColor(Color.yellow);
                        _pingDisplay.SetName($"Ping:\n {ping}");
                        return;
                    case <= 300:
                        _pingDisplay.SetColor(Color.red);
                        _pingDisplay.SetName($"Ping:\n {ping}");
                        return;
                    case > 300:
                        _pingDisplay.SetColor(Color.black);
                        _pingDisplay.SetName($"Ping:\n {ping}");
                        return;
                }
            });

            // Funny stuff
            /// Maybe later idk
        }

        private static FunctionElement _createServerElement;

        private void CreateServerInfoMenu(MenuCategory category) {
            _createServerElement = category.CreateFunctionElement("Start Server", Color.white, OnClickCreateServer);
            if (!HelperMethods.IsAndroid()) {
                category.CreateFunctionElement("Copy Server Code to Clipboard", Color.white, OnCopyServerCode);
            }
            category.CreateFunctionElement("Display Server Code", Color.white, OnDisplayServerCode);

            BoneMenuCreator.PopulateServerInfo(category);
        }

        private void OnClickCreateServer() {
            // Is a server already running? Disconnect. Otherwise, create server.
            if (currentclient.IsConnected) {
                Disconnect();
            } else
            {
                StartServer();
            }
        }

        private void OnCopyServerCode() {
            string encodedIP = IPSafety.IPSafety.EncodeIPAddress(publicIp);

            Clipboard.SetText(encodedIP);
        }

        public static void OnUpdateCreateServerText() {
            if (currentclient.IsConnected && !currentserver.IsRunning)
            {
                _createServerElement.SetName("Disconnect");
            }
            else if (currentserver.IsRunning)
            {
                _createServerElement.SetName("Stop Server");
            }
            else if (!currentclient.IsConnected)
            {
                _createServerElement.SetName("Start Server");
            }
        }

        public static FunctionElement _joinCodeElement;
        private void CreateManualJoiningMenu(MenuCategory category) {
            _joinCodeElement = category.CreateFunctionElement($"Join Server Code: {FusionPreferences.ClientSettings.ServerCode.GetValue()}", Color.green, OnClickJoinServer);

            KeyboardCreator keyboard = new KeyboardCreator();
            keyboard.CreateKeyboard(category, "Server Code Keyboard", FusionPreferences.ClientSettings.ServerCode);
        }

        public static void UpdatePreferenceValues()
        {
            _joinCodeElement.SetName($"Join Server Code: {FusionPreferences.ClientSettings.ServerCode.GetValue()}");
            _nicknameDisplay.SetName($"Current Nickname: {FusionPreferences.ClientSettings.Nickname.GetValue()}");
            PlayerIdManager.LocalId.TrySetMetadata(MetadataHelper.NicknameKey, FusionPreferences.ClientSettings.Nickname.GetValue());
        }

        private void OnClickJoinServer() {
            string code = FusionPreferences.ClientSettings.ServerCode.GetValue();
            if (code.Contains(".")) {
                ConnectToServer(code);
            }
            else {
                string decodedIp = IPSafety.IPSafety.DecodeIPAddress(code);
                ConnectToServer(decodedIp);
            }
        }

        public static string GetDisconnectReason(DisconnectReason reason)
        {
            switch (reason)
            {
                case DisconnectReason.Disconnected:
                    return "Disconnected from Server";
                case DisconnectReason.NeverConnected:
                    return "Client Failed to Connect";
                case DisconnectReason.ConnectionRejected:
                    return "Connection was Rejected";
                case DisconnectReason.ServerStopped:
                    return "Server was Stopped";
                case DisconnectReason.TimedOut:
                    return "Timed out from Server";
                case DisconnectReason.Kicked:
                    return "Kicked";
                case DisconnectReason.TransportError:
                    return "Transport error";
                default:
                    return @"¯\_(ツ)_/¯";
            }
        }

        private void OnClientDisconnect(object sender, ServerDisconnectedEventArgs client)
        {
            // Update the mod so it knows this user has left
            InternalServerHelpers.OnUserLeave(client.Client.Id);

            // Send disconnect notif to everyone
            ConnectionSender.SendDisconnect(client.Client.Id, GetDisconnectReason(client.Reason));
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

        // Server List Menu
        private static MenuCategory _serverListCategory;
        private static MenuCategory createMenu;

        private void InitializeServerListCategory(MenuCategory category)
        {
            _serverListCategory = category.CreateCategory("Server List", Color.white);

            CreateServerList();
        }
        private void CreateServerList()
        {
            _serverListCategory.Elements.Clear();

            // Get listings
            List<ServerListing> serverListings = new List<ServerListing>();
            for (int i = 0; i < FusionPreferences.ClientSettings.ServerNameList.GetValue().Count; i++)
            {
                string listingName = FusionPreferences.ClientSettings.ServerNameList.GetValue()[i];
                string listingCode = FusionPreferences.ClientSettings.ServerCodeList.GetValue()[i];

                if (listingName != "" && listingCode != "")
                {
                    ServerListing listing = new ServerListing() { Name = listingName, ServerCode = listingCode };
                    serverListings.Add(listing);
                }
            }

            // Listing Creator Menu
            createMenu = _serverListCategory.CreateCategory("Add Server", Color.green);

            KeyboardCreator serverNameKeyboard = new KeyboardCreator();
            serverNameKeyboard.CreateKeyboard(createMenu, "Edit Name", FusionPreferences.ClientSettings.ServerNameToAdd);

            KeyboardCreator serverCodeKeyboard = new KeyboardCreator();
            serverCodeKeyboard.CreateKeyboard(createMenu, "Edit Code", FusionPreferences.ClientSettings.ServerCodeToAdd);

            createMenu.CreateFunctionElement("Add Listing", Color.green, () => CreateListing(FusionPreferences.ClientSettings.ServerNameToAdd.GetValue(), FusionPreferences.ClientSettings.ServerCodeToAdd.GetValue()));

            foreach (var listing in serverListings)
            {
                var listingCategory = _serverListCategory.CreateCategory(listing.Name, Color.white);

                var listingHide = listingCategory.CreateSubPanel("Show Code", Color.yellow);
                listingHide.CreateFunctionElement($"Code: {System.Environment.NewLine}{listing.ServerCode}", Color.green, null);

                // Join Element
                listingCategory.CreateFunctionElement("Join Server", Color.green, () => ConnectToServer(listing.ServerCode));

                listingCategory.CreateFunctionElement("Remove Listing", Color.red, () => DeleteListing(listing));
            }
        }

        private void CreateListing(string listingName, string listingCode)
        {
            if (listingName != "" && listingCode != "")
            {
                if (FusionPreferences.ClientSettings.ServerNameList.GetValue().Where(name => name.ToString() == listingName).Skip(1).Any())
                {
                    FusionNotifier.Send(new FusionNotification()
                    {
                        title = "Invalid Listing",
                        showTitleOnPopup = true,
                        isMenuItem = false,
                        isPopup = true,
                        message = $"A listing with this name has already been created!",
                        popupLength = 3f,
                    });
                    return;
                }

                if (FusionPreferences.ClientSettings.ServerCodeList.GetValue().Where(code => code.ToString() == listingCode).Skip(1).Any())
                {
                    FusionNotifier.Send(new FusionNotification()
                    {
                        title = "Invalid Listing",
                        showTitleOnPopup = true,
                        isMenuItem = false,
                        isPopup = true,
                        message = $"A listing with this code has already been created!",
                        popupLength = 3f,
                    });
                    return;
                }

                ServerListing listing = new ServerListing() { Name = listingName, ServerCode = listingCode };

                FusionPreferences.ClientSettings.ServerNameToAdd.SetValue("");
                FusionPreferences.ClientSettings.ServerCodeToAdd.SetValue("");

                var serverNames = FusionPreferences.ClientSettings.ServerNameList.GetValue();
                var serverCodes = FusionPreferences.ClientSettings.ServerCodeList.GetValue();

                serverNames.Add(listing.Name);
                serverCodes.Add(listing.ServerCode);

                FusionPreferences.ClientSettings.ServerNameList.SetValue(serverNames);
                FusionPreferences.ClientSettings.ServerCodeList.SetValue(serverCodes);

                CreateServerList();

                BoneLib.BoneMenu.MenuManager.SelectCategory(_serverListCategory);
            } else
            {
                FusionNotifier.Send(new FusionNotification()
                {
                    title = "Invalid Listing",
                    showTitleOnPopup = true,
                    isMenuItem = false,
                    isPopup = true,
                    message = $"All fields must contain a value!",
                    popupLength = 3f,
                });
            }
        }

        private void DeleteListing(ServerListing listing)
        {
            var serverNames = FusionPreferences.ClientSettings.ServerNameList.GetValue();
            var serverCodes = FusionPreferences.ClientSettings.ServerCodeList.GetValue();

            serverNames.Remove(listing.Name);
            serverCodes.Remove(listing.ServerCode);

            FusionPreferences.ClientSettings.ServerNameList.SetValue(serverNames);
            FusionPreferences.ClientSettings.ServerCodeList.SetValue(serverCodes);

            CreateServerList();

            BoneLib.BoneMenu.MenuManager.SelectCategory(_serverListCategory);
        }

        public class ServerListing()
        {
            public string Name;
            public string ServerCode;
        }
    }
}