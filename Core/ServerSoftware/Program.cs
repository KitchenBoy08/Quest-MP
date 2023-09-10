using Riptide;
using Riptide.Transports.Udp;
using Riptide.Utils;

using Open.Nat;

using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.MonoBehaviours;

using System;
using System.Security;
using System.Net.NetworkInformation;
using ServerSoftware;

namespace Program
{
    public class ServerClass
    {
        public static Mapping portmap = new Mapping(Protocol.Udp, 7777, 7777, "TideFusion Server");
        public static bool hasPortForwarded = false;
        public static Server currentserver;
        public static int playerCount = 0;
        public static NatDevice natDevice;
        static void Main(string[] args)
        {
            Console.Title = "Fusion Dedicated Server";

            StartRiptideServer();
            while (currentserver == null) { }

            FetchAndOpenPort();
            InitializeCommandPrompt();
        }

        public static void InitializeCommandPrompt()
        {
            Console.Clear();

            Console.WriteLine($"Player Count: {playerCount}");

            Console.WriteLine($"Enter a Command (help for more info)");
            string command = Console.ReadLine();
            Commands.RunCommand(command);

        }

        public static void UpdateWindow()
        {
            InitializeCommandPrompt();
        }

        private static void StartRiptideServer()
        {
            currentserver = new Server();
            currentserver.TimeoutTime = 20000;
            currentserver.HeartbeatInterval = 5000;
            Console.WriteLine("Server started!");
        }

        private static void OnClientConnected(Client client)
        {
            Console.WriteLine($"Client {client.Id} connected!");
        }

        private static void OnClientDisconnected(Client client, DisconnectReason reason)
        {
            Console.WriteLine($"Client {client.Id} disconnected! Reason: {reason}");
        }

        private static void OnMessageReceived(Message message, Client client)
        {
            Console.WriteLine($"Received message from {client.Id}! Contents: {message}");
        }

        private static async void FetchAndOpenPort()
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
                        Console.WriteLine("Failed to fetch Local IP.");
                    }
                }
                else
                {
                    Console.WriteLine("No compatible NAT device found.");
                }
            }
            catch (Exception ex)
            {
                hasPortForwarded = false;
                UpdateWindow();
            }
        }

        private static async Task OpenPortAsync(NatDevice device)
        {
            try
            {
                // Open the port
                await device.CreatePortMapAsync(portmap);

                hasPortForwarded = true;
                UpdateWindow();
            }
            catch (Exception ex)
            {
                hasPortForwarded = false;
                UpdateWindow();
            }
        }

        private static string GetLocalIPAddress()
        {
            string? localIpAddress = null;

            try
            {
                NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                NetworkInterface activeInterface = networkInterfaces.FirstOrDefault(
                    iface => iface.OperationalStatus == OperationalStatus.Up &&
                             (iface.NetworkInterfaceType != NetworkInterfaceType.Loopback || iface.NetworkInterfaceType != NetworkInterfaceType.Tunnel) &&
                             iface.GetIPProperties().UnicastAddresses.Any(
                                 addr => addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork));
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

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
                hasPortForwarded = false;
                UpdateWindow();
            }

            return localIpAddress;
        }
    }
    
}
