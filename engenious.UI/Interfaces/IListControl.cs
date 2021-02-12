namespace engenious.UI
{
    /// <summary>
    /// Common interface list controls.
    /// </summary>
    public interface IListControl : IControl
    {
        /// <summary>
        /// Gets or sets the brush to use to highlight selected items.
        /// </summary>
        Brush SelectedItemBrush { get; set; }

        /// <summary>
        /// Select the first item.
        /// </summary>
        void SelectFirst();

        /// <summary>
        /// Select the last item.
        /// </summary>
        void SelectLast();

        /// <summary>
        /// Select the next item relative to the currently selected one.
        /// </summary>
        void SelectNext();

        /// <summary>
        /// Select the previous item relative to the currently selected one.
        /// </summary>
        void SelectPrevious();
    }
}
