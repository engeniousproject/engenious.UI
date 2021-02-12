using engenious.Input;

namespace engenious.UI.Controls
{
    /// <summary>
    /// A clickable UI element.
    /// </summary>
    public class Button : ContentControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Button"/> class.
        /// </summary>
        /// <param name="manager">The <see cref="BaseScreenComponent"/>.</param>
        /// <param name="style">The style to use for this control.</param>
        public Button(BaseScreenComponent manager, string style = "")
            : base(manager, style)
        {
            TabStop = true;
            CanFocus = true;

            ApplySkin(typeof(Button));
        }
        
        /// <inheritdoc />
        protected override void OnKeyPress(KeyEventArgs args)
        {
            base.OnKeyPress(args);

            if (Focused == TreeState.Active &&
                (args.Key == Keys.Enter || args.Key == Keys.Space))
            {
                EventArgs e = EventArgsPool.Instance.Take();

                OnExecuted(e);
                Executed?.Invoke(this, e);

                EventArgsPool.Instance.Release(e);

                args.Handled = true;
            }
        }

        /// <inheritdoc />
        protected override void OnLeftMouseClick(MouseEventArgs args)
        {
            base.OnLeftMouseClick(args);

            EventArgs e = EventArgsPool.Instance.Take();
            OnExecuted(e);
            Executed?.Invoke(this, e);

            EventArgsPool.Instance.Release(e);

            args.Handled = true;
        }

        /// <inheritdoc />
        protected override void OnTouchTap(TouchEventArgs args)
        {
            base.OnTouchTap(args);

            EventArgs e = EventArgsPool.Instance.Take();
            OnExecuted(e);
            Executed?.Invoke(this, e);

            EventArgsPool.Instance.Release(e);

            args.Handled = true;
        }

        /// <summary>
        /// Raises the <see cref="Executed"/> event.
        /// </summary>
        /// <param name="args">A <see cref="EventArgs"/> that contains the event data.</param>
        protected virtual void OnExecuted(EventArgs args) { }

        /// <summary>
        /// Occurs after the <see cref="Control.LeftMouseClick"/>,
        /// a <see cref="Control.KeyPress"/> using <see cref="Keys.Enter"/> or <see cref="Keys.Space"/>,
        /// or a <see cref="Control.TouchTap"/> got executed.
        /// </summary>
        public event EventDelegate Executed;
    }
}
