using Mlf.Grid2d;
using Mlf.Map2d;
using Mlf.Tiles2d;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[CustomEditor(typeof(MapDataSO))]
public class MapDataEditor : Editor
{
    Tilemap tilemap;


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();


        MapDataSO manager = (MapDataSO)target;

        if (GUILayout.Button("Load Tilemap from Grid"))
        {
            Tilemap tilemap = GameObject.Find(manager.tilemapName).GetComponent<Tilemap>();

            if (tilemap == null)
            {
                Debug.LogWarning("Couldn't load tilemap object, please check name: " + manager.tilemapName);
                return;
            }

            float3 cellWorldPos;
            Vector3Int tilePos1;
            Cell cell;
            float3 offset = new float3(0.1f, 0.1f, 0f);
            for (int x = 0; x < manager.Grid.GridSize.x; x++)
                for (int y = 0; y < manager.Grid.GridSize.y; y++)
                {
                    cell = manager.Grid.GetCell(x, y);
                    cellWorldPos = manager.GetCellWorldCoordinates(cell.pos, 0)
                        + offset;
                    tilePos1 = tilemap.layoutGrid.WorldToCell(cellWorldPos);
                    ///Debug.Log($"Updating tile GridPos: {cell.pos}, TilePos: {tilePos1}, WorldPos: {cellWorldPos}");
                    tilemap.SetTile(tilePos1,
                        manager.TileRefList.list[cell.tileRefIndex].tile);
                }
        }


        if (GUILayout.Button("Update Grid from Tilemap"))
        {
            Debug.Log("Update from Tilemap.....");
            Tilemap tilemap = GameObject.Find(manager.tilemapName).GetComponent<Tilemap>();

            if (tilemap == null)
            {
                Debug.LogWarning("Couldn't load tilemap object, please check name: " + manager.tilemapName);
                return;
            }

            Cell[] cells = new Cell[manager.Grid.GridSize.x *
                                    manager.Grid.GridSize.y];

            float3 pos;
            TileBase tile;
            Vector3Int tilePos;
            TileDataSO data;
            int index;
            int tileRefIndex;
            for (int x = 0; x < manager.Grid.GridSize.x; x++)
                for (int y = 0; y < manager.Grid.GridSize.y; y++)
                {
                    pos = manager.GetCellWorldCoordinates(new int2(x, y), 0);
                    tilePos = tilemap.layoutGrid.WorldToCell(pos);
                    tile = tilemap.GetTile(tilePos);

                    tileRefIndex = manager.TileRefList.getRefIndex(tile);

                    if (tileRefIndex == -1)
                    {
                        //Debug.Log($"Tile not found::: {tile}, tilePos:{tilePos}");
                        continue;
                    }

                    //Debug.Log("TileRefIndex:: "+tileRefIndex);
                    data = manager.TileRefList.list[tileRefIndex].data;
                    index = manager.GetGridIndex(x, y);
                    cells[index] = new Cell
                    {
                        tileRefIndex = (byte)tileRefIndex,
                        pos = new int2(x, y),
                        walkSpeed = data.walkSpeed,
                    };

                }
            manager.Grid.Cells = cells;

        }

        if (GUILayout.Button("Clear Tilemap Data"))
        {

            Tilemap tilemap = GameObject.Find(manager.tilemapName).GetComponent<Tilemap>();

            if (tilemap == null)
            {
                Debug.LogWarning("Couldn't load tilemap object, please check name: " + manager.tilemapName);
                return;
            }

            tilemap.ClearAllTiles();
        }
    }




}
