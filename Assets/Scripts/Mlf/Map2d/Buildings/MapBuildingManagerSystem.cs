using Mlf.Grid2d.Ecs;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Mlf.Map2d
{






    //[UpdateAfter(typeof(EndSimulationEntityCommandBufferSystem))]
    public class MapBuildingManagerSystem : SystemBase
    {
       
        public static NativeHashMap<int, BuildingItem> MainMapBuildings;
        public static NativeHashMap<int, BuildingItem> SecondaryMapBuildings;
        public static NativeHashMap<byte, BuildingDataStruct> BuildingReferences;
        
        protected override void OnCreate()
        {
            base.OnCreate();  
        }

        protected override void OnUpdate()
        {
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (BuildingReferences.IsCreated) BuildingReferences.Dispose();
            
            if (MainMapBuildings.IsCreated) MainMapBuildings.Dispose();
            if (SecondaryMapBuildings.IsCreated) SecondaryMapBuildings.Dispose();


        }


        public static bool CellHasBuilding(int index, MapType mapType)
        {
            return (mapType == MapType.main) ?
                MainMapBuildings.ContainsKey(index) :
                SecondaryMapBuildings.ContainsKey(index);
        }

        public static BuildingItem GetCellBuilding(int index, MapType mapType)
        {
            return (mapType == MapType.main) ?
                MainMapBuildings[index] :
                SecondaryMapBuildings[index];
        }

        //return success or failure
        public static bool AddBuilding(BuildingItem item, MapType mapType)
        {
            int index = GridSystem.GETIndex(item.pos, mapType);

            BuildingDataStruct data = BuildingReferences[item.typeId];
            var indexes = new NativeArray<int>(data.Size.x * data.Size.y, Allocator.Temp);
            if (mapType == MapType.main)
            {
                //first check if we really can build in these spots
                for (int x = 0; x < data.Size.x; x++)
                    for (int y = 0; y < data.Size.y; y++)
                    {
                        index = GridSystem.GETIndex(new int2(item.pos.x + x, item.pos.y + y));
                        if(!GridSystem.MainMapCells[index].canBuild)
                        {
                            indexes.Dispose();
                            return false;
                        }

                        indexes[x + (y * data.Size.x)] = index;
                    }

                //we are here, so no duplicates
                for(int i = 0; i < indexes.Length; i++)
                {
                    MapPlantManagerSystem.RemovePlantItem(indexes[i], mapType);
                    MainMapBuildings[indexes[i]] = item;
                    GridSystem.UpdateCell(indexes[i], mapType);
                }
            }
            else if (mapType == MapType.secondary)
            {
                //first check if we really can build in these spots
                for (int x = 0; x < data.Size.x; x++)
                    for (int y = 0; y < data.Size.y; y++)
                    {
                        index = GridSystem.GETIndex(new int2(item.pos.x + x, item.pos.y + y));
                        if (!GridSystem.SecondaryMapCells[index].canBuild)
                        {
                            indexes.Dispose();
                            return false;
                        }

                        indexes[x + (y * data.Size.x)] = index;
                    }

                //we are here, so no duplicates
                for (int i = 0; i < indexes.Length; i++)
                {
                    SecondaryMapBuildings[indexes[i]] = item;
                    GridSystem.UpdateCell(item.pos, mapType);
                }
            }

            BuildingManager.Instance.AddBuilding(item, mapType);
            indexes.Dispose();
            return true;
        }


        public static void LoadBuildingItems(MapDataSo map, MapType mapType = MapType.current)
        {

           
            Debug.Log("Loading Map BUILDINGS to ECS: " + map.id);

            //-- Update References
            byte id;
            if(!BuildingReferences.IsCreated)
            {
                BuildingReferences = new NativeHashMap<byte, BuildingDataStruct>(
                    map.buildingRefList.items.Length, Allocator.Persistent);

                for(int i = 0; i < map.buildingRefList.items.Length; i++)
                {

                    id = map.buildingRefList.items[i].typeId;
                    if (BuildingReferences.ContainsKey(id))
                    {
                        Debug.LogError("Duplicate Building Reference ID: " + id);
                    }
                    BuildingReferences[id] = BuildingDataStruct.FromBuildingDataSo(
                        map.buildingRefList.items[i]);
                }
            }


            if (mapType == MapType.current) mapType = MapType.main;


            //*4, whats the average size of biulding......
            var size = (map.buildingItems.Count > 0) ? map.buildingItems.Count * 4 : 10;
            var items = new NativeHashMap<int, BuildingItem>(
                size, Allocator.Persistent);



            BuildingItem item;
            BuildingDataStruct data;
            int index;
            for (int i = 0; i < map.buildingItems.Count; i++)
            {
                item = map.buildingItems[i];
                data = BuildingReferences[item.typeId];


                for(int x = 0; x < data.Size.x; x++)
                    for(int y = 0; y < data.Size.y; y++)
                    {
                        index = map.GetGridIndex(new int2(item.pos.x + x, item.pos.y + y));
                        if (items.ContainsKey(index))
                        {
                            Debug.LogError("Duplicate Building Reference, Index: " + index);
                            items[index] = item;
                        }
                    }
            }


            if(mapType == MapType.main)
            {
                if (MainMapBuildings.IsCreated) MainMapBuildings.Dispose();
                MainMapBuildings = items;
            }
            else if(mapType == MapType.secondary)
            {
                if (SecondaryMapBuildings.IsCreated) SecondaryMapBuildings.Dispose();
                SecondaryMapBuildings = items;
            }
            else
            {
                Debug.LogWarning("Map type not recongnized: " + mapType);
            }

            BuildingManager.Instance.LoadBuildings(map, mapType);
           

        }
    }
}
