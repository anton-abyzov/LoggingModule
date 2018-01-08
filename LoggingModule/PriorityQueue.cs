using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace LoggingModule
{
    public class PriorityQueue<TMessage> : IProducerConsumerCollection<TMessage> where TMessage: LogMessage
    {
        private TMessage[] _array;
        private int _head; // The index from which to dePriorityQueue if the PriorityQueue isn't empty.
        private int _tail; // The index at which to enPriorityQueue if the PriorityQueue isn't full.
        private int _size; // Number of elements.
        private int _version;
        private const int MinimumGrow = 4;
        private const int GrowFactor = 200; // double each time

        public PriorityQueue()
        {
            _array = Array.Empty<TMessage>();
        }

        IEnumerator<TMessage> IEnumerable<TMessage>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        public void CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (array.Rank != 1)
            {
                throw new ArgumentException(nameof(array.Rank));
            }

            if (array.GetLowerBound(0) != 0)
            {
                throw new ArgumentException(nameof(array.GetLowerBound));
            }

            int arrayLen = array.Length;
            if (index < 0 || index > arrayLen)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (arrayLen - index < _size)
            {
                throw new ArgumentException();
            }

            int numToCopy = _size;
            if (numToCopy == 0) return;

            try
            {
                int firstPart = (_array.Length - _head < numToCopy) ? _array.Length - _head : numToCopy;
                Array.Copy(_array, _head, array, index, firstPart);
                numToCopy -= firstPart;

                if (numToCopy > 0)
                {
                    Array.Copy(_array, 0, array, index + _array.Length - _head, numToCopy);
                }
            }
            catch (ArrayTypeMismatchException)
            {
                throw new ArgumentException(nameof(array));
            }
        }

        public int Count { get; }
        public bool IsSynchronized { get; }
        public object SyncRoot { get; }

        public void CopyTo(TMessage[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (arrayIndex < 0 || arrayIndex > array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            }

            int arrayLen = array.Length;
            if (arrayLen - arrayIndex < _size)
            {
                throw new ArgumentException();
            }

            int numToCopy = _size;
            if (numToCopy == 0) return;

            int firstPart = Math.Min(_array.Length - _head, numToCopy);
            Array.Copy(_array, _head, array, arrayIndex, firstPart);
            numToCopy -= firstPart;
            if (numToCopy > 0)
            {
                Array.Copy(_array, 0, array, arrayIndex + _array.Length - _head, numToCopy);
            }
        }

        public TMessage[] ToArray()
        {
            if (_size == 0)
            {
                return Array.Empty<TMessage>();
            }

            TMessage[] arr = new TMessage[_size];

            if (_head < _tail)
            {
                Array.Copy(_array, _head, arr, 0, _size);
            }
            else
            {
                Array.Copy(_array, _head, arr, 0, _array.Length - _head);
                Array.Copy(_array, 0, arr, _array.Length - _head, _tail);
            }

            return arr;
        }

        // PRIVATE Grows or shrinks the buffer to hold capacity objects. Capacity
        // must be >= _size.
        private void SetCapacity(int capacity)
        {
            TMessage[] newarray = new TMessage[capacity];
            if (_size > 0)
            {
                if (_head < _tail)
                {
                    Array.Copy(_array, _head, newarray, 0, _size);
                }
                else
                {
                    Array.Copy(_array, _head, newarray, 0, _array.Length - _head);
                    Array.Copy(_array, 0, newarray, _array.Length - _head, _tail);
                }
            }

            _array = newarray;
            _head = 0;
            _tail = (_size == capacity) ? 0 : _size;
            _version++;
        }

        public bool TryAdd(TMessage item)
        {
            if (_size == _array.Length)
            {
                int newcapacity = (int)((long)_array.Length * (long)GrowFactor / 100);
                if (newcapacity < _array.Length + MinimumGrow)
                {
                    newcapacity = _array.Length + MinimumGrow;
                }

                SetCapacity(newcapacity);
            }

            _array[_tail] = item;
            MoveNext(ref _tail);
            _size++;
            _version++;
            return true;
        }

        // Increments the index wrapping it if necessary.
        private void MoveNext(ref int index)
        {
            // It is tempting to use the remainder operator here but it is actually much slower 
            // than a simple comparison and a rarely taken branch.   
            int tmp = index + 1;
            index = (tmp == _array.Length) ? 0 : tmp;
        }

        public bool TryTake(out TMessage item)
        {
            if (_size == 0)
            {
                throw new InvalidOperationException();
            }

            TMessage removed = _array[_head];
            if (RuntimeHelpers.IsReferenceOrContainsReferences<TMessage>())
            {
                _array[_head] = default(TMessage);
            }
            MoveNext(ref _head);
            _size--;
            _version++;
            item = removed;
            return true;
        }


        // Implements an enumerator for a PriorityQueue.  The enumerator uses the
        // internal version number of the list to ensure that no modifications are
        // made to the list while an enumeration is in progress.
        [SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification =
            "not an expected scenario")]
        public struct Enumerator : IEnumerator<TMessage>,
            System.Collections.IEnumerator
        {
            private readonly PriorityQueue<TMessage> _q;
            private readonly int _version;
            private int _index; // -1 = not started, -2 = ended/disposed
            private TMessage _currentElement;

            internal Enumerator(PriorityQueue<TMessage> q)
            {
                _q = q;
                _version = q._version;
                _index = -1;
                _currentElement = default(TMessage);
            }

            public void Dispose()
            {
                _index = -2;
                _currentElement = default(TMessage);
            }

            public bool MoveNext()
            {
                if (_version != _q._version) throw new InvalidOperationException();

                if (_index == -2)
                    return false;

                _index++;

                if (_index == _q._size)
                {
                    // We've run past the last element
                    _index = -2;
                    _currentElement = default(TMessage);
                    return false;
                }

                // Cache some fields in locals to decrease code size
                TMessage[] array = _q._array;
                int capacity = array.Length;

                // _index represents the 0-based index into the PriorityQueue, however the PriorityQueue
                // doesn't have to start from 0 and it may not even be stored contiguously in memory.

                int arrayIndex = _q._head + _index; // this is the actual index into the PriorityQueue's backing array
                if (arrayIndex >= capacity)
                {
                    // NOTE: Originally we were using the modulo operator here, however
                    // on Intel processors it has a very high instruction latency which
                    // was slowing down the loop quite a bit.
                    // Replacing it with simple comparison/subtraction operations sped up
                    // the average foreach loop by 2x.

                    arrayIndex -= capacity; // wrap around if needed
                }

                _currentElement = array[arrayIndex];
                return true;
            }

            public TMessage Current
            {
                get
                {
                    if (_index < 0)
                        ThrowEnumerationNotStartedOrEnded();
                    return _currentElement;
                }
            }

            private void ThrowEnumerationNotStartedOrEnded()
            {
                throw new InvalidOperationException();
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            void IEnumerator.Reset()
            {
                if (_version != _q._version) throw new InvalidOperationException();
                _index = -1;
                _currentElement = default(TMessage);
            }
        }
    }
}