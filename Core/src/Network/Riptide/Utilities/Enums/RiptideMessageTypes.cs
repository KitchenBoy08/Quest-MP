using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Network
{
    public enum RiptideMessageTypes
    {
        #region BASE
        FusionMessage = 0,
        ServerType = 1,
        #endregion

        #region DEDICATED
        HostRequest = 10,
        ServerCommand = 11,
        SendToServer = 12,
        SendFromServer = 13,
        Broadcast = 14,
        LevelInfo = 15,
        Notification = 16,
        #endregion

        #region PUBLIC
        CreateLobby = 20,
        JoinLobby = 21,
        LobbyInfo = 22,
        LobbyDisconnect = 23,
        RequestLobbies = 24,
        UpdateLobbyInfo = 25,
        #endregion
    }
}
