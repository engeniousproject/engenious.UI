using System;
using engenious.Graphics;
using engenious.Input;

namespace engenious.UI.Controls
{
    /// <summary>
    /// Ui container control used for scrolling out of bounds elements.
    /// </summary>
    public class ScrollContainer : ContentControl
    {
        private int _scrollerMinSize;

        private Point _virtualSize;

        private readonly PropertyEventArgs<bool> _horizontalScrollbarEnabledChangedEventArgs = new PropertyEventArgs<bool>();

        #region Properties 
        /// <summary>
        /// Gets or sets the scrolling speed.
        /// </summary>
        public int ScrollSpeed { get; set; } = 20;

        /// <summary>
        /// Gets or sets whether there should be a horizontal scroll bar.
        /// </summary>
        public bool HorizontalScrollbarEnabled
        {
            get => HorizontalScrollbar.Enabled;
            set
            {
                if (HorizontalScrollbar.Enabled == value) return;

                _horizontalScrollbarEnabledChangedEventArgs.OldValue = HorizontalScrollbar.Enabled;
                _horizontalScrollbarEnabledChangedEventArgs.NewValue = value;
                _horizontalScrollbarEnabledChangedEventArgs.Handled = false;

                HorizontalScrollbar.Enabled = value;
                InvalidateDimensions();

                OnHorizontalScrollbarEnabledChanged(_horizontalScrollbarEnabledChangedEventArgs);
                HorizontalScrollbarEnabledChanged?.Invoke(this, _horizontalScrollbarEnabledChangedEventArgs);
            }
        }
        private readonly PropertyEventArgs<bool> _verticalScrollbarEnabledChangedEventArgs = new PropertyEventArgs<bool>();
        
        /// <summary>
        /// Gets or sets whether there should be a vertical scroll bar.
        /// </summary>
        public bool VerticalScrollbarEnabled
        {
            get => VerticalScrollbar.Enabled;
            set
            {
                if (VerticalScrollbar.Enabled == value) return;

                _verticalScrollbarEnabledChangedEventArgs.OldValue = VerticalScrollbar.Enabled;
                _verticalScrollbarEnabledChangedEventArgs.NewValue = value;
                _verticalScrollbarEnabledChangedEventArgs.Handled = false;

                VerticalScrollbar.Enabled = value;
                InvalidateDimensions();

                OnVerticalScrollbarEnabledChanged(_verticalScrollbarEnabledChangedEventArgs);
                VerticalScrollbarEnabledChanged?.Invoke(this, _verticalScrollbarEnabledChangedEventArgs);
            }
        }


        private ScrollbarVisibility _horizontalScrollbarVisibility, _verticalScrollbarVisibility;
        /// <summary>
        /// Gets or sets the <see cref="ScrollbarVisibility"/> for the horizontal scrollbar.
        /// </summary>
        public ScrollbarVisibility HorizontalScrollbarVisibility
        {
            get => _horizontalScrollbarVisibility;
            set
            {
                _horizontalScrollbarVisibility = value;
                switch(value)
                {
                    case ScrollbarVisibility.Always:
                        HorizontalScrollbar.Visible = true;
                        break;
                    case ScrollbarVisibility.Never:
                        HorizontalScrollbar.Visible = false;
                        break;
                    case ScrollbarVisibility.Auto:
                        RecalculateScrollbars();
                        break;
                }
            }
        }
        /// <summary>
        /// Gets or sets the <see cref="ScrollbarVisibility"/> for the vertical scrollbar.
        /// </summary>
        public ScrollbarVisibility VerticalScrollbarVisibility
        {
            get => _verticalScrollbarVisibility;
            set
            {
                _verticalScrollbarVisibility = value;
                switch(value)
                {
                    case ScrollbarVisibility.Always:
                        VerticalScrollbar.Visible = true;
                        break;
                    case ScrollbarVisibility.Never:
                        VerticalScrollbar.Visible = false;
                        break;
                    case ScrollbarVisibility.Auto:
                        RecalculateScrollbars();
                        break;
                }
            }
        }
        private readonly PropertyEventArgs<bool?> _verticalScrollbarVisibleChangedEventArgs = new PropertyEventArgs<bool?>();

        private readonly PropertyEventArgs<Point> _virtualSizeChangedEventArgs = new PropertyEventArgs<Point>();
        /// <summary>
        /// Gets the size of the virtual client region.
        /// </summary>
        public Point VirtualSize
        {
            get => _virtualSize;
            private set
            {
                if (_virtualSize == value) return;

                _virtualSizeChangedEventArgs.OldValue = _virtualSize;
                _virtualSizeChangedEventArgs.NewValue = value;
                _virtualSizeChangedEventArgs.Handled = false;

                _virtualSize = value;
                
                RecalculateScrollbars();

                OnVirtualSizeChanged(_virtualSizeChangedEventArgs);
                VirtualSizeChanged?.Invoke(this, _virtualSizeChangedEventArgs);
            }
        }

        private readonly PropertyEventArgs<int> _verticalScrollPositionChangedEventArgs = new PropertyEventArgs<int>();
        /// <summary>
        /// Gets or sets the scroll position on the vertical axis.
        /// </summary>
        public int VerticalScrollPosition
        {
            get => VerticalScrollbar.Value;
            set
            {
                int scrollRange = VirtualSize.Y - ActualClientSize.Y;
                int newPosition = Math.Max(0, Math.Min(scrollRange, value));

                _verticalScrollPositionChangedEventArgs.OldValue = VerticalScrollbar.Value;
                _verticalScrollPositionChangedEventArgs.NewValue = newPosition;
                _verticalScrollPositionChangedEventArgs.Handled = false;

                if(VerticalScrollbar.Value != newPosition)
                    VerticalScrollbar.Value = newPosition;
                InvalidateDimensions();

                OnVerticalScrollPositionChanged(_verticalScrollPositionChangedEventArgs);
                VerticalScrollPositionChanged?.Invoke(this, _verticalScrollPositionChangedEventArgs);
            }
        }

        private readonly PropertyEventArgs<int> _horizontalScrollPositionChangedEventArgs = new PropertyEventArgs<int>();
        /// <summary>
        /// Gets or sets the scroll position on the horizontal axis.
        /// </summary>
        public int HorizontalScrollPosition
        {
            get => HorizontalScrollbar.Value;
            set
            {
                int scrollRange = VirtualSize.X - ActualClientSize.X;
                int newPosition = Math.Max(0, Math.Min(scrollRange, value));


                _horizontalScrollPositionChangedEventArgs.OldValue = HorizontalScrollbar.Value;
                _horizontalScrollPositionChangedEventArgs.NewValue = newPosition;
                _horizontalScrollPositionChangedEventArgs.Handled = false;

                if (HorizontalScrollbar.Value != newPosition)
                    HorizontalScrollbar.Value = newPosition;
                InvalidateDimensions();

                OnHorizontalScrollPositionChanged(_horizontalScrollPositionChangedEventArgs);
                HorizontalScrollPositionChanged?.Invoke(this, _horizontalScrollPositionChangedEventArgs);
            }
        }

        /// <summary>
        /// Gets the horizontal scrollbar <see cref="Slider"/> control.
        /// </summary>
        public Slider HorizontalScrollbar => _horizontalScrollbar;

        /// <summary>
        /// Gets the vertical scrollbar <see cref="Slider"/> control.
        /// </summary>
        public Slider VerticalScrollbar => _verticalScrollbar;

        /// <summary>
        /// Gets the minimal size for the scrollbar grab region.
        /// </summary>
        public int ScrollerMinSize
        {
            get => _scrollerMinSize;
            set
            {
                if (_scrollerMinSize != value)
                {
                    _scrollerMinSize = value;
                    InvalidateDimensions();
                }
            }
        }

        /// <summary>
        /// Gets the currently visible area.
        /// </summary>
        public Rectangle VisibleArea =>
            new Rectangle(
                HorizontalScrollPosition, 
                VerticalScrollPosition, 
                Math.Min(VirtualSize.X, ActualSize.X), 
                Math.Min(VirtualSize.Y, ActualSize.Y));

        #endregion

        private readonly Slider _horizontalScrollbar;
        private readonly Slider _verticalScrollbar;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScrollContainer"/> class.
        /// </summary>
        /// <param name="manager">The <see cref="BaseScreenComponent"/>.</param>
        /// <param name="style">The style to use for this control.</param>
        public ScrollContainer(BaseScreenComponent manager, string style = "")
            : base(manager, style)
        {

            _horizontalScrollbar = new Slider(manager)
            {
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                SmallStep = ScrollSpeed,
            };
            _horizontalScrollbar.ValueChanged += (val) =>
            {
                HorizontalScrollPosition = val;
            };

            _verticalScrollbar = new Slider(manager)
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Right,
                Orientation = Orientation.Vertical,
                SmallStep = ScrollSpeed,
                Invert = true,
            };
            _verticalScrollbar.ValueChanged += (val) =>
            {
                VerticalScrollPosition = val;
            };
            HorizontalScrollbarEnabled = false;
            VerticalScrollbarEnabled = true;

            //Children.Add(horizontalScrollbar);
            Children.Add(_verticalScrollbar);

            CanFocus = false;
            TabStop = false;

            ApplySkin(typeof(ScrollContainer));
        }

        private void RecalculateScrollbars()
        {
            int scrollRangeX = Math.Max(0, VirtualSize.X - ActualClientSize.X);
            int scrollRangeY = Math.Max(0, VirtualSize.Y - ActualClientSize.Y);

            HorizontalScrollbar.Range = scrollRangeX;
            VerticalScrollbar.Range = scrollRangeY;
            HorizontalScrollbar.BigStep = ActualClientSize.X;
            VerticalScrollbar.BigStep = ActualClientSize.Y;

            if (scrollRangeX > 0)
                HorizontalScrollbar.KnobSize = Math.Max(20, (int)((float)ActualClientSize.X / VirtualSize.X * ActualClientSize.X));

            if (scrollRangeY > 0)
                VerticalScrollbar.KnobSize = Math.Max(20, (int)((float)ActualClientSize.Y / VirtualSize.Y * ActualClientSize.Y));

            if (HorizontalScrollbarVisibility == ScrollbarVisibility.Auto)
                HorizontalScrollbar.Visible = scrollRangeX > 0;

            if (VerticalScrollbarVisibility == ScrollbarVisibility.Auto)
                VerticalScrollbar.Visible = scrollRangeY > 0;
        }


        /// <inheritdoc />
        public override Point GetExpectedSize(Point available)
        {
            // Determine region used for the scrollbars
            Point scrollCut = new Point(VerticalScrollbar.Visible ? VerticalScrollbar.Width ?? Skin.Current.ScrollbarWidth : 0, HorizontalScrollbar.Visible ? HorizontalScrollbar.Height ?? Skin.Current.ScrollbarWidth : 0);

            Point availableContentSize = GetMaxClientSize(available) - scrollCut;
            Point result = GetMinClientSize(available);

            // expands the client area, for specific active scroll bars
            if (HorizontalScrollbarEnabled)
                availableContentSize.X = int.MaxValue;
            if (VerticalScrollbarEnabled)
                availableContentSize.Y = int.MaxValue;

            if (Content != null)
            {
                Point expected = Content.GetExpectedSize(availableContentSize);
                result.Y = Math.Max(result.Y, expected.Y) - scrollCut.Y;
                result.X = Math.Max(result.X, expected.X) - scrollCut.X;
                //TODO: min size should consider scrollbar sizes!
                
                
                Content.SetActualSize(result);
            }

            result += scrollCut + Borders;

            result = new Point(Math.Min(available.X, result.X), Math.Min(available.Y, result.Y));
            return result;
        }

        /// <inheritdoc />
        public override void SetActualSize(Point available)
        {
            Point scrollCut = new Point(VerticalScrollbar.Visible ? VerticalScrollbar.Width ??  0 : 0, HorizontalScrollbar.Visible ? HorizontalScrollbar.Height ?? 0 : 0);

            Point minSize = GetExpectedSize(available);

            // Unnecessary (we already consider min size in GetExpectedSize
            Point controlSize = new Point(Math.Min(minSize.X, available.X), Math.Min(minSize.Y, available.Y));

            SetDimension(controlSize, available);

            Point client = ActualClientSize - scrollCut;
            if (HorizontalScrollbar.Visible) client.X = minSize.X;
            if (VerticalScrollbar.Visible) client.Y = minSize.Y;

            RecalculateScrollbars();

            _horizontalScrollbar.SetActualSize(ActualClientSize);
            _verticalScrollbar.SetActualSize(ActualClientSize);
            
            // Placement
            if (Content != null)
            {

                Content.SetActualSize(new Point(Math.Max(Content.ActualSize.X, client.X), Math.Max(Content.ActualSize.Y, client.Y)));
                VirtualSize = new Point(Math.Max(VirtualSize.X, Content.ActualSize.X), Math.Max(VirtualSize.Y, Content.ActualSize.Y));
                Content.ActualPosition -= new Point(HorizontalScrollPosition, VerticalScrollPosition);
            }
            else
            {
                VirtualSize = Point.Zero;
            }
        }

        /// <inheritdoc />
        protected override void OnDrawFocusFrame(SpriteBatch batch, Rectangle contentArea, GameTime gameTime, float alpha)
        {
            if (Skin.Current?.FocusFrameBrush != null)
            {
                // border around the vertical scrollbar
                /*Rectangle? vArea = VerticalScrollbarArea;
                if (vArea.HasValue)
                {
                    Rectangle area = new Rectangle(
                        vArea.Value.X + AbsolutePosition.X, 
                        vArea.Value.Y + AbsolutePosition.Y, 
                        vArea.Value.Width, vArea.Value.Height);
                    Skin.Current.FocusFrameBrush.Draw(batch, area, alpha);
                }

                // border around the horizontal scrollbar
                Rectangle? hArea = HorizontalScrollbarArea;
                if (hArea.HasValue)
                {
                    Rectangle area = new Rectangle(
                        hArea.Value.X + AbsolutePosition.X,
                        hArea.Value.Y + AbsolutePosition.Y,
                        hArea.Value.Width, hArea.Value.Height);
                    Skin.Current.FocusFrameBrush.Draw(batch, area, alpha);
                }*/
            }
        }
        
        #region Interaction

        /// <inheritdoc />
        protected override void OnMouseScroll(MouseScrollEventArgs args)
        {
            VerticalScrollPosition -= args.Steps * ScrollSpeed;
            args.Handled = true;

            base.OnMouseScroll(args);
        }

        /// <inheritdoc />
        protected override void OnKeyPress(KeyEventArgs args)
        {
            if (Focused != TreeState.None)
            {
                switch (args.Key)
                {
                    case Keys.Left: if (HorizontalScrollbarEnabled) HorizontalScrollUp(); args.Handled = true; break;
                    case Keys.Right: if (HorizontalScrollbarEnabled) HorizontalScrollDown(); args.Handled = true; break;
                    case Keys.Up: if (VerticalScrollbarEnabled) VerticalScrollUp(); args.Handled = true; break;
                    case Keys.Down: if (VerticalScrollbarEnabled) VerticalScrollDown(); args.Handled = true; break;
                    case Keys.PageUp: if (VerticalScrollbarEnabled) VerticalScrollPageUp(); args.Handled = true; break;
                    case Keys.PageDown: if (VerticalScrollbarEnabled) VerticalScrollPageDown(); args.Handled = true; break;
                }
            }

            base.OnKeyPress(args);
        }
        #endregion

        #region Scroll Methods
        /// <summary>
        /// Scrolls up.
        /// </summary>
        public void VerticalScrollUp()
        {
            VerticalScrollPosition -= 20;
        }
        /// <summary>
        /// Scrolls up by a page.
        /// </summary>
        public void VerticalScrollPageUp()
        {
            VerticalScrollPosition -= (int)(ActualClientSize.Y * 0.7f);
        }
        /// <summary>
        /// Scrolls down.
        /// </summary>
        public void VerticalScrollDown()
        {
            VerticalScrollPosition += 20;
        }
        /// <summary>
        /// Scrolls down by a page.
        /// </summary>
        public void VerticalScrollPageDown()
        {
            VerticalScrollPosition += (int)(ActualClientSize.Y * 0.7f);
        }
        /// <summary>
        /// Scrolls left.
        /// </summary>
        public void HorizontalScrollUp()
        {
            HorizontalScrollPosition -= 20;
        }
        /// <summary>
        /// Scrolls left by a page.
        /// </summary>
        public void HorizontalScrollPageUp()
        {
            HorizontalScrollPosition -= (int)(ActualClientSize.X * 0.7f);
        }

        /// <summary>
        /// Scrolls right.
        /// </summary>
        public void HorizontalScrollDown()
        {
            HorizontalScrollPosition += 20;
        }
        /// <summary>
        /// Scrolls right by a page.
        /// </summary>
        public void HorizontalScrollPageDown()
        {
            HorizontalScrollPosition += (int)(ActualClientSize.X * 0.7f);
        }
        #endregion

        #region Events
        
        /// <summary>
        /// Raises the <see cref="HorizontalScrollbarEnabledChanged"/> event.
        /// </summary>
        /// <param name="args">A <see cref="PropertyEventArgs{Boolean}"/> that contains the event data.</param>
        protected virtual void OnHorizontalScrollbarEnabledChanged(PropertyEventArgs<bool> args) { }
                
        /// <summary>
        /// Raises the <see cref="VerticalScrollbarEnabledChanged"/> event.
        /// </summary>
        /// <param name="args">A <see cref="PropertyEventArgs{Boolean}"/> that contains the event data.</param>
        protected virtual void OnVerticalScrollbarEnabledChanged(PropertyEventArgs<bool> args) { }
                
        /// <summary>
        /// Raises the <see cref="HorizontalScrollPositionChanged"/> event.
        /// </summary>
        /// <param name="args">A <see cref="PropertyEventArgs{Int32}"/> that contains the event data.</param>
        protected virtual void OnHorizontalScrollPositionChanged(PropertyEventArgs<int> args) { }
        
        /// <summary>
        /// Raises the <see cref="VerticalScrollPositionChanged"/> event.
        /// </summary>
        /// <param name="args">A <see cref="PropertyEventArgs{Int32}"/> that contains the event data.</param>
        protected virtual void OnVerticalScrollPositionChanged(PropertyEventArgs<int> args) { }

        /// <summary>
        /// Raises the <see cref="VirtualSizeChanged"/> event.
        /// </summary>
        /// <param name="args">A <see cref="PropertyEventArgs{Point}"/> that contains the event data.</param>
        protected virtual void OnVirtualSizeChanged(PropertyEventArgs<Point> args) { }

        /// <summary>
        /// Occurs when the <see cref="HorizontalScrollbarEnabled"/> property was changed.
        /// </summary>
        public event PropertyChangedDelegate<bool>? HorizontalScrollbarEnabledChanged;

        /// <summary>
        /// Occurs when the <see cref="VerticalScrollbarEnabled"/> property was changed.
        /// </summary>
        public event PropertyChangedDelegate<bool>? VerticalScrollbarEnabledChanged;

        /// <summary>
        /// Occurs when the <see cref="HorizontalScrollPosition"/> property was changed.
        /// </summary>
        public event PropertyChangedDelegate<int>? HorizontalScrollPositionChanged;

        /// <summary>
        /// Occurs when the <see cref="VerticalScrollPosition"/> property was changed.
        /// </summary>
        public event PropertyChangedDelegate<int>? VerticalScrollPositionChanged;

        /// <summary>
        /// Occurs when the <see cref="VirtualSize"/> property was changed.
        /// </summary>
        public event PropertyChangedDelegate<Point>? VirtualSizeChanged;

        #endregion
    }

    /// <summary>
    /// Specifies the 
    /// </summary>
    public enum ScrollbarVisibility
    {
        /// <summary>
        /// Shows the scrollbar when necessary.
        /// </summary>
        Auto,
        /// <summary>
        /// Always shows the scrollbar.
        /// </summary>
        Always,
        /// <summary>
        /// Never shows the scrollbar.
        /// </summary>
        Never
    }
}
