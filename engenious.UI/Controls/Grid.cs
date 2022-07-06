using System;
using System.Collections.Generic;

namespace engenious.UI.Controls
{
    /// <summary>
    /// A ui control for organizing controls in a grid.
    /// </summary>
    public class Grid : Control
    {
        private readonly List<CellMapping> _cellMapping = new();

        /// <summary>
        /// Gets a list of <see cref="ColumnDefinition"/> for this <see cref="Grid"/>.
        /// </summary>
        public List<ColumnDefinition> Columns { get; }

        /// <summary>
        /// Gets a list of <see cref="RowDefinition"/> for this <see cref="Grid"/>.
        /// </summary>
        public List<RowDefinition> Rows { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Grid"/> class.
        /// </summary>
        /// <param name="style">The style to use for this control.</param>
        /// <param name="manager">The <see cref="BaseScreenComponent"/>.</param>
        public Grid(BaseScreenComponent? manager = null, string style = "")
            : base(manager, style)
        {
            Columns = new List<ColumnDefinition>();
            Rows = new List<RowDefinition>();
            ApplySkin(typeof(Grid));
        }

        /// <summary>
        /// Adds a <see cref="Control"/> into a specific row and column.
        /// </summary>
        /// <param name="control">The <see cref="Control"/> to add.</param>
        /// <param name="column">The column to place the control in.</param>
        /// <param name="row">The row to place the control in.</param>
        /// <param name="columnSpan">The number of columns the control should span over.</param>
        /// <param name="rowSpan">The number of rows the control should span over.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="control"/> was null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="row"/>, <paramref name="rowSpan"/>,
        /// <paramref name="column"/> or <paramref name="columnSpan"/> where out of range.
        /// </exception>
        public void AddControl(Control control, int column, int row, int columnSpan = 1, int rowSpan = 1)
        {
            if (control == null)
                throw new ArgumentNullException(nameof(control));

            if (column < 0 || column > Columns.Count - 1)
                throw new ArgumentOutOfRangeException(nameof(column));

            if (row < 0 || row > Rows.Count - 1)
                throw new ArgumentOutOfRangeException(nameof(row));

            if (columnSpan < 1)
                throw new ArgumentOutOfRangeException(nameof(columnSpan));

            if (rowSpan < 1)
                throw new ArgumentOutOfRangeException(nameof(rowSpan));

            var mapping = new CellMapping(control);
            for (int x = 0; x < columnSpan; x++)
            {
                int colIndex = column + x;
                if (colIndex < 0) continue;
                if (colIndex >= Columns.Count) continue;
                mapping.Columns.Add(Columns[colIndex]);
            }

            for (int y = 0; y < rowSpan; y++)
            {
                int rowIndex = row + y;
                if (rowIndex < 0) continue;
                if (rowIndex >= Rows.Count) continue;
                mapping.Rows.Add(Rows[rowIndex]);
            }

            _cellMapping.Add(mapping);
            Children.Add(control);
        }

        /// <inheritdoc />
        public override Point GetExpectedSize(Point available)
        {
            Point result = GetMinClientSize(available);
            Point client = GetMaxClientSize(available);

            // rest of the controls
            foreach (var mapping in _cellMapping)
            {
                Point cell = new Point();

                foreach (var column in mapping.Columns)
                {
                    if (column.ResizeMode == ResizeMode.Fixed)
                        cell.X += column.Width;
                    else
                    {
                        cell.X = client.X;
                        break;
                    }
                }
                foreach (var row in mapping.Rows)
                {
                    if (row.ResizeMode == ResizeMode.Fixed)
                        cell.Y += row.Height;
                    else
                    {
                        cell.Y = client.Y;
                        break;
                    }
                }
                mapping.ExpectedSize = mapping.Control.GetExpectedSize(cell);
            }

            #region Columns

            int totalWidth = 0, partsX = 0;

            // determine automatic columns.
            foreach (var column in Columns)
            {
                switch (column.ResizeMode)
                {
                    case ResizeMode.Parts:
                        // determine parts columns
                        partsX += column.Width;
                        break;

                    case ResizeMode.Fixed:
                        // determine static columns
                        column.ExpectedWidth = column.Width;
                        totalWidth += column.ExpectedWidth;
                        break;

                    case ResizeMode.Auto:
                        int width = column.MinWidth ?? 0;
                        foreach (var mapping in _cellMapping)
                        {
                            if (!mapping.Columns.Contains(column))
                                continue;

                            int mapWidth = 0, autoCount = 0;
                            foreach (var c in mapping.Columns)
                            {
                                if (c.ResizeMode == ResizeMode.Auto)
                                    autoCount++;
                                else if (c.ResizeMode == ResizeMode.Fixed)
                                    mapWidth += c.ExpectedWidth;
                            }

                            width = Math.Max(width, (mapping.ExpectedSize.X - mapWidth) / autoCount);
                        }

                        if (column.MaxWidth.HasValue)
                            width = Math.Min(width, column.MaxWidth.Value);

                        column.ExpectedWidth = width;
                        totalWidth += column.ExpectedWidth;
                        break;
                }
            }

            if (partsX > 0)
            {
                int partX = (client.X - totalWidth) / partsX;
                foreach (var column in Columns)
                {
                    if (column.ResizeMode != ResizeMode.Parts)
                        continue;
                    column.ExpectedWidth = partX * column.Width;
                    totalWidth += column.ExpectedWidth;
                }
            }

            #endregion

            #region Rows

            int totalHeight = 0, partsY = 0;

            foreach (var row in Rows)
            {
                switch (row.ResizeMode)
                {
                    case ResizeMode.Parts:
                        // determine parts rows
                        partsY += row.Height;
                        break;

                    case ResizeMode.Fixed:
                        // determine static rows
                        row.ExpectedHeight = row.Height;
                        totalHeight += row.ExpectedHeight;
                        break;

                    case ResizeMode.Auto:
                        // determine automatic rows
                        int height = row.MinHeight ?? 0;
                        foreach (var mapping in _cellMapping)
                        {
                            if (!mapping.Rows.Contains(row))
                                continue;

                            int mapHeight = 0, autoCount = 0;

                            foreach (var r in mapping.Rows)
                            {
                                if (r.ResizeMode == ResizeMode.Auto)
                                    autoCount++;
                                else if (r.ResizeMode == ResizeMode.Fixed)
                                    mapHeight += r.ExpectedHeight;
                            }

                            height = Math.Max(height, (mapping.ExpectedSize.Y - mapHeight) / autoCount);
                        }

                        if (row.MaxHeight.HasValue)
                            height = Math.Min(height, row.MaxHeight.Value);
                        row.ExpectedHeight = height;
                        totalHeight += row.ExpectedHeight;
                        break;
                }
            }

            if (partsY > 0)
            {
                int partY = (client.Y - totalHeight) / partsY;
                foreach (var row in Rows)
                {
                    if (row.ResizeMode != ResizeMode.Parts)
                        continue;
                    row.ExpectedHeight = partY * row.Height;
                    totalHeight += row.ExpectedHeight;
                }
            }

            #endregion

            result = new Point(Math.Max(result.X, totalWidth), Math.Max(result.Y, totalHeight));

            return result + Borders;
        }

        /// <inheritdoc />
        public override void SetActualSize(Point available)
        {
            Point minSize = GetExpectedSize(available);
            SetDimension(minSize, available);

            #region Reorg Cols & Rows

            int offsetX = 0;
            foreach (var column in Columns)
            {
                column.ActualOffset = offsetX;
                column.ActualWidth = column.ExpectedWidth;
                offsetX += column.ActualWidth;
            }

            int offsetY = 0;
            foreach (var row in Rows)
            {
                row.ActualOffset = offsetY;
                row.ActualHeight = row.ExpectedHeight;
                offsetY += row.ActualHeight;
            }

            #endregion

            #region Set Controls

            foreach (var mapping in _cellMapping)
            {
                Point cellOffset = new Point(mapping.Columns[0].ActualOffset, mapping.Rows[0].ActualOffset);

                Point cellSize = new Point();

                foreach (var c in mapping.Columns)
                    cellSize.X += c.ActualWidth;

                foreach (var c in mapping.Rows)
                    cellSize.Y += c.ActualHeight;

                mapping.Control.SetActualSize(cellSize);
                mapping.Control.ActualPosition += cellOffset;
            }

            #endregion
        }

        /// <inheritdoc />
        protected override void SetDimension(Point actualSize, Point containerSize)
        {
            var size = new Point(
                Math.Min(containerSize.X, HorizontalAlignment == HorizontalAlignment.Stretch ? containerSize.X : actualSize.X),
                Math.Min(containerSize.Y, VerticalAlignment == VerticalAlignment.Stretch ? containerSize.Y : actualSize.Y));

            Point minSize = GetMinClientSize(containerSize) + Borders;
            Point maxSize = GetMaxClientSize(containerSize) + Borders;

            size.X = Math.Max(minSize.X, Math.Min(maxSize.X, size.X));
            size.Y = Math.Max(minSize.Y, Math.Min(maxSize.Y, size.Y));

            ActualSize = size;

            var remainingSize = containerSize - actualSize;

            // Anteilige Spalten ermitteln
            int partsX = 0;
            foreach (var c in Columns)
            {
                if (c.ResizeMode == ResizeMode.FitParts)
                    partsX += c.Width;
            }

            if (partsX > 0)
            {
                int partX = remainingSize.X / partsX;
                foreach (var column in Columns)
                {
                    if (column.ResizeMode != ResizeMode.FitParts)
                        continue;
                    column.ExpectedWidth = partX * column.Width;
                    size.X += column.ExpectedWidth;
                }
            }

            // Anteilige Spalten ermitteln
            int partsY = 0;
            foreach (var c in Rows)
            {
                if (c.ResizeMode == ResizeMode.FitParts)
                    partsY += c.Height;
            }
            if (partsY > 0)
            {
                int partY = remainingSize.Y / partsY;
                foreach (var row in Rows)
                {
                    if (row.ResizeMode != ResizeMode.FitParts)
                        continue;
                    row.ExpectedHeight = partY * row.Height;
                    size.Y += row.ExpectedHeight;
                }
            }

            base.SetDimension(size, containerSize);
        }


        /// <summary>
        /// Internal class used for containing cell mappings.
        /// </summary>
        private class CellMapping
        {
            /// <summary>
            /// Gets the <see cref="Control"/> which gets mapped.
            /// </summary>
            public Control Control { get; }

            /// <summary>
            /// Gets a list <see cref="ColumnDefinition"/> of the columns which are relevant to this mapping.
            /// </summary>
            public List<ColumnDefinition> Columns { get; }

            /// <summary>
            /// Gets a list <see cref="RowDefinition"/> of the rows which are relevant to this mapping.
            /// </summary>
            public List<RowDefinition> Rows { get; }

            /// <summary>
            /// Gets or sets the expected <see cref="Control"/> size.
            /// </summary>
            public Point ExpectedSize { get; set; }

            /// <summary>
            /// Initializes a new instance o the <see cref="CellMapping"/> class.
            /// </summary>
            /// <param name="control">The <see cref="Control"/> to map.</param>
            public CellMapping(Control control)
            {
                Columns = new List<ColumnDefinition>();
                Rows = new List<RowDefinition>();
                Control = control;
            }
        }
    }

    /// <summary>
    /// A class describing the requirements for a column.
    /// </summary>
    public class ColumnDefinition
    {
        /// <summary>
        /// Gets or sets the <see cref="Controls.ResizeMode"/> used for this <see cref="ColumnDefinition"/>.
        /// </summary>
        public ResizeMode ResizeMode { get; set; }

        /// <summary>
        /// Gets or sets the minimal width required for this <see cref="ColumnDefinition"/>.
        /// </summary>
        public int? MinWidth { get; set; }

        /// <summary>
        /// Gets or sets the width for this <see cref="ColumnDefinition"/>.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the maximum allowed width for this <see cref="ColumnDefinition"/>.
        /// </summary>
        public int? MaxWidth { get; set; }

        /// <summary>
        /// Gets or sets the actual offset for this <see cref="ColumnDefinition"/>.
        /// </summary>
        public int ActualOffset { get; set; }

        /// <summary>
        /// Gets or sets the actual width for this <see cref="ColumnDefinition"/>.
        /// </summary>
        public int ActualWidth { get; set; }

        /// <summary>
        /// Gets or sets the expected width for this <see cref="ColumnDefinition"/>.
        /// </summary>
        public int ExpectedWidth { get; set; }
    }

    /// <summary>
    /// A class describing the requirements for a row.
    /// </summary>
    public class RowDefinition
    {
        /// <summary>
        /// Gets or sets the <see cref="Controls.ResizeMode"/> used for this <see cref="RowDefinition"/>.
        /// </summary>
        public ResizeMode ResizeMode { get; set; }

        /// <summary>
        /// Gets or sets the minimal height required for this <see cref="RowDefinition"/>.
        /// </summary>
        public int? MinHeight { get; set; }

        /// <summary>
        /// Gets or sets the height for this <see cref="RowDefinition"/>.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Gets or sets the maximum allowed height for this <see cref="RowDefinition"/>.
        /// </summary>
        public int? MaxHeight { get; set; }

        /// <summary>
        /// Gets or sets the actual offset for this <see cref="RowDefinition"/>.
        /// </summary>
        public int ActualOffset { get; set; }

        /// <summary>
        /// Gets or sets the actual height for this <see cref="RowDefinition"/>.
        /// </summary>
        public int ActualHeight { get; set; }

        /// <summary>
        /// Gets or sets the expected height for this <see cref="RowDefinition"/>.
        /// </summary>
        public int ExpectedHeight { get; set; }
    }

    /// <summary>
    /// Specifies the available resize modes for rows and columns of a <see cref="Grid"/>.
    /// </summary>
    public enum ResizeMode
    {
        /// <summary>
        /// Specifies the size to be fixed.
        /// </summary>
        Fixed,
        /// <summary>
        /// Specifies the size to be determined automatically.
        /// </summary>
        Auto,
        /// <summary>
        /// Specifies the size to be relative to other parts.
        /// </summary>
        Parts,
        /// <summary>
        /// Specifies the size to fit its contents but be influenced by other parts.
        /// </summary>
        FitParts
    }
}
