using Riptide;
using Server;
using Server.Enums;
using ServerSoftware.Utilities;
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
                        InternalUtils.ClosePort();
                    ServerClass.currentserver.Stop();
                    Environment.Exit(0);
                    break;
                case "kick":
                    try
                    {
                        if (!ushort.TryParse(command.modifiers[1], out ushort id))
                        {
                            ServerClass.UpdateWindow($"'{command.modifiers[1]}' is not the correct format of ID!");
                        }
                    }
                    catch (Exception e)
                    {
                        ServerClass.UpdateWindow($"Failed to parse ID with error: {e}");
                    }
                    if (ServerClass.currentserver.TryGetClient(ushort.Parse(command.modifiers[1]), out Connection client))
                    {
                        ServerClass.currentserver.DisconnectClient(client);
                        ServerClass.UpdateWindow($"Kicked client with ID: {client.Id}");
                    }
                    else
                        ServerClass.UpdateWindow($"Client not found with ID: {ushort.Parse(command.modifiers[1])}");
                    break;
                case "restart":
                    try
                    {
                        if (bool.TryParse(command.modifiers[1], out bool boolValue))
                            ServerClass.RestartServer(boolValue);
                        else
                            ServerClass.UpdateWindow("Invalid Command! Make sure to add a 'true' or 'false' value after 'restart'.");
                    }
                    catch
                    {
                        ServerClass.UpdateWindow("Invalid Command! Make sure to add a 'true' or 'false' value after 'restart'.");
                    }
                    break;
                case "stop":
                    if (ServerClass.currentserver.IsRunning)
                    {
                        ServerClass.currentserver.Stop();
                        ServerClass.UpdateWindow("Stopped server.");
                    }
                    else
                    {
                        ServerClass.UpdateWindow("Server was already stopped.");
                    }
                    break;
                case "start":
                    if (!ServerClass.currentserver.IsRunning)
                    {
                        ServerClass.StartRiptideServer();
                    }
                    else
                    {
                        ServerClass.UpdateWindow("Server was already running.");
                    }
                    break;
                case "help":
                    ServerClass.UpdateWindow(
                        "Commands are CASE SENSITIVE!\n\n" +

                        "[exit]: Closes the app and closes port map (MUST USE THIS TO CLOSE APP).\n" +
                        "[kick (Player ID)]: Kicks the client that matches the ID from the server.\n" +
                        "[restart (Reset Syncables)]: Closes and Opens the riptide server, kicking all clients. Type true or false after depending on if you want to reset syncables or not.\n" +
                        "[debug]: Displays debug info inside the prompt menu.\n" +
                        "[stop]: Stops the currently running server.\n" +
                        "[start]: Starts the server if it wasn't already running.\n" +
                        "[loadlevel (Level Barcode)]: Loads the level barcode on the host end.\n" +
                        "[reloadlevel]: Reloads the current level on the host end."
                        );
                    break;
                case "debug":
                    if (ServerClass.hostID != 0)
                        ServerClass.UpdateWindow(
                            $"Current Host ID: {ServerClass.hostID}"
                            );
                    else
                        ServerClass.UpdateWindow(
                            $"No current server host"
                            );
                    break;
                case "reloadlevel":
                    SendCommandToHost(CommandTypes.ReloadLevel);
                    ServerClass.UpdateWindow("Sent Reload");
                    break;
                case "loadlevel":
                    try
                    {
                        string levelBarcode = command.modifiers[1];
                        SendCommandToHost(CommandTypes.LoadLevel, levelBarcode);
                        ServerClass.UpdateWindow("Sent Level Load");
                    } catch (Exception e)
                    {
                        ServerClass.UpdateWindow($"Failed to send load level with error: {e}");
                    }
                    break;
                default:
                    ServerClass.UpdateWindow("Invalid Command!");
                    break;
            }
        }

        private static void SendCommandToHost(CommandTypes type, string commandString = "", int commandInt = 0)
        {
            Message commandMessage = Message.Create(MessageSendMode.Reliable, RiptideMessageTypes.ServerCommand);
            commandMessage.Release();

            commandMessage.AddInt((int)type);
            commandMessage.AddString(commandString);
            commandMessage.AddInt(commandInt);

            ServerClass.currentserver.Send(commandMessage, (ushort)ServerClass.hostID);
        }
    }

    enum CommandTypes
    {
        ReloadLevel = 0,
        LoadLevel = 1,
        ToggleOP = 2,
    }
}
