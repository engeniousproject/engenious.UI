using System;

namespace engenious.UI
{
    public sealed class SystemSpecific
    {
        private static readonly TextCopy.Clipboard Clipboard = new TextCopy.Clipboard();
        public static void ClearClipboard() => Clipboard.SetText(string.Empty);

        public static void SetClipboardText(string text) => Clipboard.SetText(text);

        public static string GetClipboardText() => Clipboard.GetText();
    }
}
