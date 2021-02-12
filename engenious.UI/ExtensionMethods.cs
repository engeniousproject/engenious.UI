using System;

namespace engenious.UI
{
    /// <summary>
    /// Extension methods for <see cref="Rectangle"/> class.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Extracts the intersection <see cref="Rectangle"/> from two rectangles intersecting each other.
        /// </summary>
        /// <param name="r1">The first rectangle to intersect.</param>
        /// <param name="r2">The second rectangle to intersect.</param>
        /// <returns>The intersection <see cref="Rectangle"/>.</returns>
        public static Rectangle Intersection(this Rectangle r1, Rectangle r2)
        {
            // Normalize
            Point r11 = new Point(Math.Min(r1.Left, r1.Right), Math.Min(r1.Top, r1.Bottom));
            Point r12 = new Point(Math.Max(r1.Left, r1.Right), Math.Max(r1.Top, r1.Bottom));
            Point r21 = new Point(Math.Min(r2.Left, r2.Right), Math.Min(r2.Top, r2.Bottom));
            Point r22 = new Point(Math.Max(r2.Left, r2.Right), Math.Max(r2.Top, r2.Bottom));

            // Find intersection
            Point r31 = new Point(Math.Max(r11.X, r21.X), Math.Max(r11.Y, r21.Y));
            Point r32 = new Point(Math.Min(r12.X, r22.X), Math.Min(r12.Y, r22.Y));
            Point dimensions = new Point(Math.Max(0, r32.X - r31.X), Math.Max(0, r32.Y - r31.Y));

            return new Rectangle(r31, dimensions);
        }

        /// <summary>
        /// Transforms a <see cref="Rectangle"/> by a <see cref="Matrix"/> and creates the AA bounding rectangle.
        /// </summary>
        /// <param name="rectangle">The <see cref="Rectangle"/> to transform.</param>
        /// <param name="transform">The transformation <see cref="Matrix"/>.</param>
        /// <returns>The transformed AA bounding <see cref="Rectangle"/>.</returns>
        public static unsafe Rectangle Transform(this Rectangle rectangle, Matrix transform)
        {
            // get points

            var p = stackalloc Vector2[4];

            p[0] = new Vector2(rectangle.X, rectangle.Y);
            p[1] = new Vector2(rectangle.X + rectangle.Width,rectangle.Y);
            p[2] = new Vector2(rectangle.X, rectangle.Y + rectangle.Height);
            p[3] = new Vector2(rectangle.X + rectangle.Width,rectangle.Y + rectangle.Height);
            
            // Transform
            Vector2.Transform(2, ref transform, p, p);
            float minX=p[0].X, minY = p[0].Y, maxX=p[0].X, maxY=p[0].Y;
            for (int i = 1; i < 4; i++)
            {
                var corner = p[i];
                if (corner.X > maxX)
                    maxX = corner.X;
                else if (corner.X < minX)
                    minX = corner.X;
                
                if (corner.Y > maxY)
                    maxY = corner.Y;
                else if (corner.Y < minY)
                    minY = corner.Y;
            }

            // Create new AABB rectangle
            return Rectangle.FromLTRB((int)minX, (int)minY, (int)maxX, (int)maxY);
        }
    }
}
