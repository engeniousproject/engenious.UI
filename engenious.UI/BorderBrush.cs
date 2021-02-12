using engenious.Graphics;

namespace engenious.UI
{
    /// <summary>
    /// A <see cref="Brush"/> for drawing borders.
    /// </summary>
    public class BorderBrush : Brush
    {
        private Texture2D _tex;

        private int _lineWidth;

        private Color _lineColor;

        private LineType _lineType;

        /// <summary>
        /// Gets or sets the background color of the brush.
        /// </summary>
        public Color BackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the line width of the border in px.
        /// </summary>
        public int LineWidth
        {
            get => _lineWidth;
            set
            {
                if (_lineWidth != value)
                {
                    _lineWidth = value;
                    RebuildTexture();
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Color"/> for the border line.
        /// </summary>
        public Color LineColor
        {
            get => _lineColor;
            set
            {
                if (_lineColor != value)
                {
                    _lineColor = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="LineType"/> for this border.
        /// </summary>
        public LineType LineType
        {
            get => _lineType;
            set
            {
                if (_lineType != value)
                {
                    _lineType = value;
                    RebuildTexture();
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BorderBrush"/> class.
        /// </summary>
        /// <param name="backgroundColor">The background color for the brush.</param>
        public BorderBrush(Color backgroundColor) :
            this(backgroundColor, LineType.None, Color.Transparent) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BorderBrush"/> class.
        /// </summary>
        /// <param name="lineType">The <see cref="UI.LineType"/> of the border line.</param>
        /// <param name="lineColor">The <see cref="Color"/> of the border line.</param>
        /// <param name="lineWidth">The line width of the border line in px. <remarks>Defaults to 1px</remarks></param>
        public BorderBrush(LineType lineType, Color lineColor, int lineWidth = 1)
            : this(Color.Transparent, lineType, lineColor, lineWidth) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BorderBrush"/> class.
        /// </summary>
        /// <param name="backgroundColor">The background color for the brush.</param>
        /// <param name="lineType">The <see cref="UI.LineType"/> of the border line.</param>
        /// <param name="lineColor">The <see cref="Color"/> of the border line.</param>
        /// <param name="lineWidth">The line width of the border line in px. <remarks>Defaults to 1px</remarks></param>
        public BorderBrush(Color backgroundColor, LineType lineType, Color lineColor, int lineWidth = 1)
        {
            BackgroundColor = backgroundColor;
            _lineType = lineType;
            _lineColor = lineColor;
            _lineWidth = lineWidth;
            RebuildTexture();
        }

        private void RebuildTexture()
        {
            // Dispose old texture
            if (_tex != null)
            {
                _tex.Dispose();
                _tex = null;
            }

            Color[] buffer;
            switch (LineType)
            {
                case LineType.None:
                    break;
                case LineType.Solid:
                    buffer = new Color[LineWidth * LineWidth];
                    for (int i = 0; i < buffer.Length; i++)
                        buffer[i] = Color.White;
                    _tex = new Texture2D(Skin.Pix.GraphicsDevice, LineWidth, LineWidth);
                    _tex.SetData(buffer);
                    MinWidth = MinHeight = (LineWidth * 2) + 1;
                    break;

                case LineType.Dotted:
                    buffer = new Color[LineWidth * LineWidth * 4];
                    for (int y = 0; y < LineWidth * 2; y++)
                    {
                        for (int x = 0; x < LineWidth * 2; x++)
                        {
                            int index = (y * LineWidth * 2) + x;
                            buffer[index] = (x < LineWidth && y < LineWidth ? Color.White : Color.Transparent);
                        }
                    }
                    _tex = new Texture2D(Skin.Pix.GraphicsDevice, LineWidth * 2, LineWidth * 2);
                    _tex.SetData(buffer);
                    MinWidth = MinHeight = (LineWidth * 2) + 1;
                    break;
            }
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch batch, Rectangle area, float alpha)
        {
            batch.Draw(Skin.Pix, area, BackgroundColor * alpha);

            // Draw border line
            if (_tex != null)
            {
                batch.Draw(_tex, 
                    new Rectangle(area.X, area.Y, area.Width, LineWidth), 
                    new Rectangle(0, 0, area.Width, LineWidth), LineColor * alpha);
                batch.Draw(_tex, 
                    new Rectangle(area.X, area.Y, LineWidth, area.Height), 
                    new Rectangle(0, 0, LineWidth, area.Height), LineColor * alpha);
                batch.Draw(_tex, 
                    new Rectangle(area.X, area.Y + area.Height - LineWidth, area.Width, LineWidth), 
                    new Rectangle(0, 0, area.Width, LineWidth), LineColor * alpha);
                batch.Draw(_tex, 
                    new Rectangle(area.X + area.Width - LineWidth, area.Y, LineWidth, area.Height),
                    new Rectangle(0, 0, LineWidth, area.Height), LineColor * alpha);
            }
        }
    }

    /// <summary>
    /// Specifies the possible types for drawing lines.
    /// </summary>
    public enum LineType
    {
        /// <summary>
        /// Do not draw any line.
        /// </summary>
        None,

        /// <summary>
        /// Draw one solid line
        /// </summary>
        Solid,

        /// <summary>
        /// Draw a dotted line
        /// </summary>
        Dotted,
    }
}
