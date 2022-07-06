using engenious.Graphics;

namespace engenious.UI.Controls
{
    /// <summary>
    /// Ui control text button.
    /// </summary>
    public class TextButton : Button
    {
        /// <summary>
        /// Gets the label containing the button text.
        /// </summary>
        public Label Label => (Label)Content!;

        /// <summary>
        /// Gets or sets the font used for rendering the button text.
        /// </summary>
        public SpriteFont? Font
        {
            get => Label.Font;
            set => Label.Font = value;
        }

        /// <summary>
        /// Gets or sets the color used for rendering the button text.
        /// </summary>
        public Color TextColor
        {
            get => Label.TextColor;
            set => Label.TextColor = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="HorizontalAlignment"/> of the text.
        /// </summary>
        public HorizontalAlignment HorizontalTextAlignment
        {
            get => Label.HorizontalTextAlignment;
            set => Label.HorizontalTextAlignment = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="VerticalAlignment"/> of the text.
        /// </summary>
        public VerticalAlignment VerticalTextAlignment
        {
            get => Label.VerticalTextAlignment;
            set => Label.VerticalTextAlignment = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the text should be wrapped ond word boundaries if necessary.
        /// </summary>
        public bool WordWrap
        {
            get => Label.WordWrap;
            set => Label.WordWrap = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextButton"/> class.
        /// </summary>
        /// <param name="text">The text of this button.</param>
        /// <param name="style">The style to use for this control.</param>
        /// <param name="manager">The <see cref="BaseScreenComponent"/>.</param>
        public TextButton(string text, BaseScreenComponent? manager = null, string style = "") : base(manager, style)
        {
            Content = new Label(manager: manager)
            {
                Text = text,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalTextAlignment = HorizontalAlignment.Center,
                VerticalTextAlignment = VerticalAlignment.Center
            };

            ApplySkin(typeof(TextButton));
        }
    }
}
