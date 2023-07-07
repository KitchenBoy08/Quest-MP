using BoneLib.BoneMenu.Elements;

using LabFusion.Representation;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Network
{
    /// <summary>
    /// Internal class used for creating network layers and updating them.
    /// </summary>
    internal static class InternalLayerHelpers
    {
        internal static NetworkLayer CurrentNetworkLayer { get; private set; }

        internal static void SetLayer(NetworkLayer layer) {
            CurrentNetworkLayer = layer;
            CurrentNetworkLayer.OnInitializeLayer();
        }

        internal static void OnLateInitializeLayer() {
            if (CurrentNetworkLayer == null)
            {
                FusionLogger.Log("Current Network Layer is null while trying OnLateInitalizeLayer!");
            } else
            {
                CurrentNetworkLayer.OnLateInitializeLayer();
            }
        }

        internal static void OnCleanupLayer() {
            if (CurrentNetworkLayer == null)
            {
                FusionLogger.Log("Current Network Layer is null while trying OnCleanupLayer!");
            }
            else
            {
                CurrentNetworkLayer.OnCleanupLayer();

                CurrentNetworkLayer = null;
            }
        }

        internal static void OnUpdateLayer() {
            if (CurrentNetworkLayer == null)
            {
                FusionLogger.Log("Current Network Layer is null while trying OnUpdateLayer!");
            }
            else
            {
                CurrentNetworkLayer.OnUpdateLayer();
            }
        }

        internal static void OnLateUpdateLayer() {
            if (CurrentNetworkLayer == null)
            {
                FusionLogger.Log("Current Network Layer is null while trying OnLateUpdateLayer!");
            }
            else
            {
                CurrentNetworkLayer.OnLateUpdateLayer();
            }
        }

        internal static void OnGUILayer() {
            if (CurrentNetworkLayer == null)
            {
                FusionLogger.Log("Current Network Layer is null while trying OnGUILayer!");
            }
            else
            {
                CurrentNetworkLayer.OnGUILayer();
            }
        }

        internal static void OnVoiceChatUpdate()
        {
            if (CurrentNetworkLayer == null)
            {
                FusionLogger.Log("Current Network Layer is null while trying OnVoiceChatUpdate!");
            }
            else
            {
                CurrentNetworkLayer.OnVoiceChatUpdate();
            }
        }

        internal static void OnVoiceBytesReceived(PlayerId id, byte[] bytes)
        {
            if (CurrentNetworkLayer == null)
            {
                FusionLogger.Log("Current Network Layer is null while trying OnVoiceBytesRecieved!");
            }
            else
            {
                CurrentNetworkLayer.OnVoiceBytesReceived(id, bytes);
            }
        }

        internal static void OnSetupBoneMenuLayer(MenuCategory category) {
            if (CurrentNetworkLayer == null)
            {
                FusionLogger.Log("Current Network Layer is null while trying OnSetupBoneMenuLayer!");
            } else
            {
                CurrentNetworkLayer.OnSetupBoneMenu(category);
            }
        }

        internal static void OnUserJoin(PlayerId id) {
            if (CurrentNetworkLayer == null)
            {
                FusionLogger.Log("Current Network Layer is null while trying OnUserJoin!");
            }
            else
            {
                CurrentNetworkLayer.OnUserJoin(id);
            }
        }
    }
}
