using System;
using engenious.Graphics;
using engenious.Input;

namespace engenious.UI.Controls
{
    /// <summary>
    /// Ui control to select a value in a range with a slider.
    /// </summary>
    public class Slider : Control
    {
        private int _range;
        /// <summary>
        /// Gets or sets the <see cref="UI.Orientation"/> of the slider.
        /// </summary>
        public Orientation Orientation { get; set; }

        /// <summary>
        /// Gets or sets the brush for the slider knob.
        /// </summary>
        public Brush KnobBrush { get; set; }

        /// <summary>
        ///  Gets or sets the maximum value of the slider.
        /// </summary>
        public int Range
        {
            get => _range;
            set
            {
                _range = value;
                Value = Math.Min(_range, Value);
            }
        }

        /// <summary>
        /// Gets or sets the size of the knob.
        /// </summary>
        public int KnobSize { get; set; }
        
        private int _sliderValue;

        /// <summary>
        /// Gets or sets the current value of the slider.
        /// </summary>
        public int Value
        {
            get => _sliderValue;
            set
            {
                _sliderValue = Math.Max(0, Math.Min(Range, value));
                RecalculateKnob();
                ValueChanged?.Invoke(_sliderValue);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the slider direction should be inverted.
        /// </summary>
        public bool Invert { get; set; }

        /// <summary>
        /// Gets or sets the big step increase/decrease used for clicking besides the knob.
        /// </summary>
        public int BigStep { get; set; } = 0;

        /// <summary>
        /// Gets or sets the small step increase/decrease used in scrolling.
        /// </summary>
        public int SmallStep { get; set; } = 1;
        
        private bool _mouseClickActive = false;

        private Rectangle _knob;

        /// <summary>
        /// Occurs when the <see cref="Value"/> property was changed.
        /// </summary>
        public event ValueChangedDelegate? ValueChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="Slider"/> class.
        /// </summary>
        /// <param name="style">The style to use for this control.</param>
        /// <param name="manager">The <see cref="BaseScreenComponent"/>.</param>
        public Slider(string style = "", BaseScreenComponent? manager = null)
            : base(style, manager)
        {
            CanFocus = true;
            TabStop = true;

            Range = 100;
            Value = 0;

            KnobBrush = null!;

            ApplySkin(typeof(Slider));
            
            CheckStyleInitialized(nameof(KnobBrush), KnobBrush);
        }

        /// <inheritdoc />
        protected override void OnDrawContent(SpriteBatch batch, Rectangle contentArea, GameTime gameTime, float alpha)
        {
            RecalculateKnob();

            // Draw background
            Background?.Draw(batch, contentArea, alpha);

            KnobBrush.Draw(batch, new Rectangle(_knob.Location + contentArea.Location, _knob.Size), alpha);
        }

        private void RecalculateKnob()
        {

            Rectangle drawableKnobSpace = new Rectangle(
                KnobSize / 2,
                KnobSize / 2,
                ActualClientArea.Width - KnobSize,
                ActualClientArea.Height - KnobSize);

            int val = Invert ? Range - Value : Value;

            Rectangle sliderKnob = new Rectangle();
            if (Orientation == Orientation.Horizontal)
            {

                // Calculate the position of the knob
                sliderKnob.Y = 0;                                                       // Y coordinate of the knob
                sliderKnob.Width = KnobSize;                                                              // the slider has a width of KnobSize
                float widthRange = ((float)drawableKnobSpace.Width / Range);                        // Calculates how many pixel correspond 1 to one value difference
                sliderKnob.X = (int)Math.Round(drawableKnobSpace.X + (widthRange * val) - KnobSize / 2);    // X coordinate of the blob
                sliderKnob.Height = ActualClientArea.Height;                                             // The knobs height is always the same as the slider
            }
            else
            {
                //Berechne die Position des SliderKnobs     
                sliderKnob.X = 0;                                                                            // Y coordinate of the knob
                sliderKnob.Height = KnobSize;                                                             // the slider has a height of KnobSize
                float heightRange = ((float)drawableKnobSpace.Height / Range);                              // Calculates how many pixel correspond 1 to one value difference
                sliderKnob.Y = (int)Math.Round(drawableKnobSpace.Y + drawableKnobSpace.Height - (heightRange * val) - KnobSize / 2);    // X coordinate of the blob
                sliderKnob.Width = ActualClientArea.Width;                                               // The knobs width is always the same as the slider
            }

            _knob = sliderKnob;
        }

        Point _grabPoint;
        
        /// <inheritdoc />
        protected override void OnLeftMouseDown(MouseEventArgs args)
        {
            

            if(_knob.Contains(args.LocalPosition))
            {
                _grabPoint = args.LocalPosition - (_knob.Location + new Point(_knob.Width / 2, _knob.Height / 2)); 
                _mouseClickActive = true;
            }
            else if(BigStep == 0)
            {
                if (Orientation == Orientation.Horizontal)
                    Value = (args.LocalPosition.X - KnobSize / 2) * Range / (ActualClientArea.Width - KnobSize);
                else
                    Value = Range - (args.LocalPosition.Y - KnobSize / 2) * Range / (ActualClientArea.Height - KnobSize);

                if (Invert)
                    Value = Range - Value;
            }
            else
            {
                int scrollDirection = Orientation ==
                    Orientation.Horizontal && args.LocalPosition.X < _knob.X || Orientation == Orientation.Vertical && args.LocalPosition.Y < _knob.Y
                    ? 1 : -1;
                Value += BigStep * scrollDirection * (Invert ? -1 : 1);
            }

        }

        /// <inheritdoc />
        protected override void OnMouseMove(MouseEventArgs args)
        {
            base.OnMouseMove(args);
            // Calculation of the value when the mouse is hold and the control is selected
            if (_mouseClickActive)
            {

                Point localMousePos = args.LocalPosition;
                if (Orientation == Orientation.Horizontal)
                {
                    // Calculate the value of the slider
                    Value = (args.LocalPosition.X - KnobSize / 2 - _grabPoint.X) * Range / (ActualClientArea.Width - KnobSize);
                    if (localMousePos.X <= 0) Value = 0;                     // When mouse position < 0 -> Value = 0
                    if (localMousePos.X >= ActualClientArea.Width) Value = Range; // When mouse position > width of control -> Value = Range
                }

                if (Orientation == Orientation.Vertical)
                {
                    // Calculate the value of the slider
                    Value = Range - (args.LocalPosition.Y - KnobSize / 2 - _grabPoint.Y) * Range / (ActualClientArea.Height - KnobSize);
                    if (Value <= 0) Value = 0;                      // When mouse position < 0 -> Value = 0
                    if (Value >= Range) Value = Range;              // When mouse position > height of control -> Value = Range
                }
                if (Invert)
                    Value = Range - Value;
            }
        }

        /// <inheritdoc />
        protected override void OnLeftMouseUp(MouseEventArgs args)
        {
            _mouseClickActive = false;
        }

        /// <summary>
        /// Represents the method that will handle the <see cref="Slider.ValueChanged"/> event of a <see cref="Slider"/>.
        /// </summary>
        /// <param name="value">The value of the slider.</param>
        public delegate void ValueChangedDelegate(int value);

        /// <inheritdoc />
        protected override void OnKeyPress(KeyEventArgs args)
        {
            if (Focused != TreeState.Active)
                return;


            if (Orientation == Orientation.Horizontal)
            {
                if (args.Key == Keys.Right && Value < Range)
                    Value++;
                if (args.Key == Keys.Left && Value > 0)
                    Value--;
            }
            else
            {
                if (args.Key == Keys.Up && Value < Range)
                    Value++;
                if (args.Key == Keys.Down && Value > 0)
                    Value--;
            }

            if (args.Key == Keys.D0)
                Value = 0;
        }

        /// <inheritdoc />
        protected override void OnMouseScroll(MouseScrollEventArgs args)
        {
            Value += args.Steps * SmallStep * (Invert ? -1 : 1);
            args.Handled = true;
            base.OnMouseScroll(args);
        }
    }
}
