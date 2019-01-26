namespace engenious.UI.Controls
{
    public class TabPage : ContainerControl
    {
        public string Title;

        public TabPage(BaseScreenComponent manager, string title) : base(manager)
        {
            Title = title;
        }
    }
}
