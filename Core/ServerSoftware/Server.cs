using Riptide;
using Riptide.Transports.Udp;
using Riptide.Utils;

using System;
using System.Security;
using System.Net.NetworkInformation;
using ServerSoftware.Utilities;
using System.Runtime.InteropServices;

namespace ServerSoftware
{
    public static class ServerClass
    {
        private const int MF_BYCOMMAND = 0x00000000;
        public const int SC_CLOSE = 0xF060;

        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();

        public static Connection host;
        public static bool hasPortForwarded = false;
        public static Riptide.Server currentserver = new();
        public static int playerCount = 0;
        public static string currentLevelBarcode;
        private static Timer timer;
        private static int tickInterval = 5;

        private static void Main(string[] args)
        {
            currentserver.TimeoutTime = 30000;
            currentserver.HeartbeatInterval = 30000;

            currentserver.ClientConnected += OnClientConnected;
            currentserver.ClientDisconnected += OnClientDisconnected;

            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_CLOSE, MF_BYCOMMAND);
            Console.Title = "TideFusion Dedicated Server";

            StartRiptideServer();

            PortHelper.OpenPort();
        }

        public static void UpdateWindow(string info = "")
        {
            Console.Clear();
            Console.WriteLine($"Player Count: {playerCount}");
            Console.WriteLine($"Server Code: {IPStuff.EncodeIPAddress(IPStuff.GetExternalIP())}");
            Console.WriteLine($"Has Auto Port Forwarded: {hasPortForwarded}");

            Console.WriteLine("\n===============================");

            if (info != "")
            {
                Console.WriteLine(info);
                Console.WriteLine("===============================\n");
            }

            Console.WriteLine("Enter a command! (Help for more info)");
            string typed = Console.ReadLine();

            Commands.Command command = new Commands.Command();
            string[] typedArray = typed.Split(" ");
            command.identifier = typedArray[0];
            command.modifiers = typedArray;

            Commands.RunCommand(command);

        }

        public static void RestartServer(bool resetData = false)
        {
            currentserver.Stop();

            StartRiptideServer(true);
        }

        private static void StartRiptideServer(bool reset = false)
        {
            if (currentserver == null)
            {
                currentserver = new();
                currentserver.TimeoutTime = 30000;
                currentserver.HeartbeatInterval = 30000;

                currentserver.ClientConnected += OnClientConnected;
                currentserver.ClientDisconnected += OnClientDisconnected;
            }

            currentserver.Start(7777, 256);

            // Create a Timer that calls the Tick method every 'interval' milliseconds
            if (timer != null)
            {
                timer.Dispose();
            } 
            timer = new Timer(Tick, null, 0, tickInterval);

            if (!reset) UpdateWindow("Server started!"); else UpdateWindow("Server restarted!");
        }

        private static void Tick(object state)
        {
            if (currentserver != null)
                currentserver.Update();
            else
            {
                currentserver = new();
                currentserver.TimeoutTime = 30000;
                currentserver.HeartbeatInterval = 30000;

                currentserver.ClientConnected += OnClientConnected;
                currentserver.ClientDisconnected += OnClientDisconnected;
            }
        }

        private static void OnClientDisconnected(object? sender, ServerDisconnectedEventArgs client)
        {
            playerCount = currentserver.ClientCount;

            if (client.Client == host)
            {
                if (playerCount != 0)
                {
                    Connection connection = currentserver.Clients[0];
                    HostUtils.TrySetHost(connection);
                } else
                {
                    RestartServer();
                }
            }
            else
            {
                UpdateWindow($"Client disconnected with ID {client.Client.Id}");
            }
        }

        private static void OnClientConnected(object? sender, ServerConnectedEventArgs client)
        {
            playerCount = currentserver.ClientCount;
            client.Client.TimeoutTime = 30000;

            if (client.Client.Id == 1)
            {
                host = client.Client;
                UpdateWindow("Obtained new host!");
            } else 
                UpdateWindow($"Client {client.Client.Id} connected.");
        }
    }
    
}
