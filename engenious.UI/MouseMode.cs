namespace engenious.UI
{
    /// <summary>
    /// Specifies possible mouse modes.
    /// </summary>
    public enum MouseMode
    {
        /// <summary>
        /// The mouse cursor is hidden and mouse event data only contains delta of mouse movements and no absolute positions.
        /// </summary>
        Captured,

        /// <summary>
        /// The cursor is freely movable and visible.
        /// </summary>
        Free
    }
}
