using System.Collections.Generic;

namespace engenious.UI.Controls
{
    public class CanvasControl : Control
    {
        private Dictionary<Control, Point> positions = new Dictionary<Control, Point>();

        public CanvasControl(BaseScreenComponent manager, string style = "")
            : base(manager, style)
        {
            ApplySkin(typeof(CanvasControl));
        }

        public void AddControl(Control control, Point position)
        {
            Children.Add(control);
            positions[control] = position;
        }

        public void RemoveControl(Control control)
        {
            Children.Remove(control);
        }

        protected override void OnInsertControl(CollectionEventArgs args)
        {
            positions.Add(args.Control, Point.Zero);
            base.OnInsertControl(args);
        }

        protected override void OnRemoveControl(CollectionEventArgs args)
        {
            positions.Remove(args.Control);
            base.OnRemoveControl(args);
        }

        public override Point GetExpectedSize(Point available)
        {
            return available;
        }

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
                child.ActualPosition = positions[child];
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

        protected override void OnLeftMouseDown(MouseEventArgs args)
        {
            args.Handled |= FlyoutActive();
            base.OnLeftMouseDown(args);
        }

        protected override void OnRightMouseDown(MouseEventArgs args)
        {
            args.Handled |= FlyoutActive();
            base.OnLeftMouseDown(args);
        }

        protected override void OnMouseEnter(MouseEventArgs args)
        {
            args.Handled |= FlyoutActive();
            base.OnMouseEnter(args);
        }

        protected override void OnMouseMove(MouseEventArgs args)
        {
            args.Handled |= FlyoutActive();
            base.OnMouseMove(args);
        }

        protected override void OnMouseLeave(MouseEventArgs args)
        {
            args.Handled |= FlyoutActive();
            base.OnMouseLeave(args);
        }

        protected override void OnLeftMouseUp(MouseEventArgs args)
        {
            args.Handled |= FlyoutActive();
            base.OnLeftMouseUp(args);
        }

        protected override void OnLeftMouseDoubleClick(MouseEventArgs args)
        {
            args.Handled |= FlyoutActive();
            base.OnLeftMouseDoubleClick(args);
        }

        protected override void OnLeftMouseClick(MouseEventArgs args)
        {
            args.Handled |= FlyoutActive();
            base.OnLeftMouseClick(args);
        }

        protected override void OnRightMouseUp(MouseEventArgs args)
        {
            args.Handled |= FlyoutActive();
            base.OnRightMouseUp(args);
        }

        protected override void OnRightMouseClick(MouseEventArgs args)
        {
            args.Handled |= FlyoutActive();
            base.OnRightMouseClick(args);
        }

        protected override void OnRightMouseDoubleClick(MouseEventArgs args)
        {
            args.Handled |= FlyoutActive();
            base.OnRightMouseDoubleClick(args);
        }
    }
}
