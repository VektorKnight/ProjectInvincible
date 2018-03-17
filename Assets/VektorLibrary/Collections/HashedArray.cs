using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VektorLibrary.Math;

namespace VektorLibrary.Collections {
    /// <summary>
    /// Hashed Array Collection by VektorKnight
    /// Effectively a combination of a Dictionary and Array.
    /// Lookup efficiency of a dictionary with the iteration efficiency of an array.
    /// -----------------------------------------------------------------------------------------
    /// * Can be a bit heavy on memory for certain types compared to other collections.
    /// * Does not currently support insertion and item order is not guaranteed to be preserved.
    /// * Does not support duplicates due to use of a dictionary for fast lookup.
    /// -----------------------------------------------------------------------------------------
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HashedArray<T> : ICollection<T> {  
        // Private: Dictionary, Array, and Free Stack
        private readonly Dictionary<T, int> _dictionary;
        private readonly Stack<int> _fragments;
        private T[] _items;
        
        // Private: Initial Size
        private readonly int _initialSize;

        // Properties: Meta
        public int Capacity => _items.Length;             // The size of the internal array
        public int Count => _dictionary.Count;            // The number of unique objects within the array
        public float Continuity => GetContinuity();       // Continuity value of the collection
        public float Fragmentation => 1f - Continuity;    // Fragmentation value of the collection (1f - Continuity)
        public int TailIndex { get; private set; }        // Represents the tail of the objects (use this for iteration)
        public bool IsReadOnly { get; }                   // TODO: Not yet implemented
        
        // Operator: Index []
        public T this[int index] {
            get { return _items[index]; }
            set {
                // Update the dictionary entry for the given index if necessary
                if (_dictionary.ContainsValue(index)) 
                    _dictionary.Remove(_items[index]);
                _dictionary.Add(value, index);
                _items[index] = value;
            }
        }

        // Constructor
        public HashedArray(int capacity = 32) {
            // Sanity Check: Capacity is power of 2
            if (!VektorMath.IsPowerOfTwo(capacity)) 
                throw new ArgumentException("Capacity must be a power of two!");
            
            // Initialize required internal collections
            _initialSize = capacity;
            _dictionary = new Dictionary<T, int>(capacity);
            _items = new T[capacity];
            _fragments = new Stack<int>();
        }
        
        /// <summary>
        /// Adds an item at the last free index or at a free index from the stack.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void Add(T item) {
            // Exit if the item already exists in the collection
            if (_dictionary.ContainsKey(item)) return;
            
            // Try to pop a free index from the stack and use it
            if (_fragments.Count > 0) {
                var freeIndex = _fragments.Pop();
                _dictionary.Add(item, freeIndex);
                _items[freeIndex] = item;
                return;
            }
            
            // Try to add the item at the next free index and resize the array if necessary
            if (TailIndex == _items.Length) Array.Resize(ref _items, _initialSize * 2);
            _dictionary.Add(item, TailIndex);
            _items[TailIndex] = item;
            TailIndex++;
        }
        
        /// <summary>
        /// Remove a specified item from the collection.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        public bool Remove(T item) {
            // Return false if the item does not exist in the collection
            if (!_dictionary.ContainsKey(item)) return false;
            
            // Remove the specified item
            var itemIndex = _dictionary[item];
            _dictionary.Remove(item);
            _items[itemIndex] = default(T);
            _fragments.Push(itemIndex);
            return true;
        }
        
        /// <summary>
        /// Attempts to compact the collection by filling fragmented indices with items from the tail.
        /// Resizes the internal array to the nearest power of 2 that will fit the current elements.
        /// This function can be expensive, use sparringly.
        /// </summary>
        public void Compact() {
            throw new NotImplementedException("Feature not yet implemented");
        }
        
        /// <summary>
        /// Calculates the continuity of the collection.
        /// Fragmentation is this 1f - this value
        /// </summary>
        /// <returns></returns>
        private float GetContinuity() {
            // Return one if the collection is empty
            if (TailIndex == 0) return 1f;
            
            // Calculate continuity based on 1f - fragmented indices / mask
            var continuity = 1f - (float)_fragments.Count / TailIndex;
            return continuity;
        }
        
        /// <summary>
        /// Clear all items from the collection and reset the state.
        /// </summary>
        public void Clear() {
            _items = new T[_initialSize];
            _dictionary.Clear();
            _fragments.Clear();
        }
        
        /// <summary>
        /// Copy the items within this collection to a specified array.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(T[] array, int arrayIndex) {
            _dictionary.Keys.CopyTo(array, arrayIndex);
        }
        
        /// <summary>
        /// Check if the specified item exists within the collection.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns></returns>
        public bool Contains(T item) {
            return _dictionary.ContainsKey(item);
        }
        
        /// <summary>
        /// Get the index of the specified item if it exists within the collection.
        /// </summary>
        /// <param name="item">The item to fetch the index of.</param>
        /// <returns>The index of the specified item if it exists. Otherwise -1.</returns>
        public int IndexOf(T item) {
            // Return -1 if the item doesn't exist in this collection
            if (!_dictionary.ContainsKey(item)) return -1;
            
            // Return the index of the specified item
            return _dictionary[item];
        }
        
        /// <summary>
        /// Fetches the enumerator for this collection.
        /// </summary>
        /// <returns>The enumerator for the internal array.</returns>
        public IEnumerator<T> GetEnumerator() {
            return _items.AsEnumerable().Cast<T>().GetEnumerator();
        }
        
        // Get an enumerator from the internal array
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}