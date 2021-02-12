using System;

namespace engenious.UI
{
    /// <summary>
    /// Helper class for operating system specific code.
    /// </summary>
    public sealed class SystemSpecific
    {
        private static readonly TextCopy.Clipboard Clipboard = new();
        /// <summary>
        /// Clears the clipboard contents.
        /// </summary>
        public static void ClearClipboard() => Clipboard.SetText(string.Empty);

        /// <summary>
        /// Set the clipboard text.
        /// </summary>
        /// <param name="text">The <see cref="string"/> to set the clipboard value to.</param>
        public static void SetClipboardText(string text) => Clipboard.SetText(text);

        /// <summary>
        /// Gets the current clipboard text.
        /// </summary>
        /// <returns>The current clipboard text.</returns>
        public static string GetClipboardText() => Clipboard.GetText();
    }
}
