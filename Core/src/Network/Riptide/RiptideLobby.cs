using JetBrains.Annotations;
using LabFusion.Network;
using LabFusion.Preferences;
using LabFusion.Utilities;
using Riptide;
using Riptide.Utils;
using Riptide.Transports;
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
        public Action CreateJoinDelegate(LobbyMetadataInfo info)
        {
            if (!info.ClientHasLevel)
            {
                return () =>
            {
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

            if (NetworkInfo.CurrentNetworkLayer is RiptideNetworkLayer riptideLayer)
            {
                return () =>
                {
                    string ip = FusionPreferences.ClientSettings.ServerCode;
                    riptideLayer.ConnectToServer(ip);
                };
            }

            return null;
        }

        //this might need to be more fusion oriented than Riptide oriented, needs investigating
        /*private ConnectionState _lobby;

        public RiptideLobby(ConnectionState lobby)
        {
            _lobby = lobby;
        }*/

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

