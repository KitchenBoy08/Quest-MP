using BoneLib.BoneMenu.Elements;
using LabFusion.Network;
using LabFusion.Preferences;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LabFusion.BoneMenu;

namespace LabFusion.Core.src.BoneMenu
{
    public class KeyboardCreator
    {
        MenuCategory keyboardCategory;
        MenuCategory digitsMenu;
        MenuCategory specialsMenu;
        MenuCategory letterMenu;
        MenuElement stringReference;
        MenuElement stringReference1;
        MenuElement stringReference2;
        MenuElement stringReference3;

        bool isCapital = true;
        string outValue = "";
        IFusionPref<string> preference;

        public static string[] lowCaseLetters = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };
        public static string[] upCaseLetters = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
        public static int[] digits = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        public static string[] specials = { "/", "-", "_", "=", ".", ",", ":", "!", "@", "#", "$", "%", "^", "&", "*", "(", ")", "<", ">" };
        public void CreateKeyboard(MenuCategory cat, string keyboardName, IFusionPref<string> pref)
        {
            preference = pref;
            keyboardCategory = cat.CreateCategory(keyboardName, Color.cyan);
            CreateButtons();
        }

        private void CreateButtons()
        {
            stringReference = keyboardCategory.CreateFunctionElement($"Current Value:" + System.Environment.NewLine + outValue, Color.green, null);
            var setValue = keyboardCategory.CreateFunctionElement("Enter", Color.yellow, () => SetValue(outValue, preference));
            var resetButton = keyboardCategory.CreateFunctionElement("Reset Keyboard", Color.red, () => ClearValue(preference));
            digitsMenu = keyboardCategory.CreateCategory("Digits", Color.white);
            specialsMenu = keyboardCategory.CreateCategory("Specials", Color.white);
            letterMenu = keyboardCategory.CreateCategory("Letters", Color.white);

            CreateDigitsMenu();
            CreateSpecialsMenu();
            CreateLetterMenu();
        }

        public void CreateDigitsMenu()
        {
            stringReference1 = digitsMenu.CreateFunctionElement($"Current Value:" + System.Environment.NewLine + outValue, Color.green, null);

            digitsMenu.Elements.Clear();
            foreach (int number in digits)
            {
                digitsMenu.CreateFunctionElement(number.ToString(), Color.white, () => OnUpdateOutValue(number.ToString(), preference));
            }
        }

        public void CreateSpecialsMenu()
        {
            stringReference2 = specialsMenu.CreateFunctionElement($"Current Value:" + System.Environment.NewLine + outValue, Color.green, null);

            specialsMenu.Elements.Clear();
            foreach (string cha in specials)
            {
                specialsMenu.CreateFunctionElement(cha, Color.white, () => OnUpdateOutValue(cha, preference));
            }
        }

        public void CreateLetterMenu()
        {
            stringReference3 = letterMenu.CreateFunctionElement($"Current Value:" + System.Environment.NewLine + outValue, Color.green, null);

            letterMenu.Elements.Clear();
            var capLock = letterMenu.CreateBoolElement("Caps Lock", Color.blue, isCapital, OnClickCapsLock);
            if (isCapital)
            {
                foreach (string letter in upCaseLetters)
                {
                    letterMenu.CreateFunctionElement(letter, Color.white, () => OnUpdateOutValue(letter, preference));
                }
            }
            else
            {
                foreach (string letter in lowCaseLetters)
                {
                    letterMenu.CreateFunctionElement(letter, Color.white, () => OnUpdateOutValue(letter, preference));
                }
            }
        }

        private void SetValue(string value, IFusionPref<string> pref)
        {
            pref.SetValue(value);
            // Ill probably do proper hooking later once I feel like it, for now I'll just call a method to update text :P
            RiptideNetworkLayer.OnSetValue();
        }

        private void ClearValue(IFusionPref<string> pref)
        {
            outValue = "";

            stringReference.SetName($"Current Value:{System.Environment.NewLine} {outValue}");
            stringReference1.SetName($"Current Value:{System.Environment.NewLine} {outValue}");
            stringReference2.SetName($"Current Value:{System.Environment.NewLine} {outValue}");
            stringReference3.SetName($"Current Value:{System.Environment.NewLine} {outValue}");
        }

        private void OnUpdateOutValue(string value, IFusionPref<string> pref)
        {
            var sb = new StringBuilder(outValue);
            sb.Append(value);
            string finalValue = sb.ToString();
            outValue = finalValue;

            stringReference.SetName($"Current Value:{System.Environment.NewLine} {outValue}");
            stringReference1.SetName($"Current Value:{System.Environment.NewLine} {outValue}");
            stringReference2.SetName($"Current Value:{System.Environment.NewLine} {outValue}");
            stringReference3.SetName($"Current Value:{System.Environment.NewLine} {outValue}");
        }

        private void OnClickCapsLock(bool obj)
        {
            if (isCapital)
            {
                isCapital = false;
            }
            else
            {
                isCapital = true;
            }

            CreateLetterMenu();
        }
    }
}
