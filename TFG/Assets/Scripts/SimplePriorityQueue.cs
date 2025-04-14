using System.Collections.Generic;

public class SimplePriorityQueue<T>
{
    private List<(T Item, float Priority)> heap = new List<(T, float)>();

    public int Count => heap.Count;

    public void Enqueue(T item, float priority)
    {
        heap.Add((item, priority));
        int i = heap.Count - 1;
        while (i > 0)
        {
            int parent = (i - 1) / 2;
            if (heap[parent].Priority <= heap[i].Priority) break;
            (heap[parent], heap[i]) = (heap[i], heap[parent]);
            i = parent;
        }
    }

    public T Dequeue()
    {
        T item = heap[0].Item;
        int last = heap.Count - 1;
        heap[0] = heap[last];
        heap.RemoveAt(last);

        int i = 0;
        while (true)
        {
            int left = 2 * i + 1;
            int right = 2 * i + 2;
            if (left >= heap.Count) break;

            int minChild = left;
            if (right < heap.Count && heap[right].Priority < heap[left].Priority)
                minChild = right;

            if (heap[i].Priority <= heap[minChild].Priority) break;

            (heap[i], heap[minChild]) = (heap[minChild], heap[i]);
            i = minChild;
        }

        return item;
    }
}