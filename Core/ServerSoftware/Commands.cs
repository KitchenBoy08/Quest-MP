using Riptide;
using Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerSoftware
{
    public static class Commands
    {
        public class Command
        {
            public string identifier = "";
            public string[] modifiers;
        }

        public static void RunCommand(Command command)
        {
            switch (command.identifier)
            {
                case "exit":
                    if (ServerClass.hasPortForwarded)
                        PortHelper.ClosePort();
                    Environment.Exit(0);
                    break;
                case "kick":
                    try
                    {
                        if (!ushort.TryParse(command.modifiers[1], out ushort id))
                        {
                            ServerClass.UpdateWindow($"'{command.modifiers[1]}' is not the correct format of ID!");
                        }
                    } catch (Exception e)
                    {
                        ServerClass.UpdateWindow($"Failed to parse ID with error: {e}");
                    }
                    if (ServerClass.currentserver.TryGetClient(ushort.Parse(command.modifiers[1]), out Connection client))
                    {
                        ServerClass.currentserver.DisconnectClient(client);
                        ServerClass.UpdateWindow($"Kicked client with ID: {client.Id}");
                    } else
                        ServerClass.UpdateWindow($"Client not found with ID: {ushort.Parse(command.modifiers[1])}");
                    break;
                case "restart":
                    ServerClass.RestartServer();
                    break;
                case "help":
                    ServerClass.UpdateWindow(
                        "Commands are CASE SENSITIVE!\n" +
                        "[exit]: Closes the app and closes port map (MUST USE THIS TO CLOSE APP)\n" +
                        "[kick (playerID)]: Kicks the client that matches the ID from the server\n" +
                        "[restart (Reset Syncables)]: Closes and Opens the riptide server, kicking all clients. Type true or false after depending on if you want to reset syncables or not.\n" +
                        "[debug]: Displays debug info inside the prompt menu"
                        );
                    break;
                case "debug":
                    if (ServerClass.host != null)
                        ServerClass.UpdateWindow(
                            $"Current Host ID: {ServerClass.host.Id}"
                            );
                    else
                        ServerClass.UpdateWindow(
                            $"No current server host"
                            );
                    break;
                default:
                    ServerClass.UpdateWindow("Invalid Command!");
                    break;
            }
        }
    }
}
