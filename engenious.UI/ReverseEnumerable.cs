﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace engenious.UI
{
    /// <summary>
    /// An enumerable that can enumerate backwards through a <see cref="List{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the items in the <see cref="List{T}"/>.</typeparam>
    public class ReverseEnumerable<T> : IEnumerable<T>
    {
        /// <summary>
        /// Sets the base <see cref="List{T}"/> to iterate through.
        /// </summary>
        public List<T> BaseList { private get; set; }
        /// <summary>
        /// The enumerator used to enumerate backwards through a <see cref="List{T}"/>.
        /// </summary>
        [Serializable]
        public struct ReverseEnumerator : IEnumerator<T>
        {
            private readonly List<T> _list;
            private int _index;
            private T _current;

            internal ReverseEnumerator(List<T> list)
            {
                _list = list;
                _index = list.Count-1;
                _current = default(T);
            }

            /// <inheritdoc />
            public void Dispose()
            {
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

        /// <summary>
        /// Gets the <see cref="ReverseEnumerator"/>.
        /// </summary>
        /// <returns>The <see cref="ReverseEnumerator"/>.</returns>
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