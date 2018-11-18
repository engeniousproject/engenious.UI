using System.Collections.Generic;

namespace EngeniousUI.Controls
{
    /// <summary>
    /// Basisklasse für alle Controls mit mehreren Inhalten
    /// </summary>
    public class ContainerControl : Control
    {
        /// <summary>
        /// Liste aller Controls
        /// </summary>
        public IList<Control> Controls { get { return Children; } }

        /// <summary>
        /// Erzeugt eine neue Instanz der ContainerControl-Klasse.
        /// </summary>
        /// <param name="manager">Der <see cref="BaseScreenComponent"/></param>
        /// <param name="style">Der zu verwendende Style</param>
        public ContainerControl(BaseScreenComponent manager, string style = "") :
            base(manager, style)
        {
            ApplySkin(typeof(ContainerControl));
        }
    }
}
