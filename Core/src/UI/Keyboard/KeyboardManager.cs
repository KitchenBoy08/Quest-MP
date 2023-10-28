using LabFusion.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;
using UnityEngine;
using TMPro;

namespace LabFusion.UI
{
    public class KeyboardManager
    {
        private static bool IsKeyboardOpen = false;
        public static void SpawnKeyboard()
        {
            if (IsKeyboardOpen)
                return;

            // Make sure we have the prefab
            if (FusionContentLoader.KeyboardPrefab == null)
            {
                FusionLogger.Warn("Missing the Keyboard prefab!");
                return;
            }

            

            // Create the GameObject
            GameObject keyboard = GameObject.Instantiate(FusionContentLoader.KeyboardPrefab);
            keyboard.SetActive(false);
            keyboard.transform.parent = BoneLib.Player.leftHand.transform;
            keyboard.SetActive(true);


            keyboard.transform.Find("Enter").gameObject.GetComponent<TextMeshPro>();
        }

        public static void EnterText(string text)
        {

        }
    }
}
