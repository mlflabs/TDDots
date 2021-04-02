using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;


namespace Mlf.Grid2d
{
    [System.Serializable]
    public class GridData
    {
        [FormerlySerializedAs("GridSize")] public int2 gridSize;
        [FormerlySerializedAs("Cells")] public Cell[] cells;


        public GridData(Cell[] cells, int2 gridSize)
        {
            this.cells = cells;
            this.gridSize = gridSize;
        }

        public GridData(int2 gridSize)
        {
            this.gridSize = gridSize;
        }

        public int GetIndex(int x, int y)
        {
            return x + (y * gridSize.x);
        }
        public int GetIndex(int2 pos)
        {
            return pos.x + (pos.y * gridSize.x);
        }

        public Cell GetCell(int x, int y)
        {
            if (x < 0 || x >= gridSize.x || y < 0 || y >= gridSize.y)
                return new Cell { pos = new int2(-1, -1) };
            //Debug.Log($"GetCell x: {x}, y:{y}, i: {GetIndex(x, y)}");
            return cells[GetIndex(x, y)];
        }

    }
}