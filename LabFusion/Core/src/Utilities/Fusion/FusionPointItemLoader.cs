﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BoneLib;

using LabFusion.Data;
using LabFusion.SDK.Points;

using UnityEngine;

namespace LabFusion.Utilities
{
    public class ItemPair
    {
        public GameObject GameObject { get; private set; }
        public Texture2D Preview { get; private set; }

        public static ItemPair LoadFromBundle(AssetBundle bundle, string name)
        {
            var itemPair = new ItemPair();

            bundle.LoadPersistentAssetAsync<GameObject>(ResourcePaths.ItemPrefix + name, (v) => { itemPair.GameObject = v; });
            bundle.LoadPersistentAssetAsync<Texture2D>(ResourcePaths.PreviewPrefix + name, (v) => { itemPair.Preview = v; });

            return itemPair;
        }
    }

    public static class FusionPointItemLoader
    {
        public static AssetBundle ItemBundle { get; private set; }

        private static readonly string[] _itemNames = new string[] {
            // BaBa Corp Cosmetics
            // Pack 1
            nameof(Gemstone),
            nameof(GloopTrail),
            nameof(Junktion),
            nameof(PulsatingMass),
            nameof(RubixCube),
            nameof(BitsTrail),
            nameof(CardboardTophat),
            nameof(Glasses3D),
            nameof(AdventureHat),
            nameof(WaistRing),
            nameof(BodyPillow),
            nameof(BritishHelm),
            nameof(CardboardDisguise),
            nameof(CatBeanie),
            nameof(CheeseHat),
            nameof(ConstructionHat),
            nameof(Cooler),
            nameof(EltonGlasses),
            nameof(EnaHat),
            nameof(Fez),
            nameof(Firework),
            nameof(Fren),
            nameof(FryCookHat),
            nameof(Gearhead),
            nameof(GuardHelmet),
            nameof(KickMeSign),
            nameof(KnollHat),
            nameof(LaytonHat),
            nameof(LegoHead),
            nameof(LightningHead),
            nameof(Monocle),
            nameof(NyanTrail),
            nameof(PartyHat),
            nameof(PieceOfResistance),
            nameof(SorcererHat),
            nameof(StormyHead),
            nameof(TestItem),
            nameof(TheBeacon),
            nameof(WeirdShades),
            nameof(BitMiner),

            // Pack 2
            nameof(AncientTablet),
            nameof(BitBackpack),
            nameof(Briefcase),
            nameof(CirclingBit),
            nameof(Floatie),
            nameof(Headset),
            nameof(HotHead),
            nameof(Jetpack),
            nameof(NESGuitar),
            nameof(OctoSpecs),
            nameof(Potion),
            nameof(PsychicPerception),
            nameof(RetroMind),
            nameof(SignalingSpiral),
            nameof(Speaker),
            nameof(VioletVortex),

            // Special
            nameof(VictoryTrophy),

            // Riggle Cosmetics
            // Pack 1
            nameof(ArrowHead),
            nameof(BucketHat),
            nameof(CBRNGasMask),
            nameof(Crown),
            nameof(Fedoral),
            nameof(FlinchsJinjle),
            nameof(GP5),
            nameof(JesterHat),
            nameof(MissedAWideGlasses),
            nameof(MissedAWideHat),
            nameof(OldTimeyPipe),
            nameof(RiotHelmet),
            nameof(Smitty),
            nameof(VirtuallyInsane),
            nameof(Wonker),
            nameof(ZestySwagShades),
        };

        private static readonly Dictionary<string, ItemPair> _itemPairs = new();

        private static AssetBundleCreateRequest _itemBundleRequest = null;

        private static void OnBundleCompleted(AsyncOperation operation)
        {
            ItemBundle = _itemBundleRequest.assetBundle;

            foreach (var item in _itemNames)
            {
                _itemPairs.Add(item, ItemPair.LoadFromBundle(ItemBundle, item));
            }
        }

        public static void OnBundleLoad()
        {
            _itemBundleRequest = FusionBundleLoader.LoadAssetBundleAsync(ResourcePaths.ItemBundle);

            if (_itemBundleRequest != null)
            {
                _itemBundleRequest.add_completed((Il2CppSystem.Action<AsyncOperation>)OnBundleCompleted);
            }
            else
                FusionLogger.Error("Item bundle failed to load!");
        }

        public static void OnBundleUnloaded()
        {
            // Unload item bundle
            if (ItemBundle != null)
                ItemBundle.Unload(true);
        }

        public static ItemPair GetPair(string name)
        {
            return _itemPairs[name];
        }
    }
}
