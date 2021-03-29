using BovineLabs.Event.Systems;
using Mlf.Brains.Actions;
using Mlf.Grid2d;
using Mlf.Grid2d.Ecs;
using Mlf.Map2d;
using Mlf.Npc;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Mlf.Brains.States
{

    //just make the default system run action system
    public struct HungerDefaultSystemTag : IComponentData { }



    /// <summary>
    /// State Selection 
    /// </summary>
    [UpdateBefore(typeof(StateSelectionSystem))]
    class HungerManagementSystem : SystemBase
    {
        EndSimulationEntityCommandBufferSystem endSimulationEcbSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            endSimulationEcbSystem =
              World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            float deltaTime = Time.DeltaTime;
            var ecb = endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();

            Entities
                .WithName("HungerManagementSystem")
                .ForEach((
                    ref HungerState state,
                    in HungerData data) =>
            {
                //calculate current hunger


                state.value += data.hungerLPS * deltaTime;
            }).Schedule();



            var job2 = Entities
               .WithName("HungerScoreSystem")
               .ForEach((Entity entity,
                         int entityInQueryIndex,
                         ref HungerScore score,
                         ref HungerState state,
                         ref CurrentBrainState currentState,
                         in StateCompleteTag completeTag,
                         in HungerData data) =>
               {


                   if (completeTag.progress == StateCompleteProgress.removingOldStateCurrentTag)
                   {
                       Debug.Log("Hunger, removeing old tag");
                       if (currentState.state == BrainStates.Eat)
                           ecb.RemoveComponent<HungerStateCurrent>(entityInQueryIndex, entity);

                   }
                   else if (completeTag.progress == StateCompleteProgress.loadingNewState)
                   {
                       if (currentState.state == BrainStates.Eat)
                       {
                           Debug.Log("Loading Hunger State");
                           ecb.AddComponent<HungerStateCurrent>(entityInQueryIndex, entity);

                       }

                   }
                   else if (completeTag.progress == StateCompleteProgress.choosingNewState)
                   {
                       Debug.LogWarning($"HungerState Scores::: {state.value}, Threshold:: {data.hungerThreshold}");
                       if (state.skipNextStateSelection)
                       {
                           state.skipNextStateSelection = false;
                           score.value = 0;
                       }
                       else if (state.value < data.hungerThreshold)//if not hungry just return 0
                       {
                           score.value = 0;
                       }
                       else
                       {
                           Debug.LogWarning($"Hunger State Value:: {state.value}");
                           score.value = ScoreUtils.calculateDefaultScore(state.value);
                           if (score.value > currentState.score)
                           {
                               currentState.score = score.value;
                               currentState.state = BrainStates.Eat;
                           }
                       }

                       Debug.Log($"Score Hunger::: {currentState.score}, {currentState.state}");

                   }



               }).Schedule(Dependency);
            Dependency = job2;
            endSimulationEcbSystem.AddJobHandleForProducer(Dependency);


        }
    }













    /// <summary>
    /// State Progress Begins Here
    /// </summary>
    public enum HungerStates : byte
    {
        FindFoodSource, RequestFoodSource, CheckIfRequestFoodSourceSuccessful, GoToFoodSource, Eat,
        NoFoodSource, Finished
    }

    [UpdateBefore(typeof(NpcOwnershipSystem))]
    class HungerStateSystem : SystemBase
    {
        private EventSystem eventSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            eventSystem = World.DefaultGameObjectInjectionWorld
                .GetOrCreateSystem<EventSystem>();
        }



        protected override void OnUpdate()
        {

            var deltaTime = Time.DeltaTime;
            var writer = eventSystem.CreateEventWriter<AskForItemOwnershipEvent>();
            //var groundTypeReferences = GridSystem.GroundTypeReferences;



            var mapIds = new List<MapComponentShared>();
            EntityManager.GetAllUniqueSharedComponentData(mapIds);

            for (int m = 0; m < mapIds.Count; m++)
            {

                GridDataStruct map;
                NativeArray<Cell> cells;
                NativeHashMap<int, PlantItem> plantItems;

                if (mapIds[m].type == MapType.main)
                {
                    map = GridSystem.MainMap;
                    cells = GridSystem.MainMapCells;
                    plantItems = MapPlantManagerSystem.MainMapPlantItems;
                }
                else
                {
                    continue;//no secondary items setup yet
                    map = GridSystem.SecondaryMap;
                    cells = GridSystem.SecondaryMapCells;
                    plantItems = MapPlantManagerSystem.MainMapPlantItems;////change to secondary
                }


                Entities
                    .WithSharedComponentFilter(mapIds[m])
                    .WithName("DefaultHungerStateSystem")
                    .WithAll<HungerDefaultSystemTag>()
                    .WithReadOnly(plantItems)
                    .WithReadOnly(cells)
                    .ForEach((
                            Entity entity,
                            ref HungerStateCurrent hungerStateCurrent,
                            ref HungerState hungerState,
                            ref MoveActionData moveActionData,
                            ref CurrentBrainState currentState,
                            //ref MapItemOwned mapItemOwned,
                            ref NpcData npcData,
                            in LocalToWorld transform) =>
                    {
                        if (hungerStateCurrent.state == HungerStates.FindFoodSource)
                        {
                            Debug.Log($"Finding Food {hungerStateCurrent.state}");
                            int2 currentMapPosition = map.getGridPosition(transform.Position);
                            PathData path;
                            int itemIndex = FindFoodSpot(currentMapPosition,
                                                        in plantItems,
                                                        in map,
                                                        in cells,
                                                        out path);

                            if (itemIndex > -1)
                            {
                                hungerStateCurrent.state = HungerStates.RequestFoodSource;
                                hungerStateCurrent.itemPosIndex = itemIndex;
                                hungerStateCurrent.itemType = ItemType.plant;
                                //hungerStateCurrent.path = path;
                            }
                            else
                            {
                                //we didn't find path
                                hungerStateCurrent.state = HungerStates.NoFoodSource;
                                //TODO show a negative thing, move state to higher number
                            }



                        } // state  = 0
                        else if (hungerStateCurrent.state == HungerStates.RequestFoodSource)
                        {
                            Debug.Log($"Requesting Food {hungerStateCurrent.state}");


                            //TODO: add other resource types
                            writer.Write(new AskForItemOwnershipEvent
                            {
                                map = map.mapType,
                                indexPos = hungerStateCurrent.itemPosIndex,
                                userId = npcData.userId,
                                entity = entity
                            });
                            hungerStateCurrent.state = HungerStates.RequestFoodSource;
                        }
                        else if (hungerStateCurrent.state == HungerStates.CheckIfRequestFoodSourceSuccessful)
                        {
                            if( plantItems[hungerStateCurrent.itemPosIndex].currentOwner == npcData.userId)
                            {
                                npcData.itemOwned = hungerStateCurrent.itemPosIndex;
                                npcData.itemType = hungerStateCurrent.itemType;
                                hungerStateCurrent.state = HungerStates.GoToFoodSource;
                            }
                            else
                            {
                                hungerStateCurrent.state = HungerStates.FindFoodSource;
                            }
                        }
                        else if (hungerStateCurrent.state == HungerStates.GoToFoodSource)
                        {
                            //Debug.Log($"Moving to item");
                            if (moveActionData.finished && moveActionData.success)
                            {
                                Debug.Log("At Item, eating...");
                                hungerStateCurrent.state = HungerStates.Eat;
                            }
                            else if (moveActionData.finished)
                            {
                                Debug.Log("Move finisehd, but no success");
                                //finisehd but faile
                                hungerStateCurrent.state = HungerStates.NoFoodSource;
                            }
                        }
                        else if (hungerStateCurrent.state == HungerStates.Eat)
                        {
                            Debug.Log("Eating, and finished");
                            //reset, but need to figure out what food we are eating, how much nutritian
                            hungerState.value = 0;
                            currentState.finished = true;
                            hungerStateCurrent.state = HungerStates.Finished;
                        }
                        else if (hungerStateCurrent.state == HungerStates.NoFoodSource)
                        {
                            Debug.Log("No Food");
                            //hungry, but no food source, show emoji, and move on
                            currentState.finished = true;
                            hungerState.skipNextStateSelection = true;
                            hungerStateCurrent.state = HungerStates.Finished;
                        }


                    }).Schedule();

                eventSystem.AddJobHandleForProducer<AskForItemOwnershipEvent>(Dependency);
            }


        }

        public static int FindFoodSpot(
            in int2 currentMapPosition,
            in NativeHashMap<int, PlantItem> plantItems,
            in GridDataStruct map,
            in NativeArray<Cell> cells,
            out PathData path)
        {
            
            
           //moveActionData.reset();


            path = new PathData();
            int currentLowerstId = -1;
            while (!path.hasPath())
            {
                NativeList<int> checking = new NativeList<int>(Allocator.Temp);
                var keys = plantItems.GetKeyArray(Allocator.Temp);
                checking.CopyFrom(keys);

                //find the closes object, pseudal test
                float lowest = int.MaxValue;

                int lowestCheckingIndex = 0;
                float current;

                for (int i = 0; i < checking.Length; i++)
                {
                    current = math.distance(currentMapPosition,
                                            plantItems[checking[i]].pos);
                    if (current < lowest && plantItems[checking[i]].currentOwner < 1)
                    {
                        lowest = current;
                        currentLowerstId = checking[i];
                        lowestCheckingIndex = i;
                    }
                }


                if (currentLowerstId == -1)
                {
                    Debug.Log($"Couldn't find food source");
                    return -1;
                    
                }

                //remove the selected, so not checked again
                Debug.Log($"Array index: {currentLowerstId}, length: {checking.Length}");
                checking.RemoveAt(lowestCheckingIndex);



                //see if we have a path
                path = UtilsPath.findPath(in currentMapPosition,
                                          plantItems[currentLowerstId].pos,
                                          //in groundTypeReferences,
                                          in cells,
                                          in map);
            }//while
            return currentLowerstId;
        }
    }
}
