using engenious.Graphics;
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
        public Label Label => (Label)Content;

        /// <summary>
        /// Gibt die Schriftart an mit der der Inhalt gezeichnet werden soll oder legt diese fest.
        /// </summary>
        public SpriteFont Font
        {
            get => Label.Font;
            set => Label.Font = value;
        }

        /// <summary>
        /// Gibt die Textfarbe an mit der der Inhalt gezeichnet werden soll oder legt diese fest.
        /// </summary>
        public Color TextColor
        {
            get => Label.TextColor;
            set => Label.TextColor = value;
        }

        /// <summary>
        /// Gibt die Ausrichtung des Textes innerhalb des Controls auf horizontaler Ebene an.
        /// </summary>
        public HorizontalAlignment HorizontalTextAlignment
        {
            get => Label.HorizontalTextAlignment;
            set => Label.HorizontalTextAlignment = value;
        }

        /// <summary>
        /// Gibt die Ausrichtung des Textes innerhalb des Controls auf vertikaler Ebene an.
        /// </summary>
        public VerticalAlignment VerticalTextAlignment
        {
            get => Label.VerticalTextAlignment;
            set => Label.VerticalTextAlignment = value;
        }

        /// <summary>
        /// Gibt an, ob das Control automatisch den Text an geeigneter Stelle
        /// umbrechen soll, falls er nicht in eine Zeile passt.
        /// </summary>
        public bool WordWrap
        {
            get => Label.WordWrap;
            set => Label.WordWrap = value;
        }

        /// <summary>
        /// Initialisiert einen Standard-Button mit Text-Inhalt.
        /// </summary>
        /// <param name="manager">Der <see cref="BaseScreenComponent"/>.</param>
        /// <param name="text">Text, der auf der Schaltfläche angezeigt werden soll.</param>
        /// <param name="style">(Optional) Der zu verwendende Style.</param>
        public TextButton(BaseScreenComponent manager, string text, string style = ""): base(manager, style)
        {
            Content = new Label(manager) { Text = text };

            ApplySkin(typeof(TextButton));
        }
    }
}
