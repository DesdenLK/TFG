using System.Collections.Generic;

public class SimplePriorityQueue<T>
{
    private List<(T Item, float Priority)> elements = new List<(T, float)>();

    public int Count => elements.Count;

    public void Enqueue(T item, float priority)
    {
        elements.Add((item, priority));
    }

    public T Dequeue()
    {
        int bestIndex = 0;

        for (int i = 0; i < elements.Count; i++)
        {
            if (elements[i].Priority < elements[bestIndex].Priority)
            {
                bestIndex = i;
            }
        }

        T bestItem = elements[bestIndex].Item;
        elements.RemoveAt(bestIndex);
        return bestItem;
    }

    public bool TryDequeue(out T item, out float priority)
    {
        if (elements.Count == 0)
        {
            item = default;
            priority = default;
            return false;
        }

        int bestIndex = 0;
        for (int i = 0; i < elements.Count; i++)
        {
            if (elements[i].Priority < elements[bestIndex].Priority)
            {
                bestIndex = i;
            }
        }

        item = elements[bestIndex].Item;
        priority = elements[bestIndex].Priority;
        elements.RemoveAt(bestIndex);
        return true;
    }
}