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
    internal class RiptideLobby : INetworkLobby
    {
        public Action CreateJoinDelegate(LobbyMetadataInfo info)
        {
            throw new NotImplementedException();
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




