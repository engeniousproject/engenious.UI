using System;
using System.Collections.Generic;
using engenious.Graphics;

namespace engenious.UI.Controls
{
    /// <summary>
    /// Ui control for displaying text.
    /// </summary>
    public class Label : Control, ITextControl
    {
        private readonly struct TextLine
        {
            public int Begin { get; }
            public int Length { get; }
            public Vector2 Size { get; }

            public TextLine(int begin, int length, Vector2 size)
            {
                Begin = begin;
                Length = length;
                Size = size;
            }

        }

        private readonly List<TextLine> _lines = new();

        private string _text = string.Empty;

        /// <summary>
        /// Occurs when the <see cref="Text"/> got changed.
        /// </summary>
        public event PropertyChangedDelegate<string>? TextChanged;

        private SpriteFont? _font;

        private Color _textColor = Color.Black;
        private Color _disabledTextColor = Color.LightGray;

        private HorizontalAlignment _horizontalTextAlignment = HorizontalAlignment.Center;

        private VerticalAlignment _verticalTextAlignment = VerticalAlignment.Center;

        private bool _wordWrap = false;
        private bool _lineWrap;
        private readonly PropertyEventArgs<string> _textChangedEventArgs = new PropertyEventArgs<string>();

        /// <inheritdoc />
        public string Text
        {
            get => _text;
            set
            {
                if (_text == value) return;

                _textChangedEventArgs.OldValue = _text;
                _textChangedEventArgs.NewValue = value;
                _textChangedEventArgs.Handled = false;
                TextChanged?.Invoke(this, _textChangedEventArgs);
                _text = value;
                InvalidateDimensions();
            }
        }

        /// <inheritdoc />
        public SpriteFont? Font
        {
            get => _font;
            set
            {
                if (_font != value)
                {
                    _font = value;
                    InvalidateDimensions();
                }
            }
        }

        /// <inheritdoc />
        public Color TextColor
        {
            get => _textColor;
            set
            {
                if (_textColor != value)
                {
                    _textColor = value;
                    InvalidateDrawing();
                }
            }
        }

        /// <summary>
        /// Gets or sets the color used to render the <see cref="Text"/> when the <see cref="Label"/> is disabled.
        /// </summary>
        public Color DisabledTextColor
        {
            get => _disabledTextColor;
            set
            {
                if (_disabledTextColor != value)
                {
                    _disabledTextColor = value;

                    // Only invalidate of its really needed
                    if (!Enabled)
                        InvalidateDrawing();
                }
            }
        }

        /// <inheritdoc />
        public HorizontalAlignment HorizontalTextAlignment
        {
            get => _horizontalTextAlignment;
            set
            {
                if (_horizontalTextAlignment != value)
                {
                    _horizontalTextAlignment = value;
                    InvalidateDimensions();
                }
            }
        }

        /// <inheritdoc />
        public VerticalAlignment VerticalTextAlignment
        {
            get => _verticalTextAlignment;
            set
            {
                if (_verticalTextAlignment != value)
                {
                    _verticalTextAlignment = value;
                    InvalidateDimensions();
                }
            }
        }
        
        /// <inheritdoc />
        public bool WordWrap
        {
            get => _wordWrap;
            set
            {
                if (_wordWrap != value)
                {
                    _wordWrap = value;
                    InvalidateDimensions();
                }
            }
        }

        /// <inheritdoc />
        public bool LineWrap
        {
            get => _lineWrap;
            set
            {
                if (_lineWrap != value)
                {
                    _lineWrap = value;
                    InvalidateDimensions();
                }
            }
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Label"/> class.
        /// </summary>
        /// <param name="manager">The <see cref="BaseScreenComponent"/>.</param>
        /// <param name="style">The style to use for this control.</param>
        public Label(BaseScreenComponent manager, string style = "")
            : base(manager, style)
        {
            ApplySkin(typeof(Label));
        }

        /// <inheritdoc />
        protected override void OnDrawContent(SpriteBatch batch, Rectangle area, GameTime gameTime, float alpha)
        {
            // check necessary constraints for rendering
            if (Font == null) return;
            var color = Enabled ? TextColor : DisabledTextColor;

            Vector2 offset = new Vector2(area.X, area.Y);

            if (WordWrap || _lineWrap)
            {
                int totalHeight = 0;
                foreach (var line in _lines)
                {
                    totalHeight += (int)line.Size.Y;
                }

                switch (VerticalTextAlignment)
                {
                    case VerticalAlignment.Top:
                        break;
                    case VerticalAlignment.Bottom:
                        offset.Y = area.Y + area.Height - totalHeight;
                        break;
                    case VerticalAlignment.Center:
                        offset.Y = area.Y + (area.Height - totalHeight) / 2;
                        break;
                }

                foreach (var line in _lines)
                {
                    switch (HorizontalTextAlignment)
                    {
                        case HorizontalAlignment.Left:
                            offset.X = area.X;
                            break;
                        case HorizontalAlignment.Center:
                            offset.X = area.X + (area.Width - line.Size.X) / 2;
                            break;
                        case HorizontalAlignment.Right:
                            offset.X = area.X + area.Width - line.Size.X;
                            break;
                    }

                    batch.DrawString(Font, Text, line.Begin, line.Length, offset, color * alpha);

                    offset.Y += (int)line.Size.Y;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(Text))
                    batch.DrawString(Font, Text, offset, color * alpha);
            }
        }

        /// <inheritdoc />
        public override Point CalculateRequiredClientSpace(Point available)
        {
            if (Font == null) return Point.Zero;

            AnalyzeText(available);

            int width = 0;
            int height = 0;

            if (WordWrap || _lineWrap)
            {
                foreach (var line in _lines)
                {
                    width = Math.Max((int)line.Size.X, width);
                    height += (int)line.Size.Y;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(Text))
                {
                    return new Point(0, (int)Math.Ceiling(Font.LineSpacing));
                }

                Vector2 lineSize = Font.MeasureString(Text);
                width = (int)lineSize.X;
                height = (int)lineSize.Y;
            }

            return new Point(Math.Min(available.X, width), Math.Min(available.Y, height));
        }

        private void AnalyzeText(Point available)
        {
            _lines.Clear();
            if (Font == null) return;

            if (string.IsNullOrEmpty(Text))
                return;


            if (_wordWrap)
                WrapWordsAndLines(available);
            else if (_lineWrap)
            {
                int iBefore = 0;
                while (iBefore < Text.Length)
                {
                    int i = Text.IndexOf('\n', iBefore);
                    if (i < 0)
                        i = Text.Length;

                    if (i > 0)
                    {
                        var size = Font.MeasureString(Text, iBefore, i - iBefore);
                        _lines.Add(new TextLine(iBefore, i - iBefore, new Vector2(size.X, size.Y)));
                        iBefore = i + 1;
                    }
                }

            }
        }

        private void WrapWordsAndLines(Point available)
        {
            if (Font == null) return;
            int iBefore = 0;
            do
            {
                int i = Text.IndexOf('\n', iBefore);

                int forUntil = i > 0 ? i : Text.Length;
                Vector2 sizeSinceBegin = new Vector2();
                int iBeforeCopy = iBefore;
                for (int index = iBefore; index < forUntil;)
                {
                    index = Text.IndexOf(' ', iBefore, forUntil - iBefore);
                    if (index < 0)
                    {
                        var word = Font.MeasureString(Text, iBefore, forUntil - iBefore);
                        var res = FitAndAddLine(iBeforeCopy, forUntil - iBeforeCopy, sizeSinceBegin, word, available.X,
                            true);
                        if (res != default)
                        {
                            FitAndAddLine(iBeforeCopy, forUntil - iBeforeCopy - (forUntil - iBefore), sizeSinceBegin,
                                default, available.X, true);
                            FitAndAddLine(iBefore, forUntil - iBefore, default, word, available.X, true);
                        }

                        iBefore = forUntil + 1;
                        break;
                    }

                    var wordSize = Font.MeasureString(Text, iBefore, index + 1 - iBefore);
                    var newLineMade = FitAndAddLine(iBeforeCopy, index - iBeforeCopy, sizeSinceBegin, wordSize,
                        available.X);
                    if (newLineMade == default)
                    {
                        FitAndAddLine(iBeforeCopy, iBefore - iBeforeCopy, sizeSinceBegin, newLineMade, available.X,
                            true);
                        sizeSinceBegin = wordSize;
                        iBeforeCopy = iBefore;
                    }
                    else
                        sizeSinceBegin = newLineMade;

                    iBefore = index + 1;
                }

                if (i < 0)
                    break;
            } while (true);
        }

        private Vector2 FitAndAddLine(int begin, int length, Vector2 lineSize, Vector2 current, float availableWidth, bool newLineWhenFitting = false)
        {
            if (newLineWhenFitting && lineSize.X + current.X <= availableWidth)
            {
                _lines.Add(new TextLine(begin, length, new Vector2(lineSize.X + current.X, Math.Max(lineSize.Y, current.Y))));
                return new Vector2();
            }
            else if (!newLineWhenFitting && lineSize.X + current.X > availableWidth)
                return new Vector2();
            else
                return new Vector2(lineSize.X + current.X, Math.Max(lineSize.Y, current.Y));
        }
    }


    //TODO: Implement with span in core or net 5
    //private void AnalyzeText(Point available)
    //{
    //    lines.Clear();
    //    if (Font == null) return;

    //    stringBuilder.Clear();

    //    if (string.IsNullOrEmpty(Text))
    //        return;

    //    string[] l = Text.Split('\n');
    //    foreach (var line in l)
    //    {
    //        string[] words = line.Split(' ');

    //        foreach (var word in words)
    //        {
    //            Vector2 size = Font.MeasureString(word);
    //            Vector2 lineSize = Font.MeasureString(stringBuilder);

    //            if (lineSize.X + size.X >= available.X)
    //            {
    //                lines.Add(stringBuilder.ToString());
    //                stringBuilder.Clear();
    //            }
    //            stringBuilder.Append(word + " ");
    //        }

    //        lines.Add(stringBuilder.ToString());
    //        stringBuilder.Clear();
    //    }
    //}
}

