using System.Collections.Generic;

public class SimplePriorityQueue<T>
{
    private List<(T Item, float Priority)> heap = new List<(T, float)>();
    private Dictionary<T, int> nodePositions = new();

    public int Count => heap.Count;

    // Afegeix un element a la cua amb una prioritat donada.
    public void Enqueue(T item, float priority)
    {
        if (nodePositions.TryGetValue(item, out int existingIndex))
        {
            // Si l'element ja existeix i la nova prioritat és més baixa, actualitza'l.
            if (priority < heap[existingIndex].Priority)
            {
                heap[existingIndex] = (item, priority);
                HeapifyUp(existingIndex);
            }
            return;
        }

        // Si l'element no existeix, afegeix-lo a la cua.
        heap.Add((item, priority));
        int i = heap.Count - 1;
        nodePositions[item] = i;
        HeapifyUp(i);
    }

    public T Dequeue()
    {
        // Elimina l'element amb la prioritat més baixa
        T item = heap[0].Item;
        nodePositions.Remove(item);

        // Si la cua està buida després de l'eliminació, retorna l'element.
        int last = heap.Count - 1;
        if (last == 0)
        {
            heap.RemoveAt(0);
            return item;
        }

        // Substitueix l'element eliminat amb l'últim element i reorganitza la cua.
        heap[0] = heap[last];
        nodePositions[heap[0].Item] = 0;
        heap.RemoveAt(last);
        HeapifyDown(0);

        return item;
    }

    // Reorganitza la cua cap amunt per mantenir l'ordre de prioritat.
    private void HeapifyUp(int i)
    {
        // Recorre cap amunt fins que l'element actual sigui més gran que el seu pare.
        while (i > 0)
        {
            int parent = (i - 1) / 2;
            if (heap[parent].Priority <= heap[i].Priority) break;
            // Si l'element actual té una prioritat més baixa que el seu pare, intercanvia'ls.
            Swap(i, parent);
            i = parent;
        }
    }

    private void HeapifyDown(int i)
    {
        // Recorre cap avall fins que l'element actual sigui més petit que els seus fills.
        while (true)
        {
            int left = 2 * i + 1;
            int right = 2 * i + 2;
            int smallest = i;

            // Comprova si els fills existeixen i si tenen una prioritat més baixa que l'element actual.
            if (left < heap.Count && heap[left].Priority < heap[smallest].Priority)
                smallest = left;
            if (right < heap.Count && heap[right].Priority < heap[smallest].Priority)
                smallest = right;

            // Si l'element actual és més petit que els seus fills, no cal fer res.
            if (smallest == i) break;

            // Intercanvia l'element actual amb el més petit dels seus fills.
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

    // Intenta actualitzar la prioritat d'un element existent a la cua.
    public bool TryUpdatePriority(T item, float newPriority)
    {
        if (!nodePositions.TryGetValue(item, out int index)) return false;
        if (newPriority >= heap[index].Priority) return false;

        heap[index] = (item, newPriority);
        HeapifyUp(index);
        return true;
    }


}