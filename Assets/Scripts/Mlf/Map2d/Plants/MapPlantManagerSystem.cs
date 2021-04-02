using BovineLabs.Event.Jobs;
using BovineLabs.Event.Systems;
using Mlf.Grid2d.Ecs;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Mlf.Map2d
{

    //[UpdateAfter(typeof(EndSimulationEntityCommandBufferSystem))]
    public class MapPlantManagerSystem : SystemBase
    {
        
        public static NativeHashMap<int, PlantItem> MainMapPlantItems;
        public static NativeHashMap<int, PlantItem> SecondaryMapPlantIems;
        
        public static NativeHashMap<byte, PlantDataStruct> PlantItemReferences;
        

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
            if (PlantItemReferences.IsCreated) PlantItemReferences.Dispose();
         
            if (MainMapPlantItems.IsCreated) MainMapPlantItems.Dispose();
            if (SecondaryMapPlantIems.IsCreated) SecondaryMapPlantIems.Dispose();
        }


        public static bool CellHasPlantItem(int index, MapType mapType)
        {
            return (mapType == MapType.main) ?
                MainMapPlantItems.ContainsKey(index) :
                SecondaryMapPlantIems.ContainsKey(index);
        }

        public static PlantItem GetCellPlantItem(int index, MapType mapType)
        {
            return (mapType == MapType.main) ?
                MainMapPlantItems[index] :
                SecondaryMapPlantIems[index];
        }

        //return success or failure
        public static bool AddPlantItem(PlantItem p, MapType mapType)
        {
            int index = GridSystem.GETIndex(p.Pos, mapType);
            if(mapType == MapType.main)
            {
                if (MainMapPlantItems.ContainsKey(index)) return false;
                MainMapPlantItems.Add(index, p);
            }
            else
            {
                if (SecondaryMapPlantIems.ContainsKey(index)) return false;
                SecondaryMapPlantIems.Add(index, p);
            }

            GridSystem.UpdateCell(p.Pos, mapType);
            PlantManager.Instance.AddPlant(p, mapType);
            return true;
        }

        public static bool UpdatePlantItem(PlantItem p, MapType mapType)
        {
            int index = GridSystem.GETIndex(p.Pos, mapType);
            if (mapType == MapType.main)
            {
                if (!MainMapPlantItems.ContainsKey(index)) return false;
                MainMapPlantItems[index] = p;
            }
            else
            {
                if (!SecondaryMapPlantIems.ContainsKey(index)) return false;
                SecondaryMapPlantIems[index] = p;
            }

            GridSystem.UpdateCell(p.Pos, mapType);
            PlantManager.Instance.UpdatePlant(p, index, mapType);
            return true;
        }

        public static bool RemovePlantItem(int2 pos, MapType map)
        {
            int index = GridSystem.GETIndex(pos, map);
            return RemovePlantItem(index, map);
        }
        
        public static bool RemovePlantItem(int index, MapType map)
        {
            
            if (map == MapType.main)
            {
                //if no plant, then its all good
                if (!MainMapPlantItems.ContainsKey(index)) return true;

                MainMapPlantItems.Remove(index);

            }
            else if (map == MapType.secondary)
            {
                if (!SecondaryMapPlantIems.ContainsKey(index)) return true;
                SecondaryMapPlantIems.Remove(index);
            }
            else
            {
                Debug.LogError("Map type not recognized:: " + map);
                return false;
            }

            GridSystem.UpdateCell(index, map);
            PlantManager.Instance.RemovePlant(index, map);
            return true;
        }
        

        public static void LoadPlantItems(MapDataSo map, MapType mapType)
        {

            
            Debug.Log("Loading Map ITEMS to ECS: " + map.id);
            
            //-- Update References
            if(!PlantItemReferences.IsCreated)
            {
                PlantItemReferences = new NativeHashMap<byte, PlantDataStruct>(
                    map.plantRefList.list.Length, Allocator.Persistent);

                for(int i = 0; i < map.plantRefList.list.Length; i++)
                {
                    PlantItemReferences[(byte)i] = PlantDataStruct.FromSo(map.plantRefList.list[i].data);
                }
            }



            if (mapType == MapType.current) mapType = MapType.main;


            var items = new NativeHashMap<int, PlantItem>(
                map.plantItems.Count, Allocator.Persistent);

            PlantItem p;
            PlantDataStruct pds;
            int index;
            for (int i = 0; i < map.plantItems.Count; i++)
            {
                p = map.plantItems[i];
                pds = PlantItemReferences[p.TypeId];

                index = map.GetGridIndex(p.Pos);
                items[index] = p;
            }

            if (mapType == MapType.main)
            {
                if (MainMapPlantItems.IsCreated) MainMapPlantItems.Dispose();
                MainMapPlantItems = items;
            }
            else if (mapType == MapType.secondary)
            {
                if (SecondaryMapPlantIems.IsCreated) SecondaryMapPlantIems.Dispose();
                SecondaryMapPlantIems = items;
            }
            else
            {
                Debug.LogWarning("Map type not recongnized: " + mapType);
            }


            //Update Mono Manager
            PlantManager.Instance.LoadPlants(map, mapType);


        }
    }
}
