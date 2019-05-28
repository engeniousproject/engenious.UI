using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace engenious.UI.Controls
{
    /// <summary>
    /// Standard-Schaltfläche mit Text.
    /// </summary>
    public class TextButton : Button
    {
        /// <summary>
        /// Initialisiert einen Standard-Button mit Text-Inhalt.
        /// </summary>
        /// <param name="manager">Der <see cref="BaseScreenComponent"/>.</param>
        /// <param name="text">Text, der auf der Schaltfläche angezeigt werden soll.</param>
        /// <param name="style">(Optional) Der zu verwendende Style.</param>
        public TextButton(BaseScreenComponent manager, string text, string style = "")
            : base(manager, style)
        {
            Content = new Label(manager) { Text = text };

            ApplySkin(typeof(Button));
        }
    }
}
