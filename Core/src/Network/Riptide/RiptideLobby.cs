using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Utilities;

using Riptide.Transports;
using Riptide.Utils;
using Riptide;

namespace LabFusion.Network
{
    internal class RiptideLobby : INetworkLobby {
        private  _lobby;  //Needs lobby data from Network Layer

        public RiptideLobby(Server lobby) { 
            _lobby = lobby;
        }

        void SetMetadata(string key, string value)
        {

        }

        bool TryGetMetadata(string key, out string value)
        {

        }

        string GetMetadata(string key)
        {

        }

        Action CreateJoinDelegate(LobbyMetadataInfo info)
        {
            if (!info.ClientHasLevel)
            {
                return () => {
                    FusionNotifier.Send(new FusionNotification()
                    {
                        title = "Failed to Join",
                        showTitleOnPopup = true,
                        isMenuItem = false,
                        isPopup = true,
                        message = $"You do not have the map {info.LevelName} installed!",
                        popupLength = 6f,
                    });
                };
            }
    }
}



