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
            } else
            {
                CurrentNetworkLayer.OnLateInitializeLayer();
            }
        }

        internal static void OnCleanupLayer() {
            if (CurrentNetworkLayer == null)
            {
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
            }
            else
            {
                CurrentNetworkLayer.OnUpdateLayer();
            }
        }

        internal static void OnLateUpdateLayer() {
            if (CurrentNetworkLayer == null)
            {
            }
            else
            {
                CurrentNetworkLayer.OnLateUpdateLayer();
            }
        }

        internal static void OnGUILayer() {
            if (CurrentNetworkLayer == null)
            {
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
            }
            else
            {
                CurrentNetworkLayer.OnVoiceBytesReceived(id, bytes);
            }
        }

        internal static void OnSetupBoneMenuLayer(MenuCategory category) {
            if (CurrentNetworkLayer == null)
            {
            } else
            {
                CurrentNetworkLayer.OnSetupBoneMenu(category);
            }
        }

        internal static void OnUserJoin(PlayerId id) {
            if (CurrentNetworkLayer == null)
            {
            }
            else
            {
                CurrentNetworkLayer.OnUserJoin(id);
            }
        }
    }
}
