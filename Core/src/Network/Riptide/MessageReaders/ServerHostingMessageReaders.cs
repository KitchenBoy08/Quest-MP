using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.SDK.Gamemodes;
using LabFusion.Senders;
using LabFusion.Utilities;
using Riptide;
using SLZ.Marrow.SceneStreaming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking.Match;

namespace LabFusion.Network
{
    public static class ServerHostingMessageReaders
    {
        [MessageHandler((ushort)RiptideMessageTypes.ServerType)]
        public static void HandleServerResponse(Message message)
        {
            ServerTypes type = (ServerTypes)message.GetInt();
            string levelBarcode = message.GetString();
            string levelName = message.GetString();

            switch (type)
            {
                #region P2P
                case (ServerTypes.P2P):
                    RiptideNetworkLayer.CurrentServerType.SetType(ServerTypes.P2P);

                    RiptideNetworkLayer.isHost = false;

                    PlayerIdManager.SetLongId(RiptideNetworkLayer.currentclient.Id);

                    ConnectionSender.SendConnectionRequest();
                    break;
                #endregion
                #region PUBLIC
                case (ServerTypes.PUBLIC):
                    RiptideNetworkLayer.CurrentServerType.SetType(ServerTypes.PUBLIC);

                    RiptideNetworkLayer.isHost = false;

                    PlayerIdManager.SetLongId(RiptideNetworkLayer.currentclient.Id);

                    ConnectionSender.SendConnectionRequest();
                    break;
                #endregion
                #region DEDICATED
                case (ServerTypes.DEDICATED):
                    if (!FusionSceneManager.HasLevel(levelBarcode) && levelBarcode != "NONE")
                    {
                        if (RiptideNetworkLayer.currentclient.IsConnected)
                            RiptideNetworkLayer.currentclient.Disconnect();

                        FusionNotifier.Send(new FusionNotification()
                        {
                            title = "Missing Level",
                            message = $"You are missing the level: \n{levelName}",
                            showTitleOnPopup = true,
                            isMenuItem = false,
                            isPopup = true,
                            type = NotificationType.ERROR,
                        });
                        return;
                    }

                    RiptideNetworkLayer.CurrentServerType.SetType(ServerTypes.DEDICATED);

                    #region HOST
                    if (RiptideNetworkLayer.currentclient.Id == 1)
                    {
                        RiptideNetworkLayer.isHost = true;

                        PlayerIdManager.SetLongId(RiptideNetworkLayer.currentclient.Id);

                        // Mimicking the OnStartServer method in order to make it custom
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

                        // Send a notification
                        FusionNotifier.Send(new FusionNotification()
                        {
                            title = "Connected to Server",
                            message = "Connected to Dedicated Server",
                            showTitleOnPopup = true,
                            isMenuItem = false,
                            isPopup = true,
                            type = NotificationType.SUCCESS,
                        });

                        if (levelBarcode == "NONE")
                            SceneStreamer.Reload();
                        else
                            SceneStreamer.Load(levelBarcode);
                    }
                    #endregion
                    #region NON-HOST
                    else
                    {
                        RiptideNetworkLayer.isHost = false;

                        PlayerIdManager.SetLongId(RiptideNetworkLayer.currentclient.Id);

                        ConnectionSender.SendConnectionRequest();
                    }
                    break;
                    #endregion
                    #endregion
            }
        }

        [MessageHandler((ushort)RiptideMessageTypes.ServerType)]
        public static void HandleClientRequest(ushort riptideID, Message message)
        {
            if (RiptideNetworkLayer.currentserver.TryGetClient(riptideID, out Connection client))
            {
                Message response = Message.Create(MessageSendMode.Reliable, 1);
                response.AddInt((int)ServerTypes.PUBLIC);
                response.AddString("NONE");
                response.AddString("NONE");

                RiptideNetworkLayer.currentserver.Send(response, client);
            }
        }
    }
}
