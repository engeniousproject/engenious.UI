using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace engenious.UI
{


    /// <summary>
    /// Erweiterte Liste für Controls
    /// </summary>
    public class ItemCollection<T> : IList<T> where T : class
    {
        private readonly ReaderWriterLockSlim lockSlim = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        public List<T> Items = new List<T>();

        public ItemCollection() { }

        public T this[int index]
        {
            get
            {
                lockSlim.EnterReadLock();
                try
                {
                    return Items[index];
                }
                finally
                {
                    lockSlim.ExitReadLock();
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
                lockSlim.EnterReadLock();
                try
                
                    {
                    return Items.Count;
                }
                finally
                {
                    lockSlim.ExitReadLock();
                }
            }
        }

        public bool IsReadOnly => false;

        public virtual void Add(T item)
        {
            lockSlim.EnterWriteLock();
            try
            {
                if (item == null)
                    throw new ArgumentNullException("Item cant be null");

                if (Items.Contains(item))
                    throw new ArgumentException("Item is already part of this collection");

                // Event werfen
                OnInsert?.Invoke(item);

                // Control einfügen
                Items.Add(item);
                OnInserted?.Invoke(item, Items.Count-1);

            }
            finally
            {
                lockSlim.ExitWriteLock();
            }
        }

        public virtual void Clear()
        {
            lockSlim.EnterWriteLock();
            try
            {
                for (int i = 0; i < Items.Count; i++)
                {
                    OnRemove?.Invoke(Items[i], i);
                }
                Items.Clear();
            }
            finally
            {
                lockSlim.ExitWriteLock();
            }
        }

        public bool Contains(T item)
        {
            lockSlim.EnterReadLock();
            try
            {
                return Items.Contains(item);
            }
            finally
            {
                lockSlim.ExitReadLock();

            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lockSlim.EnterWriteLock(); 
            try
            {
                Items.CopyTo(array, arrayIndex);
            }
            finally
            {
                lockSlim.ExitWriteLock();
            }
        }



        public int IndexOf(T item)
        {
            lockSlim.EnterReadLock();
            try
            {
                return Items.IndexOf(item);
            }
            finally
            {
                lockSlim.ExitReadLock();

            }
        }

        public virtual void Insert(int index, T item)
        {
            lockSlim.EnterWriteLock();
            try
            {
                if (item == null)
                    throw new ArgumentNullException("Item cant be null");

                if (Items.Contains(item))
                    throw new ArgumentException("Item is already part of this collection");

                OnInsert?.Invoke(item);
                // Control einfügen
                Items.Insert(index, item);

                // Event werfen
                OnInserted?.Invoke(item, index);
            }
            finally
            {
                lockSlim.ExitWriteLock();
            }
        }

        public virtual bool Remove(T item)
        {
            lockSlim.EnterWriteLock();

            try
            {
                if (item == null)
                    throw new ArgumentNullException("Item cant be null");

                if (!Items.Contains(item))
                    return false;

                // Control entfernen
                int index = Items.IndexOf(item);

                // Event
                OnRemove?.Invoke(item, index);

                Items.Remove(item);
                return true;
            }
            finally
            {
                lockSlim.ExitWriteLock();
            }

        }

        public virtual void RemoveAt(int index)
        {
            lockSlim.EnterWriteLock();

            try
            {
                if (index < 0 && index >= Items.Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                // Control entfernen
                T c = Items[index];

                // Event werfen
                OnRemove?.Invoke(c, index);

                Items.RemoveAt(index);
            }
            finally
            {
                lockSlim.ExitWriteLock();
            }

        }

        public event ItemCollectionDelegate<T> OnInsert;
        public event ItemCollectionIndexedDelegate<T> OnInserted;

        public event ItemCollectionIndexedDelegate<T> OnRemove;

        struct LockedEnumerator : IEnumerator<T>
        {
            private readonly ItemCollection<T> collection;
            private T current;
            private int index;
            public LockedEnumerator(ItemCollection<T> collection)
            {
                this.collection = collection;
                index = -1;
                current = default;
                collection.lockSlim.EnterReadLock();
            }
            public T Current => current;

            object IEnumerator.Current => current;

            public void Dispose()
            {
                collection.lockSlim.ExitReadLock();
            }

            public bool MoveNext()
            {
                index++;
                if (index < collection.Count)
                {
                    current = collection[index];
                    return true;
                }
                current = default;
                return false;
            }

            public void Reset()
            {
                index = 0;
            }
        }


        public List<T>.Enumerator GetEnumerator()
        {
            lock (Items)
            {
                return Items.GetEnumerator();
            }
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
            lockSlim.EnterWriteLock();
            try
            {
                Items.Sort(comparer);
            }
            finally
            {

                lockSlim.ExitWriteLock();
            }
        }
    }

    public delegate void ItemCollectionIndexedDelegate<T>(T item, int index);
    public delegate void ItemCollectionDelegate<T>(T item);
}
