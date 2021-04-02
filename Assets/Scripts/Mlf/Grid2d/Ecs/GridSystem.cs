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

        // ReSharper disable once MemberCanBePrivate.Global
        public float WalkSpeed;
        // ReSharper disable once MemberCanBePrivate.Global
        public float SwimSpeed;

        // ReSharper disable once MemberCanBePrivate.Global
        public bool CanBuild;
        // ReSharper disable once MemberCanBePrivate.Global
        public bool CanGrow;

        public bool HasFreshWater;

        public bool CanWalk { get => (WalkSpeed != 0) ? true : false; }

        public static GroundTypeStruct FromGroundTypeSo(TileDataSo so)
        {
            return new GroundTypeStruct
            {
                WalkSpeed = so.walkSpeed,
                SwimSpeed = so.swimSpeed,
                CanBuild = so.canBuild,
                CanGrow = so.canGrow,
                //resources
                HasFreshWater = so.hasFreshWater,
            };
        }
    }


    public struct GridDataStruct
    {
        public int ID;
        public int2 GridSize;
        public float2 CellSize;
        public float2 OriginPosition;
        public MapType MapType;


        public int GETIndex(in int2 pos)
        {
            return pos.x + (pos.y * GridSize.x);
        }
        public int GETIndex(int x, int z)
        {
            return x + (z * GridSize.x);
        }

        public int2 GETGridPosition(in float2 worldPosition)
        {
            return new int2(
            (int)((worldPosition - OriginPosition).x / CellSize.x),
            (int)((worldPosition - OriginPosition).y / CellSize.y));
        }
        public int2 GETGridPosition(in float3 worldPosition)
        { 
            var pos = UtilsGrid.ToFloat2(worldPosition);
            return new int2(
            (int)((pos - OriginPosition).x / CellSize.x),
            (int)((pos - OriginPosition).y / CellSize.y));
        }

        public Cell GETCell(in int2 pos, in NativeArray<Cell>cells)
        {
            return !ValidGridPosition(pos) ? new Cell() : cells[GETIndex(in pos)];
        }

        public bool ValidGridPosition(in int2 pos)
        {
            if (pos.x < 0 || pos.y < 0 || pos.x >= GridSize.x || pos.y >= GridSize.y)
                return false;
            return true;
        }

        public float2 GETWorldPosition(int2 gridPos)
        {
            //int2 pos = getCell(gridPos).pos;
            //return (pos * cellSize) + originPosition;
            return (gridPos * CellSize) + OriginPosition;
        }

        public float2 GETWorldPositionCellCenter(in int2 gridPos)
        {
            return (gridPos * CellSize) +
                    OriginPosition +
                    (CellSize / 2);
        }

        public float3 GETWorldPositionCellCenter(in int2 gridPos, float z)
        {
            float2 f2 = (gridPos * CellSize) +
                    OriginPosition +
                    (CellSize / 2);
            return new float3(f2.x, f2.y, z);
        }

        public float2 GetNearestPoint(float2 pos)
        {
            int2 count = (int2)(pos / CellSize);
            return count * CellSize;
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



        public static int GETIndex(in int2 pos, MapType mapType = MapType.current)
        {
            if (mapType == MapType.current) mapType = CurrentMapType;

            if (mapType == MapType.main)
                return pos.x + (pos.y * MainMap.GridSize.x);
            else
                return pos.x + (pos.y * SecondaryMap.GridSize.x);
        }
        public static int GETIndex(int x, int z, MapType mapType)
        {
            if (mapType == MapType.main)
                return x + (z * MainMap.GridSize.x);
            else
                return x + (z * SecondaryMap.GridSize.x);
        }

        public static int2 GETGridPosition(in float2 worldPosition, MapType mapType = MapType.current)
        {
            if (mapType == MapType.current) mapType = CurrentMapType;

            if (mapType == MapType.main)
                return new int2(
                    (int)((worldPosition - MainMap.OriginPosition).x / MainMap.CellSize.x),
                    (int)((worldPosition - MainMap.OriginPosition).y / MainMap.CellSize.y));
            else
                return new int2(
                    (int)((worldPosition - SecondaryMap.OriginPosition).x / SecondaryMap.CellSize.x),
                    (int)((worldPosition - SecondaryMap.OriginPosition).y / SecondaryMap.CellSize.y));
        }

        public static int2 GETGridPosition(in float3 worldPosition, MapType mapType = MapType.current)
        {
            if (mapType == MapType.current) mapType = CurrentMapType;

            var pos = UtilsGrid.ToFloat2(worldPosition);
            if (mapType == MapType.main)
            {

                return new int2(
                    (int)((pos - MainMap.OriginPosition).x / MainMap.CellSize.x),
                    (int)((pos - MainMap.OriginPosition).y / MainMap.CellSize.y));
            }
            else
            {
                return new int2(
                    (int)((pos - SecondaryMap.OriginPosition).x / SecondaryMap.CellSize.x),
                    (int)((pos - SecondaryMap.OriginPosition).y / SecondaryMap.CellSize.y));
            }

        }

        private static void SetCell(Cell c, MapType type)
        {
            int index = GETIndex(c.pos, type);
            if (type == MapType.main)
                MainMapCells[index] = c;
            else
                SecondaryMapCells[index] = c;

        }

        public static Cell GETCell(in int2 pos, MapType mapType = MapType.current)
        {
            if (mapType == MapType.current) mapType = CurrentMapType;

            if (!ValidGridPosition(pos, mapType)) return new Cell();
            if (mapType == MapType.main)
                return MainMapCells[GETIndex(in pos, mapType)];
            else
                return SecondaryMapCells[GETIndex(in pos, mapType)];

        }

        public static float2 GETCellSize(MapType mapType = MapType.current)
        {
            if (mapType == MapType.current) mapType = CurrentMapType;

            if (mapType == MapType.main)
                return MainMap.CellSize;
            else
                return SecondaryMap.CellSize;
        }
        
        public static Cell GETCell(in float2 worldPosition)
        {

            //Which map are we in
            int2 pos = new int2(
                   (int)((worldPosition - MainMap.OriginPosition).x / MainMap.CellSize.x),
                   (int)((worldPosition - MainMap.OriginPosition).y / MainMap.CellSize.y));


            if(ValidGridPosition(pos, MapType.main))
            {
                return GETCell(pos, MapType.main);
            }
            else if(ValidGridPosition(pos, MapType.secondary))
            {
                return GETCell(pos, MapType.secondary);
            }

            return new Cell();


        }


        public static bool ValidGridPosition(in int2 pos, MapType mapType)
        {
            if (mapType == MapType.main)
                return !(pos.x < 0 || pos.y < 0 ||
                    pos.x >= MainMap.GridSize.x || pos.y >= MainMap.GridSize.y);
            else
                return !(pos.x < 0 || pos.y < 0 ||
                    pos.x >= SecondaryMap.GridSize.x || pos.y >= SecondaryMap.GridSize.y);


        }

        public static float2 GETWorldPosition(in int2 gridPos, MapType mapType = MapType.current)
        {
            if (mapType == MapType.current) mapType = CurrentMapType;
            if (mapType == MapType.main)
                return (gridPos * MainMap.CellSize) + MainMap.OriginPosition;
            else
                return (gridPos * SecondaryMap.CellSize) + SecondaryMap.OriginPosition;
        }

        public static float3 GETWorldPosition(in int2 gridPos,  int z, MapType mapType = MapType.current)
        {
            if (mapType == MapType.current) mapType = CurrentMapType;
            float2 pos;
            if (mapType == MapType.main)
                pos = (gridPos * MainMap.CellSize) + MainMap.OriginPosition;
            else
                pos= (gridPos * SecondaryMap.CellSize) + SecondaryMap.OriginPosition;

            return new float3(pos.x, pos.y, z);
        }


        public static float2 GETWorldPositionCellCenter(in int2 gridPos, MapType mapType = MapType.current)
        {
            if (mapType == MapType.current) mapType = CurrentMapType;
            if (mapType == MapType.main)
                return (gridPos * MainMap.CellSize) + MainMap.OriginPosition + (MainMap.CellSize / 2);
            else
                return (gridPos * SecondaryMap.CellSize) + SecondaryMap.OriginPosition + (SecondaryMap.CellSize / 2);
        }

        public static float3 GETWorldPositionCellCenter(in int2 gridPos, float z,MapType mapType = MapType.current)
        {
            if (mapType == MapType.current) mapType = CurrentMapType;
            float2 f2;
            if (mapType == MapType.main)
                f2 = (gridPos * MainMap.CellSize) + MainMap.OriginPosition + (MainMap.CellSize / 2);
            else
                f2 = (gridPos * SecondaryMap.CellSize) + SecondaryMap.OriginPosition + (SecondaryMap.CellSize / 2);
            return new float3(f2.x, f2.y, z);
        }


        public static void UpdateGroundReferences(TileReferenceListSo referenceListSo)
        {
            if (GroundTypeReferences.IsCreated)
                GroundTypeReferences.Dispose();

            GroundTypeReferences = new NativeHashMap<byte, GroundTypeStruct>(
                referenceListSo.list.Length, Allocator.Persistent);
            for (int i = 0; i < referenceListSo.list.Length; i++)
            {
                GroundTypeReferences[(byte)i] =
                    GroundTypeStruct.FromGroundTypeSo(referenceListSo.list[i].data);

            }
        }




        public static void UpdateCell(int2 pos, MapType mapType)
        {
            //Debug.LogWarning("Updating Grid Cell trying.....");
            Cell c = GETCell(pos, mapType);
            if (c.IsDefault()) return;

            SetCell(AddCellModifiers(c, mapType), mapType);

           // Debug.LogWarning("Updated");
        }

        public static void UpdateCell(int index, MapType mapType)
        {
            //Debug.LogWarning("Updating Grid Cell trying.....");
            Cell c = GETCell(index, mapType);
            if (c.IsDefault()) return;

            SetCell(AddCellModifiers(c, mapType), mapType);

            //Debug.LogWarning("Updated");
        }


        public static Cell GETCell(int index, MapType type)
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
                return GETCell(index, GridSystem.CurrentMapType);
            }
            else
            {
                Debug.LogError("Maptype not recognized:: " + type);
            }
            return new Cell();

        }

        public static Cell AddCellModifiers(Cell c, MapType mapType)
        {
            int index = GETIndex(c.pos, mapType);
            byte typeid;
            if (MapPlantManagerSystem.CellHasPlantItem(index, mapType))
            {
                typeid = MapPlantManagerSystem.GetCellPlantItem(index, mapType).TypeId;
                c = MapPlantManagerSystem.PlantItemReferences[typeid].UpdateCell(c);
            }
            if(MapBuildingManagerSystem.CellHasBuilding(index, mapType))
            {
                typeid = MapBuildingManagerSystem.GetCellBuilding(index, mapType).typeId;
                c = MapBuildingManagerSystem.BuildingReferences[typeid].UpdateCell(c);
            }

            return c;
        }

        private static void UpdateGridData(GridDataStruct data, MapType type)
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
            Debug.Log("Updating GridData: " + map.ID);

            if (map.MapType == MapType.main)
            {
                MainMapCells = new NativeArray<Cell>(cells, Allocator.Persistent);
            }
            else
            {
                SecondaryMapCells = new NativeArray<Cell>(cells, Allocator.Persistent);
            }
        }



        public static void UpdateMap(MapDataSo map, MapType type)
        {
            Debug.Log($"GridSystem:::: 1");

            // Other systems depend on grid data, this needs to be first
            var grid = new GridDataStruct
            {
                ID = map.id,
                GridSize = map.grid.gridSize,
                CellSize = map.cellSize,
                MapType = type
            };
            UpdateGridData(grid, type);


            //Next update item, plant, building lists, these will influence cell data
            MapPlantManagerSystem.LoadPlantItems(map, type);
            MapBuildingManagerSystem.LoadBuildingItems(map, type);

            //now lets create our array, and calculate cell data
            var cells = new NativeArray<Cell>(map.grid.cells.Length, Allocator.Temp);

            for(int i = 0; i < cells.Length; i++)
            {
                cells[i] = AddCellModifiers(map.grid.cells[i],type);
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
                UpdateGroundReferences(map.tileRefList);
            }
        }
    }
}
