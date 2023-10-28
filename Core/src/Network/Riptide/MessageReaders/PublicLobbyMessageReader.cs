using LabFusion.Representation;
using LabFusion.Senders;
using LabFusion.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Riptide;
using System.Threading.Tasks;
using static LabFusion.Network.RiptideNetworkLayer;

namespace LabFusion.Network
{
    public static class PublicLobbyMessageReader
    {
        // Handle starting a server
        [MessageHandler((ushort)RiptideMessageTypes.CreateLobby)]
        public static void HandleCreateLobby(Message message)
        {
            RiptideNetworkLayer.isHost = true;

#if DEBUG
            FusionLogger.Log("SERVER START HOOKED");
#endif
            CurrentServerType.SetType(ServerTypes.PUBLIC);

            // Update player ID here since it's determined on the Riptide Client ID
            PlayerIdManager.SetLongId(currentclient.Id);

            OnUpdateRiptideLobby();

            // Call server setup
            InternalServerHelpers.OnStartServer();
        }

        [MessageHandler((ushort)RiptideMessageTypes.JoinLobby)]
        public static void HandleJoinLobby(Message message)
        {
            currentPublicHostID = message.GetShort();

            CurrentServerType.SetType(ServerTypes.PUBLIC);

            ConnectionSender.SendConnectionRequest();
        }
    }
}
