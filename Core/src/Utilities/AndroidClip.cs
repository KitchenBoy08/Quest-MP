using System;
using Android.Content;
using Android.Widget;

public class AndroidClip
{
    private readonly Context _context;

    public AndroidClip(Context context)
    {
        _context = context;
    }

    public string GetClipboardText()
    {
        var clipboardManager = (ClipboardManager)_context.GetSystemService(Context.ClipboardService);
        var clipData = clipboardManager.PrimaryClip;

        if (clipData != null && clipData.ItemCount > 0)
        {
            var clipItem = clipData.GetItemAt(0);
            return clipItem.Text;
        }

        return string.Empty;
    }

    public void CopyToClipboard(string text)
    {
        var clipboardManager = (ClipboardManager)_context.GetSystemService(Context.ClipboardService);
        var clipData = ClipData.NewPlainText("Text", text);
        clipboardManager.PrimaryClip = clipData;
    }
}