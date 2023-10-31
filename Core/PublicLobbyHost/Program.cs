using Riptide;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace PublicLobbyHost
{
    public class PublicLobbyHost
    {
        public static List<PublicLobby> lobbies = new List<PublicLobby>();
        public static Server mainHost = new Server();

        private static System.Timers.Timer lobbyTimeout;
        private static System.Timers.Timer tick;
        static void Main(string[] args)
        {
            Console.CursorVisible = false;
            tick = new System.Timers.Timer(10);
            tick.Elapsed += Tick;
            tick.AutoReset = true;
            tick.Enabled = true;

            lobbyTimeout = new System.Timers.Timer(10000);
            lobbyTimeout.Elapsed += LobbyTimeoutCheck;
            lobbyTimeout.AutoReset = true;
            lobbyTimeout.Enabled = true;

            Console.Title = "TideFusion Public Lobby Host";
            mainHost.Start(7676, 3000);

            // Hooking
            mainHost.ClientDisconnected += OnClientDisconnect;

            InitializeCmd();
        }

        private static void Tick(object sender, ElapsedEventArgs e)
        {
            mainHost.Update();
        }

        private static void LobbyTimeoutCheck(object sender, ElapsedEventArgs e)
        {
            foreach (var lobby in lobbies)
            {
                if (!mainHost.TryGetClient(lobby.hostID, out var client))
                {
                    foreach (ushort id in lobby.clientIDs)
                    {
                        if (mainHost.TryGetClient(id, out client))
                            mainHost.DisconnectClient(client);
                    }
                }
            }
        }

        private static void OnClientDisconnect(object sender, ServerDisconnectedEventArgs client)
        {
            foreach (var lobby in lobbies)
            {
                if (lobby.hostID == client.Client.Id)
                {
                    foreach (var clientID in lobby.clientIDs)
                    {
                        // Send disconnect to all clients
                        Message disconnect = Message.Create(MessageSendMode.Reliable, 25);
                        disconnect.AddUShort(0);

                        mainHost.Send(disconnect, clientID);
                    }

                    lobbies.Remove(lobby);
                    return;
                }

                if (lobby.clientIDs.Contains(client.Client.Id))
                {
                    lobby.clientIDs.Remove(client.Client.Id);

                    // Send client disconnect to the server
                    Message disconnect = Message.Create(MessageSendMode.Reliable, 25);
                    disconnect.AddUShort(client.Client.Id);

                    mainHost.Send(disconnect, lobby.hostID);
                }

                if (mainHost.ClientCount == 0)
                {
                    mainHost.Stop();
                    mainHost.Start(7777, 3000);
                }
            }
        }

        private static void InitializeCmd()
        {
            UpdateWindow();
            string typed = Console.ReadLine();
            InitializeCmd();
        }

        public static void UpdateWindow(string info = "")
        {
            Console.Clear();
            Console.WriteLine($"Current Lobby Count: {lobbies.Count}");
            Console.WriteLine($"Current client count: {mainHost.ClientCount}");

            Console.WriteLine("\n===============================");

            if (info != "")
            {
                Console.WriteLine(info + System.Environment.NewLine);
            } else
            {
                Console.WriteLine("No info to display!");

                Console.WriteLine("===============================");
            }
        }

        public static PublicLobby GetPublicLobby(ushort hostId)
        {
            foreach (var lobby in lobbies)
            {
                if (lobby.hostID == hostId)
                    return lobby;
            }
            return null;
        }

        public static void UpdateLobby(PublicLobby lobby)
        {
            for (int i = 0; i < lobbies.Count; i++)
            {
                if (lobbies[i].hostID == lobby.hostID)
                    lobbies[i] = lobby;
            }
        }
    }
}
