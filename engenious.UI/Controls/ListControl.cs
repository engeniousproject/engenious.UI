using engenious.Input;

namespace engenious.UI.Controls
{
    /// <summary>
    /// Ui base control for displaying generic list data.
    /// </summary>
    public abstract class ListControl<T> : Control, IListControl where T : class
    {
        private Brush _selectedItemBrush;

        private T? _selectedItem = null;

        /// <summary>
        /// Gets a list of all contained elements.
        /// </summary>
        public ItemCollection<T> Items { get; }

        /// <summary>
        /// Gets or sets the currently selected item.
        /// </summary>
        public T? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    SelectionEventArgs<T> args = new SelectionEventArgs<T>()
                    {
                        OldItem = _selectedItem,
                        NewItem = value
                    };

                    _selectedItem = value;

                    OnSelectedItemChanged(args);
                    SelectedItemChanged?.Invoke(this, args);
                }
            }
        }

        private readonly PropertyEventArgs<Brush> _selectedItemBrushChangedArgs = new PropertyEventArgs<Brush>();

        /// <inheritdoc />
        public Brush SelectedItemBrush
        {
            get => _selectedItemBrush;
            set
            {
                if (_selectedItemBrush == value) return;
                
                
                _selectedItemBrushChangedArgs.OldValue = _selectedItemBrush;
                _selectedItemBrushChangedArgs.NewValue = value;
                _selectedItemBrushChangedArgs.Handled = false;

                _selectedItemBrush = value;
                InvalidateDrawing();

                OnSelectedItemBrushChanged(_selectedItemBrushChangedArgs);
                SelectedItemBrushChanged?.Invoke(this, _selectedItemBrushChangedArgs);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ListControl{T}"/> class.
        /// </summary>
        /// <param name="templateGenerator">The template generator to use for generating shown controls for items.</param>
        /// <param name="style">The style to use for this control.</param>
        /// <param name="manager">The <see cref="BaseScreenComponent"/>.</param>
        public ListControl(GenerateTemplateDelegate<T> templateGenerator, BaseScreenComponent? manager = null, string style = "")
            : base(manager, style)
        {
            CanFocus = true;
            TabStop = true;

            ItemCollection<T> collection = new ItemCollection<T>();
            collection.OnInserted += OnInsert;
            collection.OnRemove += (item, index) =>
            {
                if (SelectedItem == item)
                    SelectedItem = null;
                OnRemove(item, index);
            };
            Items = collection;
            TemplateGenerator = templateGenerator;

            _selectedItemBrush = null!;

            ApplySkin(typeof(ListControl<T>));
        }

        internal static Control? DefaultGenerateControl(BaseScreenComponent? component, string style,T? item)
        {
            if (item == null) 
                return null;
            return new Label(component, style) { Text = item.ToString() ?? string.Empty };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ListControl{T}"/> class.
        /// </summary>
        /// <param name="style">The style to use for this control.</param>
        /// <param name="manager">The <see cref="BaseScreenComponent"/>.</param>
        public ListControl(BaseScreenComponent? manager = null, string style = "")
            : this(item => DefaultGenerateControl(manager, style, item), manager, style)
        {
        }

        /// <summary>
        /// Gets called when an item is gonna be removed.
        /// </summary>
        /// <param name="item">The item to be removed.</param>
        /// <param name="index">The index the item is gonna be removed from.</param>
        protected abstract void OnRemove(T item, int index);

        /// <summary>
        /// Gets called when an item was added.
        /// </summary>
        /// <param name="item">The item that was added.</param>
        /// <param name="index">The index the item was added at.</param>
        protected abstract void OnInsert(T item, int index);

        /// <inheritdoc />
        protected override void OnKeyPress(KeyEventArgs args)
        {
            // Ignore when control is not focused
            if (Focused == TreeState.None) return;

            switch (args.Key)
            {
                case Keys.Up:
                    SelectPrevious();
                    args.Handled = true;
                    break;
                case Keys.Down:
                    SelectNext();
                    args.Handled = true;
                    break;
                case Keys.Home:
                    SelectFirst();
                    args.Handled = true;
                    break;
                case Keys.End:
                    SelectLast();
                    args.Handled = true;
                    break;
            }

            base.OnKeyPress(args);
        }

        /// <inheritdoc />
        public void SelectFirst()
        {
            if (Items.Count > 0)
                SelectedItem = Items[0];
        }

        /// <inheritdoc />
        public void SelectLast()
        {
            if (Items.Count > 0)
                SelectedItem = Items[^1];
        }

        /// <inheritdoc />
        public void SelectNext()
        {
            if (SelectedItem == null)
            {
                if (Items.Count > 0)
                    SelectedItem = Items[0];
            }
            else
            {
                int index = Items.IndexOf(SelectedItem);
                if (index < Items.Count - 1)
                    SelectedItem = Items[index + 1];
            }
        }

        /// <inheritdoc />
        public void SelectPrevious()
        {
            if (SelectedItem == null)
            {
                if (Items.Count > 0)
                    SelectedItem = Items[^1];
            }
            else
            {
                int index = Items.IndexOf(SelectedItem);
                if (index > 0)
                    SelectedItem = Items[index - 1];
            }
        }

        /// <summary>
        /// Raises the <see cref="SelectedItemChanged"/> event.
        /// </summary>
        /// <param name="args">A <see cref="SelectionEventArgs{T}"/> that contains the event data.</param>
        protected virtual void OnSelectedItemChanged(SelectionEventArgs<T> args) { }

        /// <summary>
        /// Raises the <see cref="SelectedItemBrushChanged"/> event.
        /// </summary>
        /// <param name="args">A <see cref="PropertyEventArgs{Brush}"/> that contains the event data.</param>
        protected virtual void OnSelectedItemBrushChanged(PropertyEventArgs<Brush> args) { }

        /// <summary>
        /// Occurs when the <see cref="SelectedItem"/> was changed.
        /// </summary>
        public event SelectionDelegate<T>? SelectedItemChanged;

        /// <summary>
        /// Occurs when the <see cref="SelectedItemBrushChanged"/> was changed.
        /// </summary>
        public event PropertyChangedDelegate<Brush>? SelectedItemBrushChanged;

        /// <summary>
        /// Gets or sets the <see cref="GenerateTemplateDelegate{T}"/> used for generating the container controls for
        /// the <see cref="ListControl{T}"/> items.
        /// </summary>
        public GenerateTemplateDelegate<T> TemplateGenerator { get; set; }
    }

    /// <summary>
    /// Represents a method that will be used to create a templated <see cref="Control"/> for a specified generic item.
    /// </summary>
    /// <param name="item">The item to generate the templated <see cref="Control"/> for.</param>
    /// <typeparam name="T">The type of the item to be templated.</typeparam>
    public delegate Control? GenerateTemplateDelegate<in T>(T? item);
}
