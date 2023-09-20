using Riptide;
using Riptide.Transports.Udp;
using Riptide.Utils;

using Open.Nat;

using System;
using System.Security;
using System.Net.NetworkInformation;
using ServerSoftware;
using ServerSoftware.Utilities;
using System.Runtime.InteropServices;

namespace Server
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
        public static Riptide.Server currentserver;
        public static int playerCount = 0;
        public static string currentLevelBarcode;
        private static Timer timer;
        private static int tickInterval = 5;

        private static void Main(string[] args)
        {
            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_CLOSE, MF_BYCOMMAND);
            Console.Title = "Fusion Dedicated Server";

            StartRiptideServer();

            PortHelper.OpenPort();
        }

        public static void InitializeCommandPrompt(string info = "")
        {
            Console.Clear();

            Console.WriteLine($"Player Count: {playerCount}");
            Console.WriteLine($"Server Code: {IPStuff.EncodeIPAddress(IPStuff.GetExternalIP())}");
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
            string[] typedArray = typed.Split(" ");
            command.identifier = typedArray[0];
            command.modifiers = typedArray;

            Commands.RunCommand(command);

        }

        public static void UpdateWindow(string info = "")
        {
            InitializeCommandPrompt(info);
        }

        public static void RestartServer(bool resetSyncables = false)
        {
            currentserver.Stop();

            StartRiptideServer(true);
        }

        private static void StartRiptideServer(bool reset = false)
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
            client.Client.CanTimeout = false;
            playerCount = currentserver.ClientCount;

            if (client.Client == host)
            {
                RestartServer(true);
            }
        }

        private static void OnClientConnected(object? sender, ServerConnectedEventArgs client)
        {
            if (client.Client.Id == 1)
            {
                host = client.Client;
            }

            playerCount = currentserver.ClientCount;

            UpdateWindow("Obtained new host!");
        }
    }
    
}
