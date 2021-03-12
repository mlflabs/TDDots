using BovineLabs.Event.Systems;
using Mlf.Brains.Actions;
using Mlf.Grid2d;
using Mlf.Grid2d.Ecs;
using Mlf.Map2d;
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

    public enum HungerStates : byte
    {
        FindFoodSource, RequestFoodSource, GoToFoodSource, Eat,
        NoFoodSource, Finished
    }

    [UpdateBefore(typeof(MapItemManagerSystem))]
    class HungerStateSystem : SystemBase
    {
        //protected EndSimulationEntityCommandBufferSystem m_EndSimulationEcbSystem;
        private EventSystem eventSystem;

        protected override void OnCreate()
        {
            base.OnCreate();

            //m_EndSimulationEcbSystem = World
            //    .GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            //eventSystem = World.GetExistingSystem<LateSimulationEventSystem>();
            eventSystem = World.DefaultGameObjectInjectionWorld
                .GetOrCreateSystem<EventSystem>();
        }



        protected override void OnUpdate()
        {

            var deltaTime = Time.DeltaTime;
            var writer = eventSystem.CreateEventWriter<AskForItemOwnershipEvent>();
            var maps = GridSystem.Maps;
            var groundTypeReferences = GridSystem.GroundTypeReferences;



            var mapIds = new List<MapComponentShared>();
            EntityManager.GetAllUniqueSharedComponentData(mapIds);

            for (int m = 0; m < mapIds.Count; m++)
            {
                int _id = mapIds[m].mapId;
                GridDataStruct map = maps[mapIds[m].mapId];
                NativeHashMap<int, GridItem> plantItems;

                if (MapItemManagerSystem.mainMapId == _id)
                    plantItems = MapItemManagerSystem.MainMapPlantItems;
                else
                    continue;

                Entities
                    .WithSharedComponentFilter(mapIds[m])
                    .WithName("DefaultHungerStateSystem")
                    .WithAll<HungerDefaultSystemTag>()
                    .WithReadOnly(plantItems)
                    .ForEach((
                            Entity entity,
                            ref HungerStateCurrent hungerStateCurrent,
                            ref HungerState hungerState,
                            ref MoveActionData moveActionData,
                            ref CurrentBrainState currentState,
                            //ref MapItemOwned mapItemOwned,
                            //in NpcData npcData,
                            in LocalToWorld transform) =>
                    {
                        //Debug.Log($"Wander State: {wanderState.state} Finished: {completeProgressData.finished}");
                        if (hungerStateCurrent.state == HungerStates.FindFoodSource)
                        {

                            Debug.Log($"Finding Food {hungerStateCurrent.state}");
                            int2 currentMapPosition = map.getGridPosition(transform.Position);
                            moveActionData.reset();


                            PathData path = new PathData();
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
                                    if (current < lowest && plantItems[checking[i]].currentOwner == 0)
                                    {
                                        lowest = current;
                                        currentLowerstId = checking[i];
                                        lowestCheckingIndex = i;
                                    }
                                }


                                if (currentLowerstId == -1)
                                {
                                    Debug.Log($"Couldn't find food source");

                                    hungerStateCurrent.state = HungerStates.NoFoodSource;
                                    return;
                                    //TODO show a negative thing, move state to higher number
                                }

                                //remove the selected, so not checked again
                                Debug.Log($"Array index: {currentLowerstId}, length: {checking.Length}");
                                checking.RemoveAt(lowestCheckingIndex);



                                //see if we have a path
                                path = UtilsPath.findPath(in currentMapPosition,
                                                          plantItems[currentLowerstId].pos,
                                                          in groundTypeReferences,
                                                          in map);

                            }//while


                            writer.Write(new AskForItemOwnershipEvent
                            {
                                mapId = map.id,
                                indexPos = currentLowerstId,
                                ////////userId = npcData.userId,
                                entity = entity
                            });
                            /////////Debug.Log($"Asked for item ownership from: {npcData.userId}");
                            hungerStateCurrent.state = HungerStates.RequestFoodSource;
                            hungerStateCurrent.itemId = currentLowerstId;
                            hungerStateCurrent.path = path;

                        } // state  = 0
                        else if (hungerStateCurrent.state == HungerStates.RequestFoodSource)
                        {
                            Debug.Log($"Requesting Food {hungerStateCurrent.state}");
                           ///////// Debug.Log($"Checking if we have ownership:: {npcData.userId} ");
                            //Did we get ownership
                           /* if (plantItems[hungerStateCurrent.itemId].currentOwner == npcData.userId)
                            {
                                Debug.Log($"Got ownership of item:: {hungerStateCurrent.itemId}");
                                moveActionData.loadPath(
                                    in hungerStateCurrent.path,
                                    in map.gridRef.Value.cells[map.getIndex(plantItems[hungerStateCurrent.itemId].pos)].pos,
                                    in map);
                                hungerStateCurrent.state = HungerStates.GoToFoodSource;
                            }
                            else
                            {
                                Debug.Log($"Didn't get ownership, search for another item");
                                hungerStateCurrent.state = HungerStates.FindFoodSource;
                                moveActionData.reset();
                            }

                            */
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

                this.eventSystem
                    .AddJobHandleForProducer<AskForItemOwnershipEvent>(Dependency);
            }


        }
    }
}
