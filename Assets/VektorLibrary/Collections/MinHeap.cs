using System;
using System.Collections.Generic;

namespace VektorLibrary.Collections {
    // TODO: This needs some documentation and cleanup.
    public class MinHeap<T> where T : IComparable<T> {

        private Dictionary<T, int> _heapIndices;
	    private readonly T[] _heapItems;
        public int Count { get; private set; }

        public MinHeap(int maxHeapSize) {
            _heapIndices = new Dictionary<T, int>();
            _heapItems = new T[maxHeapSize];
        }
	
        public void Add(T item) {
            _heapIndices.Add(item, Count);
            _heapItems[Count] = item;
            SortUp(item);
            Count++;
        }

        public T RemoveFirst() {
            var firstItem = _heapItems[0];
            _heapIndices.Remove(firstItem);
            Count--;
            
            _heapItems[0] = _heapItems[Count];
            _heapIndices[_heapItems[0]] = 0;
            
            SortDown(_heapItems[0]);
            return firstItem;
        }

        public void UpdateItem(T item) {
            SortUp(item);
        }

        public bool Contains(T item) {
            return _heapIndices.ContainsKey(item);
        }

        private void SortDown(T item) {
            while (true) {
                var childIndexLeft = _heapIndices[item] * 2 + 1;
                var childIndexRight = _heapIndices[item] * 2 + 2;

                if (childIndexLeft < Count) {
                    var swapIndex = childIndexLeft;

                    if (childIndexRight < Count) {
                        if (_heapItems[childIndexLeft].CompareTo(_heapItems[childIndexRight]) < 0) {
                            swapIndex = childIndexRight;
                        }
                    }

                    if (item.CompareTo(_heapItems[swapIndex]) < 0) {
                        Swap (item,_heapItems[swapIndex]);
                    }
                    else {
                        return;
                    }

                }
                else {
                    return;
                }

            }
        }

        private void SortUp(T item) {
            var parentIndex = (_heapIndices[item]-1)/2;
		
            while (true) {
                var parentItem = _heapItems[parentIndex];
                if (item.CompareTo(parentItem) > 0) {
                    Swap (item,parentItem);
                }
                else {
                    break;
                }

                parentIndex = (_heapIndices[item]-1)/2;
            }
        }

        private void Swap(T itemA, T itemB) {
            _heapItems[_heapIndices[itemA]] = itemB;
            _heapItems[_heapIndices[itemB]] = itemA;
            var itemAIndex = _heapIndices[itemA];
            _heapIndices[itemA] = _heapIndices[itemB];
            _heapIndices[itemB] = itemAIndex;
        }
    }
}