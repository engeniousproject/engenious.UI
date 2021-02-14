using System;
using engenious.Graphics;

namespace engenious.UI
{
    /// <summary>
    /// A <see cref="Brush"/> that repeats or stretches given textures along the x-axis and y-axis respectively and independently.
    /// While rendering a central texture with given parameters independent from the others as well.
    /// </summary>
    public sealed class NineTileBrush : Brush
    {
        /// <summary>
        /// Gets or sets the <see cref="Texture2D"/> of the center.
        /// </summary>
        public Texture2D CenterTexture { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Texture2D"/> of the left edge.
        /// </summary>
        public Texture2D LeftTexture { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Texture2D"/> of the right edge.
        /// </summary>
        public Texture2D RightTexture { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Texture2D"/> of the top edge.
        /// </summary>
        public Texture2D TopTexture { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Texture2D"/> of the bottom edge.
        /// </summary>
        public Texture2D BottomTexture { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Texture2D"/> of the upper left corner.
        /// </summary>
        public Texture2D UpperLeftTexture { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Texture2D"/> of the upper right corner.
        /// </summary>
        public Texture2D UpperRightTexture { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Texture2D"/> of the lower left corner.
        /// </summary>
        public Texture2D LowerLeftTexture { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Texture2D"/> of the lower right corner.
        /// </summary>
        public Texture2D LowerRightTexture { get; set; }

        /// <summary>
        /// Gets or sets the color to colorize the brush with.
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NineTileBrush"/> class.
        /// </summary>
        public NineTileBrush(Texture2D centerTexture, Texture2D leftTexture, Texture2D rightTexture, Texture2D topTexture, Texture2D bottomTexture, Texture2D upperLeftTexture, Texture2D upperRightTexture, Texture2D lowerLeftTexture, Texture2D lowerRightTexture)
        {
            CenterTexture = centerTexture;
            LeftTexture = leftTexture;
            RightTexture = rightTexture;
            TopTexture = topTexture;
            BottomTexture = bottomTexture;
            UpperLeftTexture = upperLeftTexture;
            UpperRightTexture = upperRightTexture;
            LowerLeftTexture = lowerLeftTexture;
            LowerRightTexture = lowerRightTexture;
            Color = Color.White;
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch batch, Rectangle area, float alpha)
        {
            Color color = Color * alpha;

            // Center
            batch.Draw(CenterTexture,
                new Rectangle(area.X + UpperLeftTexture.Width, area.Y + UpperLeftTexture.Height, area.Width - UpperLeftTexture.Width - LowerRightTexture.Width, area.Height - UpperLeftTexture.Height - LowerRightTexture.Height),
                new Rectangle(0, 0, area.Width - UpperLeftTexture.Width - LowerRightTexture.Width, area.Height - UpperLeftTexture.Height - LowerRightTexture.Height), color);

            // Borders
            batch.Draw(TopTexture,
                new Rectangle(area.X + UpperLeftTexture.Width, area.Y, area.Width - UpperLeftTexture.Width - UpperRightTexture.Width, TopTexture.Height),
                new Rectangle(0, 0, area.Width - UpperLeftTexture.Width - UpperRightTexture.Width, TopTexture.Height), color);

            batch.Draw(BottomTexture,
                new Rectangle(area.X + UpperLeftTexture.Width, area.Y + area.Height - BottomTexture.Height, area.Width - LowerLeftTexture.Width - LowerRightTexture.Width, BottomTexture.Height),
                new Rectangle(0, 0, area.Width - LowerLeftTexture.Width - LowerRightTexture.Width, BottomTexture.Height), color);

            batch.Draw(LeftTexture,
                new Rectangle(area.X, area.Y + UpperLeftTexture.Height, LeftTexture.Width, area.Height - UpperLeftTexture.Height - LowerLeftTexture.Height),
                new Rectangle(0, 0, LeftTexture.Width, area.Height - UpperLeftTexture.Height - LowerLeftTexture.Height), color);

            batch.Draw(RightTexture,
                new Rectangle(area.X + area.Width - RightTexture.Width, area.Y + UpperRightTexture.Height, RightTexture.Width, area.Height - UpperRightTexture.Height - LowerRightTexture.Height),
                new Rectangle(0, 0, RightTexture.Width, area.Height - UpperRightTexture.Height - LowerRightTexture.Height), color);

            // Corners
            batch.Draw(UpperLeftTexture, new Rectangle(area.X, area.Y, UpperLeftTexture.Width, UpperLeftTexture.Height), color);
            batch.Draw(UpperRightTexture, new Rectangle(area.X + area.Width - UpperRightTexture.Width, area.Y, UpperRightTexture.Width, UpperRightTexture.Height), color);
            batch.Draw(LowerLeftTexture, new Rectangle(area.X, area.Y + area.Height - LowerLeftTexture.Height, LowerLeftTexture.Width, LowerLeftTexture.Height), color);
            batch.Draw(LowerRightTexture, new Rectangle(area.X + area.Width - LowerRightTexture.Width, area.Y + area.Height - LowerRightTexture.Height, LowerRightTexture.Width, LowerRightTexture.Height), color);
        }

        /// <summary>
        /// Creates a <see cref="NineTileBrush"/> by cutting a given <see cref="Texture2D"/>.
        /// </summary>
        /// <param name="texture">The <see cref="Texture2D"/> to extract the <see cref="NineTileBrush"/> from.</param>
        /// <param name="cutX">The spacing from the border to cut at the x-axis.</param>
        /// <param name="cutY">The spacing from the border to cut at the y-axis.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Is thrown when <paramref name="texture"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// Is thrown when either <paramref name="cutX"/> is smaller than zero,
        /// bigger than half the width of <paramref name="texture"/>, <paramref name="cutY"/> is smaller than zero,
        /// or bigger than half the height of <paramref name="texture"/>.
        /// </exception>
        public static NineTileBrush FromSingleTexture(Texture2D texture, int cutX, int cutY)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));

            if (cutX <= 0)
                throw new ArgumentException("cutX is too small");

            if (cutY <= 0)
                throw new ArgumentException("cutY is too small");

            if (2 * cutX >= texture.Width)
                throw new ArgumentException("cutX is too large.");

            if (2 * cutY >= texture.Height)
                throw new ArgumentException("cutY is too large.");


            #region Corners

            // Corner-Buffer
            uint[] buffer1 = new uint[cutX * cutY];

            // Upper Left
            var upperLeftTexture = new Texture2D(texture.GraphicsDevice, cutX, cutY);
            texture.GetData(0, new Rectangle(0, 0, cutX, cutY), buffer1, 0, cutX * cutY);
            upperLeftTexture.SetData(buffer1);

            // Upper Right
            var upperRightTexture = new Texture2D(texture.GraphicsDevice, cutX, cutY);
            texture.GetData(0, new Rectangle(texture.Width - cutX, 0, cutX, cutY), buffer1, 0, cutX * cutY);
            upperRightTexture.SetData(buffer1);

            // Lower Left
            var lowerLeftTexture = new Texture2D(texture.GraphicsDevice, cutX, cutY);
            texture.GetData(0, new Rectangle(0, texture.Height - cutY, cutX, cutY), buffer1, 0, cutX * cutY);
            lowerLeftTexture.SetData(buffer1);

            // Lower Right
            var lowerRightTexture = new Texture2D(texture.GraphicsDevice, cutX, cutY);
            texture.GetData(0, new Rectangle(texture.Width - cutX, texture.Height - cutY, cutX, cutY), buffer1, 0, cutX * cutY);
            lowerRightTexture.SetData(buffer1);

            #endregion

            #region Horizontal Edges

            int buffer2SizeX = (texture.Width - cutX - cutX);
            int buffer2SizeY = cutY;
            uint[] buffer2 = new uint[buffer2SizeX * buffer2SizeY];

            // Upper Border
            var topTexture = new Texture2D(texture.GraphicsDevice, buffer2SizeX, buffer2SizeY);
            texture.GetData(0, new Rectangle(cutX, 0, buffer2SizeX, buffer2SizeY), buffer2, 0, buffer2SizeX * buffer2SizeY);
            topTexture.SetData(buffer2);

            // Lower Border
            var bottomTexture = new Texture2D(texture.GraphicsDevice, buffer2SizeX, buffer2SizeY);
            texture.GetData(0, new Rectangle(cutX, texture.Height - cutY, buffer2SizeX, buffer2SizeY), buffer2, 0, buffer2SizeX * buffer2SizeY);
            bottomTexture.SetData(buffer2);

            #endregion

            #region Vertical Edges

            int buffer3SizeX = cutX;
            int buffer3SizeY = (texture.Height - cutY - cutY);
            uint[] buffer3 = new uint[buffer3SizeX * buffer3SizeY];

            // Left Border
            var leftTexture = new Texture2D(texture.GraphicsDevice, buffer3SizeX, buffer3SizeY);
            texture.GetData(0, new Rectangle(0, cutY, buffer3SizeX, buffer3SizeY), buffer3, 0, buffer3SizeX * buffer3SizeY);
            leftTexture.SetData(buffer3);

            // Right Border
            var rightTexture = new Texture2D(texture.GraphicsDevice, buffer3SizeX, buffer3SizeY);
            texture.GetData(0, new Rectangle(texture.Width - cutX, cutY, buffer3SizeX, buffer3SizeY), buffer3, 0, buffer3SizeX * buffer3SizeY);
            rightTexture.SetData(buffer3);

            #endregion

            #region Central

            int buffer4SizeX = (texture.Width - cutX - cutX);
            int buffer4SizeY = (texture.Height - cutY - cutY);
            uint[] buffer4 = new uint[buffer4SizeX * buffer4SizeY];

            // Left Border
            var centerTexture = new Texture2D(texture.GraphicsDevice, buffer4SizeX, buffer4SizeY);
            texture.GetData(0, new Rectangle(cutX, cutY, buffer4SizeX, buffer4SizeY), buffer4, 0, buffer4SizeX * buffer4SizeY);
            centerTexture.SetData(buffer4);

            #endregion


            NineTileBrush brush = new NineTileBrush(centerTexture, leftTexture, rightTexture, topTexture, bottomTexture,
                upperLeftTexture, upperRightTexture, lowerLeftTexture, lowerRightTexture);
            brush.MinWidth = (cutX * 2) + 1;
            brush.MinHeight = (cutY * 2) + 1;

            return brush;
        }
    }
}
