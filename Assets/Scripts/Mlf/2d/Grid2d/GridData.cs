using Unity.Mathematics;
using UnityEngine;


namespace Mlf.Grid2d
{
    [System.Serializable]
    public class GridData
    {
        public int2 GridSize;
        public Cell[] Cells;


        public GridData(Cell[] cells, int2 gridSize)
        {
            Cells = cells;
            GridSize = gridSize;
        }

        public GridData(int2 gridSize)
        {
            GridSize = gridSize;
        }

        public int GetIndex(int x, int y)
        {
            return x + (y * GridSize.x);
        }
        public int GetIndex(int2 pos)
        {
            return pos.x + (pos.y * GridSize.x);
        }

        public Cell GetCell(int x, int y)
        {
            if (x < 0 || x >= GridSize.x || y < 0 || y >= GridSize.y)
                return new Cell { pos = new int2(-1, -1) };
            //Debug.Log($"GetCell x: {x}, y:{y}, i: {GetIndex(x, y)}");
            return Cells[GetIndex(x, y)];
        }

    }
}