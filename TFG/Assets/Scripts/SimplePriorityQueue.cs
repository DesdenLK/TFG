using System.Collections.Generic;

public class SimplePriorityQueue<T>
{
    private List<(T Item, float Priority)> heap = new List<(T, float)>();
    private Dictionary<T, int> nodePositions = new();

    public int Count => heap.Count;

    public void Enqueue(T item, float priority)
    {
        if (nodePositions.TryGetValue(item, out int existingIndex))
        {
            if (priority < heap[existingIndex].Priority)
            {
                heap[existingIndex] = (item, priority);
                HeapifyUp(existingIndex);
            }
            return;
        }

        heap.Add((item, priority));
        int i = heap.Count - 1;
        nodePositions[item] = i;
        HeapifyUp(i);
    }

    public T Dequeue()
    {
        T item = heap[0].Item;
        nodePositions.Remove(item);

        int last = heap.Count - 1;
        if (last == 0)
        {
            heap.RemoveAt(0);
            return item;
        }

        heap[0] = heap[last];
        nodePositions[heap[0].Item] = 0;
        heap.RemoveAt(last);
        HeapifyDown(0);

        return item;
    }

    private void HeapifyUp(int i)
    {
        while (i > 0)
        {
            int parent = (i - 1) / 2;
            if (heap[parent].Priority <= heap[i].Priority) break;
            Swap(i, parent);
            i = parent;
        }
    }

    private void HeapifyDown(int i)
    {
        while (true)
        {
            int left = 2 * i + 1;
            int right = 2 * i + 2;
            int smallest = i;

            if (left < heap.Count && heap[left].Priority < heap[smallest].Priority)
                smallest = left;
            if (right < heap.Count && heap[right].Priority < heap[smallest].Priority)
                smallest = right;

            if (smallest == i) break;

            Swap(i, smallest);
            i = smallest;
        }
    }

    private void Swap(int i, int j)
    {
        (heap[i], heap[j]) = (heap[j], heap[i]);
        nodePositions[heap[i].Item] = i;
        nodePositions[heap[j].Item] = j;
    }

    public bool Contains(T item) => nodePositions.ContainsKey(item);

    public bool TryUpdatePriority(T item, float newPriority)
    {
        if (!nodePositions.TryGetValue(item, out int index)) return false;
        if (newPriority >= heap[index].Priority) return false;

        heap[index] = (item, newPriority);
        HeapifyUp(index);
        return true;
    }


}