namespace engenious.UI.Controls
{
    /// <summary>
    /// Ui  <see cref="ContainerControl"/> containing other controls.
    /// </summary>
    public class Panel : ContainerControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Panel"/> class.
        /// </summary>
        /// <param name="style">The style to use for this control.</param>
        /// <param name="manager">The <see cref="BaseScreenComponent"/>.</param>
        public Panel(BaseScreenComponent? manager = null, string style = "") : base(manager, style)
        {
            ApplySkin(typeof(Panel));
        }
    }
}
