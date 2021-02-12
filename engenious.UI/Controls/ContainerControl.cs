using System.Collections.Generic;

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
        /// <param name="manager">The <see cref="BaseScreenComponent"/>.</param>
        /// <param name="style">The style to use for this control.</param>
        public ContainerControl(BaseScreenComponent manager, string style = "") :
            base(manager, style)
        {
            ApplySkin(typeof(ContainerControl));
        }
    }
}
