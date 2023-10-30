using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.SDK.Achievements;
using LabFusion.SDK.Gamemodes;
using LabFusion.Senders;
using LabFusion.Utilities;
using Riptide;
using Riptide.Transports;
using SLZ.Bonelab;
using SLZ.Marrow.SceneStreaming;
using Steamworks.Data;
using static LabFusion.Network.RiptideNetworkLayer;
using LabFusion.Network;
using LabFusion.Preferences;
using LabFusion.Syncables;
using UnityEngine;

namespace LabFusion.Network
{
    public static class DedicatedServerMessageReader
    {
        // Handle the Server's request for us to be host
        [MessageHandler((ushort)RiptideMessageTypes.HostRequest)]
        public static void HandleHostRequest(Message message)
        {
            if (CurrentServerType.GetType() == ServerTypes.DEDICATED && isHost == false)
            {
                isHost = true;

                // Cleanup gamemodes
                GamemodeRegistration.ClearGamemodeTable();
                ModuleMessageHandler.ClearHandlerTable();

                // Cleanup information
                SyncManager.OnCleanup();
                Physics.autoSimulation = true;

                // Cleanup prefs
                FusionPreferences.ReceivedServerSettings = FusionPreferences.LocalServerSettings;

                // Update hooks
                MultiplayerHooking.Internal_OnDisconnect();

                // Create local id
                var id = new PlayerId(PlayerIdManager.LocalLongId, 0, InternalServerHelpers.GetInitialMetadata(), InternalServerHelpers.GetInitialEquippedItems());
                id.Insert();
                PlayerIdManager.ApplyLocalId();

                // Register module message handlers so they can send messages
                var names = ModuleMessageHandler.GetExistingTypeNames();
                ModuleMessageHandler.PopulateHandlerTable(names);

                // Register gamemodes
                var gamemodeNames = GamemodeRegistration.GetExistingTypeNames();
                GamemodeRegistration.PopulateGamemodeTable(gamemodeNames);

                // Update hooks
                MultiplayerHooking.Internal_OnStartServer();

                Message response = Message.Create(MessageSendMode.Reliable, (ushort)RiptideMessageTypes.HostRequest);
                response.Release();

                response.AddBool(true);

                RiptideNetworkLayer.currentclient.Send(response);

                FusionNotifier.Send(new FusionNotification()
                {
                    title = "New Host",
                    showTitleOnPopup = false,
                    isMenuItem = false,
                    isPopup = true,
                    message = $"You are now handling all server messages!",
                    popupLength = 3f,
                });

            }
            else
            {
                RiptideNetworkLayer.isHost = false;

                Message response = Message.Create(MessageSendMode.Reliable, RiptideMessageTypes.HostRequest);
                response.Release();

                response.AddBool(false);

                RiptideNetworkLayer.currentclient.Send(response);
            }
        }

        // Handle a Server Command
        [MessageHandler((ushort)RiptideMessageTypes.ServerCommand)]
        public static void HandleServerCommand(Message message)
        {
            if (CurrentServerType.GetType() == ServerTypes.DEDICATED)
            {
                CommandTypes type = (CommandTypes)message.GetInt();
                string commandString = message.GetString();
                int commandInt = message.GetInt();
                switch (type)
                {
                    case CommandTypes.ReloadLevel:
                        SceneStreamer.Reload();
                        break;
                    case CommandTypes.LoadLevel:
                        if (FusionSceneManager.HasLevel(commandString))
                            SceneStreamer.Load(commandString);
                        else
                            FusionNotifier.Send(new FusionNotification()
                            {
                                title = "Error Loading Level",
                                showTitleOnPopup = true,
                                isMenuItem = false,
                                isPopup = true,
                                message = $"The dedicated server tried loading a level which you don't have!",
                                popupLength = 3f,
                            });
                        break;
                }
            }
        }

        // Handles the disconnection of players in Dedicated servers
        [MessageHandler((ushort)RiptideMessageTypes.LobbyDisconnect)]
        public static void HandleDisconnect(Message message)
        {
            ushort id = message.GetUShort();

            if (id == 0)
            {
                currentclient.Disconnect();
            }
            else
            {
                // Update the mod so it knows this user has left
                InternalServerHelpers.OnUserLeave(id);

                // Send disconnect notif to everyone
                ConnectionSender.SendDisconnect(id);
            }
        }
    }
}
