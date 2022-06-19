using engenious.Graphics;

namespace engenious.UI.Controls
{
    /// <summary>
    /// A ui control that can be either checked or unchecked.
    /// </summary>
    public class Checkbox : ContentControl
    {
        /// <summary>
        /// Gets or sets the <see cref="Brush"/> for the <see cref="Checkbox"/>-box.
        /// </summary>
        public Brush BoxBrush { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> for the filling of the <see cref="Checkbox"/>-box.
        /// </summary>
        public Brush InnerBoxBrush { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> for hook of the <see cref="Checkbox"/>.
        /// </summary>
        public Brush HookBrush { get; set; }

        private bool _boxChecked = false;

        /// <summary>
        /// Gets or sets whether the <see cref="Checkbox"/> is checked.
        /// </summary>
        public bool Checked
        {
            get => _boxChecked;
            set
            {
                _boxChecked = value;
                CheckedChanged?.Invoke(_boxChecked);
            }
        }

        /// <summary>
        /// Occurs when the <see cref="Checked"/> state changed.
        /// </summary>
        public event CheckedChangedDelegate? CheckedChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="Checkbox"/> class.
        /// </summary>
        /// <param name="style">The style to use for this control.</param>
        /// <param name="manager">The <see cref="BaseScreenComponent"/>.</param>
        public Checkbox(string style = "", BaseScreenComponent? manager = null) : base(style, manager)
        {
            CanFocus = true;
            TabStop = true;

            BoxBrush = null!;
            InnerBoxBrush = null!;
            HookBrush = null!;
            
            ApplySkin(typeof(Checkbox));

            CheckStyleInitialized(nameof(BoxBrush), BoxBrush);
            CheckStyleInitialized(nameof(InnerBoxBrush), InnerBoxBrush);
            CheckStyleInitialized(nameof(HookBrush), HookBrush);
        }

        /// <inheritdoc />
        protected override void OnDrawContent(SpriteBatch batch, Rectangle contentArea, GameTime gameTime, float alpha)
        {
            int innerDistanceX = contentArea.Width / 18;
            int innerDistanceY = contentArea.Height / 18;

            int hookDistanceX = contentArea.Height / 7;
            int hookDistanceY = contentArea.Width / 7;

            BoxBrush.Draw(batch, contentArea, alpha);
            InnerBoxBrush.Draw(batch, new Rectangle(contentArea.X + innerDistanceX, contentArea.Y + innerDistanceY,
                contentArea.Width - innerDistanceX * 2, contentArea.Height - innerDistanceY * 2), alpha);
            if (Checked)
                HookBrush.Draw(batch, new Rectangle(contentArea.X + hookDistanceX, contentArea.Y + +hookDistanceY,
                    contentArea.Width - hookDistanceX * 2, contentArea.Height - hookDistanceY * 2), alpha);
        }


        /// <inheritdoc />
        protected override void OnLeftMouseClick(MouseEventArgs args)
        {
            Checked = !Checked;
        }


        /// <inheritdoc />
        protected override void OnKeyDown(KeyEventArgs args)
        {
            if (Focused == TreeState.Active && args.Key == Input.Keys.Enter)
                Checked = !Checked;
        }

        /// <summary>
        /// Represents the method that will handle the <see cref="Checkbox.CheckedChanged"/> event of a <see cref="Checkbox"/>.
        /// </summary>
        /// <param name="checked">Whether the <see cref="Checkbox"/> is now checked or not.</param>
        public delegate void CheckedChangedDelegate(bool @checked);
    }
}
