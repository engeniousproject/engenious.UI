using engenious.Graphics;

namespace engenious.UI.Controls
{
    /// <summary>
    /// Ui control to display an image.
    /// </summary>
    public class Image : Control
    {
        // TODO: Scaling parameter (e.g. stretch, scale, fill,...)

        /// <summary>
        /// Gets or sets the image to display as a <see cref="Texture2D"/>.
        /// </summary>
        public Texture2D? Texture { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Image"/> class.
        /// </summary>
        /// <param name="manager">The <see cref="BaseScreenComponent"/>.</param>
        /// <param name="style">The style to use for this control.</param>
        public Image(BaseScreenComponent manager, string style = "")
            : base(manager, style)
        {
            ApplySkin(typeof(Image));
        }

        /// <inheritdoc />
        public override Point CalculateRequiredClientSpace(Point available)
        {
            if (Texture != null)
                return new Point(Texture.Width, Texture.Height);
            return base.CalculateRequiredClientSpace(available);
        }

        /// <inheritdoc />
        protected override void OnDrawContent(SpriteBatch batch, Rectangle area, GameTime gameTime, float alpha)
        {
            if (Texture != null)
                batch.Draw(Texture, area, Color.White * alpha);
        }
    }
}
