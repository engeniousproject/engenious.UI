using System;
using engenious.Graphics;
using engenious.Input;

namespace engenious.UI.Controls
{
    public class ScrollContainer : ContentControl
    {

        private int scrollerMinSize;

        private Point virtualSize;

        private readonly PropertyEventArgs<bool> _horizontalScrollbarEnabledChangedEventArgs = new PropertyEventArgs<bool>();

        #region Properties 
        /// <summary>
        /// Legt die Scrollgeschwindigkeit fest
        /// </summary>
        public int ScrollSpeed { get; set; } = 20;

        /// <summary>
        /// Gibt an, ob es eine horizontale Scrollbar geben soll.
        /// </summary>
        public bool HorizontalScrollbarEnabled
        {
            get { return HorizontalScrollbar.Enabled; }
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
        /// Gibt an, ob es eine vertikale Scrollbar geben soll.
        /// </summary>
        public bool VerticalScrollbarEnabled
        {
            get { return VerticalScrollbar.Enabled; }
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


        private ScrollbarVisibility horizontalScrollbarVisibility, verticalScrollbarVisibility;
        public ScrollbarVisibility HorizontalScrollbarVisibility
        {
            get => horizontalScrollbarVisibility;
            set
            {
                horizontalScrollbarVisibility = value;
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
        public ScrollbarVisibility VerticalScrollbarVisibility
        {
            get => verticalScrollbarVisibility;
            set
            {
                verticalScrollbarVisibility = value;
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
        /// Gibt die Größe des virtuellen Client-Bereichs an.
        /// </summary>
        public Point VirtualSize
        {
            get { return virtualSize; }
            private set
            {
                if (virtualSize == value) return;

                _virtualSizeChangedEventArgs.OldValue = virtualSize;
                _virtualSizeChangedEventArgs.NewValue = value;
                _virtualSizeChangedEventArgs.Handled = false;

                virtualSize = value;
                
                RecalculateScrollbars();

                OnVirtualSizeChanged(_virtualSizeChangedEventArgs);
                VirtualSizeChanged?.Invoke(this, _virtualSizeChangedEventArgs);
            }
        }

        private readonly PropertyEventArgs<int> _verticalScrollPositionChangedEventArgs = new PropertyEventArgs<int>();
        /// <summary>
        /// Gibt die Scroll-Position auf der virtuellen Achse an oder legt diese fest.
        /// </summary>
        public int VerticalScrollPosition
        {
            get
            {
                return VerticalScrollbar.Value;
            }
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
        /// Gibt die Scroll-Position auf der horizontalen Achse an oder legt diese fest.
        /// </summary>
        public int HorizontalScrollPosition
        {
            get
            {
                return HorizontalScrollbar.Value;
            }
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

        public Slider HorizontalScrollbar
        {
            get => horizontalScrollbar;
        }

        public Slider VerticalScrollbar
        {
            get => verticalScrollbar;
        }

     
        /// <summary>
        /// Gibt die Mindestgröße für den greifbaren Scroller an.
        /// </summary>
        public int ScrollerMinSize
        {
            get { return scrollerMinSize; }
            set
            {
                if (scrollerMinSize != value)
                {
                    scrollerMinSize = value;
                    InvalidateDimensions();
                }
            }
        }

        /// <summary>
        /// Gibt den aktuell sichtbaren Bereich zurück.
        /// </summary>
        public Rectangle VisibleArea
        {
            get
            {
                return new Rectangle(
                    HorizontalScrollPosition, 
                    VerticalScrollPosition, 
                    Math.Min(VirtualSize.X, ActualSize.X), 
                    Math.Min(VirtualSize.Y, ActualSize.Y));
            }
        }

        #endregion

        private Slider horizontalScrollbar, verticalScrollbar;
        public ScrollContainer(BaseScreenComponent manager)
            : base(manager)
        {

            horizontalScrollbar = new Slider(manager)
            {
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                SmallStep = ScrollSpeed,
            };
            horizontalScrollbar.ValueChanged += (val) =>
            {
                HorizontalScrollPosition = val;
            };

            verticalScrollbar = new Slider(manager)
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Right,
                Orientation = Orientation.Vertical,
                SmallStep = ScrollSpeed,
                Invert = true,
            };
            verticalScrollbar.ValueChanged += (val) =>
            {
                VerticalScrollPosition = val;
            };
            HorizontalScrollbarEnabled = false;
            VerticalScrollbarEnabled = true;

            //Children.Add(horizontalScrollbar);
            Children.Add(verticalScrollbar);

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


        public override Point GetExpectedSize(Point available)
        {
            // Bereich ermitteln, der für die Scrollbars verwendet wird
            Point scrollCut = new Point(VerticalScrollbar.Visible ? VerticalScrollbar.Width.Value : 0, HorizontalScrollbar.Visible ? HorizontalScrollbar.Height.Value : 0);

            Point availableContentSize = GetMaxClientSize(available) - scrollCut;
            Point result = GetMinClientSize(available);

            // Client-Bereich erweitern, wenn entsprechende Scrollbars aktiv sind
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
            }

            Content.SetActualSize(result);
            result += scrollCut + Borders;

            result = new Point(Math.Min(available.X, result.X), Math.Min(available.Y, result.Y));
            return result;
        }

        public override void SetActualSize(Point available)
        {
            Point scrollCut = new Point(VerticalScrollbar.Visible ? VerticalScrollbar.Width.Value : 0, HorizontalScrollbar.Visible ? HorizontalScrollbar.Height.Value : 0);

            Point minSize = GetExpectedSize(available);

            // Unnecessary (we already consider min size in GetExpectedSize
            Point controlSize = new Point(Math.Min(minSize.X, available.X), Math.Min(minSize.Y, available.Y));

            SetDimension(controlSize, available);

            Point client = ActualClientSize - scrollCut;
            if (HorizontalScrollbar.Visible) client.X = minSize.X;
            if (VerticalScrollbar.Visible) client.Y = minSize.Y;

            RecalculateScrollbars();

            horizontalScrollbar.SetActualSize(ActualClientSize);
            verticalScrollbar.SetActualSize(ActualClientSize);
            
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

        protected override void OnDrawFocusFrame(SpriteBatch batch, Rectangle contentArea, GameTime gameTime, float alpha)
        {
            if (Skin.Current.FocusFrameBrush != null)
            {
                // Rahmen um die vertikale Scrollbar
                /*Rectangle? vArea = VerticalScrollbarArea;
                if (vArea.HasValue)
                {
                    Rectangle area = new Rectangle(
                        vArea.Value.X + AbsolutePosition.X, 
                        vArea.Value.Y + AbsolutePosition.Y, 
                        vArea.Value.Width, vArea.Value.Height);
                    Skin.Current.FocusFrameBrush.Draw(batch, area, alpha);
                }

                // Rahmen um die horizontale Scrollbar
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
        protected override void OnMouseScroll(MouseScrollEventArgs args)
        {
            VerticalScrollPosition -= args.Steps * ScrollSpeed;
            args.Handled = true;

            base.OnMouseScroll(args);
        }

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
        public void VerticalScrollUp()
        {
            VerticalScrollPosition -= 20;
        }

        public void VerticalScrollPageUp()
        {
            VerticalScrollPosition -= (int)(ActualClientSize.Y * 0.7f);
        }

        public void VerticalScrollDown()
        {
            VerticalScrollPosition += 20;
        }

        public void VerticalScrollPageDown()
        {
            VerticalScrollPosition += (int)(ActualClientSize.Y * 0.7f);
        }

        public void HorizontalScrollUp()
        {
            HorizontalScrollPosition -= 20;
        }

        public void HorizontalScrollPageUp()
        {
            HorizontalScrollPosition -= (int)(ActualClientSize.X * 0.7f);
        }

        public void HorizontalScrollDown()
        {
            HorizontalScrollPosition += 20;
        }

        public void HorizontalScrollPageDown()
        {
            HorizontalScrollPosition += (int)(ActualClientSize.X * 0.7f);
        }
        #endregion

        #region Events

        protected virtual void OnHorizontalScrollbarEnabledChanged(PropertyEventArgs<bool> args) { }
        protected virtual void OnVerticalScrollbarEnabledChanged(PropertyEventArgs<bool> args) { }

        protected virtual void OnHorizontalScrollPositionChanged(PropertyEventArgs<int> args) { }

        protected virtual void OnVerticalScrollPositionChanged(PropertyEventArgs<int> args) { }

        protected virtual void OnVirtualSizeChanged(PropertyEventArgs<Point> args) { }

        public event PropertyChangedDelegate<bool> HorizontalScrollbarEnabledChanged;

        public event PropertyChangedDelegate<bool> VerticalScrollbarEnabledChanged;

        public event PropertyChangedDelegate<int> HorizontalScrollPositionChanged;
        public event PropertyChangedDelegate<int> VerticalScrollPositionChanged;

        public event PropertyChangedDelegate<Point> VirtualSizeChanged;

        #endregion
    }

    public enum ScrollbarVisibility
    {
        Auto,
        Always,
        Never
    }
}
