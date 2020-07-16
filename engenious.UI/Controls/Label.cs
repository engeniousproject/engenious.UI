using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

using engenious.Graphics;

namespace engenious.UI.Controls
{
    /// <summary>
    /// Standard Text-Anzeige Control.
    /// </summary>
    public class Label : Control, ITextControl
    {
        readonly struct TextLine
        {
            public readonly int Begin { get; }
            public readonly int Length { get; }
            public readonly Vector2 Size { get; }

            public TextLine(int begin, int length, Vector2 size)
            {
                Begin = begin;
                Length = length;
                Size = size;
            }

        }

        private List<TextLine> lines = new List<TextLine>();

        private string text = string.Empty;

        public event PropertyChangedDelegate<String> TextChanged;

        private SpriteFont font = null;

        private Color textColor = Color.Black;

        private HorizontalAlignment horizontalTextAlignment = HorizontalAlignment.Center;

        private VerticalAlignment verticalTextAlignment = VerticalAlignment.Center;

        private bool wordWrap = false;
        private bool lineWrap;
        private readonly PropertyEventArgs<string> _textChangedEventArgs = new PropertyEventArgs<string>();
        /// <summary>
        /// Gibt den enthaltenen Text an oder legt diesen fest.
        /// </summary>
        public string Text
        {
            get { return text ?? string.Empty; }
            set
            {
                if (text == value) return;

                _textChangedEventArgs.OldValue = text;
                _textChangedEventArgs.NewValue = value;
                _textChangedEventArgs.Handled = false;
                TextChanged?.Invoke(this, _textChangedEventArgs);
                text = value;
                InvalidateDimensions();
            }
        }

        /// <summary>
        /// Gibt die Schriftart an mit der der Inhalt gezeichnet werden soll oder legt diese fest.
        /// </summary>
        public SpriteFont Font
        {
            get { return font; }
            set
            {
                if (font != value)
                {
                    font = value;
                    InvalidateDimensions();
                }
            }
        }

        /// <summary>
        /// Gibt die Textfarbe an mit der der Inhalt gezeichnet werden soll oder legt diese fest.
        /// </summary>
        public Color TextColor
        {
            get { return textColor; }
            set
            {
                if (textColor != value)
                {
                    textColor = value;
                    InvalidateDrawing();
                }
            }
        }

        /// <summary>
        /// Gibt die Ausrichtung des Textes innerhalb des Controls auf horizontaler Ebene an.
        /// </summary>
        public HorizontalAlignment HorizontalTextAlignment
        {
            get { return horizontalTextAlignment; }
            set
            {
                if (horizontalTextAlignment != value)
                {
                    horizontalTextAlignment = value;
                    InvalidateDimensions();
                }
            }
        }

        /// <summary>
        /// Gibt die Ausrichtung des Textes innerhalb des Controls auf vertikaler Ebene an.
        /// </summary>
        public VerticalAlignment VerticalTextAlignment
        {
            get { return verticalTextAlignment; }
            set
            {
                if (verticalTextAlignment != value)
                {
                    verticalTextAlignment = value;
                    InvalidateDimensions();
                }
            }
        }

        /// <summary>
        /// Gibt an, ob das Control automatisch den Text an geeigneter Stelle 
        /// umbrechen soll, falls er nicht in eine Zeile passt.
        /// </summary>
        public bool WordWrap
        {
            get { return wordWrap; }
            set
            {
                if (wordWrap != value)
                {
                    wordWrap = value;
                    InvalidateDimensions();
                }
            }
        }

        /// <summary>
        /// Gibt an, ob Zeilenumbrüche interpretiert werden sollen.
        /// </summary>
        public bool LineWrap
        {
            get => lineWrap;
            set
            {
                if (lineWrap != value)
                {
                    lineWrap = value;
                    InvalidateDimensions();
                }
            }
        }

        public Label(BaseScreenComponent manager, string style = "") :
            base(manager, style)
        {
            ApplySkin(typeof(Label));
        }

        protected override void OnDrawContent(SpriteBatch batch, Rectangle area, GameTime gameTime, float alpha)
        {
            // Rahmenbedingungen fürs Rendern checken
            if (Font == null) return;

            Vector2 offset = new Vector2(area.X, area.Y);

            if (WordWrap || lineWrap)
            {
                int totalHeight = 0;
                foreach (var line in lines)
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

                foreach (var line in lines)
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

                    batch.DrawString(Font, Text, line.Begin, line.Length, offset, TextColor * alpha);

                    offset.Y += (int)line.Size.Y;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(Text))
                    batch.DrawString(Font, Text, offset, TextColor * alpha);
            }
        }

        public override Point CalculcateRequiredClientSpace(Point available)
        {
            if (Font == null) return Point.Zero;

            AnalyzeText(available);

            int width = 0;
            int height = 0;

            if (WordWrap || lineWrap)
            {
                foreach (var line in lines)
                {
                    width = Math.Max((int)line.Size.X, width);
                    height += (int)line.Size.Y;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(Text))
                {
                    return new Point(0, Font.LineSpacing);
                }

                Vector2 lineSize = Font.MeasureString(Text);
                width = (int)lineSize.X;
                height = (int)lineSize.Y;
            }

            return new Point(Math.Min(available.X, width), Math.Min(available.Y, height));
        }

        public static void DoStuff(string asdf)
        {
            int wordStart = 0, lineStart = 0;
            for (int i = 0; i < asdf.Length; i++)
            {
                bool isNewLine = asdf[i] == '\n';
                bool isSpace = asdf[i] == ' ';
                bool isWhitespace = isNewLine || isSpace;

                if (isWhitespace)
                    Console.WriteLine($"word: {asdf.Substring(wordStart, i - wordStart)}");
                if (isNewLine)
                    Console.WriteLine($"line: {asdf.Substring(lineStart, i - lineStart)}");

                if (isWhitespace)
                    wordStart = i + 1;
                if (isNewLine)
                    lineStart = i + 1;
            }
            if (wordStart < asdf.Length)
                Console.WriteLine($"word: {asdf.Substring(wordStart, asdf.Length - wordStart)}");
            if (lineStart < asdf.Length)
                Console.WriteLine($"line: {asdf.Substring(lineStart, asdf.Length - lineStart)}");
        }

        private void AnalyzeText(Point available)
        {
            lines.Clear();
            if (Font == null) return;

            if (string.IsNullOrEmpty(Text))
                return;


            if (wordWrap)
                WrapWordsAndLines(available);
            else if (lineWrap)
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
                        lines.Add(new TextLine(iBefore, i - iBefore, new Vector2(size.X, size.Y)));
                        iBefore = i + 1;
                    }
                }

            }
        }

        private void WrapWordsAndLines(Point available)
        {
            int iBefore = 0;
            bool doBeak = false;
            while (!doBeak)
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
                        var res = FitAndAddLine(iBeforeCopy, forUntil - iBeforeCopy, sizeSinceBegin, word, available.X, true);
                        if (res != default)
                        {
                            FitAndAddLine(iBeforeCopy, forUntil - iBeforeCopy - (forUntil - iBefore), sizeSinceBegin, default, available.X, true);
                            FitAndAddLine(iBefore, forUntil - iBefore, default, word, available.X, true);
                        }
                        iBefore = forUntil + 1;
                        break;
                    }

                    var wordSize = Font.MeasureString(Text, iBefore, index + 1 - iBefore);
                    var newLineMade = FitAndAddLine(iBeforeCopy, index - iBeforeCopy, sizeSinceBegin, wordSize, available.X);
                    if (newLineMade == default)
                    {
                        FitAndAddLine(iBeforeCopy, iBefore - iBeforeCopy, sizeSinceBegin, newLineMade, available.X, true);
                        sizeSinceBegin = wordSize;
                        iBeforeCopy = iBefore;
                    }
                    else
                        sizeSinceBegin = newLineMade;
                    iBefore = index + 1;
                }

                if (i < 0)
                    break;
            }
        }

        private Vector2 FitAndAddLine(int begin, int length, Vector2 lineSize, Vector2 current, float availableWidth, bool newLineWhenFitting = false)
        {
            if (newLineWhenFitting && lineSize.X + current.X <= availableWidth)
            {
                lines.Add(new TextLine(begin, length, new Vector2(lineSize.X + current.X, Math.Max(lineSize.Y, current.Y))));
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

