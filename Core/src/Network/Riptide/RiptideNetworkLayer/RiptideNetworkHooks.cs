using LabFusion.Representation;
using LabFusion.Utilities;
using System;
using Riptide;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LabFusion.Network.RiptideNetworkLayer;
using LabFusion.Senders;

namespace LabFusion.Network
{
    public partial class RiptideNetworkLayer
    {
        public void OnStarted(object sender, System.EventArgs e)
        {
            currentserver.ClientDisconnected += OnPlayerDisconnect;

            currentclient.Connected -= OnStarted;
#if DEBUG
            FusionLogger.Log("SERVER START HOOKED");
#endif
            CurrentServerType.SetType(ServerTypes.P2P);

            // Update player ID here since it's determined on the Riptide Client ID
            PlayerIdManager.SetLongId(currentclient.Id);

            OnUpdateRiptideLobby();

            // Call server setup
            InternalServerHelpers.OnStartServer();
        }

        private void OnPlayerDisconnect(object sender, ServerDisconnectedEventArgs client)
        {
            // Update the mod so it knows this user has left
            InternalServerHelpers.OnUserLeave(client.Client.Id);

            // Send disconnect notif to everyone
            ConnectionSender.SendDisconnect(client.Client.Id, GetDisconnectReason(client.Reason));
        }

        public void OnDisconnect(object sender, Riptide.DisconnectedEventArgs disconnect)
        {
            FusionLogger.Error($"Disconnected with reason {disconnect.Reason}");
            InternalServerHelpers.OnDisconnect(GetDisconnectReason(disconnect.Reason));

            if (currentclient.IsConnected)
                currentclient.Disconnect();

            if (currentserver.IsRunning)
                currentserver.Stop();

            isHost = false;

            OnUpdateRiptideLobby();
        }
    }
}
