using Mlf.Map2d;
using Mlf.Tiles2d;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Mlf.Grid2d.Ecs
{

    [System.Serializable]
    public struct MapComponentShared : ISharedComponentData
    {
        public MapType type;
    }



    public struct GroundTypeStruct
    {

        public float walkSpeed;
        public float swimSpeed;

        public bool canBuild;
        public bool canGrow;

        public bool hasFreshWater;

        public bool canWalk { get { return walkSpeed != 0; } }

        public static GroundTypeStruct fromGroundTypeSO(TileDataSO so)
        {
            return new GroundTypeStruct
            {
                walkSpeed = so.walkSpeed,
                swimSpeed = so.swimSpeed,
                canBuild = so.canBuild,
                canGrow = so.canGrow,
                //resources
                hasFreshWater = so.hasFreshWater,
            };
        }
    }


    public struct GridDataStruct
    {
        public int id;
        public int2 gridSize;
        public float2 cellSize;
        public float2 originPosition;
        public MapType mapType;


        public int getIndex(in int2 pos)
        {
            return pos.x + (pos.y * gridSize.x);
        }
        public int getIndex(int x, int z)
        {
            return x + (z * gridSize.x);
        }

        public int2 getGridPosition(in float2 worldPosition)
        {
            return new int2(
            (int)((worldPosition - originPosition).x / cellSize.x),
            (int)((worldPosition - originPosition).y / cellSize.y));
        }
        public int2 getGridPosition(in float3 _worldPosition)
        {
            float2 worldPosition = UtilsGrid.ToFloat2(_worldPosition);
            return new int2(
            (int)((worldPosition - originPosition).x / cellSize.x),
            (int)((worldPosition - originPosition).y / cellSize.y));
        }

        public Cell getCell(in int2 pos, in NativeArray<Cell>cells)
        {
            //are we inside the grid
            if (!validGridPosition(pos)) return new Cell();
            //if (mapType == MapType.main)
                return cells[getIndex(in pos)];
            //else
            //    return cells[getIndex(in pos)];
        }

        public bool validGridPosition(in int2 pos)
        {
            if (pos.x < 0 || pos.y < 0 || pos.x >= gridSize.x || pos.y >= gridSize.y)
                return false;
            return true;
        }

        public float2 getWorldPosition(int2 gridPos)
        {
            //int2 pos = getCell(gridPos).pos;
            //return (pos * cellSize) + originPosition;
            return (gridPos * cellSize) + originPosition;
        }

        public float2 getWorldPositionCellCenter(in int2 gridPos)
        {
            return (gridPos * cellSize) +
                    originPosition +
                    (cellSize / 2);
            //return (getCell(gridPos).pos * cellSize) +
            //       originPosition +
            //       (cellSize / 2);
        }

        public float3 getWorldPositionCellCenter(in int2 gridPos, float z = 0f)
        {
            float2 f2 = (gridPos * cellSize) +
                    originPosition +
                    (cellSize / 2);
            //float2 f2 = (getCell(gridPos).pos * cellSize) +
            //        originPosition +
            //        (cellSize / 2);
            return new float3(f2.x, f2.y, z);
        }

        public float2 GetNearestPoint(float2 pos)
        {
            int2 count = (int2)(pos / cellSize);
            return count * cellSize;
        }
    }




    public class GridSystem : SystemBase
    {
        //inner refferences
        //EndSimulationEntityCommandBufferSystem m_EndSimulationEcbSystem;
        //public static NativeArray<Cell> gridMap;
        //public static int2 gridMapSize;

        //public static NativeHashMap<int, GridData> Maps;
        public static MapType CurrentMapType = MapType.main;
        public static GridDataStruct MainMap;
        public static GridDataStruct SecondaryMap;

        public static NativeArray<Cell> MainMapCells;
        public static NativeArray<Cell> SecondaryMapCells;

        public static NativeHashMap<byte, GroundTypeStruct> GroundTypeReferences;


        //default is 0. we assume we have at least one map, and first come first serve
        /*private static int _currentMapIndex;
        public static int currentMapIndex
        {
            get { return _currentMapIndex; }
            set
            {
                _currentMapIndex = value;
                //when value changes we need to also change the currentMapData value
                currentMapData = Maps[value];
            }
        }*/
        //public static GridDataStruct currentMapData;

        ///public static bool dirtyMaps;

        //public int gridWidth;
        //public int gridHeight;




        protected override void OnCreate()
        {
            base.OnCreate();

            //m_EndSimulationEcbSystem = World
            //.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            //Maps =  new NativeHashMap<int, GridData>(2,Allocator.Persistent);

        }


        protected override void OnUpdate()
        {

        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (MainMapCells.IsCreated) MainMapCells.Dispose();
            if (SecondaryMapCells.IsCreated) SecondaryMapCells.Dispose();

            if (GroundTypeReferences.IsCreated) GroundTypeReferences.Dispose();

        }



        public static int getIndex(in int2 pos, MapType mapType = MapType.current)
        {
            if (mapType == MapType.current) mapType = CurrentMapType;

            if (mapType == MapType.main)
                return pos.x + (pos.y * MainMap.gridSize.x);
            else
                return pos.x + (pos.y * SecondaryMap.gridSize.x);
        }
        public static int getIndex(int x, int z, MapType mapType)
        {
            if (mapType == MapType.main)
                return x + (z * MainMap.gridSize.x);
            else
                return x + (z * SecondaryMap.gridSize.x);
        }

        public static int2 getGridPosition(in float2 worldPosition, MapType mapType = MapType.current)
        {
            if (mapType == MapType.current) mapType = CurrentMapType;

            if (mapType == MapType.main)
                return new int2(
                    (int)((worldPosition - MainMap.originPosition).x / MainMap.cellSize.x),
                    (int)((worldPosition - MainMap.originPosition).y / MainMap.cellSize.y));
            else
                return new int2(
                    (int)((worldPosition - SecondaryMap.originPosition).x / SecondaryMap.cellSize.x),
                    (int)((worldPosition - SecondaryMap.originPosition).y / SecondaryMap.cellSize.y));
        }

        public static int2 getGridPosition(in float3 _worldPosition, MapType mapType = MapType.current)
        {
            if (mapType == MapType.current) mapType = CurrentMapType;

            float2 worldPosition = UtilsGrid.ToFloat2(_worldPosition);
            if (mapType == MapType.main)
            {

                return new int2(
                    (int)((worldPosition - MainMap.originPosition).x / MainMap.cellSize.x),
                    (int)((worldPosition - MainMap.originPosition).y / MainMap.cellSize.y));
            }
            else
            {
                return new int2(
                    (int)((worldPosition - SecondaryMap.originPosition).x / SecondaryMap.cellSize.x),
                    (int)((worldPosition - SecondaryMap.originPosition).y / SecondaryMap.cellSize.y));
            }

        }

        private static void SetCell(Cell c, MapType type)
        {
            int index = getIndex(c.pos, type);
            if (type == MapType.main)
                MainMapCells[index] = c;
            else
                SecondaryMapCells[index] = c;

        }

        public static Cell getCell(in int2 pos, MapType mapType = MapType.current)
        {
            if (mapType == MapType.current) mapType = CurrentMapType;

            if (!validGridPosition(pos, mapType)) return new Cell();
            if (mapType == MapType.main)
                return MainMapCells[getIndex(in pos, mapType)];
            else
                return SecondaryMapCells[getIndex(in pos, mapType)];

        }

        public static float2 getCellSize(MapType mapType = MapType.current)
        {
            if (mapType == MapType.current) mapType = CurrentMapType;

            if (mapType == MapType.main)
                return MainMap.cellSize;
            else
                return SecondaryMap.cellSize;
        }
        
        public static Cell getCell(in float2 worldPosition)
        {

            //Which map are we in
            int2 pos = new int2(
                   (int)((worldPosition - MainMap.originPosition).x / MainMap.cellSize.x),
                   (int)((worldPosition - MainMap.originPosition).y / MainMap.cellSize.y));


            if(validGridPosition(pos, MapType.main))
            {
                return getCell(pos, MapType.main);
            }
            else if(validGridPosition(pos, MapType.secondary))
            {
                return getCell(pos, MapType.secondary);
            }

            return new Cell();


        }


        public static bool validGridPosition(in int2 pos, MapType mapType)
        {
            if (mapType == MapType.main)
                return !(pos.x < 0 || pos.y < 0 ||
                    pos.x >= MainMap.gridSize.x || pos.y >= MainMap.gridSize.y);
            else
                return !(pos.x < 0 || pos.y < 0 ||
                    pos.x >= SecondaryMap.gridSize.x || pos.y >= SecondaryMap.gridSize.y);


        }

        public static float2 getWorldPosition(in int2 gridPos, MapType mapType = MapType.current)
        {
            if (mapType == MapType.current) mapType = CurrentMapType;
            if (mapType == MapType.main)
                return (gridPos * MainMap.cellSize) + MainMap.originPosition;
            else
                return (gridPos * SecondaryMap.cellSize) + SecondaryMap.originPosition;
        }

        public static float3 getWorldPosition(in int2 gridPos, MapType mapType = MapType.current, int z = 0)
        {
            if (mapType == MapType.current) mapType = CurrentMapType;
            float2 pos;
            if (mapType == MapType.main)
                pos = (gridPos * MainMap.cellSize) + MainMap.originPosition;
            else
                pos= (gridPos * SecondaryMap.cellSize) + SecondaryMap.originPosition;

            return new float3(pos.x, pos.y, z);
        }


        public static float2 getWorldPositionCellCenter(in int2 gridPos, MapType mapType = MapType.current)
        {
            if (mapType == MapType.current) mapType = CurrentMapType;
            if (mapType == MapType.main)
                return (gridPos * MainMap.cellSize) + MainMap.originPosition + (MainMap.cellSize / 2);
            else
                return (gridPos * SecondaryMap.cellSize) + SecondaryMap.originPosition + (SecondaryMap.cellSize / 2);
        }

        public static float3 getWorldPositionCellCenter(in int2 gridPos, MapType mapType = MapType.current, float z = 0f)
        {
            if (mapType == MapType.current) mapType = CurrentMapType;
            float2 f2;
            if (mapType == MapType.main)
                f2 = (gridPos * MainMap.cellSize) + MainMap.originPosition + (MainMap.cellSize / 2);
            else
                f2 = (gridPos * SecondaryMap.cellSize) + SecondaryMap.originPosition + (SecondaryMap.cellSize / 2);
            return new float3(f2.x, f2.y, z);
        }


        public static void UpdateGroundReferences(TileReferenceListSO referenceListSO)
        {
            if (GroundTypeReferences.IsCreated)
                GroundTypeReferences.Dispose();

            GroundTypeReferences = new NativeHashMap<byte, GroundTypeStruct>(
                referenceListSO.list.Length, Allocator.Persistent);
            for (int i = 0; i < referenceListSO.list.Length; i++)
            {
                GroundTypeReferences[(byte)i] =
                    GroundTypeStruct.fromGroundTypeSO(referenceListSO.list[i].data);

            }
        }




        public static void UpdateCell(int2 pos, MapType mapType)
        {
            //Debug.LogWarning("Updating Grid Cell trying.....");
            Cell c = getCell(pos, mapType);
            if (c.isDefault()) return;

            SetCell(AddCellModifiers(c, mapType), mapType);

           // Debug.LogWarning("Updated");
        }

        public static void UpdateCell(int index, MapType mapType)
        {
            //Debug.LogWarning("Updating Grid Cell trying.....");
            Cell c = getCell(index, mapType);
            if (c.isDefault()) return;

            SetCell(AddCellModifiers(c, mapType), mapType);

            //Debug.LogWarning("Updated");
        }


        public static Cell getCell(int index, MapType type)
        {
            

            if(type == MapType.main)
            {
                return MainMapCells[index];
            }
            else if(type == MapType.secondary)
            {
                return SecondaryMapCells[index];
            }
            else if(type == MapType.current)
            {
                return getCell(index, GridSystem.CurrentMapType);
            }
            else
            {
                Debug.LogError("Maptype not recognized:: " + type);
            }
            return new Cell();

        }

        public static Cell AddCellModifiers(Cell c, MapType mapType)
        {
            int index = getIndex(c.pos, mapType);
            byte typeid;
            if (MapPlantManagerSystem.CellHasPlantItem(index, mapType))
            {
                typeid = MapPlantManagerSystem.GetCellPlantItem(index, mapType).typeId;
                c = MapPlantManagerSystem.PlantItemReferences[typeid].UpdateCell(c);
            }
            if(MapBuildingManagerSystem.CellHasBuilding(index, mapType))
            {
                typeid = MapBuildingManagerSystem.GetCellBuilding(index, mapType).typeId;
                c = MapBuildingManagerSystem.BuildingReferences[typeid].updateCell(c);
            }

            return c;
        }

        private static void updateGridData(GridDataStruct data, MapType type)
        {
            if (type == MapType.main)
            {
                MainMap = data;
            }
            else if (type == MapType.secondary)
            {
                SecondaryMap = data;
            }
            else
            {
                Debug.LogError("Maptype not recognized:: " + type);
            }
        }

        public static void UpdateMap(Cell[] cells, in GridDataStruct map)
        {
            Debug.Log("Updating GridData: " + map.id);

            if (map.mapType == MapType.main)
            {
                MainMapCells = new NativeArray<Cell>(cells, Allocator.Persistent);
            }
            else
            {
                SecondaryMapCells = new NativeArray<Cell>(cells, Allocator.Persistent);
            }
        }



        public static void UpdateMap(MapDataSO map, MapType type)
        {
            Debug.Log($"GridSystem:::: 1");

            // Other systems depend on grid data, this needs to be first
            var grid = new GridDataStruct
            {
                id = map.id,
                gridSize = map.Grid.GridSize,
                cellSize = map.CellSize,
                mapType = type
            };
            updateGridData(grid, type);


            //Next update item, plant, building lists, these will influence cell data
            MapPlantManagerSystem.LoadPlantItems(map, type);
            MapBuildingManagerSystem.LoadBuildingItems(map, type);

            //now lets create our array, and calculate cell data
            var cells = new NativeArray<Cell>(map.Grid.Cells.Length, Allocator.Temp);

            for(int i = 0; i < cells.Length; i++)
            {
                cells[i] = AddCellModifiers(map.Grid.Cells[i],type);
            }

            if (type == MapType.main)
            {
                //if (MainMapCells.IsCreated) MainMapCells.Dispose();
                MainMapCells = new NativeArray<Cell>(cells, Allocator.Persistent);
            }
            else if(type == MapType.secondary)
            {
                MainMapCells = new NativeArray<Cell>(cells, Allocator.Persistent);
            }
            else
            {
                Debug.LogError("Maptype not recognized:: " + type);
            }

            cells.Dispose();
            //Do we need to update map tile references
            if (!GroundTypeReferences.IsCreated)
            {
                UpdateGroundReferences(map.TileRefList);
            }
        }
    }
}
