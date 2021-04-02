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
using ItemType = Mlf.Npc.ItemType;

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
        EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            _endSimulationEcbSystem =
              World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            float deltaTime = Time.DeltaTime;
            var ecb = _endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();

            Entities
                .WithName("HungerManagementSystem")
                .ForEach((
                    ref HungerState state,
                    in HungerData data) =>
            {
                //calculate current hunger


                state.Value += data.HungerLps * deltaTime;
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


                   if (completeTag.Progress == StateCompleteProgress.removingOldStateCurrentTag)
                   {
                       Debug.Log("Hunger, removeing old tag");
                       if (currentState.State == BrainStates.Eat)
                       {                    
                           ecb.RemoveComponent<HungerStateCurrent>(entityInQueryIndex, entity);
                       }

                   }
                   else if (completeTag.Progress == StateCompleteProgress.loadingNewState)
                   {
                       if (currentState.State == BrainStates.Eat)
                       {
                           Debug.Log("Loading Hunger State");
                           ecb.AddComponent<HungerStateCurrent>(entityInQueryIndex, entity);

                       }

                   }
                   else if (completeTag.Progress == StateCompleteProgress.choosingNewState)
                   {
                       Debug.LogWarning($"HungerState Scores::: {state.Value}, Threshold:: {data.HungerThreshold}");
                       if (state.SkipNextStateSelection)
                       {
                           state.SkipNextStateSelection = false;
                           score.Value = 0;
                       }
                       else if (state.Value < data.HungerThreshold)//if not hungry just return 0
                       {
                           score.Value = 0;
                       }
                       else
                       {
                           Debug.LogWarning($"Hunger State Value:: {state.Value}");
                           score.Value = ScoreUtils.CalculateDefaultScore(state.Value);
                           if (score.Value > currentState.Score)
                           {
                               currentState.Score = score.Value;
                               currentState.State = BrainStates.Eat;
                           }
                       }

                       Debug.Log($"Score Hunger::: {currentState.Score}, {currentState.State}");

                   }



               }).Schedule(Dependency);
            Dependency = job2;
            _endSimulationEcbSystem.AddJobHandleForProducer(Dependency);


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

    //[UpdateBefore(typeof(NpcOwnershipSystem))]
    //[UpdateBefore(typeof(PlantGrowthSystem))]
    class HungerStateSystem : SystemBase
    {
        private EventSystem _eventSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            _eventSystem = World.DefaultGameObjectInjectionWorld
                .GetOrCreateSystem<EventSystem>();
        }



        protected override void OnUpdate()
        {

            var deltaTime = Time.DeltaTime;
            var writer = _eventSystem.CreateEventWriter<AskForItemOwnershipEvent>();
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
                    .WithNativeDisableContainerSafetyRestriction(plantItems)
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
                        if (hungerStateCurrent.State == HungerStates.FindFoodSource)
                        {
                            Debug.Log($"Finding Food {hungerStateCurrent.State}");
                            int2 currentMapPosition = map.GETGridPosition(transform.Position);
                            PathData path;
                            int itemIndex = FindFoodSpot(currentMapPosition,
                                                        in plantItems,
                                                        in map,
                                                        in cells,
                                                        out path);

                            if (itemIndex > -1)
                            {
                                hungerStateCurrent.State = HungerStates.RequestFoodSource;
                                hungerStateCurrent.ItemPosIndex = itemIndex;
                                hungerStateCurrent.ItemType = ItemType.plant;
                                hungerStateCurrent.Path = path;
                            }
                            else
                            {
                                //we didn't find path
                                hungerStateCurrent.State = HungerStates.NoFoodSource;
                                //TODO show a negative thing, move state to higher number
                            }



                        } // state  = 0
                        else if (hungerStateCurrent.State == HungerStates.RequestFoodSource)
                        {
                            Debug.Log($"Requesting Food {hungerStateCurrent.State}");


                            //TODO: add other resource types
                            writer.Write(new AskForItemOwnershipEvent
                            {
                                Map = map.MapType,
                                IndexPos = hungerStateCurrent.ItemPosIndex,
                                Type = hungerStateCurrent.ItemType,
                                UserId = npcData.UserId,                                
                            });
                            hungerStateCurrent.State = HungerStates.CheckIfRequestFoodSourceSuccessful;
                        }
                        else if(hungerStateCurrent.State == HungerStates.CheckIfRequestFoodSourceSuccessful)
                        {
                            if(plantItems[hungerStateCurrent.ItemPosIndex].CurrentOwner == npcData.UserId)
                            {
                                Debug.LogWarning($"Loading path to food {hungerStateCurrent.Path}, {map}");
                                moveActionData.LoadPath(in hungerStateCurrent.Path, in map);
                                hungerStateCurrent.State = HungerStates.GoToFoodSource;
                                
                            }
                            else if(plantItems[hungerStateCurrent.ItemPosIndex].CurrentOwner == 0)
                            {
                                //do it one more time
                                hungerStateCurrent.State = HungerStates.RequestFoodSource;
                            }
                            else
                            {
                                //someone else has this, look for another
                                hungerStateCurrent.State = HungerStates.FindFoodSource;
                            }
                        }
                        else if (hungerStateCurrent.State == HungerStates.GoToFoodSource)
                        {
                            Debug.Log("Going to food source");
                            //Walking to food, keep checking if the item still is available
                            if (plantItems.ContainsKey(hungerStateCurrent.ItemPosIndex))
                            {
                                //still there make sure its still ours
                                //TODO
                            }
                            else
                            {
                                //Item not there
                                //See if we are still hungry
                                hungerStateCurrent.State = HungerStates.NoFoodSource;
                            }

                            //Debug.Log($"Moving to item");
                            if (moveActionData.Finished && moveActionData.Success)
                            {
                                Debug.Log("At Item, eating...");
                                hungerStateCurrent.State = HungerStates.Eat;
                            }
                            else if (moveActionData.Finished)
                            {
                                Debug.Log("Move finisehd, but no success");
                                //finisehd but faile
                                hungerStateCurrent.State = HungerStates.NoFoodSource;
                            }
                        }
                        else if (hungerStateCurrent.State == HungerStates.Eat)
                        {
                            Debug.Log("**********************************Eating, and finished");
                            //reset, but need to figure out what food we are eating, how much nutritian
                            hungerState.Value = 0;
                            currentState.Finished = true;
                            hungerStateCurrent.State = HungerStates.Finished;

                            //here clean up ownership
                            writer.Write(new AskForItemOwnershipEvent
                            {
                                Map = map.MapType,
                                IndexPos = hungerStateCurrent.ItemPosIndex,
                                UserId = npcData.UserId,
                                Type = ItemType.plant,
                            });


                        }
                        else if (hungerStateCurrent.State == HungerStates.NoFoodSource)
                        {
                            Debug.Log("No Food");
                            //hungry, but no food source, show emoji, and move on
                            currentState.Finished = true;
                            hungerState.SkipNextStateSelection = true;
                            hungerStateCurrent.State = HungerStates.Finished;

                            //see if we need to cleanup ownership
                        }


                    }).Schedule();

                _eventSystem.AddJobHandleForProducer<AskForItemOwnershipEvent>(Dependency);
            }


        }


        public static void EatItem(HungerStateCurrent hungerStateCurrent)
        {
            //what are we eating
            if(hungerStateCurrent.ItemType == ItemType.plant)
            {
                PlantItem item;
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
            while (!path.HasPath())
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
                                            plantItems[checking[i]].Pos);
                    if (current < lowest && plantItems[checking[i]].CurrentOwner < 1)
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
                path = UtilsPath.FindPath(in currentMapPosition,
                                          plantItems[currentLowerstId].Pos,
                                          //in groundTypeReferences,
                                          in cells,
                                          in map);
            }//while
            return currentLowerstId;
        }
    }
}
