using System;

namespace engenious.UI.Controls
{
    /// <summary>
    /// Ui container control for arranging controls into a line oriented horizontally or vertically.
    /// </summary>
    public class StackPanel : ContainerControl
    {
        private Orientation _orientation = Orientation.Vertical;

        private int _controlSpacing = 0;
        /// <summary>
        /// Gets or sets a value for the spacing between controls.
        /// </summary>
        public int ControlSpacing
        {
            get => _controlSpacing;
            set
            {
                if (_controlSpacing == value)
                    return;
                _controlSpacing = value;
                InvalidateDimensions();
            }
        }

        private readonly PropertyEventArgs<Orientation> _orientationChangedEventArgs = new PropertyEventArgs<Orientation>();
        /// <summary>
        /// Gets or sets the <see cref="UI.Orientation"/> for the control stack.
        /// </summary>
        public Orientation Orientation
        {
            get => _orientation;
            set
            {
                if (_orientation == value) return;

                _orientationChangedEventArgs.OldValue = _orientation;
                _orientationChangedEventArgs.NewValue = value;
                _orientationChangedEventArgs.Handled = false;

                _orientation = value;
                InvalidateDimensions();

                OnOrientationChanged(_orientationChangedEventArgs);
                OrientationChanged?.Invoke(this, _orientationChangedEventArgs);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StackPanel"/> class.
        /// </summary>
        /// <param name="style">The style to use for this control.</param>
        /// <param name="manager">The <see cref="BaseScreenComponent"/>.</param>
        public StackPanel(BaseScreenComponent? manager = null, string style = "") : base(manager, style)
        {
            ApplySkin(typeof(StackPanel));
        }

        /// <inheritdoc />
        public override Point GetExpectedSize(Point available)
        {
            Point client = GetMaxClientSize(available);
            Point result = GetMinClientSize(available);

            foreach (var control in Controls)
            {
                Point expected = control.GetExpectedSize(client);
                if (Orientation == Orientation.Horizontal)
                {
                    result.X += expected.X + _controlSpacing;
                    result.Y = Math.Max(result.Y, expected.Y);
                }
                else if (Orientation == Orientation.Vertical)
                {
                    result.Y += expected.Y + _controlSpacing;
                    result.X = Math.Max(result.X, expected.X);
                }
            }

            return result + Borders;
        }

        /// <inheritdoc />
        public override void SetActualSize(Point available)
        {
            Point minSize = GetExpectedSize(available);
            SetDimension(minSize, available);

            // Placement
            Point result = Point.Zero;
            foreach (var control in Controls)
            {
                control.SetActualSize(ActualClientSize);
                if (Orientation == Orientation.Horizontal)
                {
                    control.ActualPosition = new Point(result.X, control.ActualPosition.Y);
                    result.X += control.ActualSize.X + _controlSpacing;
                    result.Y = Math.Max(result.Y, control.ActualSize.Y);
                }
                else if (Orientation == Orientation.Vertical)
                {
                    control.ActualPosition = new Point(control.ActualPosition.X, result.Y);
                    result.Y += control.ActualSize.Y + _controlSpacing;
                    result.X = Math.Max(result.X, control.ActualSize.X);
                }
            }
        }
        
        /// <summary>
        /// Raises the <see cref="OrientationChanged"/> event.
        /// </summary>
        /// <param name="args">A <see cref="PropertyEventArgs{Orientation}"/> that contains the event data.</param>
        protected virtual void OnOrientationChanged(PropertyEventArgs<Orientation> args) { }

        /// <summary>
        /// Occurs when the <see cref="Orientation"/> property was changed.
        /// </summary>
        public event PropertyChangedDelegate<Orientation>? OrientationChanged;
    }
}
