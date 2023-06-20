using JetBrains.Annotations;
using LabFusion.Network;
using LabFusion.Preferences;
using LabFusion.Utilities;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Core.src.Network.Riptide
{
    internal class RiptideLobby : INetworkLobby
    {
<<<<<<< Updated upstream
=======
<<<<<<< HEAD
        public Action CreateJoinDelegate(LobbyMetadataInfo info) {
            if (!info.ClientHasLevel)
            {
                return ( ) =>
            {
                FusionNotifier.Send(new FusionNotification() {
                        title = "Failed to Join",
                        showTitleOnPopup = true,
                        isMenuItem = false,
                        isPopup = true,
                        message = $"You do not have the map {info.LevelName} installed!",
                        popupLength = 6f,
                    });
                };
            }

            if (NetworkInfo.CurrentNetworkLayer is RiptideNetworkLayer riptideLayer)
            {
                return () => {
                    string ip = FusionPreferences.ClientSettings.ServerCode;
                    riptideLayer.ConnectToServer(ip);
                };
            }

            return null;
=======
>>>>>>> Stashed changes
        //this might need to be more fusion oriented than rioptide oriented, needs investigating
        /*private ConnectionState _lobby;

        public RiptideLobby(ConnectionState lobby)
        {
            _lobby = lobby;
        }*/
        public Action CreateJoinDelegate(LobbyMetadataInfo info)
        {
            throw new NotImplementedException();
>>>>>>> cc9b3739aa09b24077619e87495756382b427530
        }

        public string GetMetadata(string key)
        {
            throw new NotImplementedException();
        }

        public void SetMetadata(string key, string value)
        {
            throw new NotImplementedException();
        }

        public bool TryGetMetadata(string key, out string value)
        {
            throw new NotImplementedException();
        }
    }
}

