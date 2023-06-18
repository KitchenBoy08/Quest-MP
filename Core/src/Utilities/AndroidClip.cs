using Cysharp.Threading.Tasks.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Xamarin.Essentials;

namespace LabFusion.Core.src.Utilities
{
    public static class AndroidClip
    {
        public static async Task<string> GetClippedItem()
        {
            string text = await Clipboard.GetTextAsync();
            return text;
        }

        public static async void SetClippedItem(string item)
        {
            await Clipboard.SetTextAsync(item);
        }
    }
}
