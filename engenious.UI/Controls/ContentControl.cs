using System.Linq;

namespace engenious.UI.Controls
{
    public class ContentControl : Control
    {
        private Control content;

        /// <summary>
        /// Das enthaltene Control.
        /// </summary>
        public Control Content
        {
            get => content;
            set
            {
                if (content != null)
                    Children.Remove(content);
                content = value;

                if (value != null)
                    Children.Add(value);
            }
        }

        public ContentControl(BaseScreenComponent manager, string style = "") :
            base(manager, style)
        {
            ApplySkin(typeof(ContentControl));
        }
    }
}
