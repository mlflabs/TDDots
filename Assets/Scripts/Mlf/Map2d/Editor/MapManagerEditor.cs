using Mlf.Grid2d;
using Mlf.Map2d;
using Mlf.Tiles2d;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[CustomEditor(typeof(MapManager))]
public class MapManagerEditor : Editor
{


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MapManager manager = (MapManager)target;

        if (GUILayout.Button("Load Tilemap from Grid"))
        {

            Debug.Log("Load Timemap from Grid");
            float3 cellWorldPos;
            Vector3Int tilePos1;
            Cell cell;
            float3 offset = new float3(0.1f, 0.1f, 0f);
            for (int x = 0; x < manager.mainMap.grid.gridSize.x; x++)
                for (int y = 0; y < manager.mainMap.grid.gridSize.y; y++)
                {
                    cell = manager.mainMap.grid.GetCell(x, y);
                    cellWorldPos = manager.mainMap.GetCellWorldCoordinates(cell.pos, 0)
                        + offset;
                    tilePos1 = manager.tilemap.layoutGrid.WorldToCell(cellWorldPos);
                    Debug.Log($"Updating tile GridPos: {cell.pos}, TilePos: {tilePos1}, WorldPos: {cellWorldPos}");
                    manager.tilemap.SetTile(tilePos1,
                        manager.mainMap.tileRefList.list[cell.tileRefIndex].tile);
                }
        }


        if (GUILayout.Button("Update Grid from Tilemap"))
        {
            Debug.Log("Update from Tilemap.....");

            Cell[] cells = new Cell[manager.mainMap.grid.gridSize.x *
                                    manager.mainMap.grid.gridSize.y];

            float3 pos;
            TileBase tile;
            Vector3Int tilePos;
            TileDataSo data;
            int index;
            int tileRefIndex;
            for (int x = 0; x < manager.mainMap.grid.gridSize.x; x++)
                for (int y = 0; y < manager.mainMap.grid.gridSize.y; y++)
                {
                    pos = manager.mainMap.GetCellWorldCoordinates(new int2(x, y), 0);
                    tilePos = manager.tilemap.layoutGrid.WorldToCell(pos);
                    tile = manager.tilemap.GetTile(tilePos);

                    tileRefIndex = manager.mainMap.tileRefList.GETRefIndex(tile);

                    if (tileRefIndex == -1)
                    {
                        Debug.Log($"Tile not found::: {tile}, tilePos:{tilePos}");
                        continue;
                    }

                    Debug.Log("TileRefIndex:: " + tileRefIndex);
                    data = manager.mainMap.tileRefList.list[tileRefIndex].data;
                    index = manager.mainMap.GetGridIndex(x, y);
                    cells[index] = new Cell
                    {
                        tileRefIndex = (byte)tileRefIndex,
                        pos = new int2(x, y),
                        walkSpeed = data.walkSpeed
                    };

                }
            manager.mainMap.grid.cells = cells;

        }

        if (GUILayout.Button("Clear Tilemap Data"))
        {
            manager.tilemap?.ClearAllTiles();
        }
    }




}
