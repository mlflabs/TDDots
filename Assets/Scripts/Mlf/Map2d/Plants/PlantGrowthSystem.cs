using BovineLabs.Event.Jobs;
using BovineLabs.Event.Systems;
using Mlf.Grid2d;
using Mlf.Grid2d.Ecs;
using Mlf.MyTime;
using Mlf.Npc;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static BovineLabs.Event.Containers.NativeEventStream;

namespace Mlf.Map2d
{




    //[UpdateAfter(typeof(EndSimulationEntityCommandBufferSystem))]
    //[AlwaysUpdateSystem]
    //[UpdateBefore(typeof(NpcOwnershipSystem))]
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public class PlantGrowthSystem : SystemBase
    {

        private MapType _currentMap;
        public int BatchSize = 2;
        public NativeArray<int> CurrentIndexArray;
        public Unity.Mathematics.Random Random;
        public int SkipFrames = 10;
        public int CurrentFrame = 0;

        public float GrowthMultiplierPerSec;
        NativeArray<int2> _neighbourOffsetArray;

        private EventSystem _eventSystem;

        protected override void OnCreate()
        {
            base.OnCreate();

            Random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, 100000));

            _eventSystem = World.DefaultGameObjectInjectionWorld
                .GetOrCreateSystem<EventSystem>();

            

            _neighbourOffsetArray = new NativeArray<int2>(8, Allocator.Persistent)
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

            CurrentIndexArray = new NativeArray<int>(2, Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_neighbourOffsetArray.IsCreated)
                _neighbourOffsetArray.Dispose();
            if (CurrentIndexArray.IsCreated)
                CurrentIndexArray.Dispose();
        }


        protected override void OnUpdate()
        {
            //skip frames
            CurrentFrame++;
            if (CurrentFrame < SkipFrames) return;
            CurrentFrame = 0;

            _currentMap = (_currentMap == MapType.main)?MapType.secondary:MapType.main; //here we can rotate between maps

            if(GrowthMultiplierPerSec == 0)
                GrowthMultiplierPerSec = MapManager.Instance.mainMap.growthMultiplierPerSecond;


            NativeArray<int> keys;
            //jobDependency.AddDependency(Dependency);


            //JobHandle plantJob;
            

            if (_currentMap == MapType.main)
            {
                var writer = _eventSystem.CreateEventWriter<PlantModificationEvent>();
                if (!MapPlantManagerSystem.MainMapPlantItems.IsCreated) return;
                keys = MapPlantManagerSystem.MainMapPlantItems.GetKeyArray(Allocator.TempJob);

                Dependency =  new PlantGrowthJob
                {
                    Map = GridSystem.MainMap,
                    Writer = writer,
                    Random = Random,
                    Keys = keys,
                    Plants = MapPlantManagerSystem.MainMapPlantItems,
                    PlantReferences = MapPlantManagerSystem.PlantItemReferences,
                    Cells = GridSystem.MainMapCells,
                    GrowthMultiplierPerSec = GrowthMultiplierPerSec,
                    BatchSize = BatchSize,
                    CurrentIndexArray = CurrentIndexArray,
                    NeighbourOffsetArray = _neighbourOffsetArray,
                    TimeElapsed = TimeSystem.ElapsedTime,
                }.Schedule();
                _eventSystem.AddJobHandleForProducer<PlantModificationEvent>(Dependency);

            }
            else if(_currentMap == MapType.secondary)
            {
                if (!MapPlantManagerSystem.SecondaryMapPlantIems.IsCreated) return;

                var writer = _eventSystem.CreateEventWriter<PlantModificationEvent>();
                keys = MapPlantManagerSystem.SecondaryMapPlantIems.GetKeyArray(Allocator.TempJob);
                new PlantGrowthJob
                {
                    Map = GridSystem.SecondaryMap,
                    Keys = keys,
                    Random = Random,
                    Writer = writer,
                    Plants = MapPlantManagerSystem.SecondaryMapPlantIems,
                    PlantReferences = MapPlantManagerSystem.PlantItemReferences,
                    Cells = GridSystem.SecondaryMapCells,
                    GrowthMultiplierPerSec = GrowthMultiplierPerSec,
                    BatchSize = BatchSize,
                    CurrentIndexArray = CurrentIndexArray,
                    NeighbourOffsetArray = _neighbourOffsetArray,
                    TimeElapsed = TimeSystem.ElapsedTime,
                }.Schedule();
                _eventSystem.AddJobHandleForProducer<PlantModificationEvent>(Dependency);
            }
            else
            {
                Debug.LogError($"Maptype not recognized:: {_currentMap}");
                return;
            }

            

            


            // plantJob.Complete();
                    
            Entities
                .WithName("PlantGrowthSystem")
                .ForEach((Entity entity,
                         in MyTimeTag timeData) =>
                {
                }).Schedule();


            //plantJob.Complete();
            //keys.Dispose();
            
        }


        //[BurstCompile]
        private struct PlantGrowthJob : IJob
        {
            [ReadOnly]
            public GridDataStruct Map;
            [ReadOnly]
            public NativeHashMap<int, PlantItem> Plants;
            public float GrowthMultiplierPerSec;
            public int BatchSize;
            [DeallocateOnJobCompletion]
            public NativeArray<int> Keys;
            public NativeArray<int> CurrentIndexArray;
            [ReadOnly]
            public NativeArray<int2> NeighbourOffsetArray;
            [ReadOnly]
            public NativeHashMap<byte, PlantDataStruct> PlantReferences;
            [ReadOnly]
            public NativeArray<Cell> Cells;
            public float TimeElapsed;
            public ThreadWriter Writer;
            public Unity.Mathematics.Random Random;


            public void Execute()
            {

                int arraySize = (Keys.Length >= BatchSize) ? BatchSize : Keys.Length;
                //var modifiedPlantKeys = new NativeList<int>(Allocator.Temp);
                PlantItem p;

                //Debug.Log("******************* Calculating Growth Batch Size: " + arraySize + "Growth Rate: " + growthMultiplierPerSec);
                byte growth;
                for (int ii = 0; ii < arraySize; ii++)
                {
                    CurrentIndexArray[(byte) Map.MapType]++;
                    if (CurrentIndexArray[(byte)Map.MapType] >= Keys.Length) CurrentIndexArray[(byte)Map.MapType] = 0;


                    p = (Map.MapType == MapType.main) ?
                        Plants[Keys[CurrentIndexArray[(byte)Map.MapType]]]:
                        Plants[Keys[CurrentIndexArray[(byte)Map.MapType]]];


                    growth = (byte)(GrowthMultiplierPerSec * (TimeElapsed - p.timeUpdated)); ;


                    if (growth == 0) continue;
                    //Debug.Log("Growth:: " + p.growth + "TimeDiff: " + timeDiff + "Half: " + (half)timeDiff);
                    //Debug.Log("************************************* Grown **********************");
                    p.timeUpdated = TimeElapsed;
                    p.growth += growth;
                    if (p.growth > PlantReferences[p.TypeId].GETPlantLevel(p.level).levelUpgradeGrowthPointsRequired)
                    {
                        p.growth = 0;
                        if (p.level >= PlantItem.MaxLevel)
                        {
                            //we are at max
                            //here we can spread
                            // Debug.Log("Growing up level: " + p.level);
                            //multiply/spread
                            if (PlantReferences[p.TypeId].canReproduce)
                            {
                                int randomsInt = Random.NextInt(NeighbourOffsetArray.Length - 1);//   Unity.Mathematics.Random(0,  UnityEngine.Random.Range(0, neighbourOffsetArray.Length - 1);
                                int2 newpos = NeighbourOffsetArray[randomsInt] + p.Pos;

                                int indexPos = Map.GETIndex(newpos);

                                //are we out of bounds
                                if(indexPos < 0 || indexPos >= Cells.Length)
                                {
                                    //we are out of bounds, just skip
                                    continue;
                                }

                                //is this a valid position
                                Cell c = Cells[indexPos];

                                if (c.canGrow)
                                {
                                    ////////// send new plant event
                                    //Debug.LogError("Adding new Plant");
                                    Writer.Write(new PlantModificationEvent
                                    {
                                        Map = Map.MapType,
                                        Plant = p.GETSeedling(in newpos),
                                        NewPlant = true
                                    });
                                }
                                    //MapPlantManagerSystem.AddPlantItem(p.getSeedling(in newpos), map.mapType);
                            }
                            ///////////    addPlant(in p, map);
                            

                        }
                        else
                        {
                            // Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!Plant Grown");
                            p.level++;
                            //////////// send modify plant event
                            //Debug.LogError("Modifying plant");
                            Writer.Write(new PlantModificationEvent
                            {
                                Map = Map.MapType,
                                Plant = p,
                                NewPlant = false
                            });
                        }
                    }

                    //plants[keys[currentIndexArray[(byte)map.mapType]]] = p;
                }

                //TODO: here we could also check growth multiplier..... weather....sunny...

            }
        }

        /*
        private static void addPlant(in PlantItem p, MapType map)
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
        */








        /*
        public JobHandle GetDependency()
        {
            return Dependency;
        }

        public void AddDependency(JobHandle inputDependency)
        {
            Dependency = JobHandle.CombineDependencies(Dependency, inputDependency);
        }
        */
    }
}
