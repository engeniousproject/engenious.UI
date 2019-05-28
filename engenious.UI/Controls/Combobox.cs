using engenious.UI.Interfaces;

namespace engenious.UI.Controls
{
    /// <summary>
    /// Typische Combobox zur Auswahl eines Elements aus einer Liste.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Combobox<T> : ListControl<T>, ICombobox where T : class
    {
        public Listbox<T> Selector { get; private set; }

        public bool IsOpen => Selector.Parent != null;

        public Brush ButtonBrushOpen {
            get => buttonBrushOpen;
            set
            {
                if (buttonBrushOpen == value)
                    return;
                buttonBrushOpen = value;
                if(!IsOpen)
                    imageControl.Background = value;
            }
        }

        private Brush buttonBrushOpen;

        public Brush ButtonBrushClose
        {
            get => buttonBrushClose;
            set
            {
                if (buttonBrushClose == value)
                    return;
                buttonBrushClose = value;
                if (IsOpen)
                    imageControl.Background = value;
            }
        }

        public Brush DropdownBackgroundBrush
        {
            get => Selector.Background;
            set
            {
                Selector.Background = value;
            }
        }

        private Brush buttonBrushClose;

        private Image imageControl;

        private ContentControl mainControl;

        public Combobox(BaseScreenComponent manager)
            : base(manager)
        { 
            mainControl = new ContentControl(manager)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = Border.All(0),
                Padding = Border.All(0)
            };

            var grid = new Grid(manager)
            {
                //HorizontalAlignment = HorizontalAlignment.Stretch,
                //VerticalAlignment = VerticalAlignment.Stretch,
            };
            grid.Rows.Add(new RowDefinition() { ResizeMode = ResizeMode.FitParts, Height = 1 });
            grid.Columns.Add(new ColumnDefinition() { ResizeMode = ResizeMode.FitParts, Width = 1});
            grid.Columns.Add(new ColumnDefinition() { ResizeMode = ResizeMode.Fixed, Width = 20 });
            Children.Add(grid);

            grid.AddControl(mainControl, 0, 0);
            imageControl = new Image(manager)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            grid.AddControl(imageControl, 1, 0);

            Selector = new Listbox<T>(manager);
            Selector.HorizontalAlignment = HorizontalAlignment.Left;
            Selector.VerticalAlignment = VerticalAlignment.Top;
            Selector.MaxHeight = 100;
            Selector.TemplateGenerator = GenerateControl;
            Selector.SelectedItemChanged += Selector_SelectedItemChanged;

            ApplySkin(typeof(Combobox<T>));

            Selector.ParentChanged += (s,e) =>
            {
                if (IsOpen)
                    imageControl.Background = ButtonBrushClose;
                else
                    imageControl.Background = ButtonBrushOpen;
            };
        }

        void Selector_SelectedItemChanged(Control sender, SelectionEventArgs<T> args)
        {
            if (Selector.Parent == null) return;

            ScreenManager.Flyback(Selector);
            SelectedItem = args.NewItem;
            Selector.SelectedItem = null;
            Focus();
        }

        private Control GenerateControl(T item)
        {
            return TemplateGenerator(item);
        }

        protected override void OnInsert(T item, int index)
        {
            Selector.Items.Insert(index, item);
        }

        protected override void OnRemove(T item, int index)
        {
            // TODO: Prüfen, ob es sich um das selektierte Element handelt
            Selector.Items.Remove(item);
        }

        protected override void OnSelectedItemChanged(SelectionEventArgs<T> args)
        {
            base.OnSelectedItemChanged(args);

            mainControl.Content = TemplateGenerator(args.NewItem);
        }

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
