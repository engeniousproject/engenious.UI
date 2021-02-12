using System;

namespace engenious.UI
{
    /// <summary>
    /// Struct zur Verwaltung von Rahmen-Informationen für Margins und Paddings.
    /// </summary>
    public struct Border : IEquatable<Border>
    {
        /// <summary>
        /// The border to the left.
        /// </summary>
        public int Left;

        /// <summary>
        /// The border to the top.
        /// </summary>
        public int Top;
        
        /// <summary>
        /// The border to the right.
        /// </summary>
        public int Right;

        /// <summary>
        /// The border to the bottom.
        /// </summary>
        public int Bottom;

        /// <summary>
        /// Initializes a new instance of the <see cref="Border"/> struct.
        /// </summary>
        /// <param name="left">The border to the left.</param>
        /// <param name="top">The border to the top.</param>
        /// <param name="right">The border to the right.</param>
        /// <param name="bottom">The border to the bottom.</param>
        public Border(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        /// <summary>
        /// Creates a <see cref="Border"/> struct with the same distance to all sides.
        /// </summary>
        /// <param name="value">Value for all for sides</param>
        /// <returns>The created <see cref="Border"/> struct.</returns>
        public static Border All(int value)
        {
            return new Border()
            {
                Bottom = value,
                Left = value,
                Right = value,
                Top = value
            };
        }
        
        /// <summary>
        /// Creates a <see cref="Border"/> struct with the same distance to the opposing sides.
        /// </summary>
        /// <param name="horizontal">Distance value for top and bottom.</param>
        /// <param name="vertical">Distance value for left and right.</param>
        /// <returns>The created <see cref="Border"/> struct.</returns>
        public static Border All(int horizontal, int vertical)
        {
            return new Border()
            {
                Bottom = vertical,
                Left = horizontal,
                Right = horizontal,
                Top = vertical
            };
        }


        
        /// <summary>
        /// Creates a <see cref="Border"/> struct with the same distance to the opposing sides.
        /// </summary>
        /// <param name="left">Distance to the left.</param>
        /// <param name="top">Distance to the top.</param>
        /// <param name="right">Distance to the right.</param>
        /// <param name="bottom">Distance to the bottom.</param>
        /// <returns>The created <see cref="Border"/> struct.</returns>
        public static Border All(int left, int top, int right, int bottom)
        {
            return new Border()
            {
                Bottom = bottom,
                Left = left,
                Right = right,
                Top = top
            };
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Left}/{Top}/{Right}/{Bottom}";
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is Border b && Equals(b);
        }

        /// <inheritdoc />
        public bool Equals(Border other)
        {
            return other.Left == Left &&
                other.Right == Right &&
                other.Top == Top &&
                other.Bottom == Bottom;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Left + Right + Top + Bottom;
        }
    }
}
