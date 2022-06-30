using System;
using System.Linq;

using engenious.Graphics;
using engenious.Input;

namespace engenious.UI.Controls
{
    /// <summary>
    /// Ui textbox control for text input.
    /// </summary>
    public class Textbox : ContentControl, ITextControl
    {
        private int _cursorPosition;

        private int _selectionStart;

        private readonly Label _label;

        private readonly ScrollContainer _scrollContainer;

        /// <summary>
        /// Gets or sets the current cursor position.
        /// </summary>
        public int CursorPosition
        {
            get => _cursorPosition;
            set
            {
                if (value < 0 || value > Text.Length || Font == null)
                    return;

                _cursorBlinkTime = 0;
                if (_cursorPosition != value)
                {
                    var cursorOffset = (int)Font.MeasureString(Text.Substring(0, value)).X;
                    if (cursorOffset < _scrollContainer.HorizontalScrollPosition)
                        _scrollContainer.HorizontalScrollPosition = Math.Max(0, cursorOffset);
                    else if (cursorOffset > _scrollContainer.HorizontalScrollPosition + _scrollContainer.ActualClientArea.Width)
                        _scrollContainer.HorizontalScrollPosition = Math.Max(0, cursorOffset - _scrollContainer.ActualClientArea.Width);
                    _cursorPosition = Math.Min(_label.Text.Length, value);
                    InvalidateDrawing();
                }
            }
        }

        /// <summary>
        /// Gets or sets the start of the selection range.
        /// </summary>
        public int SelectionStart
        {
            get => _selectionStart;
            set
            {
                if (_selectionStart != value)
                {
                    _selectionStart = value;
                    InvalidateDrawing();
                }
            }
        }

        /// <inheritdoc />
        public string Text { get => _label.Text; set => _label.Text = value; }

        /// <inheritdoc />
        public SpriteFont? Font { get => _label.Font; set => _label.Font = value; }

        /// <inheritdoc />
        public Color TextColor { get => _label.TextColor; set => _label.TextColor = value; }

        /// <inheritdoc />
        public HorizontalAlignment HorizontalTextAlignment { get => _label.HorizontalTextAlignment; set => _label.HorizontalTextAlignment = value; }

        /// <inheritdoc />
        public VerticalAlignment VerticalTextAlignment { get => _label.VerticalTextAlignment; set => _label.VerticalTextAlignment = value; }

        /// <inheritdoc />
        public bool WordWrap { get => _label.WordWrap; set => _label.WordWrap = value; }

        /// <inheritdoc />
        public bool LineWrap { get => _label.LineWrap; set => _label.LineWrap = value; }

        /// <inheritdoc />
        public bool FitText { get => _label.FitText; set => _label.FitText = value; }

        /// <summary>
        /// Occurs when the <see cref="Text"/> was changed.
        /// </summary>
        public event PropertyChangedDelegate<string> TextChanged
        {
            add => _label.TextChanged += value;
            remove => _label.TextChanged -= value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Textbox"/> class.
        /// </summary>
        /// <param name="style">The style to use for this control.</param>
        /// <param name="manager">The <see cref="BaseScreenComponent"/>.</param>
        public Textbox(string style = "", BaseScreenComponent? manager = null)
            : base(style, manager)
        {
            _label = new Label(style, manager)
            {
                HorizontalTextAlignment = HorizontalAlignment.Left,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Padding = Border.All(0),
                DrawFocusFrame = false
            };

            _scrollContainer = new ScrollContainer(manager: manager)
            {
                HorizontalScrollbarVisibility = ScrollbarVisibility.Never,
                VerticalScrollbarVisibility = ScrollbarVisibility.Never,
                HorizontalScrollbarEnabled = true,
                HorizontalAlignment = HorizontalAlignment.Stretch,

                Content = _label
            };
            Content = _scrollContainer;

            TabStop = true;
            CanFocus = true;

            ApplySkin(typeof(Textbox));
        }

        private int _cursorBlinkTime;
        
        /// <inheritdoc />
        protected override void OnDrawContent(SpriteBatch batch, Rectangle area, GameTime gameTime, float alpha)
        {
            if (Font == null)
                return;
            if (CursorPosition > Text.Length)
                CursorPosition = Text.Length;
            if (SelectionStart > Text.Length)
                SelectionStart = CursorPosition;

            // Selection range
            if (SelectionStart != CursorPosition)
            {
                int from = Math.Min(SelectionStart, CursorPosition);
                int to = Math.Max(SelectionStart, CursorPosition);
                var selectFrom = Font.MeasureString(Text.Substring(0, from));
                var selectTo = Font.MeasureString(Text.Substring(from, to - from));
                var rect = new Rectangle(area.X + (int)selectFrom.X - _scrollContainer.HorizontalScrollPosition, area.Y, (int)selectTo.X, (int)selectTo.Y);
                batch.Draw(Skin.Pix, rect, Color.LightBlue);
            }

            base.OnDrawContent(batch, area, gameTime, alpha);

            // Cursor (when in focus)
            if (Focused == TreeState.Active)
            {
                if (_cursorBlinkTime % 1000 < 500)
                {
                    var selectionSize = Font.MeasureString(Text.Substring(0, CursorPosition));
                    batch.Draw(Skin.Pix, new RectangleF(area.X + (int)selectionSize.X - _scrollContainer.HorizontalScrollPosition, area.Y, 1, Font.LineSpacing), TextColor);
                }
                _cursorBlinkTime += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            }
        }

        /// <inheritdoc />
        protected override void OnKeyTextPress(KeyTextEventArgs args)
        {
            // Ignore, if unfocused
            if (Focused != TreeState.Active) return;

            // Ignore control character
            if (args.Character == '\b' || args.Character == '\t' || args.Character == '\n' || args.Character == '\r')
                return;

            // Ignore ctrl hotkeys (A, X, C, V)
            if (args.Character == '\u0001' ||
                args.Character == '\u0003' ||
                args.Character == '\u0016' ||
                args.Character == '\u0018')
                return;

            // Ignore Escape
            if (args.Character == '\u001b')
                return;

            if (SelectionStart != CursorPosition)
            {
                int from = Math.Min(SelectionStart, CursorPosition);
                int to = Math.Max(SelectionStart, CursorPosition);
                Text = Text.Substring(0, from) + Text.Substring(to);
                CursorPosition = from;
                SelectionStart = from;
            }

            Text = Text.Substring(0, CursorPosition) + args.Character + Text.Substring(CursorPosition);
            CursorPosition++;
            SelectionStart++;
            args.Handled = true;
        }

        /// <inheritdoc />
        protected override void OnKeyPress(KeyEventArgs args)
        {
            // Ignore, if unfocused
            if (Focused != TreeState.Active && _scrollContainer.Focused != TreeState.Active) return;

            // Left arrow key
            if (args.Key == Keys.Left)
            {
                if (args.Ctrl)
                {
                    int x = Math.Min(CursorPosition - 1, Text.Length - 1);
                    while (x > 0 && Text[x] != ' ') x--;
                    CursorPosition = x;
                }
                else
                {
                    CursorPosition--;
                    CursorPosition = Math.Max(CursorPosition, 0);
                }

                if (!args.Shift)
                    SelectionStart = CursorPosition;
                args.Handled = true;
            }

            // Right arrow key
            if (args.Key == Keys.Right)
            {
                if (args.Ctrl)
                {
                    int x = CursorPosition;
                    while (x < Text.Length && Text[x] != ' ') x++;
                    CursorPosition = Math.Min(x + 1, Text.Length);
                }
                else
                {
                    CursorPosition++;
                    CursorPosition = Math.Min(Text.Length, CursorPosition);
                }
                if (!args.Shift)
                    SelectionStart = CursorPosition;
                args.Handled = true;
            }

            // Pos1 key
            if (args.Key == Keys.Home)
            {
                CursorPosition = 0;
                if (!args.Shift)
                    SelectionStart = CursorPosition;
                args.Handled = true;
            }

            // End key
            if (args.Key == Keys.End)
            {
                CursorPosition = Text.Length;
                if (!args.Shift)
                    SelectionStart = CursorPosition;
                args.Handled = true;
            }

            // Backspace
            if (args.Key == Keys.Back)
            {
                if (SelectionStart != CursorPosition)
                {
                    int from = Math.Min(SelectionStart, CursorPosition);
                    int to = Math.Max(SelectionStart, CursorPosition);
                    Text = Text.Substring(0, from) + Text.Substring(to);
                    CursorPosition = from;
                    SelectionStart = from;
                }
                else if (CursorPosition > 0)
                {
                    Text = Text.Substring(0, CursorPosition - 1) + Text.Substring(CursorPosition);
                    CursorPosition--;
                    SelectionStart--;
                }
                args.Handled = true;
            }

            // Del key
            if (args.Key == Keys.Delete)
            {
                if (SelectionStart != CursorPosition)
                {
                    int from = Math.Min(SelectionStart, CursorPosition);
                    int to = Math.Max(SelectionStart, CursorPosition);
                    Text = Text.Substring(0, from) + Text.Substring(to);
                    CursorPosition = from;
                    SelectionStart = from;
                }
                else if (CursorPosition < Text.Length)
                {
                    Text = Text.Substring(0, CursorPosition) + Text.Substring(CursorPosition + 1);
                }
                args.Handled = true;
            }

            // Ctrl+A (Select all)
            if (args.Key == Keys.A && args.Ctrl)
            {
                // Select everything
                SelectionStart = 0;
                CursorPosition = Text.Length;

                args.Handled = true;
            }

            // Ctrl+C (Copy)
            if (args.Key == Keys.C && args.Ctrl)
            {
                int from = Math.Min(SelectionStart, CursorPosition);
                int to = Math.Max(SelectionStart, CursorPosition);

                // Copy selection to clipboard
                if (from == to) SystemSpecific.ClearClipboard();
                else SystemSpecific.SetClipboardText(Text.Substring(from, to - from));

                args.Handled = true;
            }

            // Ctrl+X (Cut)
            if (args.Key == Keys.X && args.Ctrl)
            {
                int from = Math.Min(SelectionStart, CursorPosition);
                int to = Math.Max(SelectionStart, CursorPosition);

                // Copy selection to clipboard
                if (from == to) SystemSpecific.ClearClipboard();
                else SystemSpecific.SetClipboardText(Text.Substring(from, to - from));

                CursorPosition = from;
                SelectionStart = from;
                Text = Text.Substring(0, from) + Text.Substring(to);

                args.Handled = true;
            }

            // Ctrl+V (Paste)
            if (args.Key == Keys.V && args.Ctrl)
            {
                // Delete currently selected text
                if (SelectionStart != CursorPosition)
                {
                    int from = Math.Min(SelectionStart, CursorPosition);
                    int to = Math.Max(SelectionStart, CursorPosition);
                    Text = Text.Substring(0, from) + Text.Substring(to);
                    CursorPosition = from;
                    SelectionStart = from;
                }

                // Insert text at current position and advance cursor to lst inserted character
                string? paste = SystemSpecific.GetClipboardText();
                if (!string.IsNullOrEmpty(paste))
                {
                    Text = Text.Substring(0, CursorPosition) + paste + Text.Substring(CursorPosition);
                    CursorPosition += paste.Length;
                    SelectionStart = CursorPosition;
                }

                args.Handled = true;
            }

            args.Handled = true;

            // Passthrough ignored keys.
            if (_ignoreKeys.Contains(args.Key))
                args.Handled = false;

            base.OnKeyPress(args);
        }

        private int FindClosestPosition(Point pt)
        {
            if (Font == null) return -1;
            float oldWidth = 0;
            for (int i = 1; i <= Text.Length; i++)
            {
                var substr = Text.Substring(0, i);
                var measurement = Font.MeasureString(substr);
                //oldWidth += (measurement.X - oldWidth) / 2;
                if (Math.Abs(oldWidth - pt.X) <= Math.Abs(measurement.X - pt.X))
                    return i - 1;

                oldWidth = measurement.X;
            }
            return Text.Length;
        }

        private bool _mouseDown;

        /// <inheritdoc />
        protected override void OnLeftMouseDown(MouseEventArgs args)
        {
            base.OnLeftMouseDown(args);

            CursorPosition = FindClosestPosition(args.LocalPosition + new Point(_scrollContainer.HorizontalScrollPosition - Padding.Left, _scrollContainer.VerticalScrollPosition - Padding.Top));
            SelectionStart = CursorPosition;

            _mouseDown = true;
        }

        /// <inheritdoc />
        protected override void OnMouseMove(MouseEventArgs args)
        {
            base.OnMouseMove(args);
            if (_mouseDown)
            {
                CursorPosition = FindClosestPosition(args.LocalPosition + new Point(_scrollContainer.HorizontalScrollPosition - Padding.Left, _scrollContainer.VerticalScrollPosition - Padding.Top));
            }
        }

        /// <inheritdoc />
        protected override void OnLeftMouseUp(MouseEventArgs args)
        {
            base.OnLeftMouseUp(args);

            _mouseDown = false;
        }

        readonly Keys[] _ignoreKeys =
        {
            Keys.Escape,
            Keys.Tab,
            Keys.F1,
            Keys.F2,
            Keys.F3,
            Keys.F4,
            Keys.F5,
            Keys.F6,
            Keys.F7,
            Keys.F8,
            Keys.F9,
            Keys.F10,
            Keys.F11,
            Keys.F12,
            Keys.End,
            Keys.Pause
        };
    }


}
