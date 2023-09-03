using System.Collections.Generic;

using BoneLib.BoneMenu.Elements;

using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Preferences;

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
using Open.Nat;
using System.Threading.Tasks;
using System;
using System.ServiceModel.Channels;
using System.Linq;
using System.Net.NetworkInformation;

namespace LabFusion.Network
{
    public class RiptideNetworkLayer : NetworkLayer
    {
        internal override string Title => "Riptide";

        // AsyncCallbacks are bad!
        // In Unity/Melonloader, they can cause random crashes, especially when making a lot of calls
        public const bool AsyncCallbacks = false;
        public static Server currentserver { get; set; }
        public static Client currentclient { get; set; }
        public static string publicIp;

        internal override bool IsServer => _isServerActive;

        internal override bool IsClient => _isConnectionActive;

        private readonly RiptideVoiceManager _voiceManager = new();
        internal override IVoiceManager VoiceManager => _voiceManager;

        protected bool _isServerActive = false;
        protected bool _isConnectionActive = false;

        internal override bool ServerCanSendToHost => true;

        protected string _targetServerIP;

        internal override void OnInitializeLayer() {
            currentclient = new Client();
            currentserver = new Server();

            IPGetter.GetExternalIP(OnExternalIPAddressRetrieved);
            PlayerIdManager.SetUsername("Riptide Enjoyer");

            FusionLogger.Log("Initialized Riptide Layer");
        }

        internal override void OnLateInitializeLayer()
        {
            PlayerIdManager.SetUsername("Riptide Enjoyer");

            if (!HelperMethods.IsAndroid())
            {
                FetchAndOpenPort();
            }

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
            string Username = ("Riptide Enjoyer");
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
            FusionLogger.Log($"Recieved voice bytes [{bytes.Length}] from {GetUsername(id)}");

            // If we are deafened, no need to deal with voice chat
            if (VoiceHelper.IsDeafened)
                return;

            var handler = VoiceManager.GetVoiceHandler(id);
            handler?.OnVoiceBytesReceived(bytes);
        }

        private byte[] voiceData;
        internal override void OnVoiceChatUpdate()
        {
            /*if (NetworkInfo.HasServer)
            {
                bool voiceEnabled = VoiceHelper.IsVoiceEnabled;

                if (voiceEnabled)
                {
                    if (!MicrophoneManager.isRecording)
                    {
                        MicrophoneManager.StartMicrophone();
                        return;
                    }

                    voiceData = CompressionHelper.CompressByteArray(MicrophoneManager.GetMicrophoneData());
                } else
                {
                    return;
                }

                // Read voice data
                if (voiceData != null)
                {
                    PlayerSender.SendPlayerVoiceChat(voiceData);
                    FusionLogger.Log($"Sent Voice Data: {voiceData.Length}");
                }

                // Update the manager
                VoiceManager.Update();
            }
            else
            {
                if (Microphone.IsRecording(null))
                {
                    MicrophoneManager.StopMicrophone();
                }
            }*/
        }

        internal override void StartServer() {
            currentclient = new Client();
            currentserver = new Server();

            currentclient.Connected += OnStarted;
            currentserver.Start(7777, 256);

            currentclient.Connect("127.0.0.1:7777");
        }

        private void OnStarted(object sender, System.EventArgs e) {
#if DEBUG
            FusionLogger.Log("SERVER START HOOKED");
#endif

            // Update player ID here since it's determined on the Riptide Client ID
            PlayerIdManager.SetLongId(currentclient.Id);
            PlayerIdManager.SetUsername($"Riptide Enjoyer");

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

            if (ip.Contains("."))
            {
                currentclient.Connect(ip + ":7777");
            }
            else
            {
                string decodedIp = IPSafety.IPSafety.DecodeIPAddress(ip);
                ConnectToServer(decodedIp);

                currentclient.Connect(decodedIp + ":7777");
            }
        }

        private void OnConnect(object sender, System.EventArgs e) {
#if DEBUG
            FusionLogger.Log("SERVER CONNECT HOOKED");
#endif
            isConnecting = false;

            currentclient.Disconnected += OnClientDisconnect;
            // Update player ID here since it's determined on the Riptide Client ID
            PlayerIdManager.SetLongId(currentclient.Id);
            PlayerIdManager.SetUsername($"Riptide Enjoyer");

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

        public static FunctionElement _nicknameDisplay;
        private void CreateRiptideSettings(MenuCategory category)
        {
            var settings = category.CreateCategory("Riptide Settings", Color.blue);

            // Create Username Menu
            var nicknamePanel = settings.CreateCategory("Nickname", Color.white);
            _nicknameDisplay = nicknamePanel.CreateFunctionElement($"Current Nickname: {FusionPreferences.ClientSettings.Nickname.GetValue()}", Color.white, null);

            KeyboardCreator keyboard = new KeyboardCreator();
            keyboard.CreateKeyboard(nicknamePanel, "Nickname Keyboard", FusionPreferences.ClientSettings.Nickname);

            var nicknameInfo = nicknamePanel.CreateSubPanel("Reason for Existing", Color.red);
            nicknameInfo.CreateFunctionElement("Yes, I know", Color.yellow, null);
            nicknameInfo.CreateFunctionElement("`Fusion has this built in`", Color.yellow, null);
            nicknameInfo.CreateFunctionElement("but I wanted a keyboard", Color.yellow, null);
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

        private void OnClientDisconnect(object sender, ServerDisconnectedEventArgs client)
        {
            // Update the mod so it knows this user has left
            InternalServerHelpers.OnUserLeave(client.Client.Id);

            // Send disconnect notif to everyone
            ConnectionSender.SendDisconnect(client.Client.Id, client.Reason.ToString());
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
                popupLength = 10f,
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

        // UPNP Stuff
        private NatDevice natDevice;
        private async void FetchAndOpenPort()
        {
            try
            {
                var discoverer = new NatDiscoverer();
                var cts = new System.Threading.CancellationTokenSource(5000);
                natDevice = await discoverer.DiscoverDeviceAsync(PortMapper.Upnp, cts);

                if (natDevice != null)
                {
                    string localIp = GetLocalIPAddress();

                    if (!string.IsNullOrEmpty(localIp))
                    {
                        await OpenPortAsync(natDevice);
                    }
                    else
                    {
                        FusionLogger.Log("Failed to fetch Local IP.");
                    }
                }
                else
                {
                    FusionLogger.Log("No compatible NAT device found.");
                }
            }
            catch (Exception ex)
            {
                FusionLogger.Error($"Error: {ex.Message}");
            }
        }
        private async Task OpenPortAsync(NatDevice device)
        {
            try
            {
                // Open the port
                var portmap = new Mapping(Protocol.Udp, 7777, 7777, "MelonLoader"); ;
                await device.CreatePortMapAsync(portmap);

                MelonLogger.Msg($"Port 7777 has been opened. Protocol: UDP");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error opening port: {ex.Message}");
            }
        }

        private string GetLocalIPAddress()
        {
            string localIpAddress = null;

            try
            {
                NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

                NetworkInterface activeInterface = networkInterfaces.FirstOrDefault(
                    iface => iface.OperationalStatus == OperationalStatus.Up &&
                             (iface.NetworkInterfaceType != NetworkInterfaceType.Loopback || iface.NetworkInterfaceType != NetworkInterfaceType.Tunnel) &&
                             iface.GetIPProperties().UnicastAddresses.Any(
                                 addr => addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork));

                if (activeInterface != null)
                {
                    var ipProperties = activeInterface.GetIPProperties();
                    var ipv4Address = ipProperties.UnicastAddresses.FirstOrDefault(
                        addr => addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?.Address;

                    if (ipv4Address != null)
                    {
                        localIpAddress = ipv4Address.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error fetching local IPv4 address: {ex.Message}");
            }

            return localIpAddress;
        }
    }
}