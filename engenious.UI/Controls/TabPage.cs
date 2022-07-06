namespace engenious.UI.Controls
{
    /// <summary>
    /// Ui container control depicting a page for the <see cref="TabControl"/>.
    /// </summary>
    public class TabPage : ContainerControl
    {
        /// <summary>
        /// Gets or sets the title of the tab page.
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Image"/> class.
        /// </summary>
        /// <param name="title">The title for this page.</param>
        /// <param name="style">The style to use for this control.</param>
        /// <param name="manager">The <see cref="BaseScreenComponent"/>.</param>
        public TabPage(string title, BaseScreenComponent? manager = null, string style = "") : base(manager, style)
        {
            Title = title;
        }
    }
}
