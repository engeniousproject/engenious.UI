using engenious.Graphics;

namespace engenious.UI
{
    /// <summary>
    /// Base class for rendering brushes.
    /// </summary>
    public abstract class Brush
    {
        /// <summary>
        /// Gets the minimum width for the <see cref="Brush"/> to be rendered correctly.
        /// </summary>
        public int MinWidth { get; protected set; }

        /// <summary>
        /// Gets the minimum height for the <see cref="Brush"/> to be rendered correctly.
        /// </summary>
        public int MinHeight { get; protected set; }

        /// <summary>
        /// Renders the brush into a <see cref="SpriteBatch"/> in a specified area using an alpha value.
        /// </summary>
        /// <param name="batch">
        /// The spritebatch to draw to.
        /// <remarks>
        /// The spritebatch needs to be in drawing mode. Meaning <see cref="SpriteBatch.Begin"/> was called but
        /// <see cref="SpriteBatch.End"/> was not yet called (again) after the <see cref="SpriteBatch.Begin"/>.
        /// </remarks>
        /// </param>
        /// <param name="area">The area to render the brush to.</param>
        /// <param name="alpha">The transparency value to render the brush width.</param>
        public abstract void Draw(SpriteBatch batch, Rectangle area, float alpha);
    }
}
