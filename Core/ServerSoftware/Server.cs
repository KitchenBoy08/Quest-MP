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
        public static int hostID = 0;
        public static bool hasPortForwarded = false;
        public static Riptide.Server currentserver = new();
        public static int playerCount = 0;
        public static string currentLevelBarcode = "NONE";
        public static string currentLevelName = "NONE";
        private static Timer timer;
        private static int tickInterval = 5;

        private static void Main(string[] args)
        {
            InternalUtils.OSCheck();

            currentserver.TimeoutTime = 30000;
            currentserver.HeartbeatInterval = 30000;

            currentserver.ClientConnected += OnClientConnected;
            currentserver.ClientDisconnected += OnClientDisconnected;

            Console.Title = "TideFusion Dedicated Server";

            StartRiptideServer();

            InternalUtils.OpenUPnPPort();
        }

        public static void UpdateWindow(string info = "")
        {
            Console.Clear();
            Console.WriteLine($"Player Count: {playerCount}");
#if !DEBUG
            Console.WriteLine($"Server Code: {IPStuff.EncodeIPAddress(IPStuff.GetExternalIP())}");
#endif
            Console.WriteLine($"Has Auto Port Forwarded: {hasPortForwarded}");
            Console.WriteLine($"Current Level Barcode: {currentLevelBarcode}");
            Console.WriteLine($"Current Level Title: {currentLevelName}");
            Console.WriteLine($"Current Host ID: {hostID }");

            Console.WriteLine("\n===============================");

            if (info != "")
            {
                Console.WriteLine(info);
                Console.WriteLine("===============================\n");
            }

            Console.WriteLine("Enter a command! (help for more info)");
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

            if (resetData )
            {
                currentLevelBarcode = "NONE";
                currentLevelName = "NONE";
            }

            StartRiptideServer(true);

            hostID = 0;
        }

        public static void StartRiptideServer(bool reset = false)
        {
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
            currentserver.Update();
        }

        private static void OnClientDisconnected(object? sender, ServerDisconnectedEventArgs client)
        {
            playerCount = currentserver.ClientCount;

            if (client.Client.Id == hostID)
            {
                if (playerCount != 0)
                {
                    // TODO: Add system for changing server host
                    RestartServer();
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
                hostID = client.Client.Id;
                UpdateWindow("Obtained new host!");
            } else 
                UpdateWindow($"Client {client.Client.Id} connected.");
        }
    }
    
}
