using engenious.Audio;
using engenious.Graphics;

namespace engenious.UI
{
    /// <summary>
    /// Common interface for controls.
    /// </summary>
    public interface IControl
    {
        /// <summary>
        /// Gets or sets the reference to the current screen manager.
        /// </summary>
        BaseScreenComponent ScreenManager { get; }

        /// <summary>
        /// Gets or sets the sound to be played on click.
        /// </summary>
        SoundEffect? ClickSound { get; set; }

        /// <summary>
        /// Gets or sets the sound to be played on hover.
        /// </summary>
        SoundEffect? HoverSound { get; set; }

        /// <summary>
        /// Gets or sets the background <see cref="Brush"/> for this control.
        /// </summary>
        Brush Background { get; set; }

        /// <summary>
        /// Gets or sets the background <see cref="Brush"/> for this control when it is in hover state.
        /// </summary>
        Brush HoveredBackground { get; set; }

        /// <summary>
        /// Gets or sets the background <see cref="Brush"/> for this control when it is pressed.
        /// </summary>
        Brush PressedBackground { get; set; }

        /// <summary>
        /// Gets or sets the background <see cref="Brush"/> for this control when it is disabled.
        /// </summary>
        Brush DisabledBackground { get; set; }

        /// <summary>
        /// Gets or sets the outer spacing of the control.
        /// </summary>
        Border Margin { get; set; }

        /// <summary>
        /// Gets or sets the inner spacing of the control.
        /// </summary>
        Border Padding { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a focus frame should be drawn.
        /// </summary>
        bool DrawFocusFrame { get; set; }

        /// <summary>
        /// Gets or sets arbitrary user data.
        /// </summary>
        object? Tag { get; set; }

        /// <summary>
        /// Gets the style name of the control.
        /// </summary>
        string Style { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the control is enabled.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Gets a value indicating whether the control is enabled and not made disabled indirectly by its parent controls.
        /// </summary>
        bool AbsoluteEnabled { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the control is visible.
        /// </summary>
        bool Visible { get; set; }

        /// <summary>
        /// Gets a value indicating whether the control is visible and not made invisible indirectly by its parent controls.
        /// </summary>
        bool AbsoluteVisible { get; }

        /// <summary>
        /// Gets the root control this control is a direct or indirect child of.
        /// </summary>
        Control Root { get; }

        /// <summary>
        /// Gets an enumerable path from the <see cref="Root"/> control through its children to this control.
        /// </summary>
        ReverseEnumerable<Control> RootPath { get; }

        /// <summary>
        /// Gets the parent control this control is a child of.
        /// </summary>
        Control? Parent { get; }

        /// <summary>
        /// Gets or sets the local transformation for this control.
        /// </summary>
        Matrix Transformation { get; set; }

        /// <summary>
        /// Gets the absolute transformation by factoring in all its parents transformations.
        /// </summary>
        Matrix AbsoluteTransformation { get; }

        /// <summary>
        /// Gets or sets the alpha value for this control.
        /// </summary>
        float Alpha { get; set; }

        /// <summary>
        /// Gets or sets the absolute alpha value by factoring in all its parents alpha values.
        /// </summary>
        float AbsoluteAlpha { get; }

        /// <summary>
        /// Gets or sets the controls alignment on the horizontal axis.
        /// </summary>
        HorizontalAlignment HorizontalAlignment { get; set; }

        /// <summary>
        /// Gets or sets the controls alignment on the vertical axis.
        /// </summary>
        VerticalAlignment VerticalAlignment { get; set; }

        /// <summary>
        /// Gets or sets the optional minimum width.
        /// <remarks><c>null</c> indicates that there is no specific minimum width for this control.</remarks>
        /// </summary>
        int? MinWidth { get; set; }

        /// <summary>
        /// Gets or sets the optional width.
        /// <remarks><c>null</c> indicates that there is no specific width for this control.</remarks>
        /// </summary>
        int? Width { get; set; }

        /// <summary>
        /// Gets or sets the optional maximum width.
        /// <remarks><c>null</c> indicates that there is no specific maximum width for this control.</remarks>
        /// </summary>
        int? MaxWidth { get; set; }

        /// <summary>
        /// Gets or sets the optional minimum height.
        /// <remarks><c>null</c> indicates that there is no specific minimum height for this control.</remarks>
        /// </summary>
        int? MinHeight { get; set; }

        /// <summary>
        /// Gets or sets the optional height.
        /// <remarks><c>null</c> indicates that there is no specific height for this control.</remarks>
        /// </summary>
        int? Height { get; set; }

        /// <summary>
        /// Gets or sets the optional maximum height.
        /// <remarks><c>null</c> indicates that there is no specific maximum height for this control.</remarks>
        /// </summary>
        int? MaxHeight { get; set; }

        /// <summary>
        /// Gets or sets the controls actual render position.
        /// <remarks>This value is relative to its parent control.</remarks>
        /// </summary>
        Point ActualPosition { get; set; }

        /// <summary>
        /// Gets or sets the controls absolute position.
        /// <remarks>This value is absolute.</remarks>
        /// </summary>
        Point AbsolutePosition { get; }

        /// <summary>
        /// Gets or sets the controls actual render size.
        /// </summary>
        Point ActualSize { get; set; }

        /// <summary>
        /// Gets the actual render client area.
        /// <remarks>This value is relative to its parent control.</remarks>
        /// </summary>
        Rectangle ActualClientArea { get; }

        /// <summary>
        /// Gets the available client size using the <see cref="ActualSize"/> excluding <see cref="Margin"/> and <see cref="Padding"/>.
        /// </summary>
        Point ActualClientSize { get; }

        /// <summary>
        /// Gets the combined border of margin and padding.
        /// </summary>
        Point Borders { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the control is under a mouse cursor.
        /// </summary>
        TreeState Hovered { get; }

        /// <summary>
        /// Gets a value indicating whether a mouse button is currently pressed over this control.
        /// </summary>
        bool Pressed { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this control can get focus by switching focus using the Tabulator-key.
        /// </summary>
        bool TabStop { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the control can get focus.
        /// </summary>
        bool CanFocus { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the order to go through the controls with the Tabulator-key.
        /// </summary>
        int TabOrder { get; set; }

        /// <summary>
        /// Gets the controls <see cref="TreeState"/> indicated whether it is actively, passively focused, or not focused at all.
        /// </summary>
        TreeState Focused { get; }

        /// <summary>
        /// Gets or sets the render layer order of this control.
        /// <remarks><c>0</c> is the top most layer, whereas higher values are further in the back.</remarks>
        /// </summary>
        int ZOrder { get; set; }

        /// <summary>
        /// Updates this control.
        /// </summary>
        /// <param name="gameTime">The elapsed game time since the last update and the total elapsed game time.</param>
        void Update(GameTime gameTime);

        /// <summary>
        /// Do all pre-draw calculations.
        /// </summary>
        /// <param name="gameTime">The elapsed game time since the last pre-draw and the total elapsed game time.</param>
        void PreDraw(GameTime gameTime);

        /// <summary>
        /// Draw the control.
        /// </summary>
        /// <param name="batch">
        /// The spritebatch to draw to.
        /// <remarks>
        /// The spritebatch needs to be in drawing mode. Meaning <see cref="SpriteBatch.Begin"/> was called but
        /// <see cref="SpriteBatch.End"/> was not yet called (again) after the <see cref="SpriteBatch.Begin"/>.
        /// </remarks>
        /// </param>
        /// <param name="renderMask">The region the control gets drawn in. Everything outside of that region is cut and not rendered.</param>
        /// <param name="gameTime">Contains the elapsed time since the last pre draw, as well as total elapsed time.</param>
        void Draw(SpriteBatch batch, Rectangle renderMask, GameTime gameTime);

        /// <summary>
        /// Invalidates the drawn content and forces a redraw.
        /// </summary>
        void InvalidateDrawing();

        /// <summary>
        /// Occurs when the <see cref="Parent"/> property of this control was changed.
        /// </summary>
        event PropertyChangedDelegate<Control> ParentChanged;

        /// <summary>
        /// Occurs when the <see cref="Enabled"/> property of this control was changed.
        /// </summary>
        event PropertyChangedDelegate<bool> EnableChanged;

        /// <summary>
        /// Occurs when the <see cref="Visible"/> property of this control was changed.
        /// </summary>
        event PropertyChangedDelegate<bool> VisibleChanged;

        /// <summary>
        /// Calculates the needed space for the control.
        /// </summary>
        /// <param name="available">The available space for the control.</param>
        /// <returns>Needed space including borders.</returns>
        Point GetExpectedSize(Point available);

        /// <summary>
        /// Gets the maximum needed client size of the control.
        /// </summary>
        /// <param name="containerSize">The size of the parent container.</param>
        /// <returns>The maximum client size.</returns>
        Point GetMaxClientSize(Point containerSize);

        /// <summary>
        /// Gets the minimal needed client size of the control.
        /// </summary>
        /// <param name="containerSize">The size of the parent container.</param>
        /// <returns>The minimal client size.</returns>
        Point GetMinClientSize(Point containerSize);

        /// <summary>
        /// Sets the actual size of this control.
        /// </summary>
        /// <param name="available">The expected size for this control (including borders)</param>
        void SetActualSize(Point available);

        /// <summary>
        /// Gets a value indicating whether the size values are invalid and need to be recalculated.
        /// </summary>
        /// <returns></returns>
        bool HasInvalidDimensions();

        /// <summary>
        /// Gets the required space for the controls content.
        /// </summary>
        /// <returns>The required client space.</returns>
        Point CalculateRequiredClientSpace(Point available);

        /// <summary>
        /// Invalidates the controls dimensions and forces a recalculation.
        /// </summary>
        void InvalidateDimensions();

        /// <summary>
        /// Gets called when the resolution of the rendering surface was changed.
        /// </summary>
        void OnResolutionChanged();

        /// <summary>
        /// Starts playing the given transition.
        /// </summary>
        /// <param name="transition">The transition to start playing.</param>
        void StartTransition(Transition transition);

        /// <summary>
        /// Occurs when the mouse cursor enters the controls region.
        /// </summary>
        event MouseEventDelegate MouseEnter;
        /// <summary>
        /// Occurs when the mouse cursor leaves the controls region.
        /// </summary>
        event MouseEventDelegate MouseLeave;
        /// <summary>
        /// Occurs when the mouse cursor moves over the controls region.
        /// </summary>
        event MouseEventDelegate MouseMove;

        /// <summary>
        /// Occurs when the left mouse button was pressed on this control.
        /// </summary>
        event MouseEventDelegate LeftMouseDown;

        /// <summary>
        /// Occurs when the left mouse button was released on this control.
        /// </summary>
        event MouseEventDelegate LeftMouseUp;

        /// <summary>
        /// Occurs when the left mouse button was clicked on this control.
        /// </summary>
        event MouseEventDelegate LeftMouseClick;

        /// <summary>
        /// Occurs when the left mouse button was double clicked on this control.
        /// </summary>
        event MouseEventDelegate LeftMouseDoubleClick;

        /// <summary>
        /// Occurs when the right mouse button was pressed on this control.
        /// </summary>
        event MouseEventDelegate RightMouseDown;

        /// <summary>
        /// Occurs when the right mouse button was released on this control.
        /// </summary>
        event MouseEventDelegate RightMouseUp;

        /// <summary>
        /// Occurs when the right mouse button was clicked on this control.
        /// </summary>
        event MouseEventDelegate RightMouseClick;

        /// <summary>
        /// Occurs when the right mouse button was double clicked on this control.
        /// </summary>
        event MouseEventDelegate RightMouseDoubleClick;

        /// <summary>
        /// Occurs when the mouse scroll wheel was scrolled on this control.
        /// </summary>
        event MouseScrollEventDelegate MouseScroll;

        /// <summary>
        /// Occurs when touch input pressed was recognized on this control.
        /// </summary>
        event TouchEventDelegate TouchDown;

        /// <summary>
        /// Occurs when touch input move was recognized on this control.
        /// </summary>
        event TouchEventDelegate TouchMove;

        /// <summary>
        /// Occurs when touch input release was recognized on this control.
        /// </summary>
        event TouchEventDelegate TouchUp;

        /// <summary>
        /// Occurs when touch input tap was recognized on this control.
        /// </summary>
        event TouchEventDelegate TouchTap;

        /// <summary>
        /// Occurs when touch input double tap was recognized on this control.
        /// </summary>
        event TouchEventDelegate TouchDoubleTap;

        /// <summary>
        /// Occurs when the <see cref="Hovered"/> property was changed.
        /// </summary>
        event PropertyChangedDelegate<TreeState> HoveredChanged;

        /// <summary>
        /// Occurs when a key was pressed.
        /// </summary>
        event KeyEventDelegate KeyDown;

        /// <summary>
        /// Occurs when a key was released.
        /// </summary>
        event KeyEventDelegate KeyUp;

        /// <summary>
        /// Occurs when a key is pressed.
        /// </summary>
        event KeyEventDelegate KeyPress;

        /// <summary>
        /// Occurs on a key text input.
        /// </summary>
        event KeyTextEventDelegate KeyTextPress;

        /// <summary>
        /// Set the focus active on this control.
        /// </summary>
        void Focus();

        /// <summary>
        /// Removes the focus from this control.
        /// </summary>
        void Unfocus();

        /// <summary>
        /// Occurs when the <see cref="TabStop"/> property was changed.
        /// </summary>
        event PropertyChangedDelegate<bool> TabStopChanged;

        /// <summary>
        /// Occurs when the <see cref="CanFocus"/> property was changed.
        /// </summary>
        event PropertyChangedDelegate<bool> CanFocusChanged;

        /// <summary>
        /// Occurs when the <see cref="TabOrder"/> property was changed.
        /// </summary>
        event PropertyChangedDelegate<int> TabOrderChanged;

        /// <summary>
        /// Occurs when the <see cref="ZOrder"/> property was changed.
        /// </summary>
        event PropertyChangedDelegate<int> ZOrderChanged;

        /// <summary>
        /// Occurs when the control got focus.
        /// </summary>
        event EventDelegate GotFocus;

        /// <summary>
        /// Occurs when the control lost focus.
        /// </summary>
        event EventDelegate LostFocus;

        /// <summary>
        /// Occurs on the beginning of dragging data from this control.
        /// </summary>
        event DragEventDelegate StartDrag;

        /// <summary>
        /// Occurs while dragging data over this control.
        /// </summary>
        event DragEventDelegate DropMove;

        /// <summary>
        /// Occurs while dragging data into this controls regions.
        /// </summary>
        event DragEventDelegate DropEnter;

        /// <summary>
        /// Occurs while dragging data out of this controls regions.
        /// </summary>
        event DragEventDelegate DropLeave;

        /// <summary>
        /// Occurs while dropping data over this control.
        /// </summary>
        event DragEventDelegate EndDrop;
    }
}