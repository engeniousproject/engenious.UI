using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace engenious.UI
{


    /// <summary>
    /// Erweiterte Liste für Controls
    /// </summary>
    public class ItemCollection<T> : IList<T> where T : class
    {
        private struct PostponedAction
        {
            public enum ActionType
            {
                Add,
                Insert,
                Clear,
                RemoveAt,
                Remove,
                Sort
            }
            public ActionType Type { get; set; }
            public int Index { get; set; }
            public T Item { get; set; }
            public IComparer<T> Comparer { get; set; }

            public void Apply(ItemCollection<T> collection)
            {
                switch (Type)
                {
                    case ActionType.Add:
                        collection.AddInternal(Item);
                        break;
                    case ActionType.Insert:
                        collection.InsertInternal(Index, Item);
                        break;
                    case ActionType.Clear:
                        collection.ClearInternal();
                        break;
                    case ActionType.RemoveAt:
                        collection.RemoveAtInternal(Index);
                        break;
                    case ActionType.Remove:
                        collection.RemoveInternal(Item);
                        break;
                    case ActionType.Sort:
                        collection.SortInternal(Comparer);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        private readonly ReaderWriterLockSlim lockSlim = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        protected readonly List<T> _items = new List<T>();
        private readonly Queue<PostponedAction> _postponedActions = new Queue<PostponedAction>();

        public ItemCollection() { }

        private struct ReadLockWrapper : IDisposable
        {
            private readonly ItemCollection<T> _collection;

            public ReadLockWrapper(ItemCollection<T> collection)
            {
                _collection = collection;
                _collection.lockSlim.EnterReadLock();
            }
            public void Dispose()
            {
                _collection.lockSlim.ExitReadLock();
                _collection.TryApplyPostponedActions();
            }
        }

        private ReadLockWrapper ReadLock() => new ReadLockWrapper(this);
        private struct WriteLockOrPostponeWrapper : IDisposable
        {
            private readonly ItemCollection<T> _collection;

            public bool WasPostponed { get; }

            public WriteLockOrPostponeWrapper(ItemCollection<T> collection)
            {
                _collection = collection;
                var toPostpone = false;
                try
                {
                    if (_collection.lockSlim.IsReadLockHeld)
                        toPostpone = true;
                    else
                        _collection.lockSlim.EnterWriteLock();
                }
                catch (LockRecursionException)
                {
                    toPostpone = true;
                }

                WasPostponed = toPostpone;
            }
            public void Dispose()
            {
                if (!WasPostponed)
                    _collection.lockSlim.ExitWriteLock();
            }
        }

        private WriteLockOrPostponeWrapper WriteLockOrPostpone() => new WriteLockOrPostponeWrapper(this);

        private void TryApplyPostponedActions()
        {
            using (var writeOrPostpone = WriteLockOrPostpone())
            {
                if (writeOrPostpone.WasPostponed)
                {
                    if (_postponedActions.Count > 0)
                        ;
                    return;
                }

                while (_postponedActions.TryDequeue(out var action))
                    action.Apply(this);
            }
        }
        
        public T this[int index]
        {
            get
            {
                using (ReadLock())
                {
                    return _items[index];
                }
                
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        public int Count
        {
            get
            {
                using (ReadLock())
                {
                    return _items.Count;
                }
                
            }
        }

        public bool IsReadOnly => false;

        public virtual void Add(T item)
        {
            if (item == null)
                throw new ArgumentNullException("Item cant be null");
            using (ReadLock())
                if (_items.Contains(item))
                    throw new ArgumentException("Item is already part of this collection");
            using (var writeOrPostpone = WriteLockOrPostpone() )
            {
                if (writeOrPostpone.WasPostponed)
                    _postponedActions.Enqueue(new PostponedAction(){Type = PostponedAction.ActionType.Add,Item = item});
                else
                    AddInternal(item);
            }
        }

        private void AddInternal(T item)
        {
            // Event werfen
            OnInsert?.Invoke(item);

            // Control einfügen
            _items.Add(item);
            OnInserted?.Invoke(item, _items.Count-1);
        }

        public virtual void Clear()
        {
            using (var writeOrPostpone = WriteLockOrPostpone())
            {
                if (writeOrPostpone.WasPostponed)
                    _postponedActions.Enqueue(new PostponedAction() {Type = PostponedAction.ActionType.Clear});
                else
                    ClearInternal();
            }
        }

        private void ClearInternal()
        {
            for (int i = 0; i < _items.Count; i++)
            {
                OnRemove?.Invoke(_items[i], i);
            }
            _items.Clear();
        }

        public bool Contains(T item)
        {
            using (ReadLock())
            {
                return _items.Contains(item);
            }
            
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            using (ReadLock())
            {
                _items.CopyTo(array, arrayIndex);
            }
            
        }



        public int IndexOf(T item)
        {
            using (ReadLock())
            {
                return _items.IndexOf(item);
            }
            
        }

        public virtual void Insert(int index, T item)
        {
            if (item == null)
                throw new ArgumentNullException("Item cant be null");
            using (var writeOrPostpone = WriteLockOrPostpone())
            {
                if (writeOrPostpone.WasPostponed)
                    _postponedActions.Enqueue(new PostponedAction(){Type = PostponedAction.ActionType.Insert, Index = index, Item = item});
                else
                    InsertInternal(index, item);
            }
        }

        private void InsertInternal(int index, T item)
        {
            if (_items.Contains(item))
                throw new ArgumentException("Item is already part of this collection");
            OnInsert?.Invoke(item);
            // Control einfügen
            _items.Insert(index, item);

            // Event werfen
            OnInserted?.Invoke(item, index);
        }

        public virtual bool Remove(T item)
        {
            if (item == null)
                throw new ArgumentNullException("Item cant be null");

            using (ReadLock())
            {
                if (!_items.Contains(item))
                    return false;
            }
            
            using (var writeOrPostpone = WriteLockOrPostpone())
            {
                if (writeOrPostpone.WasPostponed)
                    _postponedActions.Enqueue(new PostponedAction(){Type=PostponedAction.ActionType.Remove, Item =item});
                else
                    RemoveInternal(item);
            }

            return true;
        }

        private bool RemoveInternal(T item)
        {
            // Control entfernen
            int index = _items.IndexOf(item);

            // Event
            OnRemove?.Invoke(item, index);

            return _items.Remove(item);
        }

        public virtual void RemoveAt(int index)
        {
            if (index < 0 && index >= _items.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            using (var writeOrPostpone = WriteLockOrPostpone())
            {
                if (writeOrPostpone.WasPostponed)
                    _postponedActions.Enqueue(new PostponedAction(){Type=PostponedAction.ActionType.RemoveAt, Index = index});
                else
                    RemoveAtInternal(index);
            }

        }

        private void RemoveAtInternal(int index)
        {
            // Control entfernen
            T c = _items[index];

            // Event werfen
            OnRemove?.Invoke(c, index);

            _items.RemoveAt(index);
        }

        public event ItemCollectionDelegate<T> OnInsert;
        public event ItemCollectionIndexedDelegate<T> OnInserted;

        public event ItemCollectionIndexedDelegate<T> OnRemove;

        /// <summary>
        /// A locking enumerator for the <see cref="ItemCollection{T}"/> class.
        /// </summary>
        public struct Enumerator : IEnumerator<T>
        {
            private readonly ItemCollection<T> _collection;
            private List<T>.Enumerator _enumerator;
            private ReadLockWrapper _readLock;

            /// <summary>
            /// Initializes a new instance of the <see cref="Enumerator"/> struct used to lock the collection while enumerating.
            /// </summary>
            /// <param name="collection">The collection of which to wrap the enumerator of.</param>
            public Enumerator(ItemCollection<T> collection)
            {
                _collection = collection;
                _readLock = _collection.ReadLock();
                _enumerator = collection._items.GetEnumerator();
            }

            /// <inheritdoc />
            public bool MoveNext() => _enumerator.MoveNext();

            void IEnumerator.Reset() => ((IEnumerator)_enumerator).Reset();

            /// <inheritdoc />
            public T Current => _enumerator.Current;

            object IEnumerator.Current => Current;

            /// <inheritdoc />
            public void Dispose()
            {
                _enumerator.Dispose();
                _readLock.Dispose();
            }
        }


        /// <summary>
        /// Returns a locking enumerator which prevents the collection from being changed.
        /// </summary>
        /// <returns>The locking <see cref="Enumerator"/>.</returns>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Sort(IComparer<T> comparer)
        {
            using (var writeOrPostpone = WriteLockOrPostpone())
            {
                if (writeOrPostpone.WasPostponed)
                    _postponedActions.Enqueue(new PostponedAction() {Type = PostponedAction.ActionType.Sort,Comparer = comparer});
                else
                    SortInternal(comparer);
            }
        }

        private void SortInternal(IComparer<T> comparer)
        {
            _items.Sort(comparer);
        }
    }

    public delegate void ItemCollectionIndexedDelegate<T>(T item, int index);
    public delegate void ItemCollectionDelegate<T>(T item);
}
