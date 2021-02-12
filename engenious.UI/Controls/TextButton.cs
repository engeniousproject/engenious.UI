﻿using engenious.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public Label Label => (Label)Content;

        /// <summary>
        /// Gets or sets the font used for rendering the button text.
        /// </summary>
        public SpriteFont Font
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
        /// <param name="manager">The <see cref="BaseScreenComponent"/>.</param>
        /// <param name="text">The text of this button.</param>
        /// <param name="style">The style to use for this control.</param>
        public TextButton(BaseScreenComponent manager, string text, string style = ""): base(manager, style)
        {
            Content = new Label(manager) { Text = text };

            ApplySkin(typeof(TextButton));
        }
    }
}
