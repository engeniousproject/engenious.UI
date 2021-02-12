using System;
using System.Collections.Generic;
using System.Linq;

namespace engenious.UI
{
    /// <summary>
    /// Specialized list for containing controls, with different orderings.
    /// </summary>
    public class ControlCollection : ItemCollection<Control>
    {
        internal readonly ItemCollection<Control> InZOrder;
        internal readonly ReverseItemCollectionEnumerable<Control> AgainstZOrder = new ReverseItemCollectionEnumerable<Control>();

        /// <summary>
        /// The <see cref="Control"/> that owns this collection.
        /// </summary>
        protected Control Owner { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlCollection"/> class.
        /// </summary>
        /// <param name="owner">The <see cref="Control"/> that owns this collection.</param>
        public ControlCollection(Control owner)
            : base()
        {
            Owner = owner;
            InZOrder = new ItemCollection<Control>();
            AgainstZOrder.BaseList = InZOrder;
        }

        /// <inheritdoc />
        public override void Add(Control item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (item.Parent != null)
                throw new ArgumentException("Item is already part of a different collection");

            // Focus management
            item.SetFocus(null);

            // Insert in tab order.
            if (item.TabOrder == 0)
                item.TabOrder = int.MaxValue;
            else
                foreach (var control in this)
                {
                    if (control.TabOrder >= item.TabOrder) control.TabOrder++;
                }

            // Insert in ZOrder
            InZOrder.Add(item);//TODO insertion sort
            item.ZOrderChanged += item_ZOrderChanged;

            base.Add(item);

            item.Parent = Owner;
            ReorderTab();
            ReorderZ(item);
            item.PathDirty = true;
            Owner.PathDirty = true;
        }

        /// <inheritdoc />
        public override void Clear()
        {
            //TODO verify?
            for (int i = 0; i < Count; i++)
            {
                this[i].SetFocus(null);
                this[i].Parent = null;
                this[i].ZOrderChanged -= item_ZOrderChanged;
                this[i].PathDirty = true;
            }
            base.Clear();
            InZOrder.Clear();

            Owner.PathDirty = true;

            ReorderZ(null);
        }

        /// <inheritdoc />
        public override void Insert(int index, Control item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (item.Parent != null)
                throw new ArgumentException("Item is already part of a different collection");

            // Focus management
            item.SetFocus(null);

            // Tab order
            if (item.TabOrder == 0)
                item.TabOrder = index;
            foreach (var control in this)
            {
                if (control.TabOrder >= item.TabOrder) control.TabOrder++;
            }

            // Insert in ZOrder
            InZOrder.Add(item);//TODO: insert sort?

            item.ZOrderChanged += item_ZOrderChanged;

            base.Insert(index, item);

            item.Parent = Owner;
            ReorderTab();
            ReorderZ(item);
            item.PathDirty = true;
            Owner.PathDirty = true;
        }

        /// <inheritdoc />
        public override bool Remove(Control item)
        {
            if (base.Remove(item))
            {
                item.SetFocus(null);

                InZOrder.Remove(item);

                item.Parent = null;
                item.ZOrderChanged -= item_ZOrderChanged;

                ReorderTab();
                ReorderZ(null);

                item.PathDirty = true;
                Owner.PathDirty = true;
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public override void RemoveAt(int index)
        {
            // Remove focus
            Control c = this[index];
            if (c != null)
            {
                c.SetFocus(null);
                base.RemoveAt(index);

                InZOrder.Remove(c);
                c.Parent = null;
                c.ZOrderChanged -= item_ZOrderChanged;
                ReorderTab();
                ReorderZ(null);
                c.PathDirty = true;
                Owner.PathDirty = true;
            }
        }

        private readonly List<Control> _tabOrderTempList = new List<Control>();

        private class TabOrderComparer : IComparer<Control>
        {
            public int Compare(Control x, Control y) =>x.TabOrder.CompareTo(y.TabOrder);
        }

        private static readonly IComparer<Control> _tabOrderComparer = new TabOrderComparer();
        private void ReorderTab()
        {
            // Recalculate tab order
            int tab = 1;
            foreach (var item in _tabOrderTempList)
            {
                _tabOrderTempList.Add(item);
            }
            _tabOrderTempList.Sort(_tabOrderComparer);
            foreach (var control in _tabOrderTempList)
                control.TabOrder = tab++;
        }

        void item_ZOrderChanged(Control sender, PropertyEventArgs<int> args)
            => ReorderZ(sender); // A control changed its ZOrder -> resort ZOrder

        private class ZOrderComparer : IComparer<Control>
        {
            public int Compare(Control x, Control y)
            {
                if (x == null || y == null)
                    return 0;
                return x.ZOrder.CompareTo(y.ZOrder);
            }
        }
        private readonly ZOrderComparer _zOrderComparer = new ZOrderComparer();
        private bool _isDoingUpdate = false;
        private void ReorderZ(Control control)
        {
            if (_isDoingUpdate) return; // Cancel if we are already reordering

            _isDoingUpdate = true;

            // move controls to fit given control
            if (control != null)
                foreach (var c in this)
                {
                    if (c != control && c.ZOrder >= control.ZOrder) c.ZOrder++;
                }

            InZOrder.Sort(_zOrderComparer);
            // Recalculate tab order
            int index = 1;
            foreach (var c in InZOrder)
                c.TabOrder = index++;

            //AgainstZOrder.BaseList = InZOrder;

            _isDoingUpdate = false;
        }
    }
}
