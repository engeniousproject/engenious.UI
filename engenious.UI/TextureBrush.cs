using System;
using engenious.Graphics;

namespace engenious.UI
{
    /// <summary>
    /// A brush using a texture.
    /// </summary>
    public sealed class TextureBrush : Brush
    {
        /// <summary>
        /// Gets or sets the <see cref="Texture2D"/> for this brush.
        /// </summary>
        public Texture2D Texture { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="TextureBrushMode"/> for this brush.
        /// </summary>
        public TextureBrushMode Mode { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextureBrush"/> class.
        /// </summary>
        /// <param name="texture">The texture to use.</param>
        /// <param name="mode">The brush mode to use.</param>
        public TextureBrush(Texture2D texture, TextureBrushMode mode)
        {
            Texture = texture;
            Mode = mode;
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch batch, Rectangle area, float alpha)
        {
            switch (Mode)
            {
                case TextureBrushMode.Stretch:
                    batch.Draw(Texture, area, Color.White * alpha);
                    break;
                case TextureBrushMode.Tile:
                {
                    int countX = area.X / Texture.Width;
                    int countY = area.Y / Texture.Height;

                    for (int x = 0; x <= countX; x++)
                    {
                        for (int y = 0; y <= countY; y++)
                        {
                            Rectangle destination = new Rectangle(area.X + x * Texture.Width, area.Y + y * Texture.Height, Texture.Width, Texture.Height);
                            batch.Draw(Texture, destination, Color.White * alpha);
                        }
                    }

                    break;
                }
                default:
                    throw new NotImplementedException();
            }
        }
    }

    /// <summary>
    /// Specifies the available brush modes for a <see cref="TextureBrush"/>.
    /// </summary>
    public enum TextureBrushMode
    {
        /// <summary>
        /// Stretch the texture.
        /// </summary>
        Stretch,
        /// <summary>
        /// Fits the texture so that the texture aspect ratio does not get changed and the whole texture is visible.
        /// </summary>
        FitMin,
        /// <summary>
        /// Fits the texture so that whole region is filled by cutting of the texture on the other axis on each side half respectively.
        /// </summary>
        FitMax,
        /// <summary>
        /// Tile the texture.
        /// </summary>
        Tile
    }
}
