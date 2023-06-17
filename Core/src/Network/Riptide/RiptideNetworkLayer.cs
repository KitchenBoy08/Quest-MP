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
using BoneLib.BoneMenu.Elements;
using LabFusion.Representation;

namespace LabFusion.Network
{
    internal class RiptideNetworkLayer : NetworkLayer
    {
        /// <summary>
        /// Returns true if this layer is hosting a server.
        /// </summary>
        internal override bool IsServer => false;

        /// <summary>
        /// Returns true if this layer is a client inside of a server (still returns true if this is the host!)
        /// </summary>
        internal override bool IsClient => false;

        /// <summary>
        /// Returns true if the networking solution allows the server to send messages to the host (Actual Server Logic vs P2P).
        /// </summary>
        internal override bool ServerCanSendToHost => true;

        /// <summary>
        /// Returns the current active lobby.
        /// </summary>
        internal override INetworkLobby CurrentLobby => null;

        /// <summary>
        /// Starts the server.
        /// </summary>
        internal override void StartServer()
        {

        }

        /// <summary>
        /// Disconnects the client from the connection and/or server.
        /// </summary>
        internal override void Disconnect(string reason = "")
        {

        }

        /// <summary>
        /// Returns the username of the player with id userId.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        internal override string GetUsername(ulong userId) => "Unknown";

        /// <summary>
        /// Returns true if this is a friend (ex. steam friends).
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        internal override bool IsFriend(ulong userId) => false;

        /// <summary>
        /// Sends the message to the specified user if this is a server.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        internal override void SendFromServer(byte userId, NetworkChannel channel, FusionMessage message) { }

        /// <summary>
        /// Sends the message to the specified user if this is a server.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        internal override void SendFromServer(ulong userId, NetworkChannel channel, FusionMessage message) { }

        /// <summary>
        /// Sends the message to the dedicated server.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        internal override void SendToServer(NetworkChannel channel, FusionMessage message) { }

        /// <summary>
        /// Sends the message to the server if this is a client. Sends to all clients if this is a server.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        internal override void BroadcastMessage(NetworkChannel channel, FusionMessage message) { }

        /// <summary>
        /// If this is a server, sends this message back to all users except for the provided id.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        internal override void BroadcastMessageExcept(byte userId, NetworkChannel channel, FusionMessage message, bool ignoreHost = true)
        {
            for (var i = 0; i < PlayerIdManager.PlayerIds.Count; i++)
            {
                var id = PlayerIdManager.PlayerIds[i];

                if (id.SmallId != userId && (id.SmallId != 0 || !ignoreHost))
                    SendFromServer(id.SmallId, channel, message);
            }
        }

        /// <summary>
        /// If this is a server, sends this message back to all users except for the provided id.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        internal override void BroadcastMessageExcept(ulong userId, NetworkChannel channel, FusionMessage message, bool ignoreHost = true)
        {
            for (var i = 0; i < PlayerIdManager.PlayerIds.Count; i++)
            {
                var id = PlayerIdManager.PlayerIds[i];
                if (id.LongId != userId && (id.SmallId != 0 || !ignoreHost))
                    SendFromServer(id.SmallId, channel, message);
            }
        }

        internal override void OnInitializeLayer()
        {

        }

        internal override void OnLateInitializeLayer() { }

        internal override void OnCleanupLayer() { }

        internal override void OnUpdateLayer() { }

        internal override void OnLateUpdateLayer() { }

        internal override void OnGUILayer() { }

        internal override void OnVoiceChatUpdate() { }

        internal override void OnVoiceBytesReceived(PlayerId id, byte[] bytes) { }

        internal override void OnUserJoin(PlayerId id) { }

        internal override void OnSetupBoneMenu(MenuCategory category) { }
    }
}
