using engenious.Graphics;

namespace engenious.UI.Controls
{
    /// <summary>
    /// Common interface for text controls.
    /// </summary>
    public interface ITextControl : IControl
    {
        /// <summary>
        /// Gets or sets the text for this control.
        /// </summary>
        string Text { get; set; }

        /// <summary>
        /// Gets or sets the font used to render the <see cref="Text"/>.
        /// </summary>
        SpriteFont? Font { get; set; }

        /// <summary>
        /// Gets or sets the color used to render the <see cref="Text"/>.
        /// </summary>
        Color TextColor { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="HorizontalAlignment"/> to render the <see cref="Text"/> with.
        /// </summary>
        HorizontalAlignment HorizontalTextAlignment { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="VerticalAlignment"/> to render the <see cref="Text"/> with.
        /// </summary>
        VerticalAlignment VerticalTextAlignment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="Text"/> should be wrapped at word boundaries
        /// if it doesn't fit otherwise.
        /// </summary>
        bool WordWrap { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="Text"/> should be wrapped at new line escape characters.
        /// </summary>
        bool LineWrap { get; set; }
    }
}
