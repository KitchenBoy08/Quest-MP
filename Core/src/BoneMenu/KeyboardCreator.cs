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
using BoneLib;
using System.Windows.Forms;

namespace LabFusion.Core.src.BoneMenu
{
    public class KeyboardCreator
    {
        MenuCategory keyboardCategory;
        MenuCategory digitsMenu;
        MenuCategory specialsMenu;
        MenuCategory letterMenu;
        MenuElement stringReference;

        bool isCapital = true;
        string outValue = "";
        IFusionPref<string> preference;

        public static string[] lowCaseLetters = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };
        public static string[] upCaseLetters = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
        public static int[] digits = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        public static string[] specials = { "/", @"\", "+", "[", "]", "{", "}", "`", "~", "-", "_", "=", ".", ",", ":", "!", "@", "#", "$", "%", "^", "&", "*", "(", ")", "<", ">" };
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

            if (!HelperMethods.IsAndroid())
            {
                keyboardCategory.CreateFunctionElement("Paste", Color.white, OnPasteServerIP);
            }

            var resetButton = keyboardCategory.CreateFunctionElement("Reset Keyboard", Color.red, () => ClearValue());
            keyboardCategory.CreateFunctionElement("Backspace", Color.white, () => BackOutValue());
            digitsMenu = keyboardCategory.CreateCategory("Digits", Color.white);
            specialsMenu = keyboardCategory.CreateCategory("Specials", Color.white);
            letterMenu = keyboardCategory.CreateCategory("Letters", Color.white);

            CreateDigitsMenu();
            CreateSpecialsMenu();
            CreateLetterMenu();
        }

        private void OnPasteServerIP()
        {
            if (Clipboard.ContainsText())
            {
                outValue = Clipboard.GetText();

                stringReference.SetName($"Current Value:" + System.Environment.NewLine + outValue);
            }
        }

        public void CreateDigitsMenu()
        {
            digitsMenu.Elements.Clear();
            foreach (int number in digits)
            {
                digitsMenu.CreateFunctionElement(number.ToString(), Color.white, () => AppendOutValue(number.ToString()));
            }
        }

        public void CreateSpecialsMenu()
        {
            specialsMenu.Elements.Clear();
            foreach (string cha in specials)
            {
                specialsMenu.CreateFunctionElement(cha, Color.white, () => AppendOutValue(cha));
            }
        }

        public void CreateLetterMenu()
        {
            letterMenu.Elements.Clear();
            var capLock = letterMenu.CreateBoolElement("Caps Lock", Color.blue, isCapital, OnClickCapsLock);
            if (isCapital)
            {
                foreach (string letter in upCaseLetters)
                {
                    letterMenu.CreateFunctionElement(letter, Color.white, () => AppendOutValue(letter));
                }
            }
            else
            {
                foreach (string letter in lowCaseLetters)
                {
                    letterMenu.CreateFunctionElement(letter, Color.white, () => AppendOutValue(letter));
                }
            }
        }

        private void SetValue(string value, IFusionPref<string> pref)
        {
            pref.SetValue(value);

            RiptideNetworkLayer.UpdatePreferenceValues();
        }

        private void ClearValue()
        {
            outValue = "";

            stringReference.SetName($"Current Value:{System.Environment.NewLine} {outValue}");
        }

        private void AppendOutValue(string value)
        {
            var sb = new StringBuilder(outValue);
            sb.Append(value);
            string finalValue = sb.ToString();
            outValue = finalValue;

            stringReference.SetName($"Current Value:{System.Environment.NewLine} {outValue}");
        }

        private void BackOutValue()
        {
            var sb = new StringBuilder(outValue);

            int length = sb.Length;
            sb.Remove(length - 1, 1);

            string finalValue = sb.ToString();
            outValue = finalValue;

            stringReference.SetName($"Current Value:{System.Environment.NewLine} {outValue}");
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
