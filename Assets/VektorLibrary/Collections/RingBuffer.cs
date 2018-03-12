using System;

namespace VektorLibrary.Collections {
    /// <summary>
    /// Ring buffer implementation.
    /// Original Author: Techgeek1
    /// Edits By: VektorKnight
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RingBuffer<T> {
        private T[] _buffer;
        private int _head = -1;
        private int _tail;
        private int _count;
        private int _mask;
        
        // Properties: Head, Tail, Count
        public T Head => _buffer[_head];
        public T Tail => _buffer[_tail];
        public int Count => _count;
        
        // Operator: [index]
        public T this[int index] {
            get {
                return _buffer[(_tail + index) & _mask];
            }
            set {
                _buffer[(_tail + index) & _mask] = value;
            }
        }

        /// <summary>
        /// Ring buffer implementation
        /// </summary>
        /// <param name="capacity">Size of the buffer. Must be a power of 2</param>
        public RingBuffer(int capacity) {
            if (!IsPowerOf2(capacity))
                throw new ArgumentException("Capacity must be a power of 2!");

            _buffer = new T[capacity];
            _mask = capacity - 1;
        }

        public void Enqueue(T value) {
            _head = (_head + 1) & _mask;
            _buffer[_head] = value;

            if (_count == _buffer.Length) {
                _tail = (_tail + 1) & _mask;
            }
            else {
                _count++;
            }
        }

        public T Dequeue() {
            if (_count == 0)
                throw new IndexOutOfRangeException();

            var value = _buffer[_tail];
            _tail = (_tail + 1) & _mask;
            _count--;

            return value;
        }

        public T GetFromHead(int offset) => _buffer[(_head - offset) & _mask];

        public void Clear() {
            _head = 0;
            _tail = 0;
            _buffer[0] = default(T);// Just setting the first one to 0 to save clearing the entire buffer
        }

        private static bool IsPowerOf2(int value) {
            // Find nearest power of 2
            var v = value;
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v++;

            return value == v;
        }
    }
}