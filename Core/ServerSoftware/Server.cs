using Riptide;
using Riptide.Transports.Udp;
using Riptide.Utils;

using Open.Nat;

using System;
using System.Security;
using System.Net.NetworkInformation;
using ServerSoftware;

namespace Server
{
    public static class ServerClass
    {
        public static Connection host;
        public static bool hasPortForwarded = false;
        public static Riptide.Server currentserver;
        public static int playerCount = 0;
        public static string currentLevelBarcode;
        private static Timer timer;
        private static int tickInterval = 5;

        private static void Main(string[] args)
        {
            Console.Title = "Fusion Dedicated Server";

            StartRiptideServer();

            PortHelper.OpenPort();
            InitializeCommandPrompt();
        }

        public static void InitializeCommandPrompt(string info = "")
        {
            Console.Clear();

            Console.WriteLine($"Player Count: {playerCount}");
            Console.WriteLine($"Server Code: {IPStuff.IPSafety.EncodeIPAddress(IPStuff.IPGetter.GetExternalIP())}");
            Console.WriteLine($"Has Auto Port Forwarded: {hasPortForwarded}");

            Console.WriteLine("\n===============================================\n");

            if (info == "")
            {
                Console.WriteLine($"Enter a Command (help for more info)");
            } else
            {
                Console.WriteLine(info);
            }

            string typed = Console.ReadLine();
            Commands.Command command = new Commands.Command();
            command.identifier = typed.Split(" ").First();

            Commands.RunCommand(command);

        }

        public static void UpdateWindow(string info = "")
        {
            InitializeCommandPrompt(info);
        }

        public static void RestartServer(bool resetSyncables = false)
        {
            currentserver.Stop();

            StartRiptideServer();

            Server.ServerClass.UpdateWindow("Restarted Server!");
        }

        private static void StartRiptideServer()
        {
            currentserver = new Riptide.Server();
            currentserver.ClientConnected += OnClientConnected;
            currentserver.ClientDisconnected += OnClientDisconnected;
            currentserver.TimeoutTime = 20000;
            currentserver.HeartbeatInterval = 5000;
            currentserver.Start(7777, 256);

            // Create a Timer that calls the Tick method every 'interval' milliseconds
            timer = new Timer(Tick, null, 0, tickInterval);

            Console.WriteLine("Server started!");
        }

        private static void Tick(object state)
        {
            currentserver.Update();
        }

        private static void OnClientDisconnected(object? sender, ServerDisconnectedEventArgs client)
        {
            playerCount = currentserver.ClientCount;

            if (client.Client == host)
            {
                RestartServer();
            }
        }

        private static void OnClientConnected(object? sender, ServerConnectedEventArgs client)
        {
            if (client.Client.Id == 1)
            {
                host = client.Client;
            }

            playerCount = currentserver.ClientCount;

            UpdateWindow();
        }
    }
    
}
