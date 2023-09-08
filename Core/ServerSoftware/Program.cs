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


namespace Program
{
    public class ServerClass
    {
        public static Server currentserver;
        static void Main(string[] args)
        {
            ServerClass server = new ServerClass();
            StartRiptideServer();
            server.FetchAndOpenPort();
            Console.ReadLine();
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

        private async void FetchAndOpenPort()
        {
            try
            {
                var discoverer = new NatDiscoverer();
                var cts = new System.Threading.CancellationTokenSource(5000);
                NatDevice natDevice = await discoverer.DiscoverDeviceAsync(PortMapper.Upnp, cts);

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
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private async Task OpenPortAsync(NatDevice device)
        {
            try
            {
                // Open the port
                var portmap = new Mapping(Protocol.Udp, 7777, 7777, "TideFusion Server"); ;
                await device.CreatePortMapAsync(portmap);

                Console.WriteLine($"Port 7777 has been opened. Protocol: UDP");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening port: {ex.Message}");
            }
        }

        private string GetLocalIPAddress()
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
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error fetching local IPv4 address: {ex.Message}");
            }

            return localIpAddress;
        }
    }
    
}
