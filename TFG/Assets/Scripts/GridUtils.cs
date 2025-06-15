using System.Collections.Generic;
using UnityEngine;

public static class GridUtils
{
    // Algorisme de Bresenham per obtenir una llista de cel·les entre dos punts en un grid 2D
    public static List<Vector2Int> Bresenham(Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> cells = new List<Vector2Int>();

        // Punts d'inici i final
        int x0 = start.x;
        int y0 = start.y;
        int x1 = end.x;
        int y1 = end.y;

        // Direcció en la que haurem de moure'ns en els dos eixos
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;

        // Error acumulat
        int err = dx - dy;

        // Mentre no arribem al punt final
        while (true)
        {
            // Afegim la cel·la actual a la llista
            cells.Add(new Vector2Int(x0, y0));

            // Si hem arribat al punt final, sortim del bucle
            if (x0 == x1 && y0 == y1)
                break;

            // Calculem l'error doble per determinar si hem de moure'ns en l'eix X o Y
            int e2 = 2 * err;

            // Si l'error és major que -dy, movem en l'eix X
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }

            // Si l'error és menor que dx, movem en l'eix Y
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }

        return cells;
    }
}
