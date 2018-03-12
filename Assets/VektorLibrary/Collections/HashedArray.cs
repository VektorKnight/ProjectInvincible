using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VektorLibrary.Collections {
    /// <summary>
    /// *WORK IN PROGRESS*
    /// Effectively a combination of a Dictionary and Array.
    /// Lookup efficiency of a Dictionary with the iteration efficiency of an array.
    /// Can be a bit heavy on memory for certain types compared to other collections.
    /// * Does not currently support insertion and item order is not guaranteed to be preserved.
    /// Original Author: VektorKnight
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HashedArray<T> : ICollection<T> {  
        // Private: Dictionary, Array, and Free Stack
        private readonly Dictionary<T, int> _itemDictionary;
        private readonly Stack<int> _freeIndices;
        private T[] _itemArray;
        
        // Private: Last Free Index & Initial Size
        private readonly int _initialSize;

        // Properties: Array Capacity and HashSet Count
        public int Capacity => _itemArray.Length;
        public int Count => _itemDictionary.Count;
        public int LastIndex { get; private set; }
        public bool IsReadOnly { get; }
        
        // Operator: Index []
        public T this[int index] {
            get { return _itemArray[index]; }
            set {
                if (_itemDictionary.ContainsValue(index)) 
                    _itemDictionary.Remove(_itemArray[index]);
                _itemDictionary.Add(value, index);
                _itemArray[index] = value;
            }
        }

        // Class Constructor: Initial Capacity / Default Max
        public HashedArray(int initial) {
            // Initialize the HashSet and Array
            _initialSize = initial;
            _itemDictionary = new Dictionary<T, int>();
            _itemArray = new T[initial];
            _freeIndices = new Stack<int>();
        }
        
        /// <summary>
        /// Adds an item at the last free index or at a free index from the stack.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void Add(T item) {
            // Exit if the item already exists in the collection
            if (_itemDictionary.ContainsKey(item)) return;
            
            // Try to pop a free index from the stack and use it
            if (_freeIndices.Count > 0) {
                var freeIndex = _freeIndices.Pop();
                _itemDictionary.Add(item, freeIndex);
                _itemArray[freeIndex] = item;
                return;
            }
            
            // Try to add the item at the next free index and resize it if necessary
            if (LastIndex == _itemArray.Length) Array.Resize(ref _itemArray, _itemArray.Length * 2);
            _itemDictionary.Add(item, LastIndex);
            _itemArray[LastIndex] = item;
            LastIndex++;
        }
        
        /// <summary>
        /// Clear all items from the collection and reset the state.
        /// </summary>
        public void Clear() {
            _itemArray = new T[_initialSize];
            _itemDictionary.Clear();
            _freeIndices.Clear();
        }
        
        /// <summary>
        /// Copy the items within this collection to a specified array.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(T[] array, int arrayIndex) {
            _itemDictionary.Keys.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Remove a specified item from the collection.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        public bool Remove(T item) {
            // Return false if the item does not exist in the collection
            if (!_itemDictionary.ContainsKey(item)) return false;
            
            // Remove the specified item
            var itemIndex = _itemDictionary[item];
            _itemDictionary.Remove(item);
            _itemArray[itemIndex] = default(T);
            _freeIndices.Push(itemIndex);
            return true;
        }
        
        /// <summary>
        /// Check if the specified item exists within the collection.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(T item) {
            return _itemDictionary.ContainsKey(item);
        }
        
        // Public method to get an enumerator from the internal array
        public IEnumerator<T> GetEnumerator() {
            return _itemArray.AsEnumerable().Cast<T>().GetEnumerator();
        }
        
        // Get an enumerator from the internal array
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}