namespace engenious.UI.Controls
{
    /// <summary>
    /// A ui control used for a flyout effect.
    /// </summary>
    internal sealed class FlyoutControl : CanvasControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FlyoutControl"/> class.
        /// </summary>
        /// <param name="style">The style to use for this control.</param>
        /// <param name="manager">The <see cref="BaseScreenComponent"/>.</param>
        internal FlyoutControl(BaseScreenComponent? manager = null, string style = "") : base(manager, style) {
            ApplySkin(typeof(FlyoutControl));
        }

        /// <inheritdoc />
        protected override void OnLeftMouseDown(MouseEventArgs args)
        {
            if (Hovered == TreeState.Active && Children.Count > 0)
                Children.Clear();

            base.OnLeftMouseDown(args);
        }

        /// <inheritdoc />
        protected override void OnRightMouseDown(MouseEventArgs args)
        {
            if (Hovered == TreeState.Active && Children.Count > 0)
                Children.Clear();

            OnRightMouseClick(args);
        }
    }
}
