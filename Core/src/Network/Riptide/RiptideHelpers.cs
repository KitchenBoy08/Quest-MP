using BoneLib.BoneMenu.Elements;
using LabFusion.Network;
using LabFusion.Preferences;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Il2CppSystem.Linq.Expressions.Interpreter.CastInstruction.CastInstructionNoT;

namespace LabFusion.Core.src.Network.Riptide
{
    public class RiptideHelpers
    {
        public static MenuCategory keyboardCategory;
        private static bool isCapital = true;
        public static string outValue = "";
        public static IFusionPref<string> preference;
        public static void CreateKeyboard(MenuCategory cat, IFusionPref<string> pref)
        {
            preference = pref;
            keyboardCategory = cat.CreateCategory("Keyboard \n (VERY JANKY)", Color.magenta);
            CreateButtons();
        }

        private static void CreateButtons()
        {
            string[] lowCaseLetters = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };
            string[] upCaseLetters = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
            int[] digits = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            var stringReference = keyboardCategory.CreateSubPanel($"Current Value:" + System.Environment.NewLine + outValue, Color.green);
            var setValue = keyboardCategory.CreateFunctionElement("Set Value", Color.yellow, () => SetValue(outValue, preference));
            var resetButton = keyboardCategory.CreateFunctionElement("Reset Value", Color.red, () => ClearValue(preference));
            var capLock = keyboardCategory.CreateBoolElement("Caps Lock", Color.blue, isCapital, OnClickCapsLock);
            var digitsMenu = keyboardCategory.CreateCategory("Digits", Color.white);

            var stringReferenceNum = digitsMenu.CreateSubPanel($"Current Value:" + System.Environment.NewLine + outValue, Color.green);
            var setValueNum = digitsMenu.CreateFunctionElement("Set Value", Color.yellow, () => SetValue(outValue, preference));
            var resetButtonNum = digitsMenu.CreateFunctionElement("Reset Value", Color.red, () => ClearValue(preference));

            foreach (int number in digits)
            {
                digitsMenu.CreateFunctionElement(number.ToString(), Color.white, () => OnUpdateOutValue(number.ToString(), preference));
            }

            keyboardCategory.CreateFunctionElement("/", Color.white, () => OnUpdateOutValue("/", preference));
            keyboardCategory.CreateFunctionElement("-", Color.white, () => OnUpdateOutValue("-", preference));
            keyboardCategory.CreateFunctionElement("_", Color.white, () => OnUpdateOutValue("_", preference));
            keyboardCategory.CreateFunctionElement("=", Color.white, () => OnUpdateOutValue("=", preference));
            keyboardCategory.CreateFunctionElement(".", Color.white, () => OnUpdateOutValue(".", preference));

            if (isCapital)
            {
                foreach (string letter in upCaseLetters)
                {
                    keyboardCategory.CreateFunctionElement(letter, Color.white, () => OnUpdateOutValue(letter, preference));
                }
            }
            else
            {
                foreach (string letter in lowCaseLetters)
                {
                    keyboardCategory.CreateFunctionElement(letter, Color.white, () => OnUpdateOutValue(letter, preference));
                }
            }
        }

        private static void SetValue(string value, IFusionPref<string> pref)
        {
            pref.SetValue(value);
            // Ill probably do proper hooking later once I feel like it, for now I'll just call a method to update text :P
            RiptideNetworkLayer.OnSetCode(value);
        }

        private static void ClearValue(IFusionPref<string> pref)
        {
            outValue = "";

            keyboardCategory.Elements.Clear();
            CreateButtons();
        }

        private static void OnUpdateOutValue(string value, IFusionPref<string> pref)
        {
            var sb = new StringBuilder(outValue);
            sb.Append(value);
            string finalValue = sb.ToString();
            outValue = finalValue;

            keyboardCategory.Elements.Clear();
            CreateButtons();
        }

        private static void OnClickCapsLock(bool obj)
        {
            if (isCapital)
            {
                isCapital = false;
            } else
            {
                isCapital = true;
            }

            keyboardCategory.Elements.Clear();
            CreateButtons();
        }

        private static byte[][] BufferPool;
        private static int BufferPoolIndex;
        public static byte[] TakeBuffer(int minSize)
        {
            if (BufferPool == null)
            {
                //
                // The pool has 8 items.
                //
                BufferPool = new byte[8][];

                for (int i = 0; i < BufferPool.Length; i++)
                    BufferPool[i] = new byte[1024 * 128];
            }

            BufferPoolIndex++;
            if (BufferPoolIndex >= BufferPool.Length)
                BufferPoolIndex = 0;

            if (BufferPool[BufferPoolIndex].Length < minSize)
            {
                BufferPool[BufferPoolIndex] = new byte[minSize + 1024];
            }

            return BufferPool[BufferPoolIndex];
        }
        public static unsafe int DecompressVoice(MemoryStream input, int length, out MemoryStream output)
        {
            {
                output = new MemoryStream();

                var from = TakeBuffer(length);
                var to = TakeBuffer(1024 * 64);

                //
                // Copy from input stream to a pinnable buffer
                //
                using (var s = new System.IO.MemoryStream(from))
                {
                    input.CopyTo(s);
                }

                uint szWritten = 0;

                output.Write(to, 0, (int)szWritten);
                return (int)output.Length;
            }
        }
    }
}
