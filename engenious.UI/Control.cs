using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using engenious.Audio;
using engenious.Graphics;

namespace engenious.UI
{
    /// <summary>
    /// Base class for all ui controls.
    /// </summary>
    public abstract class Control : IControl
    {
        private static BaseScreenComponent? _screenManager;
        /// <summary>
        /// Set the screen manager once instead of setting 
        /// it in every controls constructor.
        /// Will not set screen manager of existing control instances.
        /// </summary>
        public static void SetScreenManager(BaseScreenComponent? screenManager) => _screenManager = screenManager;

        private bool _invalidDrawing;

        private Brush? _background;

        private Brush? _hoveredBackground;

        private Brush? _pressedBackground;

        private Brush? _disabledBackground;

        private Border _margin = Border.All(0);

        private Border _padding = Border.All(0);

        private SoundEffect? _clickSound;

        private SoundEffect? _hoverSound;
        
        /// <inheritdoc />
        public BaseScreenComponent ScreenManager { get; }

        /// <inheritdoc />
        public SoundEffect? ClickSound
        {
            get => _clickSound;
            set
            {
                if (_clickSound != value)
                {
                    _clickSound = value;
                }
            }
        }

        /// <inheritdoc />
        public SoundEffect? HoverSound
        {
            get => _hoverSound;
            set
            {
                if (_hoverSound != value)
                {
                    _hoverSound = value;
                }
            }
        }

        /// <inheritdoc />
        public Brush? Background
        {
            get => _background;
            set
            {
                if (_background != value)
                {
                    _background = value;
                    InvalidateDrawing();
                }
            }
        }
        
        /// <inheritdoc />
        public Brush? HoveredBackground
        {
            get => _hoveredBackground;
            set
            {
                if (_hoveredBackground != value)
                {
                    _hoveredBackground = value;
                    InvalidateDrawing();
                }
            }
        }

        /// <inheritdoc />
        public Brush? PressedBackground
        {
            get => _pressedBackground;
            set
            {
                if (_pressedBackground != value)
                {
                    _pressedBackground = value;
                    InvalidateDrawing();
                }
            }
        }

        /// <inheritdoc />
        public Brush? DisabledBackground
        {
            get => _disabledBackground;
            set
            {
                if (_disabledBackground != value)
                {
                    _disabledBackground = value;
                    InvalidateDrawing();
                }
            }
        }

        /// <inheritdoc />
        public Border Margin
        {
            get => _margin;
            set
            {
                if (!_margin.Equals(value))
                {
                    _margin = value;
                    InvalidateDimensions();
                }
            }
        }

        /// <inheritdoc />
        public Border Padding
        {
            get => _padding;
            set
            {
                if (!_padding.Equals(value))
                {
                    _padding = value;
                    InvalidateDimensions();
                }
            }
        }

        /// <inheritdoc />
        public bool DrawFocusFrame { get; set; }

        /// <inheritdoc />
        public object? Tag { get; set; }

        /// <inheritdoc />
        public string Style { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Control"/> class.
        /// </summary>
        /// <param name="style">The style to use for this control.</param>
        /// <param name="manager">The <see cref="BaseScreenComponent"/>.</param>
        public Control(string style = "", BaseScreenComponent? manager = null)
        {
            Style = style;
            ScreenManager = manager ?? _screenManager ?? throw new ArgumentNullException(nameof(manager));

            _children = new ControlCollection(this);
            _children.OnInserted += ControlCollectionInsert;
            _children.OnRemove += ControlCollectionRemove;
            _rootPathTemp = new List<Control>();
            _rootPath = new ReverseEnumerable<Control>(_rootPathTemp);

            ScreenManager.ClientSizeChanged += (s, e) =>
            {
                OnResolutionChanged();
            };

            ApplySkin(typeof(Control));
        }
        
        
        internal static void CheckStyleInitialized<T>(string name, T value) where T : class
        {
            //TODO:
            // if (value == null)
            //     throw new Exception($"{name} was not set in Checkbox style initialization!");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        protected void ApplySkin(Type type)
        {
            // ControlSkin loader
            if (Skin.Current != null)
            {
                TypeInfo info = type.GetTypeInfo();
                if (info.IsGenericType && Skin.Current.ControlSkins.ContainsKey(type.GetGenericTypeDefinition())) // For generic types
                    Skin.Current.ControlSkins[type.GetGenericTypeDefinition()](this);
                else if (Skin.Current.ControlSkins.ContainsKey(type)) // For non generic types
                    Skin.Current.ControlSkins[type](this);
            }

            // StyleSkin loader
            if (!string.IsNullOrEmpty(Style) && // Only when the style is set
                type == GetType() &&            // Only when type == type of the control (in this case the control that calls this method)
                Skin.Current != null &&
                Skin.Current.StyleSkins.TryGetValue(Style, out var styleSkin))
            {
                styleSkin(this);
            }
        }

        /// <inheritdoc />
        public void Update(GameTime gameTime)
        {
            HandleTransitions(gameTime);
            OnUpdate(gameTime);
            foreach (var child in Children.AgainstZOrder)
                child.Update(gameTime);
        }

        /// <summary>
        /// Gets called when the control is to be updated.
        /// </summary>
        /// <param name="gameTime">Contains the elapsed time since the last update, as well as total elapsed time.</param>
        protected virtual void OnUpdate(GameTime gameTime) { }

        /// <inheritdoc />
        public void PreDraw(GameTime gameTime)
        {
            OnPreDraw(gameTime);
            foreach (var child in Children.AgainstZOrder)
                child.PreDraw(gameTime);
        }
        /// <summary>
        /// Gets called before the control gets drawn as well as before the <see cref="Children"/> get their own <see cref="OnPreDraw"/> called.
        /// </summary>
        /// <param name="gameTime">Contains the elapsed time since the last pre draw, as well as total elapsed time.</param>
        protected virtual void OnPreDraw(GameTime gameTime) { }

        private readonly RasterizerState _rasterizerState = new RasterizerState { ScissorTestEnable = true };

        /// <inheritdoc />
        public void Draw(SpriteBatch batch, Rectangle renderMask, GameTime gameTime)
        {
            if (!Visible) return;

            // Determine control size
            Rectangle controlArea = new Rectangle(AbsolutePosition, ActualSize);
            Rectangle localRenderMask = controlArea.Intersection(renderMask);

            // Set scissor region
            batch.GraphicsDevice.ScissorRectangle = localRenderMask.Transform(AbsoluteTransformation);
            batch.Begin(rasterizerState: _rasterizerState, samplerState: SamplerState.LinearWrap, transformMatrix: AbsoluteTransformation);
            OnDraw(batch, controlArea, gameTime);
            batch.End();

            foreach (var child in Children.AgainstZOrder)
            {
                Rectangle clientArea = ActualClientArea;
                clientArea.Location += AbsolutePosition;
                Rectangle clientRenderMask = clientArea.Intersection(renderMask);

                child.Draw(batch, clientRenderMask, gameTime);
            }

            _invalidDrawing = false;
        }

        /// <summary>
        /// Gets called to draw the control and all its content.
        /// </summary>
        /// <param name="batch">
        /// The spritebatch to draw to.
        /// <remarks>
        /// The spritebatch needs to be in drawing mode. Meaning <see cref="SpriteBatch.Begin"/> was called but
        /// <see cref="SpriteBatch.End"/> was not yet called (again) after the <see cref="SpriteBatch.Begin"/>.
        /// </remarks>
        /// </param>
        /// <param name="controlArea">The available area for the control. (In absolute coordinated)</param>
        /// <param name="gameTime">Contains the elapsed time since the last pre draw, as well as total elapsed time.</param>
        protected virtual void OnDraw(SpriteBatch batch, Rectangle controlArea, GameTime gameTime)
        {
            // Determine background region and draw it.
            Rectangle controlWithMargin = new Rectangle(
               controlArea.X + Margin.Left,
               controlArea.Y + Margin.Top,
               controlArea.Width - Margin.Left - Margin.Right,
               controlArea.Height - Margin.Bottom - Margin.Top);
            OnDrawBackground(batch, controlWithMargin, gameTime, AbsoluteAlpha);

            // Determine content region and draw it.
            Rectangle controlWithPadding = new Rectangle(
                controlWithMargin.X + Padding.Left,
                controlWithMargin.Y + Padding.Top,
                controlWithMargin.Width - Padding.Left - Padding.Right,
                controlWithMargin.Height - Padding.Bottom - Padding.Top);
            OnDrawContent(batch, controlWithPadding, gameTime, AbsoluteAlpha);

            // Draw a focus frame when the control is focused.
            if (Focused == TreeState.Active)
                OnDrawFocusFrame(batch, controlWithMargin, gameTime, AbsoluteAlpha);
        }

        /// <summary>
        /// Gets called to draw the background of the control.
        /// </summary>
        /// <param name="batch">
        /// The spritebatch to draw to.
        /// <remarks>
        /// The spritebatch needs to be in drawing mode. Meaning <see cref="SpriteBatch.Begin"/> was called but
        /// <see cref="SpriteBatch.End"/> was not yet called (again) after the <see cref="SpriteBatch.Begin"/>.
        /// </remarks>
        /// </param>
        /// <param name="backgroundArea">The available area for the background. (In absolute coordinated)</param>
        /// <param name="gameTime">Contains the elapsed time since the last pre draw, as well as total elapsed time.</param>
        /// <param name="alpha">The transparency value to draw the background with.</param>
        protected virtual void OnDrawBackground(SpriteBatch batch, Rectangle backgroundArea, GameTime gameTime, float alpha)
        {
            // Draw the background brush
            if (!Enabled && DisabledBackground != null)
                DisabledBackground.Draw(batch, backgroundArea, alpha);
            else if (Pressed && PressedBackground != null)
                PressedBackground.Draw(batch, backgroundArea, alpha);
            else if (Hovered != TreeState.None && HoveredBackground != null)
                HoveredBackground.Draw(batch, backgroundArea, alpha);
            else
            {
                Background?.Draw(batch, backgroundArea, alpha);
            }
        }

        /// <summary>
        /// Gets called to draw the content of the control.
        /// </summary>
        /// <param name="batch">
        /// The spritebatch to draw to.
        /// <remarks>
        /// The spritebatch needs to be in drawing mode. Meaning <see cref="SpriteBatch.Begin"/> was called but
        /// <see cref="SpriteBatch.End"/> was not yet called (again) after the <see cref="SpriteBatch.Begin"/>.
        /// </remarks>
        /// </param>
        /// <param name="contentArea">The available area for the content. (In absolute coordinated)</param>
        /// <param name="gameTime">Contains the elapsed time since the last pre draw, as well as total elapsed time.</param>
        /// <param name="alpha">The transparency value to draw the background with.</param>
        protected virtual void OnDrawContent(SpriteBatch batch, Rectangle contentArea, GameTime gameTime, float alpha)
        {
        }
        
        /// <summary>
        /// Gets called to draw the focus frame of the control.
        /// </summary>
        /// <param name="batch">
        /// The spritebatch to draw to.
        /// <remarks>
        /// The spritebatch needs to be in drawing mode. Meaning <see cref="SpriteBatch.Begin"/> was called but
        /// <see cref="SpriteBatch.End"/> was not yet called (again) after the <see cref="SpriteBatch.Begin"/>.
        /// </remarks>
        /// </param>
        /// <param name="frameArea">The available area for the focus frame. (In absolute coordinated)</param>
        /// <param name="gameTime">Contains the elapsed time since the last pre draw, as well as total elapsed time.</param>
        /// <param name="alpha">The transparency value to draw the background with.</param>
        protected virtual void OnDrawFocusFrame(SpriteBatch batch, Rectangle frameArea, GameTime gameTime, float alpha)
        {
            if (Skin.Current?.FocusFrameBrush != null && DrawFocusFrame)
                Skin.Current.FocusFrameBrush.Draw(batch, frameArea, AbsoluteAlpha);
        }

        /// <inheritdoc />
        public void InvalidateDrawing()
        {
            _invalidDrawing = true;
        }

        #region Visual Tree Handling

        private bool _enabled = true;

        private bool _visible = true;

        private Control? _parent;

        private readonly ControlCollection _children;

        private readonly PropertyEventArgs<bool> _enabledChangedEventArgs = new PropertyEventArgs<bool>();

        /// <inheritdoc />
        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled == value) return;

                _enabledChangedEventArgs.OldValue = _enabled;
                _enabledChangedEventArgs.NewValue = value;
                _enabledChangedEventArgs.Handled = false;

                _enabled = value;
                InvalidateDrawing();

                if (!_enabled) Unfocus();

                foreach (var child in Children)
                    child.Enabled = _enabled;

                OnEnableChanged(_enabledChangedEventArgs);
                EnableChanged?.Invoke(this, _enabledChangedEventArgs);
            }
        }

        /// <inheritdoc />
        public bool AbsoluteEnabled
        {
            get
            {
                foreach (var item in RootPath)
                    if (!item.Enabled)
                        return false;
                return true;
            }
        }

        private readonly PropertyEventArgs<bool> _visibleChangedEventArgs = new PropertyEventArgs<bool>();
        /// <inheritdoc />
        public bool Visible
        {
            get => _visible;
            set
            {
                if (_visible == value) return;


                _visibleChangedEventArgs.OldValue = _visible;
                _visibleChangedEventArgs.NewValue = value;
                _visibleChangedEventArgs.Handled = false;

                _visible = value;
                InvalidateDimensions();
                InvalidateDrawing();
                if (!_visible) Unfocus();

                OnVisibleChanged(_visibleChangedEventArgs);
                VisibleChanged?.Invoke(this, _visibleChangedEventArgs);
            }
        }

        /// <inheritdoc />
        public bool AbsoluteVisible
        {
            get
            {
                bool result = true;
                foreach (var item in RootPath)
                    result &= item.Visible;
                return result;
            }
        }


        private readonly CollectionEventArgs _controlCollectionInsertArgs = new CollectionEventArgs();
        private void ControlCollectionInsert(Control item, int index)
        {
            _controlCollectionInsertArgs.Control = item;
            _controlCollectionInsertArgs.Index = index;

            OnInsertControl(_controlCollectionInsertArgs);
        }

        private readonly CollectionEventArgs _controlCollectionRemoveArgs = new CollectionEventArgs();
        private void ControlCollectionRemove(Control item, int index)
        {
            _controlCollectionRemoveArgs.Control = item;
            _controlCollectionRemoveArgs.Index = index;

            OnRemoveControl(_controlCollectionRemoveArgs);
        }

        /// <inheritdoc />
        public Control Root
        {
            get
            {
                Control result = this;
                while (result.Parent != null)
                    result = result.Parent;
                return result;
            }
        }

        private readonly List<Control> _rootPathTemp;
        private readonly ReverseEnumerable<Control> _rootPath;
        /// <inheritdoc />
        public ReverseEnumerable<Control> RootPath
        {
            get
            {
                if (PathDirty)
                {
                    // Collect Path
                    _rootPathTemp.Clear();
                    Control? pointer = this;
                    do
                    {
                        _rootPathTemp.Add(pointer);
                        pointer = pointer.Parent;
                    } while (pointer != null);


                    PathDirty = false;
                }
                return _rootPath;
            }
        }


        private readonly PropertyEventArgs<Control> _parentChangedEventArgs = new PropertyEventArgs<Control>();
        /// <inheritdoc />
        public Control? Parent
        {
            get => _parent;
            internal set
            {
                if (_parent == value) return;

                _parentChangedEventArgs.OldValue = _parent;
                _parentChangedEventArgs.NewValue = value;
                _parentChangedEventArgs.Handled = false;

                _parent = value;

                OnParentChanged(_parentChangedEventArgs);
                ParentChanged?.Invoke(this, _parentChangedEventArgs);
            }
        }
        
        /// <summary>
        /// Gets the children of this control.
        /// </summary>
        protected ControlCollection Children => _children;

        /// <summary>
        /// Gets called when a control was inserted into the <see cref="Children"/> collection.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnInsertControl(CollectionEventArgs args) { }

        /// <summary>
        /// Gets called when a control was removed from the <see cref="Children"/> collection.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnRemoveControl(CollectionEventArgs args) { }

        /// <summary>
        /// Raises the <see cref="ParentChanged"/> event.
        /// </summary>
        /// <param name="args">A <see cref="PropertyEventArgs{Control}"/> that contains the event data.</param>
        protected virtual void OnParentChanged(PropertyEventArgs<Control> args) { }

        /// <summary>
        /// Raises the <see cref="EnableChanged"/> event.
        /// </summary>
        /// <param name="args">A <see cref="PropertyEventArgs{Boolean}"/> that contains the event data.</param>
        protected virtual void OnEnableChanged(PropertyEventArgs<bool> args) { }

        /// <summary>
        /// Raises the <see cref="VisibleChanged"/> event.
        /// </summary>
        /// <param name="args">A <see cref="PropertyEventArgs{Boolean}"/> that contains the event data.</param>
        protected virtual void OnVisibleChanged(PropertyEventArgs<bool> args) { }

        /// <summary>
        /// Occurs when the controls <see cref="Parent"/> was changed.
        /// </summary>
        public event PropertyChangedDelegate<Control>? ParentChanged;

        /// <summary>
        /// Occurs when the controls <see cref="Enabled"/> property was changed.
        /// </summary>
        public event PropertyChangedDelegate<bool>? EnableChanged;

        /// <summary>
        /// Occurs when the controls <see cref="Visible"/> property was changed.
        /// </summary>
        public event PropertyChangedDelegate<bool>? VisibleChanged;

        #endregion

        #region Resize- und Position-Management

        private bool _invalidDimensions = true;

        private HorizontalAlignment _horizontalAlignment = HorizontalAlignment.Center;

        private VerticalAlignment _verticalAlignment = VerticalAlignment.Center;

        private Point _actualPosition = Point.Zero;

        private Point _actualSize = Point.Zero;

        private int? _minWidth;

        private int? _width;

        private int? _maxWidth;

        private int? _minHeight;

        private int? _height;

        private int? _maxHeight;

        private Matrix _transformation = Matrix.Identity;

        private float _alpha = 1f;

        /// <inheritdoc />
        public HorizontalAlignment HorizontalAlignment
        {
            get => _horizontalAlignment;
            set
            {
                _horizontalAlignment = value;
                InvalidateDimensions();
            }
        }

        /// <inheritdoc />
        public Matrix Transformation
        {
            get => _transformation;
            set
            {
                if (_transformation != value)
                {
                    _transformation = value;
                    InvalidateDrawing();
                }
            }
        }

        /// <inheritdoc />
        public Matrix AbsoluteTransformation
        {
            get
            {
                Matrix result = Matrix.Identity;
                foreach (var item in RootPath)
                    result *= item.Transformation;
                return result;
            }
        }

        /// <inheritdoc />
        public float Alpha
        {
            get => _alpha;
            set
            {
                if (_alpha != value)
                {
                    _alpha = value;
                    InvalidateDrawing();
                }
            }
        }

        /// <inheritdoc />
        public float AbsoluteAlpha
        {
            get
            {
                float result = 1f;
                foreach (var item in RootPath)
                    result *= item.Alpha;
                return result;
            }
        }

        /// <inheritdoc />
        public VerticalAlignment VerticalAlignment
        {
            get => _verticalAlignment;
            set
            {
                _verticalAlignment = value;
                InvalidateDimensions();
            }
        }

        /// <inheritdoc />
        public int? MinWidth
        {
            get => _minWidth;
            set
            {
                if (_minWidth != value)
                {
                    _minWidth = value;
                    InvalidateDimensions();
                }
            }
        }

        /// <inheritdoc />
        public int? Width
        {
            get => _width;
            set
            {
                if (_width != value)
                {
                    _width = value;
                    InvalidateDimensions();
                }
            }
        }

        /// <inheritdoc />
        public int? MaxWidth
        {
            get => _maxWidth;
            set
            {
                if (_maxWidth != value)
                {
                    _maxWidth = value;
                    InvalidateDimensions();
                }
            }
        }

        /// <inheritdoc />
        public int? MinHeight
        {
            get => _minHeight;
            set
            {
                if (_minHeight != value)
                {
                    _minHeight = value;
                    InvalidateDimensions();
                }
            }
        }

        /// <inheritdoc />
        public int? Height
        {
            get => _height;
            set
            {
                if (_height != value)
                {
                    _height = value;
                    InvalidateDimensions();
                }
            }
        }

        /// <inheritdoc />
        public int? MaxHeight
        {
            get => _maxHeight;
            set
            {
                if (_maxHeight != value)
                {
                    _maxHeight = value;
                    InvalidateDimensions();
                }
            }
        }

        /// <inheritdoc />
        public Point ActualPosition
        {
            get => _actualPosition;
            set
            {
                if (_actualPosition != value)
                {
                    _actualPosition = value;
                    InvalidateDrawing();
                }
                _invalidDimensions = false;
            }
        }

        /// <inheritdoc />
        public Point AbsolutePosition
        {
            get
            {
                Point result = Point.Zero;
                if (Parent != null)
                {
                    result += Parent.AbsolutePosition;
                    result += Parent.ActualClientArea.Location;
                }
                result += ActualPosition;
                return result;
            }
        }

        /// <inheritdoc />
        public Point ActualSize
        {
            get => _actualSize;
            set
            {
                if (_actualSize != value)
                {
                    _actualSize = value;
                    InvalidateDrawing();
                }
                _invalidDimensions = false;
            }
        }

        /// <inheritdoc />
        public virtual Point GetExpectedSize(Point available)
        {
            if (!Visible) return Point.Zero;

            Point result = GetMinClientSize(available);
            Point client = GetMaxClientSize(available);

            // Child controls
            foreach (var child in Children)
            {
                Point size = child.GetExpectedSize(client);
                result = new Point(Math.Max(result.X, size.X), Math.Max(result.Y, size.Y));
            }
            return result + Borders;
        }

        /// <inheritdoc />
        public virtual Point GetMaxClientSize(Point containerSize)
        {
            int x = Width ?? containerSize.X;

            // Max Limiter
            if (MaxWidth.HasValue)
                x = Math.Min(MaxWidth.Value, x);

            // Min Limiter
            if (MinWidth.HasValue)
                x = Math.Max(MinWidth.Value, x);

            int y = Height ?? containerSize.Y;

            // Max Limiter
            if (MaxHeight.HasValue)
                y = Math.Min(MaxHeight.Value, y);

            // Min Limiter
            if (MinHeight.HasValue)
                y = Math.Max(MinHeight.Value, y);

            return new Point(x, y) - Borders;
        }

        /// <inheritdoc />
        public virtual Point GetMinClientSize(Point containerSize)
        {
            Point size = CalculateRequiredClientSpace(containerSize) + Borders;
            int x = Width ?? size.X;
            int y = Height ?? size.Y;

            // Max Limiter
            if (MaxWidth.HasValue)
                x = Math.Min(MaxWidth.Value, x);

            // Min Limiter
            if (MinWidth.HasValue)
                x = Math.Max(MinWidth.Value, x);

            // Max Limiter
            if (MaxHeight.HasValue)
                y = Math.Min(MaxHeight.Value, y);

            // Min Limiter
            if (MinHeight.HasValue)
                y = Math.Max(MinHeight.Value, y);

            return new Point(x, y) - Borders;
        }

        /// <inheritdoc />
        public virtual void SetActualSize(Point available)
        {
            if (!Visible)
            {
                SetDimension(Point.Zero, available);
                return;
            }

            Point minSize = GetExpectedSize(available);
            SetDimension(minSize, available);

            // Apply to child controls
            foreach (var child in Children)
                child.SetActualSize(ActualClientSize);
        }

        /// <summary>
        /// Applies automatic layout on basis of the current size and current alignments.
        /// </summary>
        /// <param name="actualSize">The actual available size of the control.</param>
        /// <param name="containerSize">The size of the container. E.g. some containers can grow indefinitely.</param>
        protected virtual void SetDimension(Point actualSize, Point containerSize)
        {
            var size = new Point(
                Math.Min(containerSize.X, HorizontalAlignment == HorizontalAlignment.Stretch ? containerSize.X : actualSize.X),
                Math.Min(containerSize.Y, VerticalAlignment == VerticalAlignment.Stretch ? containerSize.Y : actualSize.Y));

            Point minSize = GetMinClientSize(containerSize) + Borders;
            Point maxSize = GetMaxClientSize(containerSize) + Borders;

            size.X = Math.Max(minSize.X, Math.Min(maxSize.X, size.X));
            size.Y = Math.Max(minSize.Y, Math.Min(maxSize.Y, size.Y));

            ActualSize = size;

            int x = 0;
            int y = 0;

            switch (HorizontalAlignment)
            {
                case HorizontalAlignment.Center:
                    x = (containerSize.X - ActualSize.X) / 2;
                    break;
                case HorizontalAlignment.Right:
                    x = containerSize.X - ActualSize.X;
                    break;
            }

            switch (VerticalAlignment)
            {
                case VerticalAlignment.Center:
                    y = (containerSize.Y - ActualSize.Y) / 2;
                    break;
                case VerticalAlignment.Bottom:
                    y = containerSize.Y - ActualSize.Y;
                    break;
            }

            ActualPosition = new Point(x, y);
        }

        /// <inheritdoc />
        public bool HasInvalidDimensions()
        {
            bool result = _invalidDimensions;
            foreach (var child in Children)
                result |= child.HasInvalidDimensions();
            return result;
        }

        /// <inheritdoc />
        public virtual Point CalculateRequiredClientSpace(Point available)
        {
            return new Point();
        }

        /// <inheritdoc />
        public void InvalidateDimensions()
        {
            _invalidDimensions = true;
            InvalidateDrawing();
        }

        /// <inheritdoc />
        public virtual void OnResolutionChanged()
        {
            InvalidateDimensions();
        }

        /// <inheritdoc />
        public Rectangle ActualClientArea =>
            new Rectangle(
                Margin.Left + Padding.Left,
                Margin.Top + Padding.Top,
                ActualSize.X - Margin.Left - Padding.Left - Margin.Right - Padding.Right,
                ActualSize.Y - Margin.Top - Padding.Top - Margin.Bottom - Padding.Bottom);

        /// <inheritdoc />
        public Point ActualClientSize => ActualSize - Borders;

        /// <inheritdoc />
        public Point Borders =>
            new Point(
                Margin.Left + Margin.Right + Padding.Left + Padding.Right,
                Margin.Top + Margin.Bottom + Padding.Top + Padding.Bottom);

        #endregion

        #region Transitions

        private readonly Dictionary<Type, Transition> _transitionMap = new Dictionary<Type, Transition>();
        private readonly List<Transition> _transitions = new List<Transition>();

        /// <inheritdoc />
        public void StartTransition(Transition transition)
        {
            if (_transitionMap.ContainsKey(transition.GetType()))
            {
                // Remove old transition of same type.
                Transition t = _transitionMap[transition.GetType()];
                _transitionMap.Remove(transition.GetType());
                _transitions.Remove(t);
            }

            // Add new transition
            _transitionMap.Add(transition.GetType(), transition);
            _transitions.Add(transition);
        }

        private void HandleTransitions(GameTime gameTime)
        {
            // Iterate through transitions
            //List<Transition> drops = new List<Transition>();
            for (var i = 0; i < _transitions.Count; i++)
            {
                var transition = _transitions[i];
                if (!transition.Update(gameTime))
                {
                    _transitionMap.Remove(transition.GetType());
                    _transitions.RemoveAt(i--);
                }
                //drops.Add(transition);
            }
        }

        #endregion

        #region Pointer Management

        private TreeState _hovered = TreeState.None;

        private bool _dropHovered;

        private bool _pressed;

        private readonly PropertyEventArgs<TreeState> _hoveredChangedEventArgs = new PropertyEventArgs<TreeState>();

        /// <inheritdoc />
        public TreeState Hovered
        {
            get => _hovered;
            private set
            {
                if (_hovered == value) return;

                _hoveredChangedEventArgs.OldValue = _hovered;
                _hoveredChangedEventArgs.NewValue = value;
                _hoveredChangedEventArgs.Handled = false;

                _hovered = value;
                InvalidateDrawing();

                OnHoveredChanged(_hoveredChangedEventArgs);
                HoveredChanged?.Invoke(this, _hoveredChangedEventArgs);

                // Sound abspielen
                if (_hoverSound != null && _hovered == TreeState.Active && _hoveredChangedEventArgs.OldValue != TreeState.Passive)
                    _hoverSound.Play();
            }
        }

        /// <inheritdoc />
        public bool Pressed
        {
            get => _pressed;
            private set
            {
                if (_pressed != value)
                {
                    _pressed = value;
                    InvalidateDrawing();
                }
            }
        }

        /// <summary>
        /// Called from the parent control when the mouse was moved over this control.
        /// </summary>
        /// <param name="args">The Mouse parameters.</param>
        /// <returns>A value indicating whether the mouse input was handled by this control.</returns>
        internal bool InternalMouseMove(MouseEventArgs args)
        {
            // Children first (Order by Z-Order)
            bool passive = false;
            foreach (var child in Children.InZOrder)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, child);
                bool handled = child.InternalMouseMove(args);
                passive |= handled;
                args.Bubbled = handled || args.Bubbled;
            }

            // Determine if control is in hovered state (Active & Passive)
            args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, this);
            bool hovered =
                args.LocalPosition.X >= 0 &&
                args.LocalPosition.Y >= 0 &&
                args.LocalPosition.X < ActualSize.X &&
                args.LocalPosition.Y < ActualSize.Y;

            // Wenn the hover state changed -> Call matching change events
            if ((Hovered != TreeState.None) != hovered)
            {
                if (hovered)
                {
                    OnMouseEnter(args);
                    MouseEnter?.Invoke(this, args);
                }
                else
                {
                    // release pressed state
                    Pressed = false;

                    OnMouseLeave(args);
                    MouseLeave?.Invoke(this, args);
                }
            }

            // Event for mouse move
            OnMouseMove(args);
            MouseMove?.Invoke(this, args);

            // Set hover state
            TreeState newState = TreeState.None;
            if (hovered) newState = passive ? TreeState.Passive : TreeState.Active;
            Hovered = newState;

            return hovered;
        }

        internal bool InternalLeftMouseDown(MouseEventArgs args)
        {
            // Ignore if outside of control region
            Point size = ActualSize;
            if (args.LocalPosition.X < 0 || args.LocalPosition.X >= size.X ||
                args.LocalPosition.Y < 0 || args.LocalPosition.Y >= size.Y)
                return false;

            // Ignore if invisible
            if (!Visible) return false;

            // Ignore if disabled
            if (!Enabled) return true;

            // Set focus
            Focus();

            // Activate pressed state
            Pressed = true;

            // Children first (Order by Z-Order)
            foreach (var child in Children.InZOrder)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, child);
                args.Bubbled = child.InternalLeftMouseDown(args) || args.Bubbled;
                if (args.Handled) break;
            }

            // Local events
            if (!args.Handled)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, this);
                OnLeftMouseDown(args);
                LeftMouseDown?.Invoke(this, args);
            }

            return Background != null;
        }

        internal void InternalLeftMouseUp(MouseEventArgs args)
        {
            // Children first (Order by Z-Order)
            foreach (var child in Children.InZOrder)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, child);
                child.InternalLeftMouseUp(args);
            }

            // Local event
            args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, this);
            OnLeftMouseUp(args);
            LeftMouseUp?.Invoke(this, args);
        }

        internal bool InternalLeftMouseClick(MouseEventArgs args)
        {
            // 
            Point size = ActualSize;
            if (args.LocalPosition.X < 0 || args.LocalPosition.X >= size.X ||
                args.LocalPosition.Y < 0 || args.LocalPosition.Y >= size.Y)
                return false;

            // Ignore if invisible
            if (!Visible) return false;

            // Ignore if disabled
            if (!Enabled) return true;
            // Children first (Order by Z-Order)
            foreach (var child in Children.InZOrder)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, child);
                args.Bubbled = child.InternalLeftMouseClick(args) || args.Bubbled;
                if (args.Handled) break;
            }

            // Local event
            if (!args.Handled)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, this);
                OnLeftMouseClick(args);
                LeftMouseClick?.Invoke(this, args);
            }

            // play click sound
            _clickSound?.Play();

            return Background != null;
        }

        internal bool InternalLeftMouseDoubleClick(MouseEventArgs args)
        {
            // Ignore if outside of control region
            Point size = ActualSize;
            if (args.LocalPosition.X < 0 || args.LocalPosition.X >= size.X ||
                args.LocalPosition.Y < 0 || args.LocalPosition.Y >= size.Y)
                return false;

            // Ignore if invisible
            if (!Visible) return false;

            // Ignore if disabled
            if (!Enabled) return true;

            // Children first (Order by Z-Order)
            foreach (var child in Children.InZOrder)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, child);
                args.Bubbled = child.InternalLeftMouseDoubleClick(args) || args.Bubbled;
                if (args.Handled) break;
            }

            // Local event
            if (!args.Handled)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, this);
                OnLeftMouseDoubleClick(args);
                LeftMouseDoubleClick?.Invoke(this, args);
            }

            // play click sound
            _clickSound?.Play();

            return Background != null;
        }

        internal bool InternalRightMouseDown(MouseEventArgs args)
        {
            // Ignore if outside of control region
            Point size = ActualSize;
            if (args.LocalPosition.X < 0 || args.LocalPosition.X >= size.X ||
                args.LocalPosition.Y < 0 || args.LocalPosition.Y >= size.Y)
                return false;

            // Ignore if invisible
            if (!Visible) return false;

            // Ignore disabled
            if (!Enabled) return true;

            Focus();

            // Children first (Order by Z-Order)
            foreach (var child in Children.InZOrder)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, child);
                args.Bubbled = child.InternalRightMouseDown(args) || args.Bubbled;
                if (args.Handled) break;
            }

            //  Local event
            if (!args.Handled)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, this);
                OnRightMouseDown(args);
                RightMouseDown?.Invoke(this, args);
            }

            return Background != null;
        }

        internal void InternalRightMouseUp(MouseEventArgs args)
        {
            // Children first (Order by Z-Order)
            foreach (var child in Children.InZOrder)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, child);
                child.InternalRightMouseUp(args);
            }

            // Locol event
            args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, this);
            OnRightMouseUp(args);
            RightMouseUp?.Invoke(this, args);
        }

        internal bool InternalRightMouseClick(MouseEventArgs args)
        {
            // Ignore if outside of control region
            Point size = ActualSize;
            if (args.LocalPosition.X < 0 || args.LocalPosition.X >= size.X ||
                args.LocalPosition.Y < 0 || args.LocalPosition.Y >= size.Y)
                return false;

            // Ignore if invisible
            if (!Visible) return false;

            // Ignore if disabled
            if (!Enabled) return true;

            // Children first (Order by Z-Order)
            foreach (var child in Children.InZOrder)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, child);
                args.Bubbled = child.InternalRightMouseClick(args) || args.Bubbled;
                if (args.Handled) break;
            }

            // Local event
            if (!args.Handled)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, this);
                OnRightMouseClick(args);
                RightMouseClick?.Invoke(this, args);
            }

            return Background != null;
        }

        internal bool InternalRightMouseDoubleClick(MouseEventArgs args)
        {
            // Ignore if outside of control region
            Point size = ActualSize;
            if (args.LocalPosition.X < 0 || args.LocalPosition.X >= size.X ||
                args.LocalPosition.Y < 0 || args.LocalPosition.Y >= size.Y)
                return false;

            // Ignore if invisible
            if (!Visible) return false;

            // Ignore i disabled
            if (!Enabled) return true;

            // Children first (Order by Z-Order)
            foreach (var child in Children.InZOrder)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, child);
                args.Bubbled = child.InternalRightMouseDoubleClick(args) || args.Bubbled;
                if (args.Handled) break;
            }

            // Local event
            if (!args.Handled)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, this);
                OnRightMouseDoubleClick(args);
                RightMouseDoubleClick?.Invoke(this, args);
            }

            return Background != null;
        }

        internal bool InternalMouseScroll(MouseScrollEventArgs args)
        {
            // Ignore if outside of control region
            Point size = ActualSize;
            if (args.LocalPosition.X < 0 || args.LocalPosition.X >= size.X ||
                args.LocalPosition.Y < 0 || args.LocalPosition.Y >= size.Y)
                return false;

            // Ignore if invisible
            if (!Visible) return false;

            // Ignore if disabled
            if (!Enabled) return true;

            // Children first (Order by Z-Order)
            foreach (var child in Children.InZOrder)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, child);
                args.Bubbled = child.InternalMouseScroll(args) || args.Bubbled;
                if (args.Handled) break;
            }

            // Local event
            if (!args.Handled)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, this);
                OnMouseScroll(args);
                MouseScroll?.Invoke(this, args);
            }

            return Background != null;
        }

        internal bool InternalTouchDown(TouchEventArgs args)
        {
            // Ignore if outside of control region
            Point size = ActualSize;
            if (args.LocalPosition.X < 0 || args.LocalPosition.X >= size.X ||
                args.LocalPosition.Y < 0 || args.LocalPosition.Y >= size.Y)
                return false;

            // Ignore if invisible
            if (!Visible) return false;

            // Ignore if disabled
            if (!Enabled) return true;

            // Set focus
            Focus();

            // Activate pressed state
            Pressed = true;

            // Children first (Order by Z-Order)
            foreach (var child in Children.InZOrder)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, child);
                args.Bubbled = child.InternalTouchDown(args) || args.Bubbled;
                if (args.Handled) break;
            }

            // Local event
            if (!args.Handled)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, this);
                OnTouchDown(args);
                TouchDown?.Invoke(this, args);
            }

            return Background != null;
        }

        internal void InternalTouchMove(TouchEventArgs args)
        {
            // Children first (Order by Z-Order)
            foreach (var child in Children.InZOrder)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, child);
                child.InternalTouchMove(args);
                if (args.Handled) break;
            }

            // Local event
            if (!args.Handled)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, this);
                OnTouchMove(args);
                TouchMove?.Invoke(this, args);
            }
        }

        internal void InternalTouchUp(TouchEventArgs args)
        {
            // Children first (Order by Z-Order)
            foreach (var child in Children.InZOrder)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, child);
                child.InternalTouchUp(args);
                if (args.Handled) break;
            }

            // Local event
            if (!args.Handled)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, this);
                OnTouchUp(args);
                TouchUp?.Invoke(this, args);
            }
        }

        internal bool InternalTouchTap(TouchEventArgs args)
        {
            // Ignore if outside of control region
            Point size = ActualSize;
            if (args.LocalPosition.X < 0 || args.LocalPosition.X >= size.X ||
                args.LocalPosition.Y < 0 || args.LocalPosition.Y >= size.Y)
                return false;

            // Ignore if invisible
            if (!Visible) return false;

            // Ignore if disabled
            if (!Enabled) return true;

            // Set focus
            Focus();

            // Activate pressed state
            Pressed = true;

            // Children first (Order by Z-Order)
            foreach (var child in Children.InZOrder)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, child);
                args.Bubbled = child.InternalTouchTap(args) || args.Bubbled;
                if (args.Handled) break;
            }

            // Local event
            if (!args.Handled)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, this);
                OnTouchTap(args);
                TouchTap?.Invoke(this, args);
            }

            return Background != null;
        }

        internal bool InternalTouchDoubleTap(TouchEventArgs args)
        {
            // Ignore if outside of control region
            Point size = ActualSize;
            if (args.LocalPosition.X < 0 || args.LocalPosition.X >= size.X ||
                args.LocalPosition.Y < 0 || args.LocalPosition.Y >= size.Y)
                return false;

            // Ignore if invisible
            if (!Visible) return false;

            // Ignore if disabled
            if (!Enabled) return true;

            // Set focus
            Focus();

            // Activate pressed state
            Pressed = true;

            // Children first (Order by Z-Order)
            foreach (var child in Children.InZOrder)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, child);
                args.Bubbled = child.InternalTouchDoubleTap(args) || args.Bubbled;
                if (args.Handled) break;
            }

            // Local event
            if (!args.Handled)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, this);
                OnTouchDoubleTap(args);
                TouchDoubleTap?.Invoke(this, args);
            }

            return Background != null;
        }

        private Point CalculateLocalPosition(Point global, Control control)
        {
            Point absolutePosition = control.AbsolutePosition;
            Vector2 local = Vector2.Transform(
                Matrix.Invert(control.AbsoluteTransformation),
                new Vector2(global.X - absolutePosition.X, global.Y - absolutePosition.Y));
            return new Point((int)local.X, (int)local.Y);
        }

        /// <summary>
        /// Raises the <see cref="Control.MouseEnter"/> event.
        /// </summary>
        /// <param name="args">A <see cref="MouseEventArgs"/> that contains the event data.</param>
        protected virtual void OnMouseEnter(MouseEventArgs args) { }

        /// <summary>
        /// Raises the <see cref="Control.MouseLeave"/> event.
        /// </summary>
        /// <param name="args">A <see cref="MouseEventArgs"/> that contains the event data.</param>
        protected virtual void OnMouseLeave(MouseEventArgs args) { }

        /// <summary>
        /// Raises the <see cref="Control.MouseMove"/> event.
        /// </summary>
        /// <param name="args">A <see cref="MouseEventArgs"/> that contains the event data.</param>
        protected virtual void OnMouseMove(MouseEventArgs args) { }

        /// <summary>
        /// Raises the <see cref="Control.LeftMouseDown"/> event.
        /// </summary>
        /// <param name="args">A <see cref="MouseEventArgs"/> that contains the event data.</param>
        protected virtual void OnLeftMouseDown(MouseEventArgs args) { }

        /// <summary>
        /// Raises the <see cref="Control.LeftMouseUp"/> event.
        /// </summary>
        /// <param name="args">A <see cref="MouseEventArgs"/> that contains the event data.</param>
        protected virtual void OnLeftMouseUp(MouseEventArgs args) { }

        /// <summary>
        /// Raises the <see cref="Control.LeftMouseClick"/> event.
        /// </summary>
        /// <param name="args">A <see cref="MouseEventArgs"/> that contains the event data.</param>
        protected virtual void OnLeftMouseClick(MouseEventArgs args) { }

        /// <summary>
        /// Raises the <see cref="Control.LeftMouseDoubleClick"/> event.
        /// </summary>
        /// <param name="args">A <see cref="MouseEventArgs"/> that contains the event data.</param>
        protected virtual void OnLeftMouseDoubleClick(MouseEventArgs args) { }

        /// <summary>
        /// Raises the <see cref="Control.RightMouseDown"/> event.
        /// </summary>
        /// <param name="args">A <see cref="MouseEventArgs"/> that contains the event data.</param>
        protected virtual void OnRightMouseDown(MouseEventArgs args) { }

        /// <summary>
        /// Raises the <see cref="Control.RightMouseUp"/> event.
        /// </summary>
        /// <param name="args">A <see cref="MouseEventArgs"/> that contains the event data.</param>
        protected virtual void OnRightMouseUp(MouseEventArgs args) { }

        /// <summary>
        /// Raises the <see cref="Control.RightMouseClick"/> event.
        /// </summary>
        /// <param name="args">A <see cref="MouseEventArgs"/> that contains the event data.</param>
        protected virtual void OnRightMouseClick(MouseEventArgs args) { }

        /// <summary>
        /// Raises the <see cref="Control.RightMouseDoubleClick"/> event.
        /// </summary>
        /// <param name="args">A <see cref="MouseEventArgs"/> that contains the event data.</param>
        protected virtual void OnRightMouseDoubleClick(MouseEventArgs args) { }

        /// <summary>
        /// Raises the <see cref="Control.MouseScroll"/> event.
        /// </summary>
        /// <param name="args">A <see cref="MouseEventArgs"/> that contains the event data.</param>
        protected virtual void OnMouseScroll(MouseScrollEventArgs args) { }

        /// <summary>
        /// Raises the <see cref="Control.TouchDown"/> event.
        /// </summary>
        /// <param name="args">A <see cref="TouchEventArgs"/> that contains the event data.</param>
        protected virtual void OnTouchDown(TouchEventArgs args) { }

        /// <summary>
        /// Raises the <see cref="Control.TouchMove"/> event.
        /// </summary>
        /// <param name="args">A <see cref="TouchEventArgs"/> that contains the event data.</param>
        protected virtual void OnTouchMove(TouchEventArgs args) { }

        /// <summary>
        /// Raises the <see cref="Control.TouchUp"/> event.
        /// </summary>
        /// <param name="args">A <see cref="TouchEventArgs"/> that contains the event data.</param>
        protected virtual void OnTouchUp(TouchEventArgs args) { }

        /// <summary>
        /// Raises the <see cref="Control.TouchTap"/> event.
        /// </summary>
        /// <param name="args">A <see cref="TouchEventArgs"/> that contains the event data.</param>
        protected virtual void OnTouchTap(TouchEventArgs args) { }

        /// <summary>
        /// Raises the <see cref="TouchDoubleTap"/> event.
        /// </summary>
        /// <param name="args">A <see cref="TouchEventArgs"/> that contains the event data.</param>
        protected virtual void OnTouchDoubleTap(TouchEventArgs args) { }

        /// <summary>
        /// Raises the <see cref="Control.HoveredChanged"/> event.
        /// </summary>
        /// <param name="args">A <see cref="MouseEventArgs"/> that contains the event data.</param>
        protected virtual void OnHoveredChanged(PropertyEventArgs<TreeState> args) { }

        /// <inheritdoc />
        public event MouseEventDelegate? MouseEnter;

        /// <inheritdoc />
        public event MouseEventDelegate? MouseLeave;

        /// <inheritdoc />
        public event MouseEventDelegate? MouseMove;

        /// <inheritdoc />
        public event MouseEventDelegate? LeftMouseDown;

        /// <inheritdoc />
        public event MouseEventDelegate? LeftMouseUp;

        /// <inheritdoc />
        public event MouseEventDelegate? LeftMouseClick;

        /// <inheritdoc />
        public event MouseEventDelegate? LeftMouseDoubleClick;

        /// <inheritdoc />
        public event MouseEventDelegate? RightMouseDown;

        /// <inheritdoc />
        public event MouseEventDelegate? RightMouseUp;

        /// <inheritdoc />
        public event MouseEventDelegate? RightMouseClick;

        /// <inheritdoc />
        public event MouseEventDelegate? RightMouseDoubleClick;

        /// <inheritdoc />
        public event MouseScrollEventDelegate? MouseScroll;

        /// <inheritdoc />
        public event TouchEventDelegate? TouchDown;

        /// <inheritdoc />
        public event TouchEventDelegate? TouchMove;

        /// <inheritdoc />
        public event TouchEventDelegate? TouchUp;

        /// <inheritdoc />
        public event TouchEventDelegate? TouchTap;

        /// <inheritdoc />
        public event TouchEventDelegate? TouchDoubleTap;

        /// <inheritdoc />
        public event PropertyChangedDelegate<TreeState>? HoveredChanged;

        #endregion

        #region Keyboard Management

        internal void InternalKeyDown(KeyEventArgs args)
        {
            // Children first (Order by Z-Order)
            foreach (var child in Children.InZOrder)
            {
                child.InternalKeyDown(args);
                if (args.Handled)
                    break;
            }

            // Bubble up
            if (!args.Handled)
            {
                OnKeyDown(args);
                KeyDown?.Invoke(this, args);
            }
        }

        internal void InternalKeyPress(KeyEventArgs args)
        {
            // Children first (Order by Z-Order)
            foreach (var child in Children.InZOrder)
            {
                child.InternalKeyPress(args);
                if (args.Handled)
                    break;
            }

            // Bubble up
            if (!args.Handled)
            {
                OnKeyPress(args);
                KeyPress?.Invoke(this, args);
            }
        }

        internal void InternalKeyTextPress(KeyTextEventArgs args)
        {
            // Children first (Order by Z-Order)
            foreach (var child in Children.InZOrder)
            {
                child.InternalKeyTextPress(args);
                if (args.Handled)
                    break;
            }

            // Bubble up
            if (!args.Handled)
            {
                OnKeyTextPress(args);
                KeyTextPress?.Invoke(this, args);
            }
        }

        internal void InternalKeyUp(KeyEventArgs args)
        {
            // Children first (Order by Z-Order)
            foreach (var child in Children.InZOrder)
            {
                child.InternalKeyUp(args);
                if (args.Handled)
                    break;
            }

            // Bubble up
            if (!args.Handled)
            {
                OnKeyUp(args);
                KeyUp?.Invoke(this, args);
            }
        }

        /// <summary>
        /// Raises the <see cref="Control.KeyDown"/> event.
        /// </summary>
        /// <param name="args">A <see cref="KeyEventArgs"/> that contains the event data.</param>
        protected virtual void OnKeyDown(KeyEventArgs args) { }
        
        /// <summary>
        /// Raises the <see cref="Control.KeyUp"/> event.
        /// </summary>
        /// <param name="args">A <see cref="KeyEventArgs"/> that contains the event data.</param>
        protected virtual void OnKeyUp(KeyEventArgs args) { }

        /// <summary>
        /// Raises the <see cref="Control.KeyPress"/> event.
        /// </summary>
        /// <param name="args">A <see cref="KeyEventArgs"/> that contains the event data.</param>
        protected virtual void OnKeyPress(KeyEventArgs args) { }

        /// <summary>
        /// Raises the <see cref="Control.KeyTextPress"/> event.
        /// </summary>
        /// <param name="args">A <see cref="KeyTextEventArgs"/> that contains the event data.</param>
        protected virtual void OnKeyTextPress(KeyTextEventArgs args)
        {

        }

        /// <inheritdoc />
        public event KeyEventDelegate? KeyDown;

        /// <inheritdoc />
        public event KeyEventDelegate? KeyUp;

        /// <inheritdoc />
        public event KeyEventDelegate? KeyPress;

        /// <inheritdoc />
        public event KeyTextEventDelegate? KeyTextPress;

        #endregion

        #region Tabbing & Fokus

        private bool _focused;

        private bool _tabStop;

        private bool _canFocus;

        private int _tabOrder;

        private int _zOrder;
        /// <summary>
        /// A value indicating whether the <see cref="RootPath"/> is dirty and needs to be recalculated.
        /// </summary>
        internal bool PathDirty = true;


        private readonly PropertyEventArgs<bool> _tabStopChangedEventArgs = new PropertyEventArgs<bool>();
        /// <inheritdoc />
        public bool TabStop
        {
            get => _tabStop;
            set
            {
                if (_tabStop == value) return;

                _tabStopChangedEventArgs.OldValue = _tabStop;
                _tabStopChangedEventArgs.NewValue = value;
                _tabStopChangedEventArgs.Handled = false;

                _tabStop = value;

                OnTabStopChanged(_tabStopChangedEventArgs);
                TabStopChanged?.Invoke(this, _tabStopChangedEventArgs);
            }
        }

        private readonly PropertyEventArgs<bool> _canFocusChangedEventArgs = new PropertyEventArgs<bool>();
        /// <inheritdoc />
        public bool CanFocus
        {
            get => _canFocus;
            set
            {
                if (_canFocus == value) return;

                _canFocusChangedEventArgs.OldValue = _canFocus;
                _canFocusChangedEventArgs.NewValue = value;
                _canFocusChangedEventArgs.Handled = false;

                _canFocus = value;
                if (!_canFocus) Unfocus();

                OnCanFocusChanged(_canFocusChangedEventArgs);
                CanFocusChanged?.Invoke(this, _canFocusChangedEventArgs);
            }
        }

        private readonly PropertyEventArgs<int> _tabOrderChangedEventArgs = new PropertyEventArgs<int>();
        /// <inheritdoc />
        public int TabOrder
        {
            get => _tabOrder;
            set
            {
                if (_tabOrder == value) return;

                _tabOrderChangedEventArgs.OldValue = _tabOrder;
                _tabOrderChangedEventArgs.NewValue = value;
                _tabOrderChangedEventArgs.Handled = false;

                _tabOrder = value;

                OnTabOrderChanged(_tabOrderChangedEventArgs);
                TabOrderChanged?.Invoke(this, _tabOrderChangedEventArgs);
            }
        }

        /// <inheritdoc />
        public TreeState Focused
        {
            get
            {
                // The control is focused
                if (_focused) return TreeState.Active;

                // Check whether a child is focused
                foreach (var child in Children.InZOrder)
                    if (child.Focused != TreeState.None)
                        return TreeState.Passive;

                return TreeState.None; // Control isn't focused
            }
        }

        private readonly PropertyEventArgs<int> _zOrderChangedEventArgs = new PropertyEventArgs<int>();
        /// <inheritdoc />
        public int ZOrder
        {
            get => _zOrder;
            set
            {
                if (_zOrder == value) return;

                _zOrderChangedEventArgs.OldValue = _zOrder;
                _zOrderChangedEventArgs.NewValue = value;
                _zOrderChangedEventArgs.Handled = false;

                _zOrder = value;

                OnZOrderChanged(_zOrderChangedEventArgs);
                ZOrderChanged?.Invoke(this, _zOrderChangedEventArgs);
            }
        }

        /// <inheritdoc />
        public void Focus()
        {
            if (CanFocus && Visible)
                Root.SetFocus(this);
        }

        /// <inheritdoc />
        public void Unfocus()
        {
            if (_focused)
                Root.SetFocus(null);
        }

        /// <summary>
        /// Set the focus to the given control and the
        /// Visual Tree down from this control.
        /// </summary>
        /// <param name="control">The control to set the focus of.</param>
        internal void SetFocus(Control? control)
        {
            // Set focus on child controls
            foreach (var child in Children.InZOrder)
                child.SetFocus(control);

            bool hit = (control == this);
            if (_focused != hit)
            {
                EventArgs args = EventArgsPool.Instance.Take();
                if (hit)
                {
                    // Ignore if invisible, disabled and not focused
                    if (!Visible || !CanFocus || !Enabled)
                        return;

                    _focused = true;

                    // Just got focus
                    OnGotFocus(args);
                    GotFocus?.Invoke(this, args);
                }
                else
                {
                    _focused = false;

                    // Just lost focus
                    OnLostFocus(args);
                    LostFocus?.Invoke(this, args);
                }

                EventArgsPool.Instance.Release(args);

                InvalidateDrawing();
            }
        }

        /// <summary>
        /// Tab the current focus of a control one control further in the tab order.
        /// </summary>
        /// <returns>
        /// A value indicating whether the tab order could be moved a control further in the tab order.
        /// </returns>
        internal bool InternalTabbedForward()
        {
            // Ignore invisible controls
            if (!Visible) return false;

            bool findFocused = Focused != TreeState.None;

            // Root selected -> unselect
            if (_focused)
            {
                Unfocus();
            }

            // No selection -> select first element
            else if (Focused == TreeState.None && CanFocus &&
                TabStop && AbsoluteEnabled && AbsoluteVisible)
            {
                Focus();
                return true;
            }

            var controls = Children.OrderBy(c => c.TabOrder).ToArray();
            foreach (var control in controls)
            {
                // Skip, to the focused control
                if (findFocused && control.Focused != TreeState.None)
                    findFocused = false;

                if (!findFocused && control.InternalTabbedForward())
                    return true;
            }

            // Was not able to set focus
            return false;
        }

        /// <summary>
        /// Tab the current focus of a control one control back in the tab order.
        /// </summary>
        /// <returns>
        /// A value indicating whether the tab order could be moved a control back in the tab order.
        /// </returns>
        internal bool InternalTabbedBackward()
        {
            // Root selected -> unselect and exit
            if (_focused)
            {
                Unfocus();
                return false;
            }

            bool findFocused = Focused != TreeState.None;
            var controls = Children.OrderByDescending(c => c.TabOrder).ToArray();
            foreach (var control in controls)
            {
                // Skip, to the focused control
                if (findFocused && control.Focused != TreeState.None)
                    findFocused = false;

                if (!findFocused && control.InternalTabbedBackward())
                    return true;
            }

            // No focus found yet -> root
            if (CanFocus && TabStop && AbsoluteEnabled && AbsoluteVisible)
            {
                Focus();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Raises the <see cref="Control.TabStopChanged"/> event.
        /// </summary>
        /// <param name="args">A <see cref="PropertyEventArgs{Boolean}"/> that contains the event data.</param>
        protected virtual void OnTabStopChanged(PropertyEventArgs<bool> args) { }

        /// <summary>
        /// Raises the <see cref="Control.CanFocusChanged"/> event.
        /// </summary>
        /// <param name="args">A <see cref="PropertyEventArgs{Boolean}"/> that contains the event data.</param>
        protected virtual void OnCanFocusChanged(PropertyEventArgs<bool> args) { }

        /// <summary>
        /// Raises the <see cref="Control.TabOrderChanged"/> event.
        /// </summary>
        /// <param name="args">A <see cref="PropertyEventArgs{Int32}"/> that contains the event data.</param>
        protected virtual void OnTabOrderChanged(PropertyEventArgs<int> args) { }

        /// <summary>
        /// Raises the <see cref="Control.ZOrderChanged"/> event.
        /// </summary>
        /// <param name="args">A <see cref="PropertyEventArgs{Int32}"/> that contains the event data.</param>
        protected virtual void OnZOrderChanged(PropertyEventArgs<int> args) { }

        /// <summary>
        /// Raises the <see cref="Control.GotFocus"/> event.
        /// </summary>
        /// <param name="args">A <see cref="EventArgs"/> that contains the event data.</param>
        protected virtual void OnGotFocus(EventArgs args) { }

        /// <summary>
        /// Raises the <see cref="Control.LostFocus"/> event.
        /// </summary>
        /// <param name="args">A <see cref="EventArgs"/> that contains the event data.</param>
        protected virtual void OnLostFocus(EventArgs args) { }

        /// <inheritdoc />
        public event PropertyChangedDelegate<bool>? TabStopChanged;

        /// <inheritdoc />
        public event PropertyChangedDelegate<bool>? CanFocusChanged;

        /// <inheritdoc />
        public event PropertyChangedDelegate<int>? TabOrderChanged;

        /// <inheritdoc />
        public event PropertyChangedDelegate<int>? ZOrderChanged;

        /// <inheritdoc />
        public event EventDelegate? GotFocus;

        /// <inheritdoc />
        public event EventDelegate? LostFocus;

        #endregion

        #region Drag & Drop

        internal bool InternalStartDrag(DragEventArgs args)
        {
            // Ignore if outside of control region
            Point size = ActualSize;
            if (args.LocalPosition.X < 0 || args.LocalPosition.X >= size.X ||
                args.LocalPosition.Y < 0 || args.LocalPosition.Y >= size.Y)
                return false;

            // Ignore if invisible
            if (!Visible) return false;

            // Ignore if disabled
            if (!Enabled) return true;

            // Children first (Order by Z-Order)
            foreach (var child in Children.InZOrder)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, child);
                args.Bubbled = child.InternalStartDrag(args) || args.Bubbled;
                if (args.Handled) break;
            }

            // Bubble up
            if (!args.Handled)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, this);
                OnStartDrag(args);
                StartDrag?.Invoke(this, args);
            }

            return Background != null;
        }

        internal bool InternalDropMove(DragEventArgs args)
        {
            // Children first (Order by Z-Order)
            bool passive = false;
            foreach (var child in Children.InZOrder)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, child);
                bool handled = child.InternalDropMove(args);
                passive |= handled;
                args.Bubbled = handled || args.Bubbled;
            }

            args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, this);
            bool hovered =
                args.LocalPosition.X >= 0 &&
                args.LocalPosition.Y >= 0 &&
                args.LocalPosition.X < ActualSize.X &&
                args.LocalPosition.Y < ActualSize.Y;

            // When the DropHover state changed
            if ((hovered && ScreenManager.Dragging) != _dropHovered)
            {
                if (_dropHovered)
                {
                    OnDropLeave(args);
                    DropLeave?.Invoke(this, args);
                }
                else
                {
                    OnDropEnter(args);
                    DropEnter?.Invoke(this, args);
                }
            }

            OnDropMove(args);
            DropMove?.Invoke(this, args);

            _dropHovered = hovered && ScreenManager.Dragging;

            return hovered;
        }

        internal bool InternalEndDrop(DragEventArgs args)
        {
            // Ignore if outside of control region
            Point size = ActualSize;
            if (args.LocalPosition.X < 0 || args.LocalPosition.X >= size.X ||
                args.LocalPosition.Y < 0 || args.LocalPosition.Y >= size.Y)
                return false;

            // Ignore if invisible
            if (!Visible) return false;

            // Ignore if disabled
            if (!Enabled) return true;

            // Children first (Order by Z-Order)
            foreach (var child in Children.InZOrder)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, child);
                args.Bubbled = child.InternalEndDrop(args) || args.Bubbled;
                if (args.Handled) break;
            }

            // Bubble up
            if (!args.Handled)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, this);
                OnEndDrop(args);
                EndDrop?.Invoke(this, args);
            }

            // Leave event
            if (_dropHovered)
            {
                OnDropLeave(args);
                DropLeave?.Invoke(this, args);
                _dropHovered = false;
            }

            return Background != null;
        }

        /// <summary>
        /// Raises the <see cref="Control.StartDrag"/> event.
        /// </summary>
        /// <param name="args">A <see cref="DragEventArgs"/> that contains the event data.</param>
        protected virtual void OnStartDrag(DragEventArgs args) { }

        /// <summary>
        /// Raises the <see cref="Control.DropMove"/> event.
        /// </summary>
        /// <param name="args">A <see cref="DragEventArgs"/> that contains the event data.</param>
        protected virtual void OnDropMove(DragEventArgs args) { }

        /// <summary>
        /// Raises the <see cref="Control.DropEnter"/> event.
        /// </summary>
        /// <param name="args">A <see cref="DragEventArgs"/> that contains the event data.</param>
        protected virtual void OnDropEnter(DragEventArgs args) { }

        /// <summary>
        /// Raises the <see cref="Control.DropLeave"/> event.
        /// </summary>
        /// <param name="args">A <see cref="DragEventArgs"/> that contains the event data.</param>
        protected virtual void OnDropLeave(DragEventArgs args) { }

        /// <summary>
        /// Raises the <see cref="Control.EndDrop"/> event.
        /// </summary>
        /// <param name="args">A <see cref="DragEventArgs"/> that contains the event data.</param>
        protected virtual void OnEndDrop(DragEventArgs args) { }

        /// <inheritdoc />
        public event DragEventDelegate? StartDrag;

        /// <inheritdoc />
        public event DragEventDelegate? DropMove;

        /// <inheritdoc />
        public event DragEventDelegate? DropEnter;

        /// <inheritdoc />
        public event DragEventDelegate? DropLeave;

        /// <inheritdoc />
        public event DragEventDelegate? EndDrop;

        #endregion
    }

    /// <summary>
    /// Specifies the possible hover and focus states.
    /// </summary>
    public enum TreeState
    {
        /// <summary>
        /// This control is currently inactive.
        /// </summary>
        None,

        /// <summary>
        /// This control only gets set by its children.
        /// </summary>
        Passive,

        /// <summary>
        /// The control is active.
        /// </summary>
        Active
    }
}
