using BoneLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Network
{
    public static class ServerSettingsHooks
    {
        public static void OnAllowPCUsersChanged(bool obj) => PublicLobbyManager.UpdateServerInfo();
        public static void OnAllowQuestUsersChanged(bool obj) => PublicLobbyManager.UpdateServerInfo();
        public static void OnMaxPlayersChanged(byte obj) => PublicLobbyManager.UpdateServerInfo();
        public static void OnServerPrivacyChanged(ServerPrivacy privacy) => PublicLobbyManager.UpdateServerInfo();
        public static void OnChangeLevel(LevelInfo info) => PublicLobbyManager.UpdateServerInfo();
    }
}
