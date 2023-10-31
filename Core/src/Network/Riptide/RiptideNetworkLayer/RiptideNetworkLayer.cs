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
using System.ServiceModel.Channels;
using System.Linq;
using LabFusion.Data;
using LabFusion.Core.src.Network.Riptide;
using Oculus.Platform.Models;
using System;
using Steamworks;
using SLZ.Marrow.Utilities;
using System.Runtime.CompilerServices;

namespace LabFusion.Network
{
    public partial class RiptideNetworkLayer : NetworkLayer
    {
        public static class CurrentServerType
        {
            private static ServerTypes type = ServerTypes.NONE;

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

        public static Client publicLobbyClient = new();
        public static short currentPublicHostID;

        public static string PublicLobbyHost => FusionPreferences.ClientSettings.PublicLobbyIP.GetValue() + "7676";

        public static string publicIp;
        public static bool isHost = false;

        internal override bool IsServer => currentserver.IsRunning || isHost;

        internal override bool IsClient => currentclient.IsConnected;

        private readonly RiptideVoiceManager _voiceManager = new();
        internal override IVoiceManager VoiceManager => _voiceManager;

        internal override bool ServerCanSendToHost => true;

        protected string _targetServerIP;

        public static string RiptideUsername => username;
        private static string username;

        internal override void OnInitializeLayer() {
            // Initialize the RiptideLogger if in DEBUG
#if DEBUG
            RiptideLogger.Initialize(LogMethod, true);
#endif

            // Hooking
            currentserver.TimeoutTime = 30000;
            currentserver.HeartbeatInterval = 10000;

            currentclient.TimeoutTime = 30000;
            currentclient.HeartbeatInterval = 10000;

            FusionLogger.Log("Initialized Riptide Layer");
        }

        private void LogMethod(string text)
        {
            FusionLogger.Log(text);
        }

        private void InitializeUsername()
        {
            if (!HelperMethods.IsAndroid())
            {
                if (System.IO.Path.GetFileName(UnityEngine.Application.dataPath) == "BONELAB_Steam_Windows64_Data")
                {
                    if (!SteamClient.IsValid)
                        SteamClient.Init(250820, false);

                    PlayerIdManager.SetUsername(SteamClient.Name);

                    SteamClient.Shutdown();

                } else
                {
                    Oculus.Platform.Core.Initialize("5088709007839657");
                    Oculus.Platform.Users.GetLoggedInUser().OnComplete((Oculus.Platform.Message<User>.Callback)GetLoggedInUserCallback);
                }
            } else
            {
                Oculus.Platform.Core.Initialize("4215734068529064");
                Oculus.Platform.Users.GetLoggedInUser().OnComplete((Oculus.Platform.Message<User>.Callback)GetLoggedInUserCallback);
            }
        }

        private void GetLoggedInUserCallback(Oculus.Platform.Message msg)
        {
            // Attempt to get the Oculus username
            if (!msg.IsError)
                PlayerIdManager.SetUsername(msg.GetUser().OculusID);
            // If failed, assume piracy
            else
                PlayerIdManager.SetUsername("Riptide Pirate");

#if DEBUG
            FusionLogger.Log("Finished getting user callback!");
#endif
        }

        internal override void OnLateInitializeLayer()
        {
            PortHelper.OpenPort();

            HookRiptideEvents();

            IPGetter.GetExternalIP((ip) =>
            {
                publicIp = ip;
            });

            InitializeUsername();
        }

        internal override void OnCleanupLayer() {
            Disconnect();

            UnHookRiptideEvents();

            PortHelper.ClosePort();
        }

        internal override void OnUpdateLayer() {
            currentserver.Update();
            currentclient.Update();
            publicLobbyClient.Update();
        }

        internal override string GetUsername(ulong userId) {
            return $"Riptide Enjoyer {userId}";
        }

        internal override bool IsFriend(ulong userId) {
            return false;
        }

        internal override void BroadcastMessage(NetworkChannel channel, FusionMessage message) {
            // Dedicated Server Handling
            if (CurrentServerType.GetType() == ServerTypes.DEDICATED)
            {
                if (isHost)
                {
                    currentclient.Send(RiptideHandler.PrepareMessage(message, channel, (ushort)RiptideMessageTypes.Broadcast));
                }
                else
                {
                    currentclient.Send(RiptideHandler.PrepareMessage(message, channel, (ushort)RiptideMessageTypes.SendToServer));
                }
            } 
            
            // P2P Handling
            else if (CurrentServerType.GetType() == ServerTypes.P2P)
            {
                if (IsServer)
                {
                    currentserver.SendToAll(RiptideHandler.PrepareMessage(message, channel, (ushort)RiptideMessageTypes.FusionMessage));
                }
                else
                {
                    currentclient.Send(RiptideHandler.PrepareMessage(message, channel, (ushort)RiptideMessageTypes.FusionMessage));
                }
            } 
            
            // Public Lobby Handling
            else if (CurrentServerType.GetType() == ServerTypes.PUBLIC)
            {
                if (isHost)
                    currentclient.Send(RiptideHandler.PrepareMessage(message, channel, (ushort)RiptideMessageTypes.SendToServer, (short)currentclient.Id));
                else
                    currentclient.Send(RiptideHandler.PrepareMessage(message, channel, (ushort)RiptideMessageTypes.Broadcast, currentPublicHostID));
            }
        }

        internal override void SendToServer(NetworkChannel channel, FusionMessage message) {
            // Dedicated Server Handling
            if (CurrentServerType.GetType() == ServerTypes.DEDICATED)
            {
                currentclient.Send(RiptideHandler.PrepareMessage(message, channel, (ushort)RiptideMessageTypes.SendToServer));
            } 

            // P2P Handling
            else if (CurrentServerType.GetType() == ServerTypes.P2P)
            {
                currentclient.Send(RiptideHandler.PrepareMessage(message, channel, (ushort)RiptideMessageTypes.FusionMessage));
            }

            // Public Lobby Handling
            else if (CurrentServerType.GetType() == ServerTypes.PUBLIC)
            {
                if (isHost)
                    currentclient.Send(RiptideHandler.PrepareMessage(message, channel, (ushort)RiptideMessageTypes.SendToServer, (short)currentclient.Id));
                else
                    currentclient.Send(RiptideHandler.PrepareMessage(message, channel, (ushort)RiptideMessageTypes.SendToServer, currentPublicHostID));
            }
        }

        internal override void SendFromServer(byte userId, NetworkChannel channel, FusionMessage message) {
            var id = PlayerIdManager.GetPlayerId(userId);
            if (id != null)
                SendFromServer(id.LongId, channel, message);
        }

        internal override void SendFromServer(ulong userId, NetworkChannel channel, FusionMessage message) {
            // Dedicated Server Handling
            if (CurrentServerType.GetType() == ServerTypes.DEDICATED)
            {
                if (isHost)
                {
                    currentclient.Send(RiptideHandler.PrepareMessage(message, channel, (ushort)RiptideMessageTypes.SendFromServer, (short)userId));
                }
            }

            // P2P Handling
            else if (CurrentServerType.GetType() == ServerTypes.P2P)
            {
                if (IsServer)
                {
                    if (userId == PlayerIdManager.LocalLongId)
                        currentserver.Send(RiptideHandler.PrepareMessage(message, channel, (ushort)RiptideMessageTypes.FusionMessage), (ushort)PlayerIdManager.LocalLongId);
                    else if (currentserver.TryGetClient((ushort)userId, out Connection client))
                        currentserver.Send(RiptideHandler.PrepareMessage(message, channel, (ushort)RiptideMessageTypes.FusionMessage), client);
                }
            }

            // Public Lobby Handling
            else if (CurrentServerType.GetType() == ServerTypes.PUBLIC)
            {
                if (isHost)
                {
                    currentclient.Send(RiptideHandler.PrepareMessage(message, channel, (ushort)RiptideMessageTypes.SendFromServer, (short)userId));
                }
            }
        }

        // P2P Server Create
        private void OnClickCreateServer()
        {
            // Is a server already running? Disconnect
            if (IsServer || IsClient)
            {
                Disconnect();
            }
            // Otherwise, start a server
            else
            {
                StartServer();
            }
        }

        // Public Lobby Create
        private void OnClickCreateLobby()
        {
            // Is a server already running? Disconnect
            if (IsServer || IsClient)
            {
                Disconnect();
            }
            // Otherwise, create a lobby
            else
            {
                PublicLobbyManager.CreatePublicLobby();
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

        private static bool isConnecting;
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

            var request = Riptide.Message.Create(MessageSendMode.Reliable, (ushort)RiptideMessageTypes.ServerType);
            currentclient.Send(request);
        }

        internal override void Disconnect(string reason = "") {
            if (IsServer)
                currentserver.Stop();

            if (IsClient)
                currentclient.Disconnect();

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
            MultiplayerHooking.OnDisconnect += OnLeaveServer;

            // Riptide Hooks
            currentclient.Disconnected += OnDisconnect;
            currentclient.ConnectionFailed += OnConnectionFail;
            Hooking.OnLevelInitialized += OnLevelLoad;

            // Preference Hooks
            FusionPreferences.LocalServerSettings.Privacy.OnValueChanged += ServerSettingsHooks.OnServerPrivacyChanged;
            FusionPreferences.LocalServerSettings.MaxPlayers.OnValueChanged += ServerSettingsHooks.OnMaxPlayersChanged;
            FusionPreferences.LocalServerSettings.AllowQuestUsers.OnValueChanged += ServerSettingsHooks.OnAllowQuestUsersChanged;
            FusionPreferences.LocalServerSettings.AllowPCUsers.OnValueChanged += ServerSettingsHooks.OnAllowPCUsersChanged;
            Hooking.OnLevelInitialized += ServerSettingsHooks.OnChangeLevel;
        }

        private void OnLevelLoad(LevelInfo info)
        {
            if (CurrentServerType.GetType() == ServerTypes.DEDICATED && isHost)
            {
                Riptide.Message levelInfo = Riptide.Message.Create(MessageSendMode.Reliable, (ushort)RiptideMessageTypes.LevelInfo);
                levelInfo.Release();

                levelInfo.AddString(info.barcode);
                levelInfo.AddString(info.title);
                currentclient.Send(levelInfo);

                PermissionList.SetPermission(PlayerIdManager.LocalLongId, PlayerIdManager.LocalUsername, PermissionLevel.DEFAULT);
                var playerId = PlayerIdManager.GetPlayerId(PlayerIdManager.LocalLongId);

                if (playerId != null && NetworkInfo.IsServer)
                {
                    playerId.TrySetMetadata(MetadataHelper.PermissionKey, PermissionLevel.DEFAULT.ToString());
                }
            }
        }

        private void OnConnectionFail(object sender, ConnectionFailedEventArgs info)
        {
            FusionNotifier.Send(new FusionNotification()
            {
                title = "Connection Failed",
                showTitleOnPopup = true,
                isMenuItem = false,
                isPopup = true,
                message = $"Failed to connect to server!",
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

            FusionLogger.Error($"Failed to connect to server!");
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

        private void OnLeaveServer() {
            VoiceManager.RemoveAll();
        }

        private void UnHookRiptideEvents() {
            // Add server hooks
            MultiplayerHooking.OnMainSceneInitialized -= OnUpdateRiptideLobby;
            GamemodeManager.OnGamemodeChanged -= OnGamemodeChanged;
            MultiplayerHooking.OnPlayerJoin -= OnUserJoin;
            MultiplayerHooking.OnPlayerLeave -= OnPlayerLeave;
            MultiplayerHooking.OnServerSettingsChanged -= OnUpdateRiptideLobby;
            MultiplayerHooking.OnDisconnect -= OnLeaveServer;

            // Riptide Hooks
            currentclient.Disconnected -= OnDisconnect;
            currentclient.ConnectionFailed -= OnConnectionFail;
            Hooking.OnLevelInitialized -= OnLevelLoad;

            // Preference Hooks
            FusionPreferences.LocalServerSettings.Privacy.OnValueChanged -= ServerSettingsHooks.OnServerPrivacyChanged;
            FusionPreferences.LocalServerSettings.MaxPlayers.OnValueChanged -= ServerSettingsHooks.OnMaxPlayersChanged;
            FusionPreferences.LocalServerSettings.AllowQuestUsers.OnValueChanged -= ServerSettingsHooks.OnAllowQuestUsersChanged;
            FusionPreferences.LocalServerSettings.AllowPCUsers.OnValueChanged -= ServerSettingsHooks.OnAllowPCUsersChanged;
        }

        public static void OnUpdateRiptideLobby() {
            // Update bonemenu items
            OnUpdateCreateServerText();

            if (!publicLobbyClient.IsConnected)
                publicLobbyClient.Connect(PublicLobbyHost);
        }

        internal override void OnSetupBoneMenu(MenuCategory category) {
            // Create the basic options
            CreateMatchmakingMenu(category);
            BoneMenuCreator.CreateGamemodesMenu(category);
            CreateRiptideSettings(category);
            BoneMenuCreator.CreateSettingsMenu(category);
            BoneMenuCreator.CreateNotificationsMenu(category);

#if DEBUG
            // Debug only (dev tools)
            BoneMenuCreator.CreateDebugMenu(category);
#endif
        }

        // Matchmaking menu
        private MenuCategory _serverInfoCategory;
        private MenuCategory _manualJoiningCategory;
        private MenuCategory _publicLobbyCategory;

        private void CreateMatchmakingMenu(MenuCategory category) {
            // Root category
            var matchmaking = category.CreateCategory("Matchmaking", Color.red);

            // Server making
            _serverInfoCategory = matchmaking.CreateCategory("Server Info", Color.white);
            CreateServerInfoMenu(_serverInfoCategory);

            // Manual joining
            _manualJoiningCategory = matchmaking.CreateCategory("Manual Joining", Color.white);
            CreateManualJoiningMenu(_manualJoiningCategory);

            _publicLobbyCategory = matchmaking.CreateCategory("Public Lobbies", Color.white);
            CreatePublicLobbyMenu(_publicLobbyCategory);

            // Server List
            InitializeServerListCategory(matchmaking);
        }

        private void CreatePublicLobbyMenu(MenuCategory category)
        {
            category.Elements.Clear();
            _publicLobbyCategory.CreateFunctionElement("Refresh", Color.yellow, () =>
            {
                CreatePublicLobbyMenu(_publicLobbyCategory);
                PublicLobbyManager.RequestLobbies(_publicLobbyCategory);
            });
        }

        private static FunctionElement _nicknameDisplay;
        private static FunctionElement _pingDisplay;
        private static FunctionElement _publicLobbyIP;
        private void CreateRiptideSettings(MenuCategory category)
        {
            var settings = category.CreateCategory("Riptide Settings", Color.blue);

            // Create Connection Info
            var connectionInfo = settings.CreateCategory("Connection Stuff", Color.white);

            var pingMenu = connectionInfo.CreateCategory("Ping Menu", Color.white);
            _pingDisplay = pingMenu.CreateFunctionElement("Ping:\n (REFRESH)", Color.grey, null);
            pingMenu.CreateFunctionElement("Refresh", Color.blue, () =>
            {
                if (currentclient.RTT == -1)
                {
                    _pingDisplay.SetColor(Color.yellow);
                    _pingDisplay.SetName("Not connected to server!");
                    return;
                }

                int ping = currentclient.RTT;
                switch (ping)
                {
                    case <= 100:
                        _pingDisplay.SetColor(Color.green);
                        break;
                    case <= 200:
                        _pingDisplay.SetColor(Color.yellow);
                        break;
                    case <= 300:
                        _pingDisplay.SetColor(Color.red);
                        break;
                    case > 300:
                        _pingDisplay.SetColor(Color.black);
                        break;
                }
                _pingDisplay.SetName($"Ping:\n {ping}");
            });

            // Create Public Lobby Settings
            var publicLobbySettings = settings.CreateCategory("Public Lobby Settings", Color.magenta);
            publicLobbySettings.CreateFunctionElement("Current Public Lobby IP:", Color.white, null);
            _publicLobbyIP = publicLobbySettings.CreateFunctionElement(FusionPreferences.ClientSettings.PublicLobbyIP.GetValue(), Color.white, null);
            KeyboardCreator keyboard = new KeyboardCreator();
            keyboard.CreateKeyboard(publicLobbySettings, "Change Public Lobby IP", FusionPreferences.ClientSettings.PublicLobbyIP);

        }

        private static FunctionElement _createServerElement;
        private static FunctionElement _createLobbyElemebt;

        private void CreateServerInfoMenu(MenuCategory category) {
            _createServerElement = category.CreateFunctionElement("Start P2P Server", Color.white, OnClickCreateServer, "P2P Lobbies Require that you PORT FORWARD.");
            _createLobbyElemebt = category.CreateFunctionElement("Create Public Lobby", Color.white, OnClickCreateLobby);

            if (!HelperMethods.IsAndroid()) {
                category.CreateFunctionElement("Copy Server Code to Clipboard", Color.white, OnCopyServerCode);
            }
            category.CreateFunctionElement("Display Server Code", Color.white, OnDisplayServerCode);

            BoneMenuCreator.PopulateServerInfo(category);
        }

        private void OnCopyServerCode() {
            string encodedIP = IPSafety.IPSafety.EncodeIPAddress(publicIp);

            Clipboard.SetText(encodedIP);
        }

        private static void OnUpdateCreateServerText() {
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
                _createServerElement.SetName("Start P2P Server");
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
            _publicLobbyIP.SetName(FusionPreferences.ClientSettings.PublicLobbyIP.GetValue());
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

        private void OnDisplayServerCode()
        {
            if (publicIp == null || publicIp == string.Empty)
            {
                FusionNotifier.Send(new FusionNotification()
                {
                    title = "Server Code",
                    showTitleOnPopup = true,
                    isMenuItem = false,
                    isPopup = true,
                    message = $"Failed to obtain IP address! Please retry!",
                    popupLength = 3f,
                    type = NotificationType.ERROR
                });
                IPGetter.GetExternalIP((ip) =>
                {
                    publicIp = ip;
                });
            } else
            {
                string encodedIP = IPSafety.IPSafety.EncodeIPAddress(publicIp);

                FusionNotifier.Send(new FusionNotification()
                {
                    title = "Server Code",
                    showTitleOnPopup = true,
                    isMenuItem = false,
                    isPopup = true,
                    message = $"Code: \n{encodedIP}",
                    popupLength = 20f,
                });
            }
        }

        internal override bool CheckSupported() => true;
        internal override bool CheckValidation() => true;

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