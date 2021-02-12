namespace engenious.UI.Interfaces
{
    /// <summary>
    /// Interface for common combobox properties.
    /// </summary>
    public interface ICombobox : IListControl
    {
        /// <summary>
        /// Gets a value indicating whether the dropdown is open or not.
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> used to render the drop down button with when the dropdown is open.
        /// </summary>
        Brush ButtonBrushOpen { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> used to render the drop down button with when the dropdown is closed.
        /// </summary>
        Brush ButtonBrushClose { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> used to render the dropdown background.
        /// </summary>
        Brush DropdownBackgroundBrush { get; set; }
    }
}
