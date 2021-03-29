using BovineLabs.Event.Jobs;
using BovineLabs.Event.Systems;
using Mlf.Grid2d;
using Mlf.Grid2d.Ecs;
using Mlf.MyTime;
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
    public class PlantGrowthSystem : SystemBase
    {
        public int batchSize = 10;
        public int currentIndex = 0;
        public int skipFrames = 10;
        public int currentFrame = 0;

        public float growthMultiplierPerSec;
        NativeArray<int2> neighbourOffsetArray;

        protected override void OnCreate()
        {
            base.OnCreate();

            neighbourOffsetArray = new NativeArray<int2>(8, Allocator.Persistent)
            {
                [0] = new int2(-1, 0),
                [1] = new int2(+1, 0),
                [2] = new int2(0, +1),
                [3] = new int2(0, -1),
                [4] = new int2(-1, -1),
                [5] = new int2(-1, +1),
                [6] = new int2(+1, -1),
                [7] = new int2(+1, +1)
            };


        }

        protected override void OnUpdate()
        {
            //skip frames
            currentFrame++;
            if (currentFrame < skipFrames) return;
            currentFrame = 0;

            MapType map = MapType.main; //here we can rotate between maps




            growthMultiplierPerSec = (map == MapType.main)?
                MapManager.Instance.MainMap.growthMultiplierPerSecond : 0; //add secondary later



            var keys = (map == MapType.main)? 
                MapPlantManagerSystem.MainMapPlantItems.GetKeyArray(Allocator.Temp) :
                MapPlantManagerSystem.SecondaryMapPlantIems.GetKeyArray(Allocator.Temp); 


            int arraySize = (keys.Length >= batchSize)? batchSize : keys.Length;
            var modifiedPlantKeys = new NativeList<int>(Allocator.Temp);
            PlantItem p;

            //Debug.Log("******************* Calculating Growth Batch Size: " + arraySize + "Growth Rate: " + growthMultiplierPerSec);
            byte growth;
            for (int ii = 0; ii < arraySize; ii++)
            {
                currentIndex++;
                if (currentIndex >= keys.Length) currentIndex = 0;
                



                p = (map == MapType.main)?
                    MapPlantManagerSystem.MainMapPlantItems[keys[currentIndex]] :
                    MapPlantManagerSystem.SecondaryMapPlantIems[keys[currentIndex]]; 

                
                growth = (byte)(growthMultiplierPerSec * (TimeSystem.ElapsedTime - p.timeUpdated)); ;
                

                if (growth == 0) continue;
                //Debug.Log("Growth:: " + p.growth + "TimeDiff: " + timeDiff + "Half: " + (half)timeDiff);
                //Debug.Log("************************************* Grown **********************");
                p.timeUpdated = TimeSystem.ElapsedTime;
                p.growth += growth;
                if(p.growth > MapPlantManagerSystem.PlantItemReferences[p.typeId]
                    .getPlantLevel(p.level).growthPoints)
                {
                    p.growth = 0;
                    if(p.level >= PlantItem.MaxLevel)
                    {
                        //we are at max
                        //here we can spread
                       // Debug.Log("Growing up level: " + p.level);
                        //multiply/spread
                        if(MapPlantManagerSystem.PlantItemReferences[p.typeId].CanReproduce)
                            addPlant(in p, map);


                    }
                    else
                    {
                       // Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!Plant Grown");
                        p.level++;
                        modifiedPlantKeys.Add(keys[currentIndex]);
                    }
                }

                MapPlantManagerSystem.MainMapPlantItems[keys[currentIndex]] = p;
            }

            if(modifiedPlantKeys.Length > 0)
                PlantManager.Instance.UpdatePlants(modifiedPlantKeys.ToArray(), map);

            //TODO: here we could also check growth multiplier..... weather....sunny...
            
            Entities
                .WithName("PlantGrowthSystem")
                .ForEach((Entity entity,
                         in MyTimeTag timeData) =>
                {
                }).ScheduleParallel();
        }


        private void addPlant(in PlantItem p, MapType map)
        {
            //find a suitable spot that is adjescent//try it once, if failed, just skip
            //this adds randomness
            //get random adjescent position
           // Debug.Log("Trying to add new plant");
            int randomsInt = UnityEngine.Random.Range(0, neighbourOffsetArray.Length - 1);
            int2 newpos = neighbourOffsetArray[randomsInt] + p.pos;

            //is this a valid position
            Cell c = GridSystem.getCell(newpos, map);

            if (c.canGrow)
                MapPlantManagerSystem.AddPlantItem(p.getSeedling(in newpos), map);
            //else
                //Debug.Log($"Could't grow on this tile {c.pos}");
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if(neighbourOffsetArray.IsCreated)
                neighbourOffsetArray.Dispose();


        }


    }
}
