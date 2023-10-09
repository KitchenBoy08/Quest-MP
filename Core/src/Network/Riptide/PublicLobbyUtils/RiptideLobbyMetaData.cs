using LabFusion.Senders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Core.src.Network.Riptide.PublicLobbyUtils
{
    public class RiptideLobbyMetaData
    {
        public int ServerID;

        public ushort hostID;

        public string LobbyName;
        public Version LobbyVersion;
        public int PlayerCount;

        /// Lobby settings
        public bool NametagsEnabled;
        public int Privacy;
        public TimeScaleMode TimeScaleMode;
        public int MaxPlayers;
        public bool VoicechatEnabled;
        public bool AllowQuestUsers;
        public bool AllowPCUsers;

        /// Lobby status
        public string LevelName;

        public string GamemodeName;
        public bool IsGamemodeRunning;

        public bool ClientHasLevel;
    }
}
