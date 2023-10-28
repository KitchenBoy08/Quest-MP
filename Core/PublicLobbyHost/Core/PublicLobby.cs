using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PublicLobbyHost
{
    public class PublicLobby
    {
        public int ServerID;

        public ushort hostID;
        public List<ushort> clientIDs = new List<ushort>();

        // Custom lobby meta data (This is just for now. I will likely switch to using fusions Lobby metadata later when I have a lot of time on my hands)
        public string LobbyName;
        public string LobbyVersion;
        public int PlayerCount;

        /// Lobby settings
        public int Privacy;
        public byte MaxPlayers;
        public bool AllowQuestUsers;
        public bool AllowPCUsers;

        /// Lobby status
        public string LevelName;
    }
}
