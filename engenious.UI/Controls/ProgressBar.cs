using System;
using engenious.Graphics;

namespace engenious.UI.Controls
{
    /// <summary>
    /// Ui control for displaying visual progress using a bar. 
    /// </summary>
    public class ProgressBar : Control
    {
        private Orientation _orientation = Orientation.Horizontal;

        private int _barValue = 0;

        private int _maximum = 100;

        private Brush? _barBrush;

        private readonly PropertyEventArgs<Orientation> _orientationChangedEventArgs = new PropertyEventArgs<Orientation>();
        
        /// <summary>
        /// Gets or sets the <see cref="UI.Orientation"/> for this <see cref="ProgressBar"/>.
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

        private readonly PropertyEventArgs<int> _valueChangedEventArgs = new PropertyEventArgs<int>();
        /// <summary>
        /// Gets or sets the current value of the progress.
        /// </summary>
        public int Value
        {
            get => _barValue;
            set
            {
                if (_barValue == value) return;
                
                
                _valueChangedEventArgs.OldValue = _barValue;
                _valueChangedEventArgs.NewValue = value;
                _valueChangedEventArgs.Handled = false;

                _barValue = value;
                InvalidateDrawing();

                OnValueChanged(_valueChangedEventArgs);
                ValueChanged?.Invoke(this, _valueChangedEventArgs);
            }
        }
        private readonly PropertyEventArgs<int> _maximumChangedEventArgs = new PropertyEventArgs<int>();
        /// <summary>
        /// Gets or sets the maximum value of the progress.
        /// </summary>
        public int Maximum
        {
            get => _maximum;
            set
            {
                if (_maximum == value) return;

                _maximumChangedEventArgs.OldValue = _maximum;
                _maximumChangedEventArgs.NewValue = value;
                _maximumChangedEventArgs.Handled = false;

                _maximum = value;
                InvalidateDrawing();

                OnMaximumChanged(_maximumChangedEventArgs);
                MaximumChanged?.Invoke(this, _maximumChangedEventArgs);
            }
        }
        private readonly PropertyEventArgs<Brush> _barBrushChangedEventArgs = new PropertyEventArgs<Brush>();
        /// <summary>
        /// Gets or sets the brush used to draw the progress bar.
        /// </summary>
        public Brush? BarBrush
        {
            get => _barBrush;
            set
            {
                if (_barBrush == value) return;

                _barBrushChangedEventArgs.OldValue = _barBrush;
                _barBrushChangedEventArgs.NewValue = value;
                _barBrushChangedEventArgs.Handled = false;

                _barBrush = value;
                InvalidateDrawing();

                OnBarBrushChanged(_barBrushChangedEventArgs);
                BarBrushChanged?.Invoke(this, _barBrushChangedEventArgs);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressBar"/> class.
        /// </summary>
        /// <param name="style">The style to use for this control.</param>
        /// <param name="manager">The <see cref="BaseScreenComponent"/>.</param>
        public ProgressBar(string style = "", BaseScreenComponent? manager= null)
            : base(style, manager)
        {
            ApplySkin(typeof(ProgressBar));
        }

        /// <inheritdoc />
        protected override void OnDrawContent(SpriteBatch batch, Rectangle contentArea, GameTime gameTime, float alpha)
        {
            if (BarBrush == null) return;

            int m = Math.Max(0, Maximum);
            int v = Math.Max(0, Math.Min(m, Value));
            float part = m > 0 ? (float)v / m : 1f;

            if (Orientation == Orientation.Horizontal)
                BarBrush.Draw(batch, new Rectangle(contentArea.X, contentArea.Y, 
                    (int)(contentArea.Width * part), contentArea.Height), alpha);
            else if (Orientation == Orientation.Vertical)
            {
                BarBrush.Draw(batch, new Rectangle(contentArea.X, contentArea.Y,
                    contentArea.Width, (int)(contentArea.Height * part)), alpha);
            }
        }
        
        /// <summary>
        /// Raises the <see cref="OrientationChanged"/> event.
        /// </summary>
        /// <param name="args">A <see cref="PropertyEventArgs{Orientation}"/> that contains the event data.</param>
        protected virtual void OnOrientationChanged(PropertyEventArgs<Orientation> args) { }

        /// <summary>
        /// Raises the <see cref="ValueChanged"/> event.
        /// </summary>
        /// <param name="args">A <see cref="PropertyEventArgs{Int32}"/> that contains the event data.</param>
        protected virtual void OnValueChanged(PropertyEventArgs<int> args) { }

        /// <summary>
        /// Raises the <see cref="MaximumChanged"/> event.
        /// </summary>
        /// <param name="args">A <see cref="PropertyEventArgs{Int32}"/> that contains the event data.</param>
        protected virtual void OnMaximumChanged(PropertyEventArgs<int> args) { }

        /// <summary>
        /// Raises the <see cref="BarBrushChanged"/> event.
        /// </summary>
        /// <param name="args">A <see cref="PropertyEventArgs{Brush}"/> that contains the event data.</param>
        protected virtual void OnBarBrushChanged(PropertyEventArgs<Brush> args) { }

        /// <summary>
        /// Occurs when the <see cref="Orientation"/> was changed.
        /// </summary>
        public event PropertyChangedDelegate<Orientation>? OrientationChanged;

        /// <summary>
        /// Occurs when the <see cref="Value"/> was changed.
        /// </summary>
        public event PropertyChangedDelegate<int>? ValueChanged;

        /// <summary>
        /// Occurs when the <see cref="Maximum"/> value was changed.
        /// </summary>
        public event PropertyChangedDelegate<int>? MaximumChanged;

        /// <summary>
        /// Occurs when the <see cref="BarBrush"/> was changed.
        /// </summary>
        public event PropertyChangedDelegate<Brush>? BarBrushChanged;
    }
}
