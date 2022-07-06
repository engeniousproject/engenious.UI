using engenious.UI.Interfaces;

namespace engenious.UI.Controls
{
    /// <summary>
    /// A ui control Combobox for selecting an element from a dropdown list.
    /// </summary>
    /// <typeparam name="T">The contained element type.</typeparam>
    public class Combobox<T> : ListControl<T>, ICombobox where T : class
    {
        /// <summary>
        /// Gets the <see cref="Listbox{T}"/> control containing the selectable elements.
        /// </summary>
        public Listbox<T> Selector { get; }

        /// <inheritdoc />
        public bool IsOpen => Selector.Parent != null;

        /// <inheritdoc />
        public Brush ButtonBrushOpen {
            get => _buttonBrushOpen;
            set
            {
                if (_buttonBrushOpen == value)
                    return;
                _buttonBrushOpen = value;
                if(!IsOpen)
                    _imageControl.Background = value;
            }
        }

        private Brush _buttonBrushOpen;

        /// <inheritdoc />
        public Brush ButtonBrushClose
        {
            get => _buttonBrushClose;
            set
            {
                if (_buttonBrushClose == value)
                    return;
                _buttonBrushClose = value;
                if (IsOpen)
                    _imageControl.Background = value;
            }
        }

        /// <inheritdoc />
        public Brush DropdownBackgroundBrush
        {
            get => Selector.Background;
            set => Selector.Background = value;
        }

        private Brush _buttonBrushClose;

        private readonly Image _imageControl;

        private readonly ContentControl _mainControl;

        /// <summary>
        /// Initializes a new instance of the <see cref="Combobox{T}"/> class.
        /// </summary>
        /// <param name="style">The style to use for this control.</param>
        /// <param name="manager">The <see cref="BaseScreenComponent"/>.</param>
        public Combobox(BaseScreenComponent? manager = null, string style = "")
            : this(item => DefaultGenerateControl(manager, style, item), manager, style)
        {
            
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Combobox{T}"/> class.
        /// </summary>
        /// <param name="templateGenerator">The template generator to use for generating shown controls for items.</param>
        /// <param name="style">The style to use for this control.</param>
        /// <param name="manager">The <see cref="BaseScreenComponent"/>.</param>
        public Combobox(GenerateTemplateDelegate<T> templateGenerator, BaseScreenComponent? manager = null, string style = "")
            : base(templateGenerator, manager, style)
        { 
            _mainControl = new ContentControl(manager: manager)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = Border.All(0),
                Padding = Border.All(0)
            };

            var grid = new Grid(manager: manager)
            {
                //HorizontalAlignment = HorizontalAlignment.Stretch,
                //VerticalAlignment = VerticalAlignment.Stretch,
            };
            grid.Rows.Add(new RowDefinition() { ResizeMode = ResizeMode.FitParts, Height = 1 });
            grid.Columns.Add(new ColumnDefinition() { ResizeMode = ResizeMode.FitParts, Width = 1});
            grid.Columns.Add(new ColumnDefinition() { ResizeMode = ResizeMode.Fixed, Width = 20 });
            Children.Add(grid);

            grid.AddControl(_mainControl, 0, 0);
            _imageControl = new Image(manager: manager)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            grid.AddControl(_imageControl, 1, 0);

            Selector = new Listbox<T>(manager: manager);
            Selector.HorizontalAlignment = HorizontalAlignment.Left;
            Selector.VerticalAlignment = VerticalAlignment.Top;
            Selector.MaxHeight = 100;
            Selector.TemplateGenerator = GenerateControl;
            Selector.SelectedItemChanged += Selector_SelectedItemChanged;

            _buttonBrushOpen = null!;
            _buttonBrushClose = null!;

            ApplySkin(typeof(Combobox<T>));

            Selector.ParentChanged += (s,e) =>
            {
                _imageControl.Background = IsOpen ? ButtonBrushClose : ButtonBrushOpen;
            };
            // TODO:
            // CheckStyleInitialized(nameof(ButtonBrushOpen), ButtonBrushOpen);
            // CheckStyleInitialized(nameof(ButtonBrushClose), ButtonBrushClose);
        }

        private void Selector_SelectedItemChanged(Control sender, SelectionEventArgs<T> args)
        {
            if (Selector.Parent == null) return;

            ScreenManager.Flyback(Selector);
            SelectedItem = args.NewItem;
            Selector.SelectedItem = null;
            Focus();
        }

        private Control? GenerateControl(T? item)
        {
            return TemplateGenerator(item);
        }

        /// <inheritdoc />
        protected override void OnInsert(T item, int index)
        {
            Selector.Items.Insert(index, item);
        }

        /// <inheritdoc />
        protected override void OnRemove(T item, int index)
        {
            // TODO: Check whether it is the selected element
            Selector.Items.Remove(item);
        }

        /// <inheritdoc />
        protected override void OnSelectedItemChanged(SelectionEventArgs<T> args)
        {
            base.OnSelectedItemChanged(args);

            _mainControl.Content = TemplateGenerator(args.NewItem);
        }

        /// <inheritdoc />
        protected override void OnLeftMouseClick(MouseEventArgs args)
        {
            base.OnLeftMouseClick(args);

            if (Selector.Parent == null)
            {
                Selector.Width = ActualSize.X - Margin.Left - Margin.Right;
                ScreenManager.Flyout(Selector, new Point(AbsolutePosition.X + Margin.Left, AbsolutePosition.Y + ActualSize.Y));
            }
            else
            {
                ScreenManager.Flyback(Selector);
            }
            args.Handled = true;
        }
    }
}
