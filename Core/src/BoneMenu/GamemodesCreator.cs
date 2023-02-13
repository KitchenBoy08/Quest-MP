﻿using BoneLib.BoneMenu;
using BoneLib.BoneMenu.Elements;

using LabFusion.SDK.Gamemodes;

using UnityEngine;

namespace LabFusion.BoneMenu
{
    internal static partial class BoneMenuCreator
    {
        private static MenuCategory _gamemodesCategory;
        private static FunctionElement _gamemodeElement;

        public static void CreateGamemodesMenu(MenuCategory category) {
            // Root category
            _gamemodesCategory = category.CreateCategory("Gamemodes", Color.cyan);
        }

        public static void SetActiveGamemodeText(string text) {
            if (_gamemodeElement != null)
                _gamemodeElement.SetName(text);
        }

        public static void RefreshGamemodes() {
            // Clear existing gamemodes just incase
            ClearGamemodes();

            // Add stop button
            _gamemodeElement = _gamemodesCategory.CreateFunctionElement("No Active Gamemode", Color.white, () =>
            {
                if (Gamemode.ActiveGamemode != null)
                    Gamemode.ActiveGamemode.StopGamemode();
            });

            // Add necessary gamemodes
            foreach (var gamemode in GamemodeManager.Gamemodes) {
                // Make sure the gamemode isnt null
                if (gamemode == null)
                    continue;

                // Make sure this gamemode should be in bonemenu
                if (gamemode.VisibleInBonemenu) {
                    var upperCategory = _gamemodesCategory.CreateCategory(gamemode.GamemodeCategory, Color.white);
                    var lowerCategory = upperCategory.CreateCategory(gamemode.GamemodeName, Color.white);
                    gamemode.OnBoneMenuCreated(lowerCategory);
                }
            }
        }

        public static void ClearGamemodes() {
            // Clear all gamemodes from the list
            _gamemodesCategory.Elements.Clear();
        }
    }
}