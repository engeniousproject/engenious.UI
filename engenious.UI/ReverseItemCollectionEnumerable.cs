﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace engenious.UI
{
    /// <summary>
    /// An enumerable that can enumerate backwards through a <see cref="ItemCollection{T}"/>.
    /// </summary>
    /// <seealso cref="ReverseEnumerable{T}"/>
    /// <typeparam name="T">The type of the items in the <see cref="ItemCollection{T}"/>.</typeparam>
    public class ReverseItemCollectionEnumerable<T> : IEnumerable<T> where T : class
    {
        private readonly ItemCollection<T> _baseList;
                
        /// <summary>
        /// Initializes a new instance of the <see cref="ReverseEnumerable{T}"/> class.
        /// </summary>
        /// <param name="baseList">The list to make reverse iterable.</param>
        public ReverseItemCollectionEnumerable(ItemCollection<T> baseList)
        {
            _baseList = baseList;
        }

        /// <summary>
        /// The enumerator used to enumerate backwards through a <see cref="ItemCollection{T}"/>.
        /// </summary>
        [Serializable]
        public struct ReverseEnumerator : IEnumerator<T>
        {
            private readonly ItemCollection<T> _list;
            private int _index;
            private T? _current;
            private ItemCollection<T>.Enumerator _lockingEnumerator;

            internal ReverseEnumerator(ItemCollection<T> list)
            {
                _lockingEnumerator = list.GetEnumerator();
                _list = list;
                _index = list.Count-1;
                _current = default;
            }

            /// <inheritdoc />
            public void Dispose()
            {
                _lockingEnumerator.Dispose();
            }

            /// <inheritdoc />
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

            /// <inheritdoc />
            public T Current => _current!;

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
                _current = default;
            }
        }

        /// <summary>
        /// Gets the <see cref="ReverseEnumerator"/>.
        /// </summary>
        /// <returns>The <see cref="ReverseEnumerator"/>.</returns>
        public ReverseEnumerator GetEnumerator()
        {
            return new ReverseEnumerator(_baseList);
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