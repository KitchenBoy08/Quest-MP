using Open.Nat;
using Riptide;
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
                    if (Server.ServerClass.hasPortForwarded)
                        PortHelper.Close();
                    break;
                case "kick":
                    try
                    {
                        if (!ushort.TryParse(command.modifiers[1], out ushort id))
                        {
                            Server.ServerClass.UpdateWindow($"{id} is not the correct format of ID!");
                        }
                    } catch 
                    {
                        Server.ServerClass.UpdateWindow($"Incorrect ID format!");
                    }
                    if (ushort.Parse(command.modifiers[1].ToString()) != 0)
                    {
                        if (Server.ServerClass.currentserver.TryGetClient(ushort.Parse(command.modifiers[1]), out Connection player))
                        {
                            Server.ServerClass.currentserver.DisconnectClient(player);
                            Server.ServerClass.UpdateWindow($"Kicked player with ID {player.Id}");
                        }
                    } else
                    {
                        Server.ServerClass.UpdateWindow("Invalid Command! Add a player ID!");
                    }
                    break;
                case "restart":
                    Server.ServerClass.RestartServer();
                    break;
                case "help":
                    Server.ServerClass.UpdateWindow(
                        "[exit]: Closes the app and closes port map (MUST USE THIS TO CLOSE APP)\n" +
                        "[kick (playerID)]: Kicks the client that matches the ID from the server\n" +
                        "[restart (Reset Syncables)]: Closes and Opens the riptide server, kicking all clients. Type true or false after depending on if you want to reset syncables or not.");
                    break;
                default:
                    Server.ServerClass.UpdateWindow("Invalid Command!");
                    break;
            }
        }
    }
}
