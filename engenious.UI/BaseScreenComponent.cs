﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using engenious.Content;
using engenious.Graphics;
using engenious.Input;
using engenious.UI.Controls;
using Vector2 = engenious.Vector2;

namespace engenious.UI
{
    /// <summary>
    /// Basisklasse für alle MonoGame-Komponenten
    /// </summary>
    public class BaseScreenComponent : DrawableGameComponent
    {
        /// <summary>
        /// Maximaler Standard Delay zwischen zwei Clicks innerhalb eines Double Clicks.
        /// </summary>
        public const int DEFAULTDOUBLECLICKDELAY = 500;

        private ContainerControl root;

        private FlyoutControl flyout;

        private SpriteBatch batch;

        private MouseMode mouseMode;

        struct InvokeAction
        {
            public InvokeAction(Action action, ManualResetEvent resetEvent)
            {
                Action = action;
                ResetEvent = resetEvent;
            }

            public Action Action { get; }
            public ManualResetEvent ResetEvent { get; }
        }

        private ConcurrentQueue<InvokeAction> invokes = new ConcurrentQueue<InvokeAction>();

        /// <summary>
        /// Prefix für die Titel-Leiste
        /// </summary>
        public string TitlePrefix
        {
            get { return _titlePrefix; }
            set
            {
                _titlePrefix = value;
                _titleDirty = true;
            }
        }

        /// <summary>
        /// Gibt das Root-Control zurück oder legt dieses fest.
        /// </summary>
        public ContentControl Frame { get; private set; }

        /// <summary>
        /// Gibt das Control an, das zum navigieren der Screens verwendet wird.
        /// </summary>
        public ContainerControl ScreenTarget { get; set; }

        /// <summary>
        /// Legt das Standard-Template für eine Navigation zu einem Screen fest.
        /// </summary>
        public Transition NavigateToTransition { get; set; }

        /// <summary>
        /// Legt das Standard-Template für eine Navigation von einem Screen weg fest.
        /// </summary>
        public Transition NavigateFromTransition { get; set; }

        /// <summary>
        /// Referenz zum MonoGame Content Manager.
        /// </summary>
        public ContentManager Content { get; private set; }

        /// <summary>
        /// Gibt an, ob gerade ein Drag-Vorgang im Gange ist.
        /// </summary>
        public bool Dragging { get { return DraggingArgs != null && DraggingArgs.Handled; } }

        /// <summary>
        /// Legt fest, ob es GamePad Support geben soll (nicht unterstützt bisher)
        /// </summary>
        public bool GamePadEnabled { get; set; }

        /// <summary>
        /// Legt fest, ob es Maus Support geben soll
        /// </summary>
        public bool MouseEnabled { get; set; }

        /// <summary>
        /// Legt fest, ob es Touch Support geben soll.
        /// </summary>
        //public bool TouchEnabled { get; set; }

        /// <summary>
        /// Legt fest, ob es Keyboard Support geben soll.
        /// </summary>
        public bool KeyboardEnabled { get; set; }

        /// <summary>
        /// Gibt den maximalen Zeitraum in Millisekunden zwischen zwei Clicks an um einen Double Click auszulösen oder legt diesen fest.
        /// </summary>
        public int DoubleClickDelay { get; set; }

        /// <summary>
        /// Gibt den aktuellen Modus der Maus zurück.
        /// </summary>
        public MouseMode MouseMode
        {
            get
            {
                return mouseMode;
            }
            private set
            {
                if (mouseMode != value)
                {
                    mouseMode = value;
                    Game.IsMouseVisible = (mouseMode != MouseMode.Captured);
                    Game.IsCursorGrabbed = (mouseMode == MouseMode.Captured);

                    if (mouseMode == MouseMode.Free)
                        Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
                    else
                        resetMouse = true;
                }
            }
        }

        /// <summary>
        /// Reset (ignore) mouse-position. Used to compensate first movement after mouse-capture.
        /// </summary>
        private bool resetMouse = false;

        /// <summary>
        /// Erzeugt eine neue Instanz der Klasse BaseScreenComponent.
        /// </summary>
        /// <param name="game">Die aktuelle Game-Instanz.</param>
        public BaseScreenComponent(IGame game)
            : base(game)
        {
            Content = game.Content;

            KeyboardEnabled = true;
            MouseEnabled = true;
            GamePadEnabled = true;
            //TouchEnabled = true;
            DoubleClickDelay = DEFAULTDOUBLECLICKDELAY;

            _pressedKeys = ((Keys[]) Enum.GetValues(typeof(Keys))).Where(x => (int)x != 0).Select(k => (int)k).Distinct().Select(idx => UnpressedKeyTimestamp).ToArray();

#if !ANDROID

            Game.KeyPress += (s, e) =>
            {
                if (Game.IsActive)
                {
                    KeyTextEventArgs args = new KeyTextEventArgs() { Character = e };

                    root.InternalKeyTextPress(args);
                }
            };

#endif

            Game.Resized += (s, e) =>
            {
                if (ClientSizeChanged != null)
                    ClientSizeChanged(s, e);
            };
        }

        /// <summary>
        /// Lädt die für MonoGameUI notwendigen Content-Dateien.
        /// </summary>
        protected override void LoadContent()
        {
            Skin.Pix = new Texture2D(GraphicsDevice, 1, 1);
            Skin.Pix.SetData<Color>(stackalloc [] { Color.White });

            Skin.Current = new Skin(Content);
            batch = new SpriteBatch(GraphicsDevice);

            root = new ContainerControl(this)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            Frame = new ContentControl(this)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            root.Controls.Add(Frame);

            ContainerControl screenContainer = new ContainerControl(this)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            Frame.Content = screenContainer;
            ScreenTarget = screenContainer;

            flyout = new FlyoutControl(this)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            root.Controls.Add(flyout);
        }

        private bool lastLeftMouseButtonPressed = false;

        private bool lastRightMouseButtonPressed = false;

        private int lastMouseWheelValue = 0;

        private Point lastMousePosition = Point.Zero;

        private TimeSpan? lastLeftClick = null;

        private TimeSpan? lastRightClick = null;

        //private TimeSpan? lastTouchTap = null;

        private int? draggingId = null;

        internal DragEventArgs DraggingArgs { get; private set; }

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
            invokes.Enqueue(new InvokeAction(invokedAction, resetEvent));
            resetEvent.WaitOne();
            resetEvent.Dispose();
        }

        /// <summary>
        /// Handling aller Eingaben, Mausbewegungen und Updaten aller Screens und Controls.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            while(invokes.TryDequeue(out var act))
            {
                act.Action.Invoke();
                act.ResetEvent.Set();
            }

            if (Game.IsActive)
            {

                #region Mouse Interaction

                if (MouseEnabled)
                {
                    // Mausposition anhand des Mouse Modes ermitteln
                    MouseState mouse;
                    Point mousePosition;
                    if (MouseMode == MouseMode.Captured && !resetMouse)
                    {

                        mouse = Mouse.GetState();
                        mousePosition = new Point(
                            mouse.X - (lastMousePosition.X),
                            mouse.Y - (lastMousePosition.Y));
                    }
                    else if(resetMouse)
                    {
                        Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
                        lastMousePosition = new Point(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
                        mouse = Mouse.GetState();
                        mousePosition = new Point();
                        resetMouse = false;
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
                    if (mousePosition != lastMousePosition)
                    {
                        mouseEventArgs.Handled = false;

                        root.InternalMouseMove(mouseEventArgs);
                        if (!mouseEventArgs.Handled)
                            MouseMove?.Invoke(mouseEventArgs);

                        // Start Drag Handling
                        if (mouse.LeftButton == ButtonState.Pressed &&
                            DraggingArgs == null)
                        {
                            DraggingArgs = DragEventArgsPool.Instance.Take();
                            DraggingArgs.GlobalPosition = mousePosition;
                            DraggingArgs.LocalPosition = mousePosition;

                            draggingId = null;

                            root.InternalStartDrag(DraggingArgs);
                            if (!DraggingArgs.Handled)
                                StartDrag?.Invoke(DraggingArgs);
                        }

                        // Drop move
                        if (mouse.LeftButton == ButtonState.Pressed &&
                            DraggingArgs != null &&
                            draggingId == null &&
                            DraggingArgs.Handled)
                        {
                            //TODO: perhaps pool single object?
                            DragEventArgs args = DragEventArgsPool.Instance.Take();

                            args.GlobalPosition = mousePosition;
                            args.LocalPosition = mousePosition;
                            args.Content = DraggingArgs.Content;
                            args.Icon = DraggingArgs.Icon;
                            args.Sender = DraggingArgs.Sender;

                            root.InternalDropMove(args);
                            if (!args.Handled)
                                DropMove?.Invoke(args);
                        }
                    }

                    // Linke Maustaste
                    if (mouse.LeftButton == ButtonState.Pressed)
                    {
                        if (!lastLeftMouseButtonPressed)
                        {
                            mouseEventArgs.Handled = false;

                            // Linke Maustaste wurde neu gedrückt
                            root.InternalLeftMouseDown(mouseEventArgs);
                            if (!mouseEventArgs.Handled)
                                LeftMouseDown?.Invoke(mouseEventArgs);
                        }
                        lastLeftMouseButtonPressed = true;
                    }
                    else
                    {
                        if (lastLeftMouseButtonPressed)
                        {
                            // Handle Drop
                            if (DraggingArgs != null && DraggingArgs.Handled)
                            {
                                DragEventArgs args = DragEventArgsPool.Instance.Take();
                                args.GlobalPosition = mousePosition;
                                args.LocalPosition = mousePosition;
                                args.Content = DraggingArgs.Content;
                                args.Icon = DraggingArgs.Icon;
                                args.Sender = DraggingArgs.Sender;

                                root.InternalEndDrop(args);
                                if (!args.Handled)
                                    EndDrop?.Invoke(args);
                            }

                            // Discard Dragging Infos
                            DragEventArgsPool.Instance.Release(DraggingArgs);
                            DraggingArgs = null;
                            draggingId = null;

                            // Linke Maustaste wurde losgelassen
                            mouseEventArgs.Handled = false;

                            root.InternalLeftMouseClick(mouseEventArgs);
                            if (!mouseEventArgs.Handled)
                                LeftMouseClick?.Invoke(mouseEventArgs);

                            if (lastLeftClick.HasValue &&
                                gameTime.TotalGameTime - lastLeftClick.Value < TimeSpan.FromMilliseconds(DoubleClickDelay))
                            {
                                // Double Left Click
                                mouseEventArgs.Handled = false;

                                root.InternalLeftMouseDoubleClick(mouseEventArgs);
                                if (!mouseEventArgs.Handled)
                                    LeftMouseDoubleClick?.Invoke(mouseEventArgs);

                                lastLeftClick = null;
                            }
                            else
                            {
                                lastLeftClick = gameTime.TotalGameTime;
                            }

                            // Mouse Up
                            mouseEventArgs.Handled = false;

                            root.InternalLeftMouseUp(mouseEventArgs);
                            if (!mouseEventArgs.Handled)
                                LeftMouseUp?.Invoke(mouseEventArgs);
                        }
                        lastLeftMouseButtonPressed = false;
                    }

                    // Rechte Maustaste
                    if (mouse.RightButton == ButtonState.Pressed)
                    {
                        if (!lastRightMouseButtonPressed)
                        {
                            // Rechte Maustaste neu gedrückt
                            mouseEventArgs.Handled = false;

                            root.InternalRightMouseDown(mouseEventArgs);
                            if (!mouseEventArgs.Handled)
                                RightMouseDown?.Invoke(mouseEventArgs);
                        }
                        lastRightMouseButtonPressed = true;
                    }
                    else
                    {
                        if (lastRightMouseButtonPressed)
                        {
                            // Rechte Maustaste losgelassen
                            mouseEventArgs.Handled = false;
                            root.InternalRightMouseClick(mouseEventArgs);
                            if (!mouseEventArgs.Handled)
                                RightMouseClick?.Invoke(mouseEventArgs);

                            if (lastRightClick.HasValue &&
                                gameTime.TotalGameTime - lastRightClick.Value < TimeSpan.FromMilliseconds(DoubleClickDelay))
                            {
                                // Double Left Click
                                mouseEventArgs.Handled = false;

                                root.InternalRightMouseDoubleClick(mouseEventArgs);
                                if (!mouseEventArgs.Handled)
                                    RightMouseDoubleClick?.Invoke(mouseEventArgs);

                                lastRightClick = null;
                            }
                            else
                            {
                                lastRightClick = gameTime.TotalGameTime;
                            }

                            mouseEventArgs.Handled = false;

                            root.InternalRightMouseUp(mouseEventArgs);
                            if (!mouseEventArgs.Handled)
                                RightMouseUp?.Invoke(mouseEventArgs);
                        }
                        lastRightMouseButtonPressed = false;
                    }

                    // Mousewheel
                    if (lastMouseWheelValue != mouse.ScrollWheelValue)
                    {
                        int diff = (mouse.ScrollWheelValue - lastMouseWheelValue);

                        MouseScrollEventArgs scrollArgs = new MouseScrollEventArgs
                        {
                            MouseMode = MouseMode,
                            GlobalPosition = mousePosition,
                            LocalPosition = mousePosition,
                            Steps = diff
                        };
                        root.InternalMouseScroll(scrollArgs);
                        if (!scrollArgs.Handled)
                            MouseScroll?.Invoke(scrollArgs);

                        lastMouseWheelValue = mouse.ScrollWheelValue;
                    }

                    // Potentieller Positionsreset
                    if (MouseMode == MouseMode.Free)
                    {
                        lastMousePosition = mousePosition;
                    }
                    else if (mousePosition.X != 0 || mousePosition.Y != 0)
                    {
                        lastMousePosition = mouse.Location;
                    }

                    MouseEventArgsPool.Instance.Release(mouseEventArgs);
                }

                #endregion

                #region Keyboard Interaction

                if (KeyboardEnabled)
                {
                    KeyboardState keyboard = Keyboard.GetState();

                    bool shift = keyboard.IsKeyDown(Keys.LeftShift) | keyboard.IsKeyDown(Keys.RightShift);
                    bool ctrl = keyboard.IsKeyDown(Keys.LeftControl) | keyboard.IsKeyDown(Keys.RightControl);
                    bool alt = keyboard.IsKeyDown(Keys.LeftAlt) | keyboard.IsKeyDown(Keys.RightAlt);

                    KeyEventArgs args;

                    for (int i = 0; i < _pressedKeys.Length; i++)
                    {
                        var key = (Keys) (i + 1); 
                        if (keyboard.IsKeyDown(key))
                        {
                            // ReSharper disable once CompareOfFloatsByEqualityOperator
                            if (_pressedKeys[i] == UnpressedKeyTimestamp)
                            {
                                // Taste ist neu

                                args = KeyEventArgsPool.Take();

                                args.Key = key;
                                args.Shift = shift;
                                args.Ctrl = ctrl;
                                args.Alt = alt;

                                root.InternalKeyDown(args);

                                if (!args.Handled)
                                {
                                    KeyDown?.Invoke(args);
                                }

                                KeyEventArgsPool.Release(args);

                                args = KeyEventArgsPool.Take();

                                args.Key = key;
                                args.Shift = shift;
                                args.Ctrl = ctrl;
                                args.Alt = alt;

                                root.InternalKeyPress(args);
                                _pressedKeys[i] = gameTime.TotalGameTime.TotalMilliseconds + 500;

                                KeyEventArgsPool.Release(args);

                                // Spezialfall Tab-Taste (falls nicht verarbeitet wurde)
                                if (key == Keys.Tab && !args.Handled)
                                {
                                    if (shift) root.InternalTabbedBackward();
                                    else root.InternalTabbedForward();
                                }
                            }
                            else
                            {
                                // Taste ist immernoch gedrückt
                                if (_pressedKeys[i] <= gameTime.TotalGameTime.TotalMilliseconds)
                                {
                                    args = KeyEventArgsPool.Take();

                                    args.Key = key;
                                    args.Shift = shift;
                                    args.Ctrl = ctrl;
                                    args.Alt = alt;


                                    root.InternalKeyPress(args);
                                    if (!args.Handled)
                                    {
                                        KeyPress?.Invoke(args);
                                    }

                                    KeyEventArgsPool.Release(args);

                                    _pressedKeys[i] = gameTime.TotalGameTime.TotalMilliseconds + 50;
                                }
                            }
                        }
                        else
                        {
                            // ReSharper disable once CompareOfFloatsByEqualityOperator
                            if (_pressedKeys[i] != UnpressedKeyTimestamp)
                            {
                                // Taste losgelassen
                                args = KeyEventArgsPool.Take();

                                args.Key = key;
                                args.Shift = shift;
                                args.Ctrl = ctrl;
                                args.Alt = alt;

                                root.InternalKeyUp(args);
                                _pressedKeys[i] = UnpressedKeyTimestamp;

                                if (!args.Handled)
                                {
                                    KeyUp?.Invoke(args);
                                }

                                KeyEventArgsPool.Release(args);
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

                //                // Linke Maustaste wurde losgelassen
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

            if (root.HasInvalidDimensions())
            {
                Point available = new Point(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
                Point required = root.GetExpectedSize(available);
                root.SetActualSize(available);
            }

            root.Update(gameTime);

            #endregion

            #region Form anpassen


            if (_titleDirty || (ActiveScreen?.Title != _lastActiveScreenTitle))
            {
                string screentitle = ActiveScreen?.Title ?? string.Empty;
                string windowtitle = TitlePrefix + (string.IsNullOrEmpty(screentitle) ? string.Empty : " - " + screentitle);

                var window = Game.RenderingSurface as Window;

                if (window != null && window.Title != windowtitle)
                    window.Title = windowtitle;

                _titleDirty = false;
                _lastActiveScreenTitle = ActiveScreen?.Title;
            }

            #endregion
        }

        /// <summary>
        /// Zeichnet Screens und Controls.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            root.PreDraw(gameTime);
            root.Draw(batch, GraphicsDevice.Viewport.Bounds, gameTime);

            // Drag Overlay
            if (DraggingArgs != null && DraggingArgs.Handled && DraggingArgs.Icon != null)
            {
                batch.Begin();
                if (DraggingArgs.IconSize != Point.Zero)
                    batch.Draw(DraggingArgs.Icon, new Rectangle(lastMousePosition, DraggingArgs.IconSize), Color.White);
                else
                    batch.Draw(DraggingArgs.Icon, new Vector2(lastMousePosition.X, lastMousePosition.Y), Color.White);
                batch.End();
            }
        }

        private List<Screen> historyStack = new List<Screen>();

        /// <summary>
        /// Gibt an ob der aktuelle History Stack eine Navigation Back-Navigation erlaubt.
        /// </summary>
        public bool CanGoBack { get { return historyStack.Count > 1; } }

        private Screen activeScreen = null;
        private bool _titleDirty;
        private string _lastActiveScreenTitle;
        private string _titlePrefix;

        /// <summary>
        /// Referenz auf den aktuellen Screen.
        /// </summary>
        public Screen ActiveScreen
        {
            get
            {
                return activeScreen;
            }
            private set
            {
                if (activeScreen != value)
                {
                    activeScreen = value;
                    _titleDirty = true;
                }
            }
        }

        /// <summary>
        /// Liste der History.
        /// </summary>
        public IEnumerable<Screen> History { get { return historyStack; } }

        /// <summary>
        /// Navigiert den Screen Manager zum angegebenen Screen.
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="parameter">Ein Parameter für den neuen Screen.</param>
        /// <returns>Gibt an ob die Navigation durchgeführt wurde.</returns>
        public bool NavigateToScreen(Screen screen, object parameter = null)
        {
            return Navigate(screen, false, parameter);
        }

        /// <summary>
        /// Navigiert zurück, sofern möglich.
        /// </summary>
        /// <returns>Gibt an ob die Navigation durchgeführt wurde.</returns>
        public bool NavigateBack()
        {
            if (CanGoBack)
            {
                historyStack.Remove(ActiveScreen);
                Screen screen = historyStack[0];
                Navigate(screen, true, null);
            }

            return false;
        }

        private bool Navigate(Screen screen, bool isBackNavigation, object parameter = null)
        {
            bool overlayed = false;

            _titleDirty = true;
            // Navigation ankündigen und prüfen, ob das ok geht.
            var args = new NavigationEventArgs()
            {
                IsBackNavigation = isBackNavigation,
                Parameter = parameter,
                Screen = screen,
            };

            // Schritt 1: Vorherigen Screen "abmelden"
            if (ActiveScreen != null)
            {
                overlayed = ActiveScreen.IsOverlay;

                ActiveScreen.InternalNavigateFrom(args);

                // Abbruch durch Screen eingeleitet
                if (args.Cancel) { return false; }

                // Screen deaktivieren
                ActiveScreen.IsActiveScreen = false;

                // Spezialfall (aktueller Screen nicht in History, neuer Screen Overlay)
                if (!ActiveScreen.InHistory && screen != null && screen.IsOverlay)
                    historyStack.Insert(0, ActiveScreen);

                // Ausblenden, wenn der neue Screen nicht gerade ein Overlay ist
                if (screen == null || !screen.IsOverlay)
                {
                    // Überblenden, falls erforderlich
                    if (NavigateFromTransition != null)
                    {
                        ActiveScreen.Alpha = 1f;
                        var trans = NavigateFromTransition.Clone(ActiveScreen);
                        trans.Finished += (s, e) =>
                        {
                            ScreenTarget.Controls.Remove(e);
                            ((Screen)e).IsVisibleScreen = false;
                        };
                        activeScreen.StartTransition(trans);
                    }
                    else
                    {
                        ScreenTarget.Controls.Remove(ActiveScreen);
                        ActiveScreen.IsVisibleScreen = false;
                    }
                }

                // NavigatedFrom-Event aufrufen
                args.Cancel = false;
                args.Handled = false;
                args.IsBackNavigation = isBackNavigation;
                args.Screen = screen;
                ActiveScreen.InternalNavigatedFrom(args);

                // entfernen
                args.Screen = ActiveScreen;
                ActiveScreen = null;
            }
            else args.Screen = null;

            // Schritt 2: zum neuen Screen navigieren
            if (screen != null)
            {
                // NavigateTo-Event aufrufen
                args.Cancel = false;
                args.Handled = false;
                args.IsBackNavigation = isBackNavigation;
                screen.InternalNavigateTo(args);

                // Neuen Screen einhängen
                if (historyStack.Contains(screen))
                    historyStack.Remove(screen);
                if (screen.InHistory)
                    historyStack.Insert(0, screen);

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

                // Default Mouse Mode anwenden
                MouseMode = ActiveScreen.DefaultMouseMode;

                // Navigate-Events aufrufen
                args.Cancel = false;
                args.Handled = false;
                args.IsBackNavigation = isBackNavigation;
                ActiveScreen.InternalNavigatedTo(args);
            }

            return true;
        }

        /// <summary>
        /// Navigiert bis zum ersten Screen zurück.
        /// </summary>
        public void NavigateHome()
        {
            while (CanGoBack)
                NavigateBack();
        }

        /// <summary>
        /// Öffnet ein Flyout.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="position"></param>
        public void Flyout(Control control, Point position)
        {
            flyout.AddControl(control, position);
        }

        /// <summary>
        /// Schließt ein Flyout wieder.
        /// </summary>
        /// <param name="control"></param>
        public void Flyback(Control control)
        {
            flyout.RemoveControl(control);
        }

        /// <summary>
        /// Wechselt in den Catured Mouse Mode.
        /// </summary>
        public void CaptureMouse()
        {
            MouseMode = MouseMode.Captured;
        }

        /// <summary>
        /// Wechselt in den Free Mouse Mode.
        /// </summary>
        public void FreeMouse()
        {
            MouseMode = MouseMode.Free;
        }

        public event MouseEventBaseDelegate MouseMove;

        public event DragEventDelegate StartDrag;

        public event DragEventDelegate DropMove;

        public event DragEventDelegate EndDrop;

        public event MouseEventBaseDelegate LeftMouseUp;

        public event MouseEventBaseDelegate LeftMouseDown;

        public event MouseEventBaseDelegate LeftMouseClick;

        public event MouseEventBaseDelegate LeftMouseDoubleClick;

        public event MouseEventBaseDelegate RightMouseUp;

        public event MouseEventBaseDelegate RightMouseDown;

        public event MouseEventBaseDelegate RightMouseClick;

        public event MouseEventBaseDelegate RightMouseDoubleClick;

        public event MouseScrollEventBaseDelegate MouseScroll;

        //public event TouchEventBaseDelegate TouchDown;

        //public event TouchEventBaseDelegate TouchMove;

        //public event TouchEventBaseDelegate TouchUp;

        //public event TouchEventBaseDelegate TouchTap;

        //public event TouchEventBaseDelegate TouchDoubleTap;

        /// <summary>
        /// Event, das aufgerufen wird, wenn eine Taste gedrückt wird.
        /// </summary>
        public event KeyEventBaseDelegate KeyDown;

        /// <summary>
        /// Event, das aufgerufen wird, wenn eine Taste gedrückt ist.
        /// </summary>
        public event KeyEventBaseDelegate KeyPress;

        /// <summary>
        /// Event, das aufgerufen wird, wenn eine taste losgelassen wird.
        /// </summary>
        public event KeyEventBaseDelegate KeyUp;

        /// <summary>
        /// Event, das aufgerufen wird, wenn die Fenstergröße geändert wurde.
        /// </summary>
        public event EventHandler ClientSizeChanged;
    }
}