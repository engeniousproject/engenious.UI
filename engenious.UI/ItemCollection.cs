using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace engenious.UI
{
    /// <summary>
    /// A specialized thread-safe list that can be manipulated while reading from it, by collecting and postponing these actions.
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
        private readonly ReaderWriterLockSlim _lockSlim = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        /// <summary>
        /// The contained items list.
        /// </summary>
        protected readonly List<T> Items = new List<T>();
        private readonly Queue<PostponedAction> _postponedActions = new Queue<PostponedAction>();

        private readonly struct ReadLockWrapper : IDisposable
        {
            private readonly ItemCollection<T> _collection;

            public ReadLockWrapper(ItemCollection<T> collection)
            {
                _collection = collection;
                _collection._lockSlim.EnterReadLock();
            }
            public void Dispose()
            {
                _collection._lockSlim.ExitReadLock();
                _collection.TryApplyPostponedActions();
            }
        }

        private ReadLockWrapper ReadLock() => new ReadLockWrapper(this);
        private readonly struct WriteLockOrPostponeWrapper : IDisposable
        {
            private readonly ItemCollection<T> _collection;

            public bool WasPostponed { get; }

            public WriteLockOrPostponeWrapper(ItemCollection<T> collection)
            {
                _collection = collection;
                var toPostpone = false;
                try
                {
                    if (_collection._lockSlim.IsReadLockHeld)
                        toPostpone = true;
                    else
                        _collection._lockSlim.EnterWriteLock();
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
                    _collection._lockSlim.ExitWriteLock();
            }
        }

        private WriteLockOrPostponeWrapper WriteLockOrPostpone() => new WriteLockOrPostponeWrapper(this);

        private void TryApplyPostponedActions()
        {
            using (var writeOrPostpone = WriteLockOrPostpone())
            {
                if (writeOrPostpone.WasPostponed)
                {
                    return;
                }

                while (_postponedActions.TryDequeue(out var action))
                    action.Apply(this);
            }
        }

        /// <inheritdoc />
        public T this[int index]
        {
            get
            {
                using (ReadLock())
                {
                    return Items[index];
                }
                
            }

            set => throw new NotSupportedException();
        }

        /// <inheritdoc />
        public int Count
        {
            get
            {
                using (ReadLock())
                {
                    return Items.Count;
                }
                
            }
        }

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        public virtual void Add(T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            using (ReadLock())
                if (Items.Contains(item))
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
            // Call event
            OnInsert?.Invoke(item);

            // Insert control
            Items.Add(item);
            OnInserted?.Invoke(item, Items.Count-1);
        }

        /// <inheritdoc />
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
            for (int i = 0; i < Items.Count; i++)
            {
                OnRemove?.Invoke(Items[i], i);
            }
            Items.Clear();
        }

        /// <inheritdoc />
        public bool Contains(T item)
        {
            using (ReadLock())
            {
                return Items.Contains(item);
            }
            
        }

        /// <inheritdoc />
        public void CopyTo(T[] array, int arrayIndex)
        {
            using (ReadLock())
            {
                Items.CopyTo(array, arrayIndex);
            }
            
        }


        /// <inheritdoc />
        public int IndexOf(T item)
        {
            using (ReadLock())
            {
                return Items.IndexOf(item);
            }
            
        }

        /// <inheritdoc />
        public virtual void Insert(int index, T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
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
            if (Items.Contains(item))
                throw new ArgumentException("Item is already part of this collection");
            OnInsert?.Invoke(item);
            // Insert control
            Items.Insert(index, item);

            // Call event
            OnInserted?.Invoke(item, index);
        }

        /// <inheritdoc />
        public virtual bool Remove(T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            using (ReadLock())
            {
                if (!Items.Contains(item))
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
            // Remove control
            int index = Items.IndexOf(item);

            // Event
            OnRemove?.Invoke(item, index);

            return Items.Remove(item);
        }

        /// <inheritdoc />
        public virtual void RemoveAt(int index)
        {
            if (index < 0 && index >= Items.Count)
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
            // Remove control
            T c = Items[index];

            // Call event
            OnRemove?.Invoke(c, index);

            Items.RemoveAt(index);
        }

        /// <summary>
        /// Occurs when an item is going to be inserted into the collection.
        /// </summary>
        public event ItemCollectionDelegate<T> OnInsert;

        /// <summary>
        /// Occurs when an item was inserted into the collection.
        /// </summary>
        public event ItemCollectionIndexedDelegate<T> OnInserted;

        /// <summary>
        /// Occurs when an item is going to be removed from the collection.
        /// </summary>
        public event ItemCollectionIndexedDelegate<T> OnRemove;

        /// <summary>
        /// A locking enumerator for the <see cref="ItemCollection{T}"/> class.
        /// </summary>
        public struct Enumerator : IEnumerator<T>
        {
            private List<T>.Enumerator _enumerator;
            private readonly ReadLockWrapper _readLock;

            /// <summary>
            /// Initializes a new instance of the <see cref="Enumerator"/> struct used to lock the collection while enumerating.
            /// </summary>
            /// <param name="collection">The collection of which to wrap the enumerator of.</param>
            public Enumerator(ItemCollection<T> collection)
            {
                _readLock = collection.ReadLock();
                _enumerator = collection.Items.GetEnumerator();
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

        /// <summary>
        /// Sorts this list by using a comparer.
        /// </summary>
        /// <param name="comparer">The comparer to order by.</param>
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
            Items.Sort(comparer);
        }
    }

    /// <summary>
    /// Represents the method that will handle item changes at a specific index.
    /// </summary>
    /// <param name="item">The item that will be used.</param>
    /// <param name="index">The index the change happened at.</param>
    /// <typeparam name="T">The type of the item.</typeparam>
    public delegate void ItemCollectionIndexedDelegate<in T>(T item, int index);
    /// <summary>
    /// Represents the method that will handle item changes.
    /// </summary>
    /// <param name="item">The item that is affected.</param>
    /// <typeparam name="T">The type of the item.</typeparam>
    public delegate void ItemCollectionDelegate<in T>(T item);
}
