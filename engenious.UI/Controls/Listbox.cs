using System.Linq;
using engenious.Graphics;

namespace engenious.UI.Controls
{
    /// <summary>
    /// Ui control for generic list data.
    /// </summary>
    /// <typeparam name="T">Type of the contained elements</typeparam>
    public class Listbox<T> : ListControl<T> where T : class
    {
        /// <summary>
        /// Gets the <see cref="Controls.ScrollContainer"/> which contains the <see cref="StackPanel"/> for the items.
        /// </summary>
        public ScrollContainer ScrollContainer { get; }

        /// <summary>
        /// Gets the <see cref="Controls.StackPanel"/> that is used for the list items.
        /// </summary>
        public StackPanel StackPanel { get; }

        /// <summary>
        /// Gets or sets the orientation of the listed elements.
        /// </summary>
        public Orientation Orientation
        {
            get => StackPanel.Orientation;
            set
            {
                StackPanel.Orientation = value;
                switch (StackPanel.Orientation)
                {
                    case Orientation.Vertical:
                        StackPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
                        StackPanel.VerticalAlignment = VerticalAlignment.Top;
                        ScrollContainer.HorizontalScrollbarEnabled = false;
                        ScrollContainer.VerticalScrollbarEnabled = true;
                        break;
                    case Orientation.Horizontal:
                        StackPanel.HorizontalAlignment = HorizontalAlignment.Left;
                        StackPanel.VerticalAlignment = VerticalAlignment.Stretch;
                        ScrollContainer.HorizontalScrollbarEnabled = true;
                        ScrollContainer.VerticalScrollbarEnabled = false;
                        break;
                }
            }
        }

        /// <summary>
        /// Gets the container control containing the <see cref="ListControl{T}.SelectedItem"/>.
        /// </summary>
        private Control SelectedItemContainer => GetItemContainer(SelectedItem);

        /// <summary>
        /// Gets the container control containing a specific list element.
        /// </summary>
        /// <param name="item">The list element to get the associated control of.</param>
        /// <returns>The container control containing the given list item.</returns>
        private Control GetItemContainer(T item)
        {
            if (item != null)
                foreach (var c in StackPanel.Controls)
                {
                    if (c.Tag == item)
                        return c;
                }
            return null;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Listbox{T}"/> class.
        /// </summary>
        /// <param name="manager">The <see cref="BaseScreenComponent"/>.</param>
        /// <param name="style">The style to use for this control.</param>
        public Listbox(BaseScreenComponent manager, string style = "")
            : base(manager, style)
        {
            ScrollContainer = new ScrollContainer(manager)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            Children.Add(ScrollContainer);

            StackPanel = new StackPanel(manager);
            Orientation = Orientation.Vertical;
            ScrollContainer.Content = StackPanel;

            CanFocus = true;

            ApplySkin(typeof(Listbox<T>));
        }

        /// <inheritdoc />
        protected override void OnInsert(T item, int index)
        {
            Control control = TemplateGenerator(item);
            ContentControl wrapper = new ContentControl(ScreenManager)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Content = control
            };
       
            wrapper.Tag = item;
            StackPanel.Controls.Insert(index, wrapper);
        }

        /// <inheritdoc />
        protected override void OnRemove(T item, int index)
        {
            Control control = GetItemContainer(item);
            if (control != null)
                StackPanel.Controls.Remove(control);
            if (StackPanel.Controls.Count == 0)
                InvalidateDimensions();
        }

        /// <inheritdoc />
        protected override void OnDrawBackground(SpriteBatch batch, Rectangle backgroundArea, GameTime gameTime, float alpha)
        {
            base.OnDrawBackground(batch, backgroundArea, gameTime, alpha);

            // Draw background brush for selected item
            Control control = SelectedItemContainer;
            if (control != null)
            {
                SelectedItemBrush?.Draw(batch,
                    new Rectangle(control.AbsolutePosition, control.ActualSize), alpha);
            }
        }

        /// <inheritdoc />
        protected override void OnSelectedItemChanged(SelectionEventArgs<T> args)
        {
            base.OnSelectedItemChanged(args);

            if (args.NewItem != null)
                EnsureVisibility(args.NewItem);
        }

        /// <summary>
        /// Ensures that a given item is visible in the listbox by scrolling into the appropriate range.
        /// </summary>
        /// <param name="item">The item to show in visible range.</param>
        public void EnsureVisibility(T item)
        {
            Control container = GetItemContainer(item);
            Rectangle visibleArea = ScrollContainer.VisibleArea;

            // Element too far down -> scroll up
            if (container.ActualPosition.Y + container.ActualSize.Y > visibleArea.Bottom)
                ScrollContainer.VerticalScrollPosition =
                    container.ActualPosition.Y + container.ActualSize.Y - ActualClientSize.Y;

            // Element too far up -> scroll down
            if (container.ActualPosition.Y < visibleArea.Top)
                ScrollContainer.VerticalScrollPosition = container.ActualPosition.Y;

            // Element too far right -> scroll left
            if (container.ActualPosition.X + container.ActualSize.X > visibleArea.Right)
                ScrollContainer.HorizontalScrollPosition =
                    container.ActualPosition.X + container.ActualSize.X - ActualClientSize.X;

            // Element too far left -> scroll right
            if (container.ActualPosition.X < visibleArea.Left)
                ScrollContainer.HorizontalScrollPosition = container.ActualPosition.X;
        }

        /// <inheritdoc />
        protected override void OnLeftMouseClick(MouseEventArgs args)
        {
            base.OnLeftMouseClick(args);

            Control nextSelected = null;
            foreach (var item in StackPanel.Controls)
            {
                if (item.Hovered != TreeState.None)
                {
                    nextSelected = item;
                    break;
                }
            }

            if (nextSelected != null)
                SelectedItem = nextSelected.Tag as T;
            else SelectedItem = null;
        }
    }
}
