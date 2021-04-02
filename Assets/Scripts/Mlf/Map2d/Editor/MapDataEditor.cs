using Mlf.Grid2d;
using Mlf.Map2d;
using Mlf.Tiles2d;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
// ReSharper disable Unity.PerformanceCriticalCodeInvocation
// ReSharper disable Unity.PerformanceCriticalCodeNullComparison

[CustomEditor(typeof(MapDataSo))]
public class MapDataEditor : Editor
{
    private MapDataSo manager;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();


        manager = (MapDataSo) target;

        if (GUILayout.Button("Load Tilemap from Grid"))
        {
            var tilemap = GameObject.Find(manager.tilemapName).GetComponent<Tilemap>();
            var tilemap2 = GameObject.Find(manager.tilemapSecondaryName).GetComponent<Tilemap>();

            if (tilemap == null || tilemap2 == null)
            {
                Debug.LogWarning("Couldn't load tilemap object, please check name: " + manager.tilemapName);
                return;
            }

            float3 cellWorldPos;
            Vector3Int tilePos1;
            Cell cell;
            float3 offset = new float3(0.1f, 0.1f, 0f);
            for (int x = 0; x < manager.grid.gridSize.x; x++)
                for (int y = 0; y < manager.grid.gridSize.y; y++)
                {

                    //lower tile
                    cell = manager.grid.GetCell(x, y);
                    cellWorldPos = manager.GetCellWorldCoordinates(cell.pos, 0) + offset;
                    tilePos1 = tilemap.layoutGrid.WorldToCell(cellWorldPos);
                    ///Debug.Log($"Updating tile GridPos: {cell.pos}, TilePos: {tilePos1}, WorldPos: {cellWorldPos}");

                    if(manager.tileRefList.list[cell.tileRefIndex].isLowerGround)
                        tilemap.SetTile(tilePos1, manager.tileRefList.list[cell.tileRefIndex].tile);
                    else
                    {
                        tilemap2.SetTile(tilePos1, manager.tileRefList.list[cell.tileRefIndex].tile);
                        //see if we have lower ground specified as well
                        if(manager.tileRefList.list[cell.tileRefIndex].lowerGround != null)
                        {
                            tilemap.SetTile(tilePos1, manager.tileRefList.list[cell.tileRefIndex].lowerGround);
                        }
                    }
                       

                }
        }


        if (GUILayout.Button("Update Grid from Tilemap"))
        {
            Debug.Log("Update from Tilemap.....");
            Tilemap tilemap = GameObject.Find(manager.tilemapName).GetComponent<Tilemap>();
            Tilemap tilemap2 = GameObject.Find(manager.tilemapSecondaryName).GetComponent<Tilemap>();

            if (tilemap == null || tilemap2 == null)
            {
                Debug.LogWarning("Couldn't load tilemap object, please check name: " + 
                                 manager.tilemapName);
                return;
            }

            Cell[] cells = new Cell[manager.grid.gridSize.x *
                                    manager.grid.gridSize.y];

            float3 pos;
            TileBase tile;
            TileBase tile2;
            TileBase secondaryTile;
            Vector3Int tilePos;
            TileDataSo data;
            int index;
            int tileRefIndex;
            for (int x = 0; x < manager.grid.gridSize.x; x++)
                for (int y = 0; y < manager.grid.gridSize.y; y++)
                {
                    pos = manager.GetCellWorldCoordinates(new int2(x, y), 0);
                    tilePos = tilemap.layoutGrid.WorldToCell(pos);


                    tile2 = tilemap2.GetTile(tilePos);
                    tileRefIndex = manager.tileRefList.GETRefIndex(tile2);

                    if(tileRefIndex == -1)
                    {
                        //No upper ground tile, see if we have lower ground.....
                        tile = tilemap.GetTile(tilePos);

                        tileRefIndex = manager.tileRefList.GETRefIndex(tile);

                        if (tileRefIndex == -1)
                        {
                            //Debug.Log($"Tile not found::: {tile}, tilePos:{tilePos}");
                            continue;
                        }
                    }

                    

                    //Debug.Log("TileRefIndex:: "+tileRefIndex);
                    data = manager.tileRefList.list[tileRefIndex].data;
                    index = manager.GetGridIndex(x, y);
                    cells[index] = Cell.FromTileDataSo(data, new int2(x, y), (byte)tileRefIndex);
                        
                }
            manager.grid.cells = cells;

        }



        if (GUILayout.Button("Update Grid Plants from Tilemap"))
        {

            LoadPlantsFromTilemap();



        }











        if (GUILayout.Button("Clear Tilemap Data"))
        {

            Tilemap tilemap = GameObject.Find(manager.tilemapName).GetComponent<Tilemap>();

            if (tilemap == null)
            {
                Debug.LogWarning("Couldn't load tilemap object, please check name: " + 
                                 manager.tilemapName);
                //return;
            }
            else
                tilemap.ClearAllTiles();

            tilemap = GameObject.Find(manager.tilemapSecondaryName).GetComponent<Tilemap>();

            if (tilemap == null)
            {
                Debug.LogWarning("Couldn't load secondary object, please check name: " + 
                                 manager.tilemapName);
                //return;
            }
            else
                tilemap.ClearAllTiles();


            tilemap = GameObject.Find(manager.plantTilemapName).GetComponent<Tilemap>();
            if (tilemap == null)
            {
                Debug.LogWarning("Couldn't load secondary object, please check name: " + 
                                 manager.tilemapName);
                //return;
            }
            else
                tilemap.ClearAllTiles();

        }
    }
    
    private void LoadPlantsFromTilemap()
    {
        Debug.Log("Loading plants from Tilemap.....");

        Tilemap tilemap = GameObject.Find(manager.plantTilemapName).GetComponent<Tilemap>();

        if (tilemap == null)
        {
            Debug.LogWarning("Couldn't load plant tilemap object, please check name: " + 
                             manager.tilemapName);
            return;
        }

        List<PlantItem> plants = new List<PlantItem>();

        float3 pos;
        TileBase tile;
        Vector3Int tilePos;
        PlantDataSo data;
        int index;
        int tileRefIndex;
        for (int x = 0; x < manager.grid.gridSize.x; x++)
            for (int y = 0; y < manager.grid.gridSize.y; y++)
            {
                pos = manager.GetCellWorldCoordinates(new int2(x, y), 0);
                tilePos = tilemap.layoutGrid.WorldToCell(pos);
                tile = tilemap.GetTile(tilePos);

                //Debug.Log($"Tile: {tile.name}");

                tileRefIndex = manager.plantRefList.GETRefIndex(tile);


                //did we get a tilemap?
                if (tileRefIndex == -1)
                {
                    Debug.Log($"Tile not found::: {tile}, tilePos:{tilePos.ToString()}");
                    continue;
                }

                //Debug.Log("TileRefIndex:: "+tileRefIndex);
                data = manager.plantRefList.list[tileRefIndex].data;
                index = manager.GetGridIndex(x, y);
                plants.Add(new PlantItem
                {
                    Pos = new int2(x, y),
                    TypeId = (byte)tileRefIndex,
                });

            }
        manager.plantItems = plants;
    }
}
