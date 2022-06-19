using engenious.Graphics;
using engenious.Input;

namespace engenious.UI.Controls
{
    /// <summary>
    /// Ui container control to split a region into two resizeable sub panels.
    /// </summary>
    public class Splitter : Control
    {
        private Control? _slot1;

        private int? _slot1MinSize;

        private int? _slot1MaxSize;

        private Control? _slot2;

        private int? _slot2MinSize;

        private int? _slot2MaxSize;

        private int _splitterSize;

        private int _splitterPosition;

        private readonly PropertyEventArgs<Control> _slot1ChangedEventArgs = new PropertyEventArgs<Control>();

        /// <summary>
        /// Gets or sets the control for the first slot.
        /// <remarks>
        /// In orientation <see cref="UI.Orientation.Horizontal"/> the left one;
        /// or <see cref="UI.Orientation.Horizontal"/> the top one.</remarks>
        /// </summary>
        public Control? Slot1
        {
            get => _slot1;
            set
            {
                if (_slot1 == value) return;

                _slot1ChangedEventArgs.OldValue = _slot1;
                _slot1ChangedEventArgs.NewValue = value;
                _slot1ChangedEventArgs.Handled = false;

                if (_slot1 != null)
                {
                    Children.Remove(_slot1);
                    _slot1 = null;
                }

                if (value != null)
                {
                    _slot1 = value;
                    Children.Add(_slot1);
                }

                InvalidateDimensions();

                OnSlot1Changed(_slot1ChangedEventArgs);
                Slot1Changed?.Invoke(this, _slot1ChangedEventArgs);
            }
        }
        private readonly PropertyEventArgs<int?> _slot1MinSizeChangedEventArgs = new PropertyEventArgs<int?>();
        /// <summary>
        /// Gets or sets the minimum required size for the first slot.
        /// <remarks>Use <c>null</c> for no specific minimum size.</remarks>
        /// </summary>
        public int? Slot1MinSize
        {
            get => _slot1MinSize;
            set
            {
                if (_slot1MinSize == value) return;

                _slot1MinSizeChangedEventArgs.OldValue = _slot1MinSize;
                _slot1MinSizeChangedEventArgs.NewValue = value;
                _slot1MinSizeChangedEventArgs.Handled = false;

                _slot1MinSize = value;

                InvalidateDimensions();

                OnSlot1MinSizeChanged(_slot1MinSizeChangedEventArgs);
                Slot1MinSizeChanged?.Invoke(this, _slot1MinSizeChangedEventArgs);
            }
        }

        private readonly PropertyEventArgs<int?> _slot1MaxSizeChangedEventArgs = new PropertyEventArgs<int?>();
        /// <summary>
        /// Gets or sets the maximum required size for the first slot.
        /// <remarks>Use <c>null</c> for no specific maximum size.</remarks>
        /// </summary>
        public int? Slot1MaxSize
        {
            get => _slot1MaxSize;
            set
            {
                if (_slot1MaxSize == value) return;
                
                _slot1MaxSizeChangedEventArgs.OldValue = _slot1MaxSize;
                _slot1MaxSizeChangedEventArgs.NewValue = value;
                _slot1MaxSizeChangedEventArgs.Handled = false;

                _slot1MaxSize = value;

                InvalidateDimensions();

                OnSlot1MaxSizeChanged(_slot1MaxSizeChangedEventArgs);
                Slot1MaxSizeChanged?.Invoke(this, _slot1MaxSizeChangedEventArgs);
            }
        }
        private readonly PropertyEventArgs<Control> _slot2ChangedEventArgs = new PropertyEventArgs<Control>();

        /// <summary>
        /// Gets or sets the control for the second slot.
        /// <remarks>
        /// In orientation <see cref="UI.Orientation.Horizontal"/> the right one;
        /// or <see cref="UI.Orientation.Horizontal"/> the bottom one.</remarks>
        /// </summary>
        public Control? Slot2
        {
            get => _slot2;
            set
            {
                if (_slot2 == value) return;

                _slot2ChangedEventArgs.OldValue = _slot2;
                _slot2ChangedEventArgs.NewValue = value;
                _slot2ChangedEventArgs.Handled = false;

                if (_slot2 != null)
                {
                    Children.Remove(_slot2);
                    _slot2 = null;
                }

                if (value != null)
                {
                    _slot2 = value;
                    Children.Add(_slot2);
                }

                InvalidateDimensions();

                OnSlot2Changed(_slot2ChangedEventArgs);
                Slot2Changed?.Invoke(this, _slot2ChangedEventArgs);
            }
        }

        private readonly PropertyEventArgs<int?> _slot2MinSizeChangedEventArgs = new PropertyEventArgs<int?>();
        /// <summary>
        /// Gets or sets the minimum required size for the second slot.
        /// <remarks>Use <c>null</c> for no specific minimal size.</remarks>
        /// </summary>
        public int? Slot2MinSize
        {
            get => _slot2MinSize;
            set
            {
                if (_slot2MinSize == value) return;

                _slot2MinSizeChangedEventArgs.OldValue = _slot2MinSize;
                _slot2MinSizeChangedEventArgs.NewValue = value;
                _slot2MinSizeChangedEventArgs.Handled = false;

                _slot2MinSize = value;

                InvalidateDimensions();

                OnSlot2MinSizeChanged(_slot2MinSizeChangedEventArgs);
                Slot2MinSizeChanged?.Invoke(this, _slot2MinSizeChangedEventArgs);
            }
        }
        private readonly PropertyEventArgs<int?> _slot2MaxSizeChangedEventArgs = new PropertyEventArgs<int?>();
        /// <summary>
        /// Gets or sets the maximum required size for the second slot.
        /// <remarks>Use <c>null</c> for no specific maximum size.</remarks>
        /// </summary>
        public int? Slot2MaxSize
        {
            get => _slot2MaxSize;
            set
            {
                if (_slot2MaxSize == value) return;

                _slot2MaxSizeChangedEventArgs.OldValue = _slot2MaxSize;
                _slot2MaxSizeChangedEventArgs.NewValue = value;
                _slot2MaxSizeChangedEventArgs.Handled = false;

                _slot2MaxSize = value;

                InvalidateDimensions();

                OnSlot2MaxSizeChanged(_slot2MaxSizeChangedEventArgs);
                Slot2MaxSizeChanged?.Invoke(this, _slot2MaxSizeChangedEventArgs);
            }
        }

        private readonly PropertyEventArgs<int> _splitterSizeChangedEventArgs = new PropertyEventArgs<int>();
        /// <summary>
        /// Gets or sets the size of the splitter region.
        /// </summary>
        public int SplitterSize
        {
            get => _splitterSize;
            set
            {
                if (_splitterSize == value) return;

                _splitterSizeChangedEventArgs.OldValue = _splitterSize;
                _splitterSizeChangedEventArgs.NewValue = value;
                _splitterSizeChangedEventArgs.Handled = false;

                _splitterSize = value;

                InvalidateDimensions();

                OnSplitterSizeChanged(_splitterSizeChangedEventArgs);
                SplitterSizeChanged?.Invoke(this, _splitterSizeChangedEventArgs);
            }
        }

        private readonly PropertyEventArgs<int> _splitterPositionChangedEventArgs = new PropertyEventArgs<int>();
        /// <summary>
        /// Gets or sets the position of the splitter region.
        /// </summary>
        public int SplitterPosition
        {
            get => _splitterPosition;
            set
            {
                if (_splitterPosition == value) return;

                _splitterPositionChangedEventArgs.OldValue = _splitterPosition;
                _splitterPositionChangedEventArgs.NewValue = value;
                _splitterPositionChangedEventArgs.Handled = false;

                _splitterPosition = value;
                InvalidateDimensions();

                OnSplitterPositionChanged(_splitterPositionChangedEventArgs);
                SplitterPositionChanged?.Invoke(this, _splitterPositionChangedEventArgs);
            }
        }

        /// <summary>
        /// Gets the actual position of the splitter.
        /// </summary>
        public int ActualSplitterPosition
        {
            get
            {
                int result = SplitterPosition;

                // size limits of the first slot
                if (result < Slot1MinSize)
                    result = Slot1MinSize.Value;
                if (result > Slot1MaxSize)
                    result = Slot1MaxSize.Value;

                // bottom / right control boundary
                if (Orientation == Orientation.Horizontal)
                {
                    // Slot2 constraints
                    int slot2Width = ActualClientSize.X - SplitterSize - result;
                    if (slot2Width < Slot2MinSize)
                        result = ActualClientSize.X - SplitterSize - Slot2MinSize.Value;
                    if (slot2Width > Slot2MaxSize)
                        result = ActualClientSize.X - SplitterSize - Slot2MaxSize.Value;

                    if (result >= ActualSize.X - SplitterSize)
                        result = ActualSize.X - SplitterSize;
                }
                else if (Orientation == Orientation.Vertical)
                {
                    // Slot2 constraints
                    int slot2Height = ActualClientSize.Y - SplitterSize - result;
                    if (slot2Height < Slot2MinSize)
                        result = ActualClientSize.Y - SplitterSize - Slot2MinSize.Value;
                    if (slot2Height > Slot2MaxSize)
                        result = ActualClientSize.Y - SplitterSize - Slot2MaxSize.Value;

                    if (result >= ActualSize.Y - SplitterSize)
                        result = ActualSize.Y - SplitterSize;
                }

                // top / left control boundary
                if (result < 0) result = 0;

                return result;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="UI.Orientation"/> of the splitter.
        /// <remarks>
        /// <see cref="UI.Orientation.Horizontal"/> splits into a left side(<see cref="Slot1"/>)
        /// and a right side(<see cref="Slot2"/>);
        /// <see cref="UI.Orientation.Vertical"/> splits into a top side(<see cref="Slot1"/>)
        /// and a bottom side(<see cref="Slot2"/>).
        /// </remarks>
        /// </summary>
        public Orientation Orientation { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> used for the splitter in <see cref="UI.Orientation.Horizontal"/> <see cref="Orientation"/>.
        /// </summary>
        public Brush? SplitterBrushHorizontal { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> used for the splitter in <see cref="UI.Orientation.Vertical"/> <see cref="Orientation"/>.
        /// </summary>
        public Brush? SplitterBrushVertical { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Splitter"/> class.
        /// </summary>
        /// <param name="style">The style to use for this control.</param>
        /// <param name="manager">The <see cref="BaseScreenComponent"/>.</param>
        public Splitter(string style = "", BaseScreenComponent? manager = null)
            : base(style, manager)
        {
            CanFocus = true;
            TabStop = true;

            ApplySkin(typeof(Splitter));
        }

        /// <inheritdoc />
        public override Point GetExpectedSize(Point available)
        {
            if (!Visible) return Point.Zero;
            return GetMaxClientSize(available) + Borders;
        }

        /// <inheritdoc />
        public override void SetActualSize(Point available)
        {
            if (!Visible)
            {
                SetDimension(Point.Zero, available);
                return;
            }

            Point minSize = GetExpectedSize(available);
            SetDimension(minSize, available);

            Rectangle client = ActualClientArea;
            if (Orientation == Orientation.Horizontal)
            {
                // Split left and right
                Slot1?.SetActualSize(new Point(ActualSplitterPosition, client.Height));
                if (Slot2 != null)
                {
                    Slot2.SetActualSize(new Point(client.Width - SplitterSize - ActualSplitterPosition, client.Height));
                    Slot2.ActualPosition += new Point(ActualSplitterPosition + SplitterSize, 0);
                }
            }
            else if (Orientation == Orientation.Vertical)
            {
                // Split top and bottom
                Slot1?.SetActualSize(new Point(client.Width, ActualSplitterPosition));
                if (Slot2 != null)
                {
                    Slot2.SetActualSize(new Point(client.Width, client.Height - SplitterSize - ActualSplitterPosition));
                    Slot2.ActualPosition += new Point(0, ActualSplitterPosition + SplitterSize);
                }
            }
        }

        private bool _dragging = false;

        /// <inheritdoc />
        protected override void OnLeftMouseDown(MouseEventArgs args)
        {
            if (Orientation == Orientation.Horizontal)
            {
                if (args.LocalPosition.X > ActualSplitterPosition &&
                    args.LocalPosition.X <= ActualSplitterPosition + SplitterSize)
                {
                    _dragging = true;
                    args.Handled = true;
                }
            }
            else if (Orientation == Orientation.Vertical)
            {
                if (args.LocalPosition.Y > ActualSplitterPosition &&
                    args.LocalPosition.Y <= ActualSplitterPosition + SplitterSize)
                {
                    _dragging = true;
                    args.Handled = true;
                }
            }
        }

        /// <inheritdoc />
        protected override void OnLeftMouseUp(MouseEventArgs args)
        {
            if (_dragging)
            {
                _dragging = false;
                args.Handled = true;
            }
        }

        /// <inheritdoc />
        protected override void OnMouseMove(MouseEventArgs args)
        {
            if (_dragging)
            {
                if (Orientation == Orientation.Horizontal)
                    SplitterPosition = args.LocalPosition.X;
                else if (Orientation == Orientation.Vertical)
                    SplitterPosition = args.LocalPosition.Y;
                args.Handled = true;
            }
        }

        /// <inheritdoc />
        protected override void OnDrawBackground(SpriteBatch batch, Rectangle backgroundArea, GameTime gameTime, float alpha)
        {
            base.OnDrawBackground(batch, backgroundArea, gameTime, alpha);

            // Apply splitter Brush (Horizontal)
            if (Orientation == Orientation.Horizontal && SplitterBrushHorizontal != null)
            {
                Rectangle area = new Rectangle(
                    backgroundArea.X + ActualSplitterPosition, backgroundArea.Y,
                    SplitterSize, backgroundArea.Height);
                SplitterBrushHorizontal.Draw(batch, area, alpha);
            }

            // Apply splitter Brush (Vertical)
            if (Orientation == Orientation.Vertical && SplitterBrushVertical != null)
            {
                Rectangle area = new Rectangle(
                    backgroundArea.X, backgroundArea.Y + ActualSplitterPosition,
                    backgroundArea.Width, SplitterSize);
                SplitterBrushVertical.Draw(batch, area, alpha);
            }
        }

        /// <inheritdoc />
        protected override void OnDrawFocusFrame(SpriteBatch batch, Rectangle contentArea, GameTime gameTime, float alpha)
        {
            if (Skin.Current?.FocusFrameBrush != null)
            {
                if (Orientation == Orientation.Horizontal)
                {
                    Rectangle area = new Rectangle(
                        contentArea.X + ActualSplitterPosition, contentArea.Y,
                        SplitterSize, contentArea.Height);
                    Skin.Current.FocusFrameBrush.Draw(batch, area, alpha);
                }

                if (Orientation == Orientation.Vertical)
                {
                    Rectangle area = new Rectangle(
                        contentArea.X, contentArea.Y + ActualSplitterPosition,
                        contentArea.Width, SplitterSize);
                    Skin.Current.FocusFrameBrush.Draw(batch, area, alpha);
                }
            }
        }

        /// <inheritdoc />
        protected override void OnKeyPress(KeyEventArgs args)
        {
            if (Focused == TreeState.Active)
            {
                if (Orientation == Orientation.Horizontal)
                {
                    if (args.Key == Keys.Left)
                    {
                        SplitterPosition = ActualSplitterPosition - 5;
                        args.Handled = true;
                    }
                    if (args.Key == Keys.Right)
                    {
                        SplitterPosition = ActualSplitterPosition + 5;
                        args.Handled = true;
                    }
                }
                if (Orientation == Orientation.Vertical)
                {
                    if (args.Key == Keys.Up)
                    {
                        SplitterPosition = ActualSplitterPosition - 5;
                        args.Handled = true;
                    }
                    if (args.Key == Keys.Down)
                    {
                        SplitterPosition = ActualSplitterPosition + 5;
                        args.Handled = true;
                    }
                }
            }
            base.OnKeyPress(args);
        }

        /// <summary>
        /// Raises the <see cref="Slot1Changed"/> event.
        /// </summary>
        /// <param name="args">A <see cref="PropertyEventArgs{Control}"/> that contains the event data.</param>
        protected virtual void OnSlot1Changed(PropertyEventArgs<Control> args) { }

        /// <summary>
        /// Raises the <see cref="Slot1MinSizeChanged"/> event.
        /// </summary>
        /// <param name="args">A <see><cref>PropertyEventArgs{Nullable{Int32}}</cref></see> that contains the event data.</param>
        protected virtual void OnSlot1MinSizeChanged(PropertyEventArgs<int?> args) { }

        /// <summary>
        /// Raises the <see cref="Slot1MaxSizeChanged"/> event.
        /// </summary>
        /// <param name="args">A <see><cref>PropertyEventArgs{Nullable{Int32}}</cref></see> that contains the event data.</param>
        protected virtual void OnSlot1MaxSizeChanged(PropertyEventArgs<int?> args) { }

        /// <summary>
        /// Raises the <see cref="Slot2Changed"/> event.
        /// </summary>
        /// <param name="args">A <see cref="PropertyEventArgs{Control}"/> that contains the event data.</param>
        protected virtual void OnSlot2Changed(PropertyEventArgs<Control> args) { }

        /// <summary>
        /// Raises the <see cref="Slot2MinSizeChanged"/> event.
        /// </summary>
        /// <param name="args">A <see><cref>PropertyEventArgs{Nullable{Int32}}</cref></see> that contains the event data.</param>
        protected virtual void OnSlot2MinSizeChanged(PropertyEventArgs<int?> args) { }

        /// <summary>
        /// Raises the <see cref="Slot2MaxSizeChanged"/> event.
        /// </summary>
        /// <param name="args">A <see><cref>PropertyEventArgs{Nullable{Int32}}</cref></see> that contains the event data.</param>
        protected virtual void OnSlot2MaxSizeChanged(PropertyEventArgs<int?> args) { }

        /// <summary>
        /// Raises the <see cref="SplitterPositionChanged"/> event.
        /// </summary>
        /// <param name="args">A <see cref="PropertyEventArgs{Int32}"/> that contains the event data.</param>
        protected virtual void OnSplitterPositionChanged(PropertyEventArgs<int> args) { }

        /// <summary>
        /// Raises the <see cref="SplitterSizeChanged"/> event.
        /// </summary>
        /// <param name="args">A <see cref="PropertyEventArgs{Int32}"/> that contains the event data.</param>
        protected virtual void OnSplitterSizeChanged(PropertyEventArgs<int> args) { }

        /// <summary>
        /// Occurs when the <see cref="Slot1"/> control was changed.
        /// </summary>
        public event PropertyChangedDelegate<Control>? Slot1Changed;

        /// <summary>
        /// Occurs when the <see cref="Slot2"/> control was changed.
        /// </summary>
        public event PropertyChangedDelegate<Control>? Slot2Changed;

        /// <summary>
        /// Occurs when the <see cref="Slot1MinSize"/> property was changed.
        /// </summary>
        public event PropertyChangedDelegate<int?>? Slot1MinSizeChanged;

        /// <summary>
        /// Occurs when the <see cref="Slot1MaxSize"/> property was changed.
        /// </summary>
        public event PropertyChangedDelegate<int?>? Slot1MaxSizeChanged;

        /// <summary>
        /// Occurs when the <see cref="Slot2MinSize"/> property was changed.
        /// </summary>
        public event PropertyChangedDelegate<int?>? Slot2MinSizeChanged;

        /// <summary>
        /// Occurs when the <see cref="Slot2MaxSize"/> property was changed.
        /// </summary>
        public event PropertyChangedDelegate<int?>? Slot2MaxSizeChanged;

        /// <summary>
        /// Occurs when the <see cref="SplitterPosition"/> property was changed.
        /// </summary>
        public event PropertyChangedDelegate<int>? SplitterPositionChanged;

        /// <summary>
        /// Occurs when the <see cref="SplitterSize"/> property was changed.
        /// </summary>
        public event PropertyChangedDelegate<int>? SplitterSizeChanged;
    }
}
