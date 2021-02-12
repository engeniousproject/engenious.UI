﻿using System.Linq;

namespace engenious.UI.Controls
{
    /// <summary>
    /// A ui element base class for a control containing another control.
    /// </summary>
    public class ContentControl : Control
    {
        private Control _content;

        /// <summary>
        /// Gets or sets the <see cref="Control"/> contained in this control.
        /// </summary>
        public Control Content
        {
            get => _content;
            set
            {
                if (_content != null)
                    Children.Remove(_content);
                _content = value;

                if (value != null)
                    Children.Add(value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentControl"/> class.
        /// </summary>
        /// <param name="manager">The <see cref="BaseScreenComponent"/>.</param>
        /// <param name="style">The style to use for this control.</param>
        public ContentControl(BaseScreenComponent manager, string style = "") :
            base(manager, style)
        {
            ApplySkin(typeof(ContentControl));
        }
    }
}
