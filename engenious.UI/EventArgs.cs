using engenious.Graphics;
using engenious.Input;
using engenious.UI.Controls;

namespace engenious.UI
{
    /// <summary>
    /// Base class for all event data of UI events.
    /// </summary>
    public class EventArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether the event was already handled.
        /// </summary>
        public bool Handled { get; set; }
    }

    /// <summary>
    /// Event data for DragDrop events
    /// </summary>
    public class DragEventArgs : PointerEventArgs
    {
        /// <summary>
        /// Gets or sets the control which raised the event containing the event data.
        /// </summary>
        public Control Sender { get; set; }

        /// <summary>
        /// Gets or sets a optional <see cref="Texture2D"/> containing the texture that should be rendered while dragging.
        /// </summary>
        public Texture2D Icon { get; set; }

        /// <summary>
        /// Gets or sets the size of the <see cref="Icon"/> to show while dragging.
        /// </summary>
        public Point IconSize { get; set; }

        /// <summary>
        /// Gets or sets the content that is dragged.
        /// </summary>
        public object Content { get; set; }
    }

    /// <summary>
    /// Base class for position based events (Mouse, Touch)
    /// </summary>
    public abstract class PointerEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether the event bubbled up from a child control.
        /// </summary>
        public bool Bubbled { get; set; }

        /// <summary>
        /// Gets or sets the position of the mouse pointer relative to the current control.
        /// </summary>
        public Point LocalPosition { get; set; }

        /// <summary>
        /// Gets or sets the position of the mouse pointer in global screen-space.
        /// </summary>
        public Point GlobalPosition { get; set; }
    }

    /// <summary>
    /// Event data for property changed events.
    /// </summary>
    /// <typeparam name="T">The type of the changed property.</typeparam>
    public class PropertyEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Gets or sets the old value of the changed property.
        /// </summary>
        public T OldValue { get; set; }

        /// <summary>
        /// Gets or sets the new value of the changed property.
        /// </summary>
        public T NewValue { get; set; }
    }
    
    /// <summary>
    /// Event data for mouse events.
    /// </summary>
    public class MouseEventArgs : PointerEventArgs
    {
        /// <summary>
        /// Gets or sets the current <see cref="UI.MouseMode"/> of the mouse.
        /// </summary>
        public MouseMode MouseMode { get; set; }
    }

    /// <summary>
    /// Event data for mouse scroll events.
    /// </summary>
    public class MouseScrollEventArgs : MouseEventArgs
    {
        /// <summary>
        /// Gets or sets a value indicating the number of steps that where scrolled.
        /// </summary>
        public int Steps { get; set; }
    }

    /// <summary>
    /// Event data for touch events.
    /// </summary>
    public class TouchEventArgs : PointerEventArgs
    {
        /// <summary>
        /// Gets or sets a value indicating the ID of the touch point.
        /// </summary>
        public int TouchId { get; set; }
    }

    /// <summary>
    /// Event data for key events.
    /// </summary>
    public class KeyEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether a shift key is pressed.
        /// </summary>
        public bool Shift { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a control key is pressed.
        /// </summary>
        public bool Ctrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a alt key is pressed.
        /// </summary>
        public bool Alt { get; set; }

        /// <summary>
        /// Gets or sets the key that raised the event.
        /// </summary>
        public Keys Key { get; set; }
    }

    /// <summary>
    /// Event data for text input events.
    /// </summary>
    public class KeyTextEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the typed character.
        /// </summary>
        public char Character { get; set; }
    }

    /// <summary>
    /// Event data for selection change events.
    /// </summary>
    public class SelectionEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Gets or sets the previously selected element.
        /// </summary>
        public T OldItem { get; set; }

        /// <summary>
        /// Gets or sets the newly selected element.
        /// </summary>
        public T NewItem { get; set; }
    }

    /// <summary>
    /// Event data for collection change events.
    /// </summary>
    public class CollectionEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the control that was affected.
        /// </summary>
        public Control Control { get; set; }

        /// <summary>
        /// Gets or sets the index of the element that was affected.
        /// </summary>
        public int Index { get; set; }
    }

    /// <summary>
    /// Event data for navigation events.
    /// </summary>
    public class NavigationEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether the navigation should be canceled.
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets or sets the parameter given by the previous screen.
        /// </summary>
        public object Parameter { get; set; }

        /// <summary>
        /// Gets or sets the secondary <see cref="UI.Controls.Screen"/> that takes part in the navigation.
        /// </summary>
        public Screen Screen { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether it is a navigation backwards.
        /// </summary>
        public bool IsBackNavigation { get; set; }
    }

    /// <summary>
    /// Represents the method that will handle generic events without additional parameters.
    /// </summary>
    /// <param name="sender">The control that raised the event.</param>
    /// <param name="args">The event data for the event.</param>
    public delegate void EventDelegate(Control sender, EventArgs args);

    /// <summary>
    /// Represents the method that will handle drag events for the <see cref="BaseScreenComponent"/>.
    /// </summary>
    /// <param name="sender">The control that raised the event.</param>
    /// <param name="args">The event data for the event.</param>
    public delegate void DragEventDelegate(Control sender, DragEventArgs args);

    /// <summary>
    /// Represents the method that will handle drag events for the <see cref="BaseScreenComponent"/>.
    /// </summary>
    /// <param name="args">The event data for the event.</param>
    public delegate void DragEventBaseDelegate(DragEventArgs args);

    /// <summary>
    /// Represents the method that will handle mouse events.
    /// </summary>
    /// <param name="sender">The control that raised the event.</param>
    /// <param name="args">The event data for the event.</param>
    public delegate void MouseEventDelegate(Control sender, MouseEventArgs args);

    /// <summary>
    /// Represents the method that will handle mouse events for the <see cref="BaseScreenComponent"/>.
    /// </summary>
    /// <param name="args">The event data for the event.</param>
    public delegate void MouseEventBaseDelegate(MouseEventArgs args);

    /// <summary>
    /// Represents the method that will handle mouse scroll events.
    /// </summary>
    /// <param name="sender">The control that raised the event.</param>
    /// <param name="args">The event data for the event.</param>
    public delegate void MouseScrollEventDelegate(Control sender, MouseScrollEventArgs args);

    /// <summary>
    /// Represents the method that will handle mouse scroll events for the <see cref="BaseScreenComponent"/>.
    /// </summary>
    /// <param name="args">The event data for the event.</param>
    public delegate void MouseScrollEventBaseDelegate(MouseScrollEventArgs args);

    /// <summary>
    /// Represents the method that will handle touch events.
    /// </summary>
    /// <param name="sender">The control that raised the event.</param>
    /// <param name="args">The event data for the event.</param>
    public delegate void TouchEventDelegate(Control sender, TouchEventArgs args);

    /// <summary>
    /// Represents the method that will handle touch events for the <see cref="BaseScreenComponent"/>.
    /// </summary>
    /// <param name="args">The event data for the event.</param>
    public delegate void TouchEventBaseDelegate(TouchEventArgs args);

    /// <summary>
    /// Represents the method that will handle key events.
    /// </summary>
    /// <param name="sender">The control that raised the event.</param>
    /// <param name="args">The event data for the event.</param>
    public delegate void KeyEventDelegate(Control sender, KeyEventArgs args);

    /// <summary>
    /// Represents the method that will handle key events for the <see cref="BaseScreenComponent"/>.
    /// </summary>
    /// <param name="args">The event data for the event.</param>
    public delegate void KeyEventBaseDelegate(KeyEventArgs args);

    /// <summary>
    /// Represents the method that will handle text input events.
    /// </summary>
    /// <param name="sender">The control that raised the event.</param>
    /// <param name="args">The event data for the event.</param>
    public delegate void KeyTextEventDelegate(Control sender, KeyTextEventArgs args);

    /// <summary>
    /// Represents the method that will handle selection changed events.
    /// </summary>
    /// <typeparam name="T">The type of the items in the collection.</typeparam>
    /// <param name="sender">The control that raised the event.</param>
    /// <param name="args">The event data for the event.</param>
    public delegate void SelectionDelegate<T>(Control sender, SelectionEventArgs<T> args);

    /// <summary>
    /// Represents the method that will handle collection changed events.
    /// </summary>
    /// <param name="sender">The control that raised the event.</param>
    /// <param name="args">The event data for the event.</param>
    public delegate void CollectionDelegate(Control sender, CollectionEventArgs args);

    /// <summary>
    /// Represents the method that will handle property changed events.
    /// </summary>
    /// <typeparam name="T">The type of the changed property.</typeparam>
    /// <param name="sender">The control that raised the event.</param>
    /// <param name="args">The event data for the event.</param>
    public delegate void PropertyChangedDelegate<T>(Control sender, PropertyEventArgs<T> args);
}
