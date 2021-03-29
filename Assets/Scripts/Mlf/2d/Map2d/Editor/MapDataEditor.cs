using Mlf.Grid2d;
using Mlf.Map2d;
using Mlf.Tiles2d;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[CustomEditor(typeof(MapDataSO))]
public class MapDataEditor : Editor
{
    Tilemap tilemap;
    MapDataSO manager;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();


        manager = (MapDataSO)target;

        if (GUILayout.Button("Load Tilemap from Grid"))
        {
            Tilemap tilemap = GameObject.Find(manager.tilemapName).GetComponent<Tilemap>();
            Tilemap tilemap2 = GameObject.Find(manager.tilemapSecondaryName).GetComponent<Tilemap>();

            if (tilemap == null || tilemap2 == null)
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

                    //lower tile
                    cell = manager.Grid.GetCell(x, y);
                    cellWorldPos = manager.GetCellWorldCoordinates(cell.pos, 0) + offset;
                    tilePos1 = tilemap.layoutGrid.WorldToCell(cellWorldPos);
                    ///Debug.Log($"Updating tile GridPos: {cell.pos}, TilePos: {tilePos1}, WorldPos: {cellWorldPos}");

                    if(manager.TileRefList.list[cell.tileRefIndex].isLowerGround)
                        tilemap.SetTile(tilePos1, manager.TileRefList.list[cell.tileRefIndex].tile);
                    else
                    {
                        tilemap2.SetTile(tilePos1, manager.TileRefList.list[cell.tileRefIndex].tile);
                        //see if we have lower ground specified as well
                        if(manager.TileRefList.list[cell.tileRefIndex].lowerGround != null)
                        {
                            tilemap.SetTile(tilePos1, manager.TileRefList.list[cell.tileRefIndex].lowerGround);
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
                Debug.LogWarning("Couldn't load tilemap object, please check name: " + manager.tilemapName);
                return;
            }

            Cell[] cells = new Cell[manager.Grid.GridSize.x *
                                    manager.Grid.GridSize.y];

            float3 pos;
            TileBase tile;
            TileBase tile2;
            TileBase secondaryTile;
            Vector3Int tilePos;
            TileDataSO data;
            int index;
            int tileRefIndex;
            for (int x = 0; x < manager.Grid.GridSize.x; x++)
                for (int y = 0; y < manager.Grid.GridSize.y; y++)
                {
                    pos = manager.GetCellWorldCoordinates(new int2(x, y), 0);
                    tilePos = tilemap.layoutGrid.WorldToCell(pos);


                    tile2 = tilemap2.GetTile(tilePos);
                    tileRefIndex = manager.TileRefList.getRefIndex(tile2);

                    if(tileRefIndex == -1)
                    {
                        //No upper ground tile, see if we have lower ground.....
                        tile = tilemap.GetTile(tilePos);

                        tileRefIndex = manager.TileRefList.getRefIndex(tile);

                        if (tileRefIndex == -1)
                        {
                            //Debug.Log($"Tile not found::: {tile}, tilePos:{tilePos}");
                            continue;
                        }
                    }

                    

                    //Debug.Log("TileRefIndex:: "+tileRefIndex);
                    data = manager.TileRefList.list[tileRefIndex].data;
                    index = manager.GetGridIndex(x, y);
                    cells[index] = Cell.FromTileDataSO(data, new int2(x, y), (byte)tileRefIndex);
                        
                }
            manager.Grid.Cells = cells;

        }



        if (GUILayout.Button("Update Grid Plants from Tilemap"))
        {

            loadPlantsFromTilemap();



        }











        if (GUILayout.Button("Clear Tilemap Data"))
        {

            Tilemap tilemap = GameObject.Find(manager.tilemapName).GetComponent<Tilemap>();

            if (tilemap == null)
            {
                Debug.LogWarning("Couldn't load tilemap object, please check name: " + manager.tilemapName);
                //return;
            }
            else
                tilemap.ClearAllTiles();

            tilemap = GameObject.Find(manager.tilemapSecondaryName).GetComponent<Tilemap>();

            if (tilemap == null)
            {
                Debug.LogWarning("Couldn't load secondary object, please check name: " + manager.tilemapName);
                //return;
            }
            else
                tilemap.ClearAllTiles();


            tilemap = GameObject.Find(manager.plantTilemapName).GetComponent<Tilemap>();
            if (tilemap == null)
            {
                Debug.LogWarning("Couldn't load secondary object, please check name: " + manager.tilemapName);
                //return;
            }
            else
                tilemap.ClearAllTiles();

        }
    }
    
    private void loadPlantsFromTilemap()
    {
        Debug.Log("Loading plants from Tilemap.....");

        Tilemap tilemap = GameObject.Find(manager.plantTilemapName).GetComponent<Tilemap>();

        if (tilemap == null)
        {
            Debug.LogWarning("Couldn't load plant tilemap object, please check name: " + manager.tilemapName);
            return;
        }

        List<PlantItem> plants = new List<PlantItem>();

        float3 pos;
        TileBase tile;
        Vector3Int tilePos;
        PlantDataSO data;
        int index;
        int tileRefIndex;
        for (int x = 0; x < manager.Grid.GridSize.x; x++)
            for (int y = 0; y < manager.Grid.GridSize.y; y++)
            {
                pos = manager.GetCellWorldCoordinates(new int2(x, y), 0);
                tilePos = tilemap.layoutGrid.WorldToCell(pos);
                tile = tilemap.GetTile(tilePos);

                //Debug.Log($"Tile: {tile.name}");

                tileRefIndex = manager.PlantRefList.getRefIndex(tile);


                //did we get a tilemap?
                if (tileRefIndex == -1)
                {
                    Debug.Log($"Tile not found::: {tile}, tilePos:{tilePos}");
                    continue;
                }

                //Debug.Log("TileRefIndex:: "+tileRefIndex);
                data = manager.PlantRefList.list[tileRefIndex].data;
                index = manager.GetGridIndex(x, y);
                plants.Add(new PlantItem
                {
                    pos = new int2(x, y),
                    typeId = (byte)tileRefIndex,
                });

            }
        manager.PlantItems = plants;
    }



}
