using Mlf.Map2d;
using Mlf.Tiles2d;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Mlf.Grid2d.Ecs
{

    public struct GridBlob
    {
        public BlobArray<Cell> cells;

        public static BlobAssetReference<GridBlob> ConstructBlobdata(Cell[] _cells)
        {

            var builder = new BlobBuilder(Allocator.Temp);

            ref var root = ref builder.ConstructRoot<GridBlob>();


            var cells = builder.Allocate(ref root.cells, _cells.Length);
            //var values = builder.Construct(ref root.cells);

            for (int i = 0; i < _cells.Length; i++)
            {
                cells[i] = _cells[i];
            }

            var rootRef = builder.CreateBlobAssetReference<GridBlob>(Allocator.Persistent);

            return rootRef;

        }
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
        public BlobAssetReference<GridBlob> gridRef;


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

        public Cell getCell(in int2 pos)
        {
            //are we inside the grid
            if (!validGridPosition(pos)) return new Cell();
            return gridRef.Value.cells[getIndex(in pos)];
        }

        public bool validGridPosition(in int2 pos)
        {
            if (pos.x < 0 || pos.y < 0 || pos.x >= gridSize.x || pos.y >= gridSize.y)
                return false;
            return true;
        }

        public float2 getWorldPosition(int2 gridPos)
        {
            int2 pos = getCell(gridPos).pos;
            return (pos * cellSize) + originPosition;
        }

        public float2 getWorldPositionCellCenter(in int2 gridPos)
        {
            return (getCell(gridPos).pos * cellSize) +
                    originPosition +
                    (cellSize / 2);
        }

        public float3 getWorldPositionCellCenter(in int2 gridPos, float z = 0f)
        {
            float2 f2 = (getCell(gridPos).pos * cellSize) +
                    originPosition +
                    (cellSize / 2);
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
        public static NativeList<GridDataStruct> Maps;

        //default is 0. we assume we have at least one map, and first come first serve
        private static int _currentMapIndex;
        public static int currentMapIndex
        {
            get { return _currentMapIndex; }
            set
            {
                _currentMapIndex = value;
                //when value changes we need to also change the currentMapData value
                currentMapData = Maps[value];
            }
        }
        public static GridDataStruct currentMapData;
        public static NativeHashMap<byte, GroundTypeStruct> GroundTypeReferences;
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
            if (Maps.IsCreated) Maps.Dispose();

            if (GroundTypeReferences.IsCreated) GroundTypeReferences.Dispose();

        }



        public static bool CanAddBuilding(in int2 pos, in int2 size)
        {

            //int gridIndex = -1;
            int height = -1;
            //int index;
            Cell c;
            for (int x = pos.x; x < pos.x + size.x; x++)
                for (int z = pos.y; z < pos.y + size.y; z++)
                {
                    int index = currentMapData.getIndex(x, z);
                    c = currentMapData.getCell(new int2(x, z));
                    if (height == -1)
                        height = c.pos.y;

                    if (height == c.pos.y)
                    {
                        if (UtilsGrid.canBuild(in c, in GroundTypeReferences) == false ||
                            c.buildingId != 0)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            return true;
        }

        public static bool AddBuildingToGrid(byte buildingId, in int2 pos, in int2 size, int mapid)
        {
            Debug.Log("***********************");
            //Get mapid
            int mapIndex = -1;
            for (int i = 0; i < Maps.Length; i++)
            {
                if (Maps[i].id == mapid)
                {
                    mapIndex = i;
                    break;
                }
            }
            if (mapIndex == -1)
            {
                Debug.LogWarning($"Couldnt modify map with id: {mapid}");
            }

            //Test if we can modify it
            if (!CanAddBuilding(pos, size))
            {
                Debug.LogWarning($"Not able to build building at: {pos} size: {size}");
                return false; ;
            }

            Debug.Log("**************************Adding Building to GridSystem");
            //Modify cells
            Cell[] cells = Maps[mapIndex].gridRef.Value.cells.ToArray();

            int cellIndex;
            Cell c;
            for (int x = 0; x < size.x; x++)
                for (int z = 0; z < size.y; z++)
                {
                    cellIndex = Maps[mapIndex].getIndex(new int2(x + pos.x, z + pos.y));
                    c = cells[cellIndex];
                    c.buildingId = buildingId;
                    cells[cellIndex] = c;
                    //Debug.Log($"Changed Grid index: {cellIndex}");
                }

            UpdateMap(cells, mapid);
            Debug.Log("======================= Ref modified");
            //Create new blob and save it
            return true;
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





        public static void UpdateMap(Cell[] cells, int id)
        {
            Debug.Log("Updating GridData: " + id);

            int mapIndex = -1;
            for (int i = 0; i < Maps.Length; i++)
            {
                if (Maps[i].id == id)
                {
                    mapIndex = i;
                    break;
                }
            }
            if (mapIndex == -1)
            {
                Debug.LogWarning($"Couldnt find GridData with id: {id}");
            }

            BlobAssetReference<GridBlob> mapRef = GridBlob.ConstructBlobdata(cells);

            GridDataStruct data = Maps[mapIndex];
            data.gridRef.Dispose();
            data.gridRef = mapRef;

            Maps[mapIndex] = data;

            //see if we need to update current map
            if (currentMapIndex == mapIndex)
            {
                currentMapData = Maps[mapIndex];
            }
        }

        public static void UpdateMap(MapDataSO map)
        {

            //create blob 
            BlobAssetReference<GridBlob> mapRef = GridBlob.ConstructBlobdata(map.Grid.Cells);

            if (!Maps.IsCreated)
                Maps = new NativeList<GridDataStruct>(Allocator.Persistent);


            //TODO if we are UPDATING, just modify the reference, destroy the old blob
            var grid = new GridDataStruct
            {
                id = map.id,
                gridSize = map.Grid.GridSize,
                cellSize = map.CellSize,
                gridRef = mapRef
            };



            //see if we already have this map
            int mapindex = -1;
            for (int i = 0; i < Maps.Length; i++)
            {
                if (Maps[i].id == map.id)
                {
                    mapindex = i;
                    break;
                }
            }

            if (mapindex == -1)
            {
                Maps.Add(grid);
            }
            else
            {
                Maps[mapindex] = grid;
            }

            //see if we need to update current map
            if (currentMapIndex == mapindex)
            {
                currentMapData = Maps[mapindex];
            }
            else if (mapindex == -1 && currentMapIndex == 0)
            {
                //in case this is our first map
                currentMapData = Maps[0];
            }

            //Do we need to update map tile references
            if (!GroundTypeReferences.IsCreated)
            {
                UpdateGroundReferences(map.TileRefList);
            }
        }
    }
}
