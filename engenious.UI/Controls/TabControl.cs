using System.Linq;

namespace engenious.UI.Controls
{
    /// <summary>
    /// Ui container control for organizing controls into pages.
    /// </summary>
    public class TabControl : Control
    {
        /// <summary>
        /// Gets a list of all <see cref="TabPage"/> controls organized by this control.
        /// </summary>
        public ItemCollection<TabPage> Pages { get; }

        /// <summary>
        /// Gets or sets the content of the <see cref="TabControl"/>.
        /// </summary>
        private Control? Content
        {
            get => Children.FirstOrDefault();
            set
            {
                if (Content != value)
                {
                    Children.Clear();
                    if (value != null)
                        Children.Add(value);
                }
            }
        }

        /// <summary>
        /// The controls needed for visualization.
        /// </summary>
        private readonly StackPanel _tabListStack;
        private readonly ContentControl _tabPage;

        /// <summary>
        /// The needed brushes.
        /// </summary>
        private Brush _tabActiveBrush;
        private Brush _tabBrush;
        private Brush _tabPageBackground;
        private Brush _tabListBackground;

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> used for highlighting the currently active tab in the tab list.
        /// </summary>
        public Brush TabActiveBrush
        {
            get => _tabActiveBrush;
            set
            {
                _tabActiveBrush = value;
                if (_tabListStack.Controls.Count > 0)
                    _tabListStack.Controls.ElementAt(SelectedTabIndex).Background = _tabActiveBrush;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> used for rendering the non active tabs in the tab list.
        /// </summary>
        public Brush TabBrush
        {
            get => _tabBrush;
            set
            {
                _tabBrush = value;
                if (_tabListStack.Controls.Count > 0)
                {
                    foreach (Control c in _tabListStack.Controls)
                    {
                        int index = _tabListStack.Controls.IndexOf(c);
                        if (index != SelectedTabIndex)
                            c.Background = TabBrush;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> used as a background for a <see cref="TabPage"/>.
        /// </summary>
        public Brush TabPageBackground
        {
            get => _tabPageBackground;
            set
            {
                _tabPageBackground = value;
                _tabPage.Background = TabPageBackground;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> used for the background of the tab list.
        /// </summary>
        public Brush TabListBackground
        {
            get => _tabListBackground;
            set
            {
                _tabListBackground = value;
                _tabListStack.Background = TabListBackground;
            }
        }

        private int _tabSpacing;

        /// <summary>
        /// Gets or sets the spacing between tabs in the tab list.
        /// </summary>
        public int TabSpacing
        {
            get => _tabSpacing;
            set
            {
                _tabSpacing = value;
                foreach (Control tabLabel in _tabListStack.Controls)
                    tabLabel.Margin = new Border(0, 0, _tabSpacing, 0);
            }
        }

        /// <summary>
        /// Gets the index of the currently selected tab.
        /// </summary>
        public int SelectedTabIndex { get; private set; } = 0;

        /// <summary>
        /// Gets the currently selected tab.
        /// </summary>
        public TabPage SelectedTab => Pages[SelectedTabIndex];

        /// <summary>
        /// Initializes a new instance of the <see cref="TabControl"/> class.
        /// </summary>
        /// <param name="style">The style to use for this control.</param>
        /// <param name="manager">The <see cref="BaseScreenComponent"/>.</param>
        public TabControl(string style = "", BaseScreenComponent? manager = null) : base(style, manager)
        {
            Pages = new ItemCollection<TabPage>();
            Pages.OnInserted += OnInserted;
            Pages.OnRemove += OnRemove;

            var tabControlGrid = new Grid(manager: manager)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            tabControlGrid.Columns.Add(new ColumnDefinition() { ResizeMode = ResizeMode.Parts, Width = 1 });
            tabControlGrid.Rows.Add(new RowDefinition() { ResizeMode = ResizeMode.Auto });
            tabControlGrid.Rows.Add(new RowDefinition() { ResizeMode = ResizeMode.Parts, Height = 1 });
            Content = tabControlGrid;


            _tabListStack = new StackPanel(manager: manager);
            _tabListStack.HorizontalAlignment = HorizontalAlignment.Stretch;
            _tabListStack.Orientation = Orientation.Horizontal;
            _tabListStack.Background = TabListBackground;
            tabControlGrid.AddControl(_tabListStack, 0, 0);

            _tabPage = new ContentControl(manager: manager);
            _tabPage.HorizontalAlignment = HorizontalAlignment.Stretch;
            _tabPage.VerticalAlignment = VerticalAlignment.Stretch;
            _tabPage.Background = TabPageBackground;
            tabControlGrid.AddControl(_tabPage, 0, 1);

            _tabBrush = null!;
            _tabActiveBrush = null!;
            _tabPageBackground = null!;
            _tabListBackground = null!;

            ApplySkin(typeof(TabControl));
            
            CheckStyleInitialized(nameof(TabBrush), TabBrush);
            CheckStyleInitialized(nameof(TabActiveBrush), TabActiveBrush);
            CheckStyleInitialized(nameof(TabPageBackground), TabPageBackground);
            CheckStyleInitialized(nameof(TabListBackground), TabListBackground);
        }

        /// <summary>
        /// Wird aufgerufen wenn ein neues Element zu "Pages" hinzugefügt wird, erstellt einen neuen Eintrag in der TabList
        /// </summary>
        private void OnInserted(TabPage item, int index)
        {
            Label title = new Label(manager: ScreenManager);
            title.Text = item.Title;
            title.Padding = Border.All(10);
            title.Background = TabBrush;
            title.Margin = new Border(0, 0, TabSpacing, 0);
            title.LeftMouseClick += (s, e) => SelectTab(Pages.IndexOf(item));
            title.CanFocus = true;
            title.TabStop = true;
            title.KeyDown += (s, e) =>
            {
                if (e.Key == Input.Keys.Enter && title.Focused == TreeState.Active)
                    SelectTab(Pages.IndexOf(item));
            };
            _tabListStack.Controls.Add(title);

            SelectTab(SelectedTabIndex);
        }

        /// <summary>
        /// Called when a page is removed from <see cref="Pages"/> removes the entry from the tab list.
        /// </summary>
        private void OnRemove(TabPage item, int index)
        {
            _tabListStack.Controls.RemoveAt(index);                         // Remove the tab
            if (Pages.Count > 0)                                            // Only when there are pages left
            {
                if (index >= _tabListStack.Controls.Count)                  // If the last page is removed...
                    SelectedTabIndex = _tabListStack.Controls.Count - 1;    // Set the tab index to the "new" last index
                else SelectedTabIndex = index;                              // Otherwise, set the selected tab index to the current index

                SelectTab(SelectedTabIndex);                                // Select the tab
            }
            _tabListStack.InvalidateDimensions();                           // Renew the tab list stack
        }

        /// <summary>
        /// Select a tab page with a given index.
        /// </summary>
        /// <param name="index">The index of the tab page to select.</param>
        public void SelectTab(int index)
        {
            _tabListStack.Controls.ElementAt(SelectedTabIndex).Background = TabBrush;
            SelectedTabIndex = index;
            _tabListStack.Controls.ElementAt(index).Background = TabActiveBrush;

            _tabPage.Content = Pages.ElementAt(SelectedTabIndex);

            SelectedTabChanged?.Invoke(this, Pages.ElementAt(index), SelectedTabIndex);
        }

        /// <summary>
        /// Select a tab page.
        /// </summary>
        /// <param name="page">The <see cref="TabPage"/> to select</param>
        public void SelectTab(TabPage page)
        {
            try
            {
                _tabListStack.Controls.ElementAt(SelectedTabIndex).Background = TabBrush;
                SelectedTabIndex = Pages.IndexOf(page);
            }
            finally
            {
                _tabListStack.Controls.ElementAt(SelectedTabIndex).Background = TabActiveBrush;
                _tabPage.Content = Pages.ElementAt(SelectedTabIndex);

                SelectedTabChanged?.Invoke(this, page, SelectedTabIndex);
            }

        }
        
        /// <summary>
        /// Occurs when the <see cref="SelectedTab"/> changed.
        /// </summary>
        public event SelectionChangedDelegate? SelectedTabChanged;

        /// <summary>
        /// Represents the method that will handle the <see cref="TabControl.SelectedTabChanged"/> event of a <see cref="TabControl"/>.
        /// </summary>
        /// <param name="control">The control the event occured on.</param>
        /// <param name="tab">The tab page that was selected.</param>
        /// <param name="index">The index of the tab page that was selected.</param>
        public delegate void SelectionChangedDelegate(Control control, TabPage tab, int index);
    }
}
