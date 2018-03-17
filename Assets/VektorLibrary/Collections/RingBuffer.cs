using System;
using VektorLibrary.Math;

namespace VektorLibrary.Collections {
    /// <summary>
    /// Ring buffer implementation.
    /// Original Author: Techgeek1
    /// Edits By: VektorKnight
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RingBuffer<T> {
        private readonly T[] _buffer;
        private readonly int _mask;
        private int _head = -1;
        private int _tail;

        // Properties: Head, Tail, Count
        public int Count { get; private set; }
        public T Head => _buffer[_head];
        public T Tail => _buffer[_tail];
        
        // Operator: []
        public T this[int index] {
            get { return _buffer[(_tail + index) & _mask]; }
            set { _buffer[(_tail + index) & _mask] = value; }
        }

        /// <summary>
        /// Ring buffer implementation
        /// </summary>
        /// <param name="capacity">Size of the buffer. Must be a power of 2.</param>
        public RingBuffer(int capacity) {
            // Sanity Check: Capacity must be a power of 2
            if (!VektorMath.IsPowerOfTwo(capacity))
                throw new ArgumentException("Capacity must be a power of 2!");
            
            // Initialize the buffer and set the mask
            _buffer = new T[capacity];
            _mask = capacity - 1;
        }
        
        
        public void Enqueue(T value) {
            _head = (_head + 1) & _mask;
            _buffer[_head] = value;

            if (Count == _buffer.Length) {
                _tail = (_tail + 1) & _mask;
            }
            else {
                Count++;
            }
        }

        public T Dequeue() {
            if (Count == 0)
                throw new IndexOutOfRangeException();

            var value = _buffer[_tail];
            _tail = (_tail + 1) & _mask;
            Count--;

            return value;
        }

        public T GetFromHead(int offset) => _buffer[(_head - offset) & _mask];

        public void Clear() {
            _head = 0;
            _tail = 0;
            _buffer[0] = default(T);// Just setting the first one to 0 to save clearing the entire buffer
        }
    }
}