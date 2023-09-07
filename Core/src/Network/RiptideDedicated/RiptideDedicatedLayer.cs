using LabFusion.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Riptide;
using BoneLib.BoneMenu.Elements;
using LabFusion.BoneMenu;
using UnityEngine;

namespace LabFusion.Core.src.Network.RiptideDedicated
{
    public class RiptideDedicatedLayer : NetworkLayer
    {
        internal override string Title => "Riptide Dedicated";
        private Client _client;

        internal override bool CheckSupported()
        {
            // Supported on all devices, so return true
            return true;
        }

        internal override bool CheckValidation()
        {
            // All libs are required as plugins, anyway, so return true
            return true;
        }

        internal override void Disconnect(string reason = "")
        {
            throw new NotImplementedException();
        }

        internal override void OnCleanupLayer()
        {
            throw new NotImplementedException();
        }

        internal override void OnInitializeLayer()
        {
            _client = new Client();
        }

        internal override void StartServer()
        {
            // Server isn't hosted in a socket or on device, duh
            return;
        }

        internal override void SendToServer(NetworkChannel channel, FusionMessage message)
        {
            base.SendToServer(channel, message);
        }

        internal override void SendFromServer(byte userId, NetworkChannel channel, FusionMessage message)
        {
            base.SendFromServer(userId, channel, message);
        }

        internal override void OnSetupBoneMenu(MenuCategory category)
        {
            // Create the basic options
            CreateMatchmakingMenu(category);
            BoneMenuCreator.CreateGamemodesMenu(category);
            CreateRiptideSettings(category);
            BoneMenuCreator.CreateSettingsMenu(category);
            BoneMenuCreator.CreateNotificationsMenu(category);
            BoneMenuCreator.CreateBanListMenu(category);

#if DEBUG
            // Debug only (dev tools)
            BoneMenuCreator.CreateDebugMenu(category);
#endif
        }

        private void CreateMatchmakingMenu(MenuCategory category)
        {
            var matchmaking = category.CreateCategory("Matchmaking", Color.red);

            var manualJoining = matchmaking.CreateCategory("Manual Joining", Color.white);

            var serverListings = matchmaking.CreateCategory("Server List", Color.white);
        }

        private void CreateRiptideSettings(MenuCategory category)
        {

        }
    }
}
