using System;
using System.Collections.Generic;
using System.Linq;

namespace engenious.UI.Controls
{
    public class Grid : Control
    {
        private List<CellMapping> cellMapping = new List<CellMapping>();

        public List<ColumnDefinition> Columns { get; private set; }

        public List<RowDefinition> Rows { get; private set; }

        public Grid(BaseScreenComponent manager, string style = "") :
            base(manager, style)
        {
            Columns = new List<ColumnDefinition>();
            Rows = new List<RowDefinition>();
            ApplySkin(typeof(Grid));
        }

        public void AddControl(Control control, int column, int row, int columnSpan = 1, int rowSpan = 1)
        {
            if (control == null)
                throw new ArgumentNullException("control");

            if (column < 0 || column > Columns.Count - 1)
                throw new ArgumentOutOfRangeException("column");

            if (row < 0 || row > Rows.Count - 1)
                throw new ArgumentOutOfRangeException("row");

            if (columnSpan < 1)
                throw new ArgumentOutOfRangeException("columnSpan");

            if (rowSpan < 1)
                throw new ArgumentOutOfRangeException("rowSpan");

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

            cellMapping.Add(mapping);
            Children.Add(control);
        }

        public override Point GetExpectedSize(Point available)
        {
            Point result = GetMinClientSize(available);
            Point client = GetMaxClientSize(available);

            // Restliche Controls
            foreach (var mapping in cellMapping)
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

            // Statische Spalten ermitteln
            int totalWidth = 0;
            foreach (var column in Columns)
            {
                if (column.ResizeMode != ResizeMode.Fixed)
                    continue;
                column.ExpectedWidth = column.Width;
                totalWidth += column.ExpectedWidth;
            }

            // Automatische Spalten ermitteln
            foreach (var column in Columns)
            {
                if (column.ResizeMode != ResizeMode.Auto)
                    continue;

                int width = column.MinWidth.HasValue ? column.MinWidth.Value : 0;
                foreach (var mapping in cellMapping)
                {
                    if (!mapping.Columns.Contains(column))
                        continue;
                    int mapWidth = 0;

                    foreach (var c in mapping.Columns)
                    {
                        if (c.ResizeMode != ResizeMode.Fixed)
                            continue;
                        mapWidth += c.ExpectedWidth;
                    }

                    int autoCount = 0;
                    foreach (var c in mapping.Columns)
                    {
                        if (c.ResizeMode != ResizeMode.Fixed)
                            continue;
                        autoCount++;
                    }
                    width = Math.Max(width, (mapping.ExpectedSize.X - mapWidth) / autoCount);
                }
                if (column.MaxWidth.HasValue)
                    width = Math.Min(width, column.MaxWidth.Value);
                column.ExpectedWidth = width;
                totalWidth += column.ExpectedWidth;
            }

            // Anteilige Spalten ermitteln
            int partsX = 0;

            foreach (var column in Columns)
                if (column.ResizeMode == ResizeMode.Parts)
                    partsX += column.Width;

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

            // Statische Zeilen ermitteln
            int totalHeight = 0;
            foreach (var row in Rows)
            {
                if (row.ResizeMode != ResizeMode.Fixed)
                    continue;
                row.ExpectedHeight = row.Height;
                totalHeight += row.ExpectedHeight;
            }

            // Automatische Spalten ermitteln
            foreach (var row in Rows)
            {
                if (row.ResizeMode != ResizeMode.Auto)
                    continue;
                int height = row.MinHeight ?? 0;
                foreach (var mapping in cellMapping)
                {
                    if (!mapping.Rows.Contains(row))
                        continue;

                    int mapHeight = 0;
                    int autoCount = 0;

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
            }

            // Anteilige Spalten ermitteln

            int partsY = 0;
            foreach (var c in Rows)
            {
                if (c.ResizeMode == ResizeMode.Parts)
                    partsY += c.Height;
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

            foreach (var mapping in cellMapping)
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
        /// Interne Klasse zur Haltung der Cell-Mappings
        /// </summary>
        private class CellMapping
        {
            /// <summary>
            /// Referenz auf das entsprechende Control
            /// </summary>
            public Control Control { get; private set; }

            /// <summary>
            /// Auflistung der betroffenen Columns
            /// </summary>
            public List<ColumnDefinition> Columns { get; private set; }

            /// <summary>
            /// Auflistung der betroffenen Rows
            /// </summary>
            public List<RowDefinition> Rows { get; private set; }

            /// <summary>
            /// Erwartete Control-Größe
            /// </summary>
            public Point ExpectedSize { get; set; }

            public CellMapping(Control control)
            {
                Columns = new List<ColumnDefinition>();
                Rows = new List<RowDefinition>();
                Control = control;
            }
        }
    }

    public class ColumnDefinition
    {
        public ResizeMode ResizeMode { get; set; }

        public int? MinWidth { get; set; }

        public int Width { get; set; }

        public int? MaxWidth { get; set; }

        public int ActualOffset { get; set; }

        public int ActualWidth { get; set; }

        public int ExpectedWidth { get; set; }
    }

    public class RowDefinition
    {
        public ResizeMode ResizeMode { get; set; }

        public int? MinHeight { get; set; }

        public int Height { get; set; }

        public int? MaxHeight { get; set; }

        public int ActualOffset { get; set; }

        public int ActualHeight { get; set; }

        public int ExpectedHeight { get; set; }
    }

    public enum ResizeMode
    {
        Fixed,
        Auto,
        Parts,
        FitParts
    }
}
