using Cysharp.Threading.Tasks.Triggers;
using net.sf.jni4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace LabFusion.Core.src.Utilities
{
    public static class AndroidClip
    {
        public static void PasteClipboard(string clipboard)
        {
            //Start JVM
            Bridge.CreateJVM(new BridgeSetup());

            //

            //Paste Data
            ClipData pData = clipboardManager.getPrimaryClip();
            ClipData.Item item = pData.getItemAt(0);
            String txtpaste = item.getText().toString();

        }
    }
}
