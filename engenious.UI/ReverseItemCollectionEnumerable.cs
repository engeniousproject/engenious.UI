using System;
using System.Collections;
using System.Collections.Generic;

namespace engenious.UI
{
    public class ReverseItemCollectionEnumerable<T> : IEnumerable<T> where T : class
    {
        
        public ItemCollection<T> BaseList { private get; set; }
        [Serializable]
        public struct ReverseEnumerator : IEnumerator<T>, IEnumerator
        {
            private readonly ItemCollection<T> _list;
            private int _index;
            private T _current;
            private ItemCollection<T>.Enumerator _lockingEnumerator;

            internal ReverseEnumerator(ItemCollection<T> list)
            {
                _lockingEnumerator = list.GetEnumerator();
                _list = list;
                _index = list.Count-1;
                _current = default(T);
            }

            public void Dispose()
            {
                _lockingEnumerator.Dispose();
            }

            public bool MoveNext()
            {
                var localList = _list;

                if (_index >= 0)
                {
                    _current = localList[_index];
                    _index--;
                    return true;
                }

                return MoveNextRare();
            }

            private bool MoveNextRare()
            {
                _index = -1;
                _current = default(T);
                return false;
            }

            public T Current => _current;

            object IEnumerator.Current
            {
                get
                {
                    if (_index == _list.Count || _index == -1)
                    {
                        throw new InvalidOperationException("enumeration operation not possible");
                    }

                    return Current;
                }
            }

            void IEnumerator.Reset()
            {
                _index = _list.Count - 1;
                _current = default(T);
            }
        }

        public ReverseEnumerator GetEnumerator()
        {
            return new ReverseEnumerator(BaseList);
        }
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}