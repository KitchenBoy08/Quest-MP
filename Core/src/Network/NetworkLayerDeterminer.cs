using BoneLib;

using LabFusion.Data;
using LabFusion.Debugging;
using LabFusion.Preferences;
using LabFusion.Utilities;
using Steamworks;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Network {
    public static class NetworkLayerDeterminer {
        public static NetworkLayer LoadedLayer { get; private set; }
        public static string LoadedTitle { get; private set; }

        public static NetworkLayer GetDefaultLayer() {
            if (HelperMethods.IsAndroid())
<<<<<<< HEAD
                return NetworkLayer.GetLayer<RiptideNetworkLayer>();
=======
                return NetworkLayer.GetLayer<ProxySteamVRNetworkLayer>();
>>>>>>> 660a2c688948238ba1838c5e0fb439055d41a7c5

            return NetworkLayer.GetLayer<SteamVRNetworkLayer>();
        }

        public static NetworkLayer VerifyLayer(NetworkLayer layer) {
            if (layer.CheckValidation()) {
                return layer;
            }
            else if (layer.TryGetFallback(out var fallback)) {
                return VerifyLayer(fallback);
            }
            else {
                return NetworkLayer.GetLayer<EmptyNetworkLayer>();
            }
        }

        public static void LoadLayer() {
            var title = FusionPreferences.ClientSettings.NetworkLayerTitle.GetValue();
            if (!NetworkLayer.LayerLookup.TryGetValue(title, out var layer)) {
                layer = GetDefaultLayer();
            }

            layer = VerifyLayer(layer);

            LoadedLayer = layer;
            LoadedTitle = layer.Title;
        }
    }
}
