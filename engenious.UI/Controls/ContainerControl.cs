namespace engenious.UI.Controls
{
    /// <summary>
    /// A ui control base class for all container controls.
    /// </summary>
    public class ContainerControl : Control
    {
        /// <summary>
        /// Gets a list of all controls.
        /// </summary>
        public ControlCollection Controls => Children;

        /// <summary>
        /// Initializes a new instance of the <see cref="CanvasControl"/> class.
        /// </summary>
        /// <param name="style">The style to use for this control.</param>
        /// <param name="manager">The <see cref="BaseScreenComponent"/>.</param>
        public ContainerControl(string style = "", BaseScreenComponent? manager = null) :
            base(style, manager)
        {
            ApplySkin(typeof(ContainerControl));
        }
    }
}
