using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using engenious.Content;
using engenious.Graphics;
using engenious.Input;
using engenious.UI.Controls;

namespace engenious.UI
{
    /// <summary>
    /// Base screen component manager for engenious Components.
    /// </summary>
    public class BaseScreenComponent : DrawableGameComponent
    {
        /// <summary>
        /// Maximum default delay[ms] between clicks to be recognized as double clicks.
        /// </summary>
        public const int DefaultDoubleClickDelay = 500;

        private ContainerControl _root = null!;

        private FlyoutControl _flyout;

        private SpriteBatch _batch;

        private MouseMode _mouseMode;

        private struct InvokeAction
        {
            public InvokeAction(Action action, ManualResetEvent resetEvent)
            {
                Action = action;
                ResetEvent = resetEvent;
            }

            public Action Action { get; }
            public ManualResetEvent ResetEvent { get; }
        }

        private readonly ConcurrentQueue<InvokeAction> _invokes = new();

        /// <summary>
        /// Prefix für die Titel-Leiste
        /// </summary>
        public string TitlePrefix
        {
            get => _titlePrefix;
            set
            {
                _titlePrefix = value;
                _titleDirty = true;
            }
        }

        /// <summary>
        /// Gets the root control containing all controls
        /// </summary>
        public ContentControl Frame { get; private set; }

        /// <summary>
        /// Gibt das Control an, das zum navigieren der Screens verwendet wird.
        /// </summary>
        public ContainerControl ScreenTarget { get; set; }

        /// <summary>
        /// Gets or sets the default transition to use to navigate to a screen.
        /// </summary>
        public Transition? NavigateToTransition { get; set; }

        /// <summary>
        /// Gets or sets the default transition to use to navigate away from a screen.
        /// </summary>
        public Transition? NavigateFromTransition { get; set; }

        /// <summary>
        /// Gets the engenious ContentManager used for loading game content.
        /// </summary>
        public ContentManagerBase Content { get; }

        /// <summary>
        /// Gets a value indicating whether dragging currently happens.
        /// </summary>
        public bool Dragging => DraggingArgs != null && DraggingArgs.Handled;

        /// <summary>
        /// Gets or sets a value indicating whether game pad input should be activated. (Currently not supported)
        /// </summary>
        public bool GamePadEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether mouse input should be activated. 
        /// </summary>
        public bool MouseEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether touch input should be activated. (Currently not supported)
        /// </summary>
        public bool TouchEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether keyboard input should be activated. 
        /// </summary>
        public bool KeyboardEnabled { get; set; }

        /// <summary>
        /// Gets or sets maximum delay[ms] between clicks to be recognized as double clicks.
        /// <remarks>Default value is <see cref="DefaultDoubleClickDelay"/>.</remarks>
        /// </summary>
        public int DoubleClickDelay { get; set; }

        /// <summary>
        /// Gets or sets the current <see cref="UI.MouseMode"/>.
        /// </summary>
        public MouseMode MouseMode
        {
            get => _mouseMode;
            private set
            {
                if (_mouseMode != value)
                {
                    _mouseMode = value;
                    Game.IsMouseVisible = (_mouseMode != MouseMode.Captured);
                    Game.IsCursorGrabbed = (_mouseMode == MouseMode.Captured);

                    if (_mouseMode == MouseMode.Free)
                        Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
                    else
                        _resetMouse = true;
                }
            }
        }

        /// <summary>
        /// Reset (ignore) mouse-position. Used to compensate first movement after mouse-capture.
        /// </summary>
        private bool _resetMouse = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseScreenComponent"/> class.
        /// </summary>
        /// <param name="game">The game instance for this <see cref="BaseScreenComponent"/>.</param>
        public BaseScreenComponent(IGame game)
            : base(game)
        {
            Content = game.Content;
            _root = null!;
            Frame = null!;
            _flyout = null!;
            _batch = null!;
            KeyboardEnabled = true;
            MouseEnabled = true;
            GamePadEnabled = true;
            TouchEnabled = true;
            DoubleClickDelay = DefaultDoubleClickDelay;

            _pressedKeys = ((Keys[]) Enum.GetValues(typeof(Keys))).Where(x => (int)x != 0).Select(k => (int)k).Distinct().Select(idx => UnpressedKeyTimestamp).ToArray();

#if !ANDROID

            Game.KeyPress += (s, e) =>
            {
                if (Game.IsActive)
                {
                    KeyTextEventArgs args = new KeyTextEventArgs() { Character = e };

                    _root.InternalKeyTextPress(args);
                }
            };

#endif

            Game.Resized += (s, e) =>
            {
                ClientSizeChanged?.Invoke(s, e);
            };

            ScreenTarget = null!;
        }
        
        /// <inheritdoc />
        protected override void LoadContent()
        {
            Skin.Pix = new Texture2D(GraphicsDevice, 1, 1);
            Skin.Pix.SetData<Color>(stackalloc [] { Color.White });

            Skin.Current = new Skin(Content);
            _batch = new SpriteBatch(GraphicsDevice);

            _root = new ContainerControl(manager: this)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            Frame = new ContentControl(manager: this)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            _root.Controls.Add(Frame);

            ContainerControl screenContainer = new ContainerControl(manager: this)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            Frame.Content = screenContainer;
            ScreenTarget = screenContainer;

            _flyout = new FlyoutControl(manager: this)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            _root.Controls.Add(_flyout);
        }

        private bool _lastLeftMouseButtonPressed = false;

        private bool _lastRightMouseButtonPressed = false;

        private int _lastMouseWheelValue = 0;

        private Point _lastMousePosition = Point.Zero;

        private TimeSpan? _lastLeftClick = null;

        private TimeSpan? _lastRightClick = null;

        //private TimeSpan? lastTouchTap = null;

        private int? _draggingId = null;

        internal DragEventArgs? DraggingArgs { get; private set; }

        //private Dictionary<Keys, double> pressedKeys = new Dictionary<Keys, double>();

        private readonly double[] _pressedKeys;

        private const double UnpressedKeyTimestamp = -1d;

        /// <summary>
        /// Führt einen Delegaten im UI-Thread aus.
        /// </summary>
        /// <param name="invokedAction">Ein Delegat, der eine aufzurufende Methode im Threadkontext des Steuerelements enthält.</param>
        public void Invoke(Action invokedAction)
        {
            if (invokedAction == null)
                throw new ArgumentNullException(nameof(invokedAction));
            var resetEvent = new ManualResetEvent(false);
            _invokes.Enqueue(new InvokeAction(invokedAction, resetEvent));
            resetEvent.WaitOne();
            resetEvent.Dispose();
        }

        /// <summary>
        /// Handling of all input and update all screens and controls.
        /// </summary>
        /// <param name="gameTime">Contains the elapsed time since the last update, as well as total elapsed time.</param>
        public override void Update(GameTime gameTime)
        {
            while(_invokes.TryDequeue(out var act))
            {
                act.Action.Invoke();
                act.ResetEvent.Set();
            }

            if (Game.IsActive)
            {

                #region Mouse Interaction

                if (MouseEnabled)
                {
                    // Determine mouse position dependant on current mouse mode
                    MouseState mouse;
                    Point mousePosition;
                    if (MouseMode == MouseMode.Captured && !_resetMouse)
                    {

                        mouse = Mouse.GetState();
                        mousePosition = new Point(
                            mouse.X - (_lastMousePosition.X),
                            mouse.Y - (_lastMousePosition.Y));
                    }
                    else if(_resetMouse)
                    {
                        Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
                        _lastMousePosition = new Point(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
                        mouse = Mouse.GetState();
                        mousePosition = new Point();
                        _resetMouse = false;
                    }
                    else
                    {
                        mouse = Mouse.GetCursorState();
                        mousePosition = mouse.Location;
                    }


                    MouseEventArgs mouseEventArgs = MouseEventArgsPool.Instance.Take();

                    mouseEventArgs.MouseMode = MouseMode;
                    mouseEventArgs.GlobalPosition = mousePosition;
                    mouseEventArgs.LocalPosition = mousePosition;

                    // Mouse Move
                    if (mousePosition != _lastMousePosition)
                    {
                        mouseEventArgs.Handled = false;

                        _root.InternalMouseMove(mouseEventArgs);
                        if (!mouseEventArgs.Handled)
                            MouseMove?.Invoke(mouseEventArgs);

                        // Start Drag Handling
                        if (mouse.LeftButton == ButtonState.Pressed &&
                            DraggingArgs == null)
                        {
                            DraggingArgs = DragEventArgsPool.Instance.Take();
                            DraggingArgs.GlobalPosition = mousePosition;
                            DraggingArgs.LocalPosition = mousePosition;

                            _draggingId = null;

                            _root.InternalStartDrag(DraggingArgs);
                            if (!DraggingArgs.Handled)
                                StartDrag?.Invoke(DraggingArgs);
                        }

                        // Drop move
                        if (mouse.LeftButton == ButtonState.Pressed &&
                            DraggingArgs != null &&
                            _draggingId == null &&
                            DraggingArgs.Handled)
                        {
                            //TODO: perhaps pool single object?
                            DragEventArgs args = DragEventArgsPool.Instance.Take();

                            args.GlobalPosition = mousePosition;
                            args.LocalPosition = mousePosition;
                            args.Content = DraggingArgs.Content;
                            args.Icon = DraggingArgs.Icon;
                            args.Sender = DraggingArgs.Sender;

                            _root.InternalDropMove(args);
                            if (!args.Handled)
                                DropMove?.Invoke(args);
                        }
                    }

                    // Left mouse button
                    if (mouse.LeftButton == ButtonState.Pressed)
                    {
                        if (!_lastLeftMouseButtonPressed)
                        {
                            mouseEventArgs.Handled = false;

                            // Left mouse button was just newly pressed
                            _root.InternalLeftMouseDown(mouseEventArgs);
                            if (!mouseEventArgs.Handled)
                                LeftMouseDown?.Invoke(mouseEventArgs);
                        }
                        _lastLeftMouseButtonPressed = true;
                    }
                    else
                    {
                        if (_lastLeftMouseButtonPressed)
                        {
                            // Handle Drop
                            if (DraggingArgs != null)
                            {
                                if (DraggingArgs.Handled)
                                {
                                    DragEventArgs args = DragEventArgsPool.Instance.Take();
                                    args.GlobalPosition = mousePosition;
                                    args.LocalPosition = mousePosition;
                                    args.Content = DraggingArgs.Content;
                                    args.Icon = DraggingArgs.Icon;
                                    args.Sender = DraggingArgs.Sender;

                                    _root.InternalEndDrop(args);
                                    if (!args.Handled)
                                        EndDrop?.Invoke(args);
                                }
                                // Discard Dragging Infos
                                DragEventArgsPool.Instance.Release(DraggingArgs);
                            }
                            
                            DraggingArgs = null;
                            _draggingId = null;

                            // Left mouse button was released
                            mouseEventArgs.Handled = false;

                            _root.InternalLeftMouseClick(mouseEventArgs);
                            if (!mouseEventArgs.Handled)
                                LeftMouseClick?.Invoke(mouseEventArgs);

                            if (_lastLeftClick.HasValue &&
                                gameTime.TotalGameTime - _lastLeftClick.Value < TimeSpan.FromMilliseconds(DoubleClickDelay))
                            {
                                // Double Left Click
                                mouseEventArgs.Handled = false;

                                _root.InternalLeftMouseDoubleClick(mouseEventArgs);
                                if (!mouseEventArgs.Handled)
                                    LeftMouseDoubleClick?.Invoke(mouseEventArgs);

                                _lastLeftClick = null;
                            }
                            else
                            {
                                _lastLeftClick = gameTime.TotalGameTime;
                            }

                            // Mouse Up
                            mouseEventArgs.Handled = false;

                            _root.InternalLeftMouseUp(mouseEventArgs);
                            if (!mouseEventArgs.Handled)
                                LeftMouseUp?.Invoke(mouseEventArgs);
                        }
                        _lastLeftMouseButtonPressed = false;
                    }

                    // Right mouse button
                    if (mouse.RightButton == ButtonState.Pressed)
                    {
                        if (!_lastRightMouseButtonPressed)
                        {
                            // Right mouse button was just newly pressed
                            mouseEventArgs.Handled = false;

                            _root.InternalRightMouseDown(mouseEventArgs);
                            if (!mouseEventArgs.Handled)
                                RightMouseDown?.Invoke(mouseEventArgs);
                        }
                        _lastRightMouseButtonPressed = true;
                    }
                    else
                    {
                        if (_lastRightMouseButtonPressed)
                        {
                            // Right mouse button was released
                            mouseEventArgs.Handled = false;
                            _root.InternalRightMouseClick(mouseEventArgs);
                            if (!mouseEventArgs.Handled)
                                RightMouseClick?.Invoke(mouseEventArgs);

                            if (_lastRightClick.HasValue &&
                                gameTime.TotalGameTime - _lastRightClick.Value < TimeSpan.FromMilliseconds(DoubleClickDelay))
                            {
                                // Double Left Click
                                mouseEventArgs.Handled = false;

                                _root.InternalRightMouseDoubleClick(mouseEventArgs);
                                if (!mouseEventArgs.Handled)
                                    RightMouseDoubleClick?.Invoke(mouseEventArgs);

                                _lastRightClick = null;
                            }
                            else
                            {
                                _lastRightClick = gameTime.TotalGameTime;
                            }

                            mouseEventArgs.Handled = false;

                            _root.InternalRightMouseUp(mouseEventArgs);
                            if (!mouseEventArgs.Handled)
                                RightMouseUp?.Invoke(mouseEventArgs);
                        }
                        _lastRightMouseButtonPressed = false;
                    }

                    // Mousewheel
                    if (_lastMouseWheelValue != mouse.ScrollWheelValue)
                    {
                        int diff = (mouse.ScrollWheelValue - _lastMouseWheelValue);

                        MouseScrollEventArgs scrollArgs = new MouseScrollEventArgs
                        {
                            MouseMode = MouseMode,
                            GlobalPosition = mousePosition,
                            LocalPosition = mousePosition,
                            Steps = diff
                        };
                        _root.InternalMouseScroll(scrollArgs);
                        if (!scrollArgs.Handled)
                            MouseScroll?.Invoke(scrollArgs);

                        _lastMouseWheelValue = mouse.ScrollWheelValue;
                    }

                    // Potential mouse position reset.
                    if (MouseMode == MouseMode.Free)
                    {
                        _lastMousePosition = mousePosition;
                    }
                    else if (mousePosition.X != 0 || mousePosition.Y != 0)
                    {
                        _lastMousePosition = mouse.Location;
                    }

                    MouseEventArgsPool.Instance.Release(mouseEventArgs);
                }

                #endregion

                #region Keyboard Interaction

                if (KeyboardEnabled)
                {
                    KeyboardState keyboard = Keyboard.GetState();

                    bool shift = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);
                    bool ctrl = keyboard.IsKeyDown(Keys.LeftControl) || keyboard.IsKeyDown(Keys.RightControl);
                    bool alt = keyboard.IsKeyDown(Keys.LeftAlt) || keyboard.IsKeyDown(Keys.RightAlt);

                    KeyEventArgs args;

                    for (int i = 0; i < _pressedKeys.Length; i++)
                    {
                        var key = (Keys) (i + 1); 
                        if (keyboard.IsKeyDown(key))
                        {
                            // ReSharper disable once CompareOfFloatsByEqualityOperator
                            if (_pressedKeys[i] == UnpressedKeyTimestamp)
                            {
                                // Newly pressed key

                                args = KeyEventArgsPool.Instance.Take();

                                args.Key = key;
                                args.Shift = shift;
                                args.Ctrl = ctrl;
                                args.Alt = alt;

                                _root.InternalKeyDown(args);

                                if (!args.Handled)
                                {
                                    KeyDown?.Invoke(args);
                                }

                                KeyEventArgsPool.Instance.Release(args);

                                args = KeyEventArgsPool.Instance.Take();

                                args.Key = key;
                                args.Shift = shift;
                                args.Ctrl = ctrl;
                                args.Alt = alt;

                                _root.InternalKeyPress(args);
                                _pressedKeys[i] = gameTime.TotalGameTime.TotalMilliseconds + 500;

                                KeyEventArgsPool.Instance.Release(args);

                                // Special key Tab for focus switching (if it wasn't handles)
                                if (key == Keys.Tab && !args.Handled)
                                {
                                    if (shift) _root.InternalTabbedBackward();
                                    else _root.InternalTabbedForward();
                                }
                            }
                            else
                            {
                                // Key still pressed
                                if (_pressedKeys[i] <= gameTime.TotalGameTime.TotalMilliseconds)
                                {
                                    args = KeyEventArgsPool.Instance.Take();

                                    args.Key = key;
                                    args.Shift = shift;
                                    args.Ctrl = ctrl;
                                    args.Alt = alt;


                                    _root.InternalKeyPress(args);
                                    if (!args.Handled)
                                    {
                                        KeyPress?.Invoke(args);
                                    }

                                    KeyEventArgsPool.Instance.Release(args);

                                    _pressedKeys[i] = gameTime.TotalGameTime.TotalMilliseconds + 50;
                                }
                            }
                        }
                        else
                        {
                            // ReSharper disable once CompareOfFloatsByEqualityOperator
                            if (_pressedKeys[i] != UnpressedKeyTimestamp)
                            {
                                // Key released
                                args = KeyEventArgsPool.Instance.Take();

                                args.Key = key;
                                args.Shift = shift;
                                args.Ctrl = ctrl;
                                args.Alt = alt;

                                _root.InternalKeyUp(args);
                                _pressedKeys[i] = UnpressedKeyTimestamp;

                                if (!args.Handled)
                                {
                                    KeyUp?.Invoke(args);
                                }

                                KeyEventArgsPool.Instance.Release(args);
                            }
                        }
                    }
                }

                #endregion

                #region Touchpanel Interaction

                //if (TouchEnabled)
                //{
                //    TouchCollection touchPoints = TouchPanel.GetState();
                //    foreach (var touchPoint in touchPoints)
                //    {
                //        Point point = touchPoint.Position.ToPoint();
                //        TouchEventArgs args = new TouchEventArgs()
                //        {
                //            TouchId = touchPoint.Id,
                //            GlobalPosition = point,
                //            LocalPosition = point
                //        };

                //        switch (touchPoint.State)
                //        {
                //            case TouchLocationState.Pressed:
                //                root.InternalTouchDown(args);
                //                if (!args.Handled && TouchDown != null)
                //                    TouchDown(args);
                //                break;
                //            case TouchLocationState.Moved:

                //                // Touch Move
                //                root.InternalTouchMove(args);
                //                if (!args.Handled && TouchMove != null)
                //                    TouchMove(args);

                //                // Start Dragging
                //                if (DraggingArgs == null)
                //                {
                //                    DraggingArgs = new DragEventArgs()
                //                    {
                //                        GlobalPosition = point,
                //                        LocalPosition = point,
                //                    };

                //                    draggingId = touchPoint.Id;

                //                    root.InternalStartDrag(DraggingArgs);
                //                    if (!DraggingArgs.Handled && StartDrag != null)
                //                        StartDrag(DraggingArgs);
                //                }

                //                // Drop move
                //                if (DraggingArgs != null &&
                //                    draggingId == touchPoint.Id &&
                //                    DraggingArgs.Handled)
                //                {
                //                    DragEventArgs moveArgs = new DragEventArgs()
                //                    {
                //                        GlobalPosition = point,
                //                        LocalPosition = point,
                //                        Content = DraggingArgs.Content,
                //                        Icon = DraggingArgs.Icon,
                //                        Sender = DraggingArgs.Sender
                //                    };

                //                    root.InternalDropMove(moveArgs);
                //                    if (!args.Handled && DropMove != null)
                //                        DropMove(moveArgs);
                //                }

                //                break;
                //            case TouchLocationState.Released:

                //                // Handle Drop
                //                if (DraggingArgs != null &&
                //                    draggingId == touchPoint.Id &&
                //                    DraggingArgs.Handled)
                //                {
                //                    DragEventArgs dropArgs = new DragEventArgs()
                //                    {
                //                        GlobalPosition = point,
                //                        LocalPosition = point,
                //                        Content = DraggingArgs.Content,
                //                        Icon = DraggingArgs.Icon,
                //                        Sender = DraggingArgs.Sender
                //                    };

                //                    root.InternalEndDrop(dropArgs);
                //                    if (!args.Handled && EndDrop != null)
                //                        EndDrop(dropArgs);
                //                }

                //                // Discard Dragging Infos
                //                DraggingArgs = null;
                //                draggingId = null;

                //                // Left mouse button released
                //                TouchEventArgs tapArgs = new TouchEventArgs
                //                {
                //                    TouchId = touchPoint.Id,
                //                    GlobalPosition = point,
                //                    LocalPosition = point
                //                };

                //                root.InternalTouchTap(tapArgs);
                //                if (!tapArgs.Handled && TouchTap != null)
                //                    TouchTap(tapArgs);

                //                if (lastTouchTap.HasValue &&
                //                gameTime.TotalGameTime - lastLeftClick.Value < TimeSpan.FromMilliseconds(DoubleClickDelay))
                //                {
                //                    // Double Tap
                //                    TouchEventArgs doubleTapArgs = new TouchEventArgs
                //                    {
                //                        TouchId = touchPoint.Id,
                //                        GlobalPosition = point,
                //                        LocalPosition = point
                //                    };

                //                    root.InternalTouchDoubleTap(doubleTapArgs);
                //                    if (!doubleTapArgs.Handled && TouchDoubleTap != null)
                //                        TouchDoubleTap(doubleTapArgs);

                //                    lastTouchTap = null;
                //                }
                //                else
                //                {
                //                    lastTouchTap = gameTime.TotalGameTime;
                //                }

                //                root.InternalTouchUp(args);
                //                if (!args.Handled && TouchUp != null)
                //                    TouchUp(args);
                //                break;
                //        }
                //    }
                //}

                #endregion
            }

            #region Recalculate Sizes

            if (_root.HasInvalidDimensions())
            {
                Point available = new Point(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
                Point required = _root.GetExpectedSize(available);
                _root.SetActualSize(available);
            }

            _root.Update(gameTime);

            #endregion

            #region Form anpassen


            if (_titleDirty || (ActiveScreen?.Title != _lastActiveScreenTitle))
            {
                string screenTitle = ActiveScreen?.Title ?? string.Empty;
                string windowTitle = TitlePrefix + (string.IsNullOrEmpty(screenTitle) ? string.Empty : " - " + screenTitle);

                if (Game.RenderingSurface is Window window && window.Title != windowTitle)
                    window.Title = windowTitle;

                _titleDirty = false;
                _lastActiveScreenTitle = ActiveScreen?.Title;
            }

            #endregion
        }

        /// <summary>
        /// Draws the screens und controls.
        /// </summary>
        /// <param name="gameTime">Contains the elapsed time since the last draw, as well as total elapsed time.</param>
        public override void Draw(GameTime gameTime)
        {
            _root.PreDraw(gameTime);
            _root.Draw(_batch, GraphicsDevice.Viewport.Bounds, gameTime);

            // Drag Overlay
            if (DraggingArgs != null && DraggingArgs.Handled && DraggingArgs.Icon != null)
            {
                _batch.Begin();
                if (DraggingArgs.IconSize != Point.Zero)
                    _batch.Draw(DraggingArgs.Icon, new Rectangle(_lastMousePosition, DraggingArgs.IconSize), Color.White);
                else
                    _batch.Draw(DraggingArgs.Icon, new Vector2(_lastMousePosition.X, _lastMousePosition.Y), Color.White);
                _batch.End();
            }
        }

        private readonly List<Screen> _historyStack = new List<Screen>();

        /// <summary>
        /// Gets a value indicating whether <see cref="NavigateBack"/> can be executed.
        /// </summary>
        public bool CanGoBack => _historyStack.Count > 1;

        private Screen? _activeScreen = null;
        private bool _titleDirty;
        private string? _lastActiveScreenTitle;
        private string _titlePrefix = string.Empty;

        /// <summary>
        /// Gets a reference to the currently active <see cref="Screen"/>.
        /// </summary>
        public Screen? ActiveScreen
        {
            get => _activeScreen;
            private set
            {
                if (_activeScreen != value)
                {
                    _activeScreen = value;
                    _titleDirty = true;
                }
            }
        }

        /// <summary>
        /// Gets a list of the navigation <see cref="Screen"/> history used to navigate back and forth.
        /// </summary>
        public IEnumerable<Screen> History => _historyStack;

        /// <summary>
        /// Navigates to the <see cref="Screen"/>.
        /// </summary>
        /// <param name="screen">The <see cref="Screen"/> to navigate to.</param>
        /// <param name="parameter">A parameter passed to the screen on navigation.</param>
        /// <returns>Gets a value indicating whether the navigation was executed.</returns>
        public bool NavigateToScreen(Screen screen, object? parameter = null)
        {
            return Navigate(screen, false, parameter);
        }

        /// <summary>
        /// Navigates to the previous <see cref="Screen"/> in the <see cref="History"/> if possible.
        /// </summary>
        /// <param name="parameter">A parameter passed to the screen on navigation.</param>
        /// <returns>Gets a value indicating whether the navigation was executed.</returns>
        public bool NavigateBack(object? parameter = null)
        {
            if (CanGoBack)
            {
                if (ActiveScreen != null)
                    _historyStack.Remove(ActiveScreen);
                Screen screen = _historyStack[0];
                Navigate(screen, true, parameter);
            }

            return false;
        }

        private bool Navigate(Screen? screen, bool isBackNavigation, object? parameter = null)
        {
            bool overlayed = false;

            _titleDirty = true;
            var args = new NavigationEventArgs()
            {
                IsBackNavigation = isBackNavigation,
                Parameter = parameter,
                Screen = screen,
            };

            // Step 1: "deregister" previous screen
            if (ActiveScreen != null)
            {
                overlayed = ActiveScreen.IsOverlay;

                ActiveScreen.InternalNavigateFrom(args);

                // previous screen canceled navigation
                if (args.Cancel) { return false; }

                // deactivate previous screen
                ActiveScreen.IsActiveScreen = false;

                // Special case (active screen not in history, new Screen Overlay)
                if (!ActiveScreen.InHistory && screen != null && screen.IsOverlay)
                    _historyStack.Insert(0, ActiveScreen);

                // transition out, if the new screen is not an overlay
                if (screen == null || !screen.IsOverlay)
                {
                    // transition, if necessary
                    if (NavigateFromTransition != null)
                    {
                        ActiveScreen.Alpha = 1f;
                        var trans = NavigateFromTransition.Clone(ActiveScreen);
                        trans.Finished += (s, e) =>
                        {
                            ScreenTarget.Controls.Remove(e);
                            ((Screen)e).IsVisibleScreen = false;
                        };
                        ActiveScreen.StartTransition(trans);
                    }
                    else
                    {
                        ScreenTarget.Controls.Remove(ActiveScreen);
                        ActiveScreen.IsVisibleScreen = false;
                    }
                }

                // Call NavigatedFrom event
                args.Cancel = false;
                args.Handled = false;
                args.IsBackNavigation = isBackNavigation;
                args.Screen = screen;
                ActiveScreen.InternalNavigatedFrom(args);

                // remove
                args.Screen = ActiveScreen;
                ActiveScreen = null;
            }
            else args.Screen = null;

            // Step 2: navigate to the new screen
            if (screen != null)
            {
                // Call NavigateTo event
                args.Cancel = false;
                args.Handled = false;
                args.IsBackNavigation = isBackNavigation;
                screen.InternalNavigateTo(args);

                // Add new screen to history
                if (_historyStack.Contains(screen))
                    _historyStack.Remove(screen);
                if (screen.InHistory)
                    _historyStack.Insert(0, screen);

                if (!overlayed)
                {
                    if (NavigateToTransition != null)
                    {
                        screen.Alpha = 0f;
                        var trans = NavigateToTransition.Clone(screen);
                        screen.StartTransition(trans);
                    }
                    screen.IsVisibleScreen = true;
                    ScreenTarget.Controls.Add(screen);
                }

                ActiveScreen = screen;
                ActiveScreen.IsActiveScreen = true;

                // apply default Mouse Mode
                MouseMode = ActiveScreen.DefaultMouseMode;

                // call Navigate events
                args.Cancel = false;
                args.Handled = false;
                args.IsBackNavigation = isBackNavigation;
                ActiveScreen.InternalNavigatedTo(args);
            }

            return true;
        }

        /// <summary>
        /// Navigates back to the initial screen.
        /// </summary>
        public void NavigateHome()
        {
            while (CanGoBack)
                NavigateBack();
        }

        /// <summary>
        /// Opens a flyout.
        /// </summary>
        /// <param name="control">The control to flyout.</param>
        /// <param name="position">The position for the control.</param>
        public void Flyout(Control control, Point position)
        {
            _flyout.AddControl(control, position);
        }

        /// <summary>
        /// Closes a flyout.
        /// </summary>
        /// <param name="control">The control to remove from the flyout.</param>
        public void Flyback(Control control)
        {
            _flyout.RemoveControl(control);
        }

        /// <summary>
        /// Captures the mouse.
        /// <remarks>Sets <see cref="MouseMode"/> to <see cref="UI.MouseMode.Captured"/>.</remarks>
        /// </summary>
        public void CaptureMouse()
        {
            MouseMode = MouseMode.Captured;
        }
        
        /// <summary>
        /// Frees the mouse.
        /// <remarks>Sets <see cref="MouseMode"/> to <see cref="UI.MouseMode.Free"/>.</remarks>
        /// </summary>
        public void FreeMouse()
        {
            MouseMode = MouseMode.Free;
        }

        /// <summary>
        /// Occurs when the mouse was moved on this <see cref="BaseScreenComponent"/>.
        /// </summary>
        public event MouseEventBaseDelegate? MouseMove;

        /// <summary>
        /// Occurs when a drag event was started on this <see cref="BaseScreenComponent"/>.
        /// </summary>
        public event DragEventBaseDelegate? StartDrag;

        /// <summary>
        /// Occurs when the mouse was moved while dragging on this <see cref="BaseScreenComponent"/>.
        /// </summary>
        public event DragEventBaseDelegate? DropMove;

        /// <summary>
        /// Occurs when the drag event was stopped on this <see cref="BaseScreenComponent"/>.
        /// </summary>
        public event DragEventBaseDelegate? EndDrop;

        /// <summary>
        /// Occurs when the left mouse button was released on this <see cref="BaseScreenComponent"/>.
        /// </summary>
        public event MouseEventBaseDelegate? LeftMouseUp;

        /// <summary>
        /// Occurs when the left mouse button was pressed on this <see cref="BaseScreenComponent"/>.
        /// </summary>
        public event MouseEventBaseDelegate? LeftMouseDown;

        /// <summary>
        /// Occurs when the left mouse button was clicked on this <see cref="BaseScreenComponent"/>.
        /// </summary>
        public event MouseEventBaseDelegate? LeftMouseClick;

        /// <summary>
        /// Occurs when the left mouse button was double clicked on this <see cref="BaseScreenComponent"/>.
        /// </summary>
        public event MouseEventBaseDelegate? LeftMouseDoubleClick;

        /// <summary>
        /// Occurs when the right mouse button was released on this <see cref="BaseScreenComponent"/>.
        /// </summary>
        public event MouseEventBaseDelegate? RightMouseUp;

        /// <summary>
        /// Occurs when the right mouse button was pressed on this <see cref="BaseScreenComponent"/>.
        /// </summary>
        public event MouseEventBaseDelegate? RightMouseDown;

        /// <summary>
        /// Occurs when the right mouse button was clicked on this <see cref="BaseScreenComponent"/>.
        /// </summary>
        public event MouseEventBaseDelegate? RightMouseClick;

        /// <summary>
        /// Occurs when the right mouse button was double clicked on this <see cref="BaseScreenComponent"/>.
        /// </summary>
        public event MouseEventBaseDelegate? RightMouseDoubleClick;

        /// <summary>
        /// Occurs when the right mouse scroll wheel was scrolled on this <see cref="BaseScreenComponent"/>.
        /// </summary>
        public event MouseScrollEventBaseDelegate? MouseScroll;

        //public event TouchEventBaseDelegate? TouchDown;

        //public event TouchEventBaseDelegate? TouchMove;

        //public event TouchEventBaseDelegate? TouchUp;

        //public event TouchEventBaseDelegate? TouchTap;

        //public event TouchEventBaseDelegate? TouchDoubleTap;

        /// <summary>
        /// Occurs when a key on the keyboard was pressed on this <see cref="BaseScreenComponent"/>.
        /// </summary>
        public event KeyEventBaseDelegate? KeyDown;

        /// <summary>
        /// Occurs when a key on the keyboard is pressed on this <see cref="BaseScreenComponent"/>.
        /// </summary>
        public event KeyEventBaseDelegate? KeyPress;

        /// <summary>
        /// Occurs when a key on the keyboard was released on this <see cref="BaseScreenComponent"/>.
        /// </summary>
        public event KeyEventBaseDelegate? KeyUp;

        /// <summary>
        /// Occurs when the client(e.g. a window) size was changed.
        /// </summary>
        public event EventHandler? ClientSizeChanged;
    }
}