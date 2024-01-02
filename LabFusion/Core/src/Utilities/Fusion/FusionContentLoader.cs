using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BoneLib;

using LabFusion.Data;

using UnityEngine;

namespace LabFusion.Utilities
{
    public static class FusionContentLoader
    {
        public static AssetBundle ContentBundle { get; private set; }

        public static GameObject PointShopPrefab { get; private set; }
        public static GameObject InfoBoxPrefab { get; private set; }
        public static GameObject CupBoardPrefab { get; private set; }

        public static GameObject EntangledLinePrefab { get; private set; }

        public static GameObject AchievementPopupPrefab { get; private set; }
        public static GameObject BitPopupPrefab { get; private set; }
        public static GameObject MutePopupPrefab { get; private set; }

        public static Texture2D SabrelakeLogo { get; private set; }
        public static Texture2D LavaGangLogo { get; private set; }

        public static AudioClip GeoGrpFellDownTheStairs { get; private set; }
        public static AudioClip BouncingStrong { get; private set; }

        public static AudioClip LavaGangVictory { get; private set; }
        public static AudioClip SabrelakeVictory { get; private set; }

        public static AudioClip LavaGangFailure { get; private set; }
        public static AudioClip SabrelakeFailure { get; private set; }

        public static AudioClip DMTie { get; private set; }

        public static AudioClip BitGet { get; private set; }

        public static AudioClip UISelect { get; private set; }
        public static AudioClip UIDeny { get; private set; }
        public static AudioClip UIConfirm { get; private set; }
        public static AudioClip UITurnOff { get; private set; }
        public static AudioClip UITurnOn { get; private set; }

        public static AudioClip PurchaseFailure { get; private set; }
        public static AudioClip PurchaseSuccess { get; private set; }

        public static AudioClip EquipItem { get; private set; }
        public static AudioClip UnequipItem { get; private set; }

        public static Texture2D NotificationInformation { get; private set; }
        public static Texture2D NotificationWarning { get; private set; }
        public static Texture2D NotificationError { get; private set; }
        public static Texture2D NotificationSuccess { get; private set; }

        // Laser cursor
        public static GameObject LaserCursor { get; private set; }
        public static AudioClip LaserPulseSound { get; private set; }
        public static AudioClip LaserRaySpawn { get; private set; }
        public static AudioClip LaserRayDespawn { get; private set; }
        public static AudioClip LaserPrismaticSFX { get; private set; }

        private static readonly string[] _combatSongNames = new string[6] {
            "music_FreqCreepInModulationBuggyPhysics",
            "music_SicklyBugInitiative",
            "music_SyntheticCavernsRemix",
            "music_WWWonderlan",
            "music_SmigglesInDespair",
            "music_AppenBeyuge",
        };

        private static readonly List<AudioClip> _combatPlaylist = new();
        public static AudioClip[] CombatPlaylist => _combatPlaylist.ToArray();

        private static AssetBundleCreateRequest _contentBundleRequest = null;

        private static void OnBundleCompleted(AsyncOperation operation)
        {
            ContentBundle = _contentBundleRequest.assetBundle;

            ContentBundle.LoadPersistentAssetAsync<GameObject>(ResourcePaths.PointShopPrefab, (v) => { PointShopPrefab = v; });
            ContentBundle.LoadPersistentAssetAsync<GameObject>(ResourcePaths.InfoBoxPrefab, (v) => { InfoBoxPrefab = v; });
            ContentBundle.LoadPersistentAssetAsync<GameObject>(ResourcePaths.CupBoardPrefab, (v) => { CupBoardPrefab = v; });

            ContentBundle.LoadPersistentAssetAsync<GameObject>(ResourcePaths.EntangledLinePrefab, (v) => { EntangledLinePrefab = v; });

            ContentBundle.LoadPersistentAssetAsync<GameObject>(ResourcePaths.AchievementPopupPrefab, (v) => { AchievementPopupPrefab = v; });
            ContentBundle.LoadPersistentAssetAsync<GameObject>(ResourcePaths.BitPopupPrefab, (v) => { BitPopupPrefab = v; });
            ContentBundle.LoadPersistentAssetAsync<GameObject>(ResourcePaths.MutePopupPrefab, (v) => { MutePopupPrefab = v; });

            ContentBundle.LoadPersistentAssetAsync<Texture2D>(ResourcePaths.SabrelakeLogo, (v) => { SabrelakeLogo = v; });
            ContentBundle.LoadPersistentAssetAsync<Texture2D>(ResourcePaths.LavaGangLogo, (v) => { LavaGangLogo = v; });

            foreach (var song in _combatSongNames)
            {
                ContentBundle.LoadPersistentAssetAsync<AudioClip>(song, (v) => { _combatPlaylist.Add(v); });
            }

            ContentBundle.LoadPersistentAssetAsync<AudioClip>(ResourcePaths.GeoGrpFellDownTheStairs, (v) => { GeoGrpFellDownTheStairs = v; });
            ContentBundle.LoadPersistentAssetAsync<AudioClip>(ResourcePaths.BouncingStrong, (v) => { BouncingStrong = v; });

            ContentBundle.LoadPersistentAssetAsync<AudioClip>(ResourcePaths.LavaGangVictory, (v) => { LavaGangVictory = v; });
            ContentBundle.LoadPersistentAssetAsync<AudioClip>(ResourcePaths.SabrelakeVictory, (v) => { SabrelakeVictory = v; });

            ContentBundle.LoadPersistentAssetAsync<AudioClip>(ResourcePaths.LavaGangFailure, (v) => { LavaGangFailure = v; });
            ContentBundle.LoadPersistentAssetAsync<AudioClip>(ResourcePaths.SabrelakeFailure, (v) => { SabrelakeFailure = v; });

            ContentBundle.LoadPersistentAssetAsync<AudioClip>(ResourcePaths.DMTie, (v) => { DMTie = v; });

            ContentBundle.LoadPersistentAssetAsync<AudioClip>(ResourcePaths.BitGet, (v) => { BitGet = v; });

            ContentBundle.LoadPersistentAssetAsync<AudioClip>(ResourcePaths.UISelect, (v) => { UISelect = v; });
            ContentBundle.LoadPersistentAssetAsync<AudioClip>(ResourcePaths.UIDeny, (v) => { UIDeny = v; });
            ContentBundle.LoadPersistentAssetAsync<AudioClip>(ResourcePaths.UIConfirm, (v) => { UIConfirm = v; });
            ContentBundle.LoadPersistentAssetAsync<AudioClip>(ResourcePaths.UITurnOff, (v) => { UITurnOff = v; });
            ContentBundle.LoadPersistentAssetAsync<AudioClip>(ResourcePaths.UITurnOn, (v) => { UITurnOn = v; });

            ContentBundle.LoadPersistentAssetAsync<AudioClip>(ResourcePaths.PurchaseFailure, (v) => { PurchaseFailure = v; });
            ContentBundle.LoadPersistentAssetAsync<AudioClip>(ResourcePaths.PurchaseSuccess, (v) => { PurchaseSuccess = v; });

            ContentBundle.LoadPersistentAssetAsync<AudioClip>(ResourcePaths.EquipItem, (v) => { EquipItem = v; });
            ContentBundle.LoadPersistentAssetAsync<AudioClip>(ResourcePaths.UnequipItem, (v) => { UnequipItem = v; });

            ContentBundle.LoadPersistentAssetAsync<Texture2D>(ResourcePaths.NotificationInformation, (v) => { NotificationInformation = v; });
            ContentBundle.LoadPersistentAssetAsync<Texture2D>(ResourcePaths.NotificationWarning, (v) => { NotificationWarning = v; });
            ContentBundle.LoadPersistentAssetAsync<Texture2D>(ResourcePaths.NotificationError, (v) => { NotificationError = v; });
            ContentBundle.LoadPersistentAssetAsync<Texture2D>(ResourcePaths.NotificationSuccess, (v) => { NotificationSuccess = v; });

            ContentBundle.LoadPersistentAssetAsync<GameObject>(ResourcePaths.LaserCursor, (v) => { LaserCursor = v; });
            ContentBundle.LoadPersistentAssetAsync<AudioClip>(ResourcePaths.LaserPulseSound, (v) => { LaserPulseSound = v; });
            ContentBundle.LoadPersistentAssetAsync<AudioClip>(ResourcePaths.LaserRaySpawn, (v) => { LaserRaySpawn = v; });
            ContentBundle.LoadPersistentAssetAsync<AudioClip>(ResourcePaths.LaserRayDespawn, (v) => { LaserRayDespawn = v; });
            ContentBundle.LoadPersistentAssetAsync<AudioClip>(ResourcePaths.LaserPrismaticSFX, (v) => { LaserPrismaticSFX = v; });
        }

        public static void OnBundleLoad()
        {
            _contentBundleRequest = FusionBundleLoader.LoadAssetBundleAsync(ResourcePaths.ContentBundle);

            if (_contentBundleRequest != null)
            {
                _contentBundleRequest.add_completed((Il2CppSystem.Action<AsyncOperation>)OnBundleCompleted);
            }
            else
                FusionLogger.Error("Content Bundle failed to load!");
        }

        public static void OnBundleUnloaded()
        {
            // Unload content bundle
            if (ContentBundle != null)
                ContentBundle.Unload(true);
        }
    }
}
