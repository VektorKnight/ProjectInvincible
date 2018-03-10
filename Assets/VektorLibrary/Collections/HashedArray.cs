using System;
using System.Collections.Generic;
using UnityEngine;

namespace VektorLibrary.Collections {
    public class HashedArray<T> {  
        // Private: HashSet, Array, and Free Stack
        private readonly Dictionary<T, int> _itemDictionary;
        private readonly Stack<int> _freeIndices;
        private T[] _itemArray;
        
        // Private: Last Free Index
        private int _nextFreeIndex;
        
        // Properties: Array Capacity and HashSet Count
        public int Capacity => _itemArray.Length;
        public int Count => _itemDictionary.Count;
        public int Length => _nextFreeIndex - 1;
        
        // Operator: Index []
        public T this[int index] => (index >= 0 && index < _itemArray.Length) ? _itemArray[index] : default(T);

        // Class Constructor: Initial Capacity / Default Max
        public HashedArray(int initial) {
            // Initialize the HashSet and Array
            _itemDictionary = new Dictionary<T, int>();
            _itemArray = new T[initial];
            _freeIndices = new Stack<int>();
        }
        
        /// <summary>
        /// Adds an item at the last free index or at the end of the current elements.
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
            if (_nextFreeIndex == _itemArray.Length) Array.Resize(ref _itemArray, _itemArray.Length * 2);
            _itemDictionary.Add(item, _nextFreeIndex);
            _itemArray[_nextFreeIndex] = item;
            _nextFreeIndex++;
        }
        
        /// <summary>
        /// Remove a specified item from the collection.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        public void Remove(T item) {
            // Exit if the item does not exist in the collection
            if (!_itemDictionary.ContainsKey(item)) return;
            
            // Remove the specified item
            var itemIndex = _itemDictionary[item];
            _itemDictionary.Remove(item);
            _itemArray[itemIndex] = default(T);
            _freeIndices.Push(itemIndex);
        }
        
        /// <summary>
        /// Check if the specified item exists within the collection.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(T item) {
            return _itemDictionary.ContainsKey(item);
        }
    }
}