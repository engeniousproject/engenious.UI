using System.Collections.Generic;

namespace engenious.UI.Controls
{
    /// <summary>
    /// A ui container element with arbitrary placement of child controls.
    /// </summary>
    public class CanvasControl : Control
    {
        private readonly Dictionary<Control, Point> _positions = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="CanvasControl"/> class.
        /// </summary>
        /// <param name="manager">The <see cref="BaseScreenComponent"/>.</param>
        /// <param name="style">The style to use for this control.</param>
        public CanvasControl(BaseScreenComponent manager, string style = "")
            : base(manager, style)
        {
            ApplySkin(typeof(CanvasControl));
        }

        /// <summary>
        /// Adds a <see cref="Control"/> at a specific position to this container.
        /// </summary>
        /// <param name="control">The <see cref="Control"/> to add.</param>
        /// <param name="position">The position for the <see cref="Control"/>.</param>
        public void AddControl(Control control, Point position)
        {
            Children.Add(control);
            _positions[control] = position;
        }

        /// <summary>
        /// Removes a <see cref="Control"/> from this container.
        /// </summary>
        /// <param name="control">The <see cref="Control"/> to remove.</param>
        public void RemoveControl(Control control)
        {
            Children.Remove(control);
        }

        /// <inheritdoc />
        protected override void OnInsertControl(CollectionEventArgs args)
        {
            _positions.Add(args.Control, Point.Zero);
            base.OnInsertControl(args);
        }

        /// <inheritdoc />
        protected override void OnRemoveControl(CollectionEventArgs args)
        {
            _positions.Remove(args.Control);
            base.OnRemoveControl(args);
        }

        /// <inheritdoc />
        public override Point GetExpectedSize(Point available)
        {
            return available;
        }

        /// <inheritdoc />
        public override void SetActualSize(Point available)
        {
            if (!Visible)
            {
                SetDimension(Point.Zero, available);
                return;
            }

            SetDimension(available, available);

            // Auf andere Controls anwenden
            foreach (var child in Children)
            {
                child.SetActualSize(ActualClientSize);
                child.ActualPosition = _positions[child];
            }

        }

        private bool FlyoutActive()
        {
            foreach (var child in Children)
            {
                if (child.Hovered == TreeState.Active || child.Hovered == TreeState.Passive)
                    return true;
            }

            return false;
        }

        /// <inheritdoc />
        protected override void OnLeftMouseDown(MouseEventArgs args)
        {
            args.Handled |= FlyoutActive();
            base.OnLeftMouseDown(args);
        }

        /// <inheritdoc />
        protected override void OnRightMouseDown(MouseEventArgs args)
        {
            args.Handled |= FlyoutActive();
            base.OnLeftMouseDown(args);
        }

        /// <inheritdoc />
        protected override void OnMouseEnter(MouseEventArgs args)
        {
            args.Handled |= FlyoutActive();
            base.OnMouseEnter(args);
        }

        /// <inheritdoc />
        protected override void OnMouseMove(MouseEventArgs args)
        {
            args.Handled |= FlyoutActive();
            base.OnMouseMove(args);
        }

        /// <inheritdoc />
        protected override void OnMouseLeave(MouseEventArgs args)
        {
            args.Handled |= FlyoutActive();
            base.OnMouseLeave(args);
        }

        /// <inheritdoc />
        protected override void OnLeftMouseUp(MouseEventArgs args)
        {
            args.Handled |= FlyoutActive();
            base.OnLeftMouseUp(args);
        }

        /// <inheritdoc />
        protected override void OnLeftMouseDoubleClick(MouseEventArgs args)
        {
            args.Handled |= FlyoutActive();
            base.OnLeftMouseDoubleClick(args);
        }

        /// <inheritdoc />
        protected override void OnLeftMouseClick(MouseEventArgs args)
        {
            args.Handled |= FlyoutActive();
            base.OnLeftMouseClick(args);
        }

        /// <inheritdoc />
        protected override void OnRightMouseUp(MouseEventArgs args)
        {
            args.Handled |= FlyoutActive();
            base.OnRightMouseUp(args);
        }

        /// <inheritdoc />
        protected override void OnRightMouseClick(MouseEventArgs args)
        {
            args.Handled |= FlyoutActive();
            base.OnRightMouseClick(args);
        }

        /// <inheritdoc />
        protected override void OnRightMouseDoubleClick(MouseEventArgs args)
        {
            args.Handled |= FlyoutActive();
            base.OnRightMouseDoubleClick(args);
        }
    }
}
