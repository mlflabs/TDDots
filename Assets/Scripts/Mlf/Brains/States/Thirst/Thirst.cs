using Unity.Entities;
using UnityEngine;

namespace Mlf.Brains.States
{

    //just make the default system run action system
    public struct ThirstDefaultSystemTag : IComponentData { }

    [UpdateBefore(typeof(StateSelectionSystem))]
    class ThirstManagementSystem : SystemBase
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
                .WithName("ThirstrManagementSystem")
                .WithAll<ThirstDefaultSystemTag>()
                .ForEach((ref ThirstState state, in ThirstData data) =>
            {
                //calculate current thirst


                state.Value += data.ThirstLps * deltaTime;
            }).Schedule();



            var job2 = Entities
               .WithName("ThirstScoreSystem")
               .ForEach((Entity entity,
                         int entityInQueryIndex,
                         ref ThirstScore score,
                         ref ThirstState state,
                         ref CurrentBrainState currentState,
                         in StateCompleteTag completeTag,
                         in ThirstData data) =>
               {


                   if (completeTag.Progress == StateCompleteProgress.removingOldStateCurrentTag)
                   {
                       Debug.Log("Thirst, removeing old tag");
                       if (currentState.State == BrainStates.Drink)
                           ecb.RemoveComponent<ThirstStateCurrent>(entityInQueryIndex, entity);

                   }
                   else if (completeTag.Progress == StateCompleteProgress.loadingNewState)
                   {
                       Debug.Log("Is it Thirst");
                       if (currentState.State == BrainStates.Drink)
                       {
                           Debug.Log("============ Loading Thirst State");
                           ecb.AddComponent<ThirstStateCurrent>(entityInQueryIndex, entity);
                       }


                   }
                   else if (completeTag.Progress == StateCompleteProgress.choosingNewState)
                   {
                       if (state.SkipNextStateSelection)
                       {
                           state.SkipNextStateSelection = false;
                           score.Value = 0;
                       }
                       else if (state.Value < data.ThirstThreshold)//if not hungry just return 0
                       {
                           score.Value = 0;
                       }
                       else
                       {
                           score.Value = ScoreUtils.CalculateDefaultScore(state.Value);
                           if (score.Value > currentState.Score)
                           {
                               currentState.Score = score.Value;
                               currentState.State = BrainStates.Drink;
                           }
                       }

                       Debug.Log($"Score Thirst::: {currentState.Score}, {currentState.State}");

                   }


                   //calculate current thirst
                   Debug.Log("Updated thirstScore");
                   if (state.SkipNextStateSelection)
                   {
                       state.SkipNextStateSelection = false;
                       score.Value = 0;

                   }
                   else if (state.Value < data.ThirstThreshold)  //if not thirsy just return 0
                   {
                       score.Value = 0;
                       return;
                   }

                   //see if we have any food available
                   //
                   score.Value = ScoreUtils.CalculateDefaultScore(state.Value);
               }).Schedule(Dependency);

            Dependency = job2;
            _endSimulationEcbSystem.AddJobHandleForProducer(Dependency);

        }
    }

    public enum ThirstStates : byte
    {
        FindWaterSource, RequestWaterSource, GoToWaterSource, Drink,
        NoWaterSource, Finished
    }

    /*
  //[UpdateBefore(typeof(MapItemManagerSystem))]
  class ThirstStateSystem : SystemBase
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
          //var writer = eventSystem.CreateEventWriter<AskForItemOwnershipEvent>();
          var maps = GridSystem.Maps;
          var groundTypeReferences = GridSystem.GroundTypeReferences;
          //var waterRefByte = (byte)GroundType.water;


          var mapIds = new List<MapComponentShared>();
          EntityManager.GetAllUniqueSharedComponentData(mapIds);

          for (int m = 0; m < mapIds.Count; m++)
          {
              //int mapId = mapIds[m].mapId;
              GridDataStruct map = maps[mapIds[m].mapId];

              Entities
                  .WithSharedComponentFilter(mapIds[m])
                  .WithName("DefaultthirstStateSystem")
                  .WithReadOnly(maps)
                  .ForEach((
                          Entity entity,
                          ref ThirstStateCurrent thirstStateCurrent,
                          ref ThirstState thirstState,
                          ref MoveActionData moveActionData,
                          ref CurrentBrainState currentState,
                          ref MapItemOwned mapItemOwned,
                          in NpcData npcData,
                          in LocalToWorld transform) =>
                  {


                      //Debug.Log($"Wander State: {wanderState.state} Finished: {completeProgressData.finished}");
                      if (thirstStateCurrent.state == ThirstStates.FindWaterSource)
                      {

                          Debug.Log($"Finding Water {thirstStateCurrent.state}");
                          int2 currentMapPosition = map.getGridPosition(transform.Position);
                          moveActionData.reset();

                          PathData path = new PathData();
                          int2 destination = int2.zero;
                          while (!path.hasPath())
                          {
                              NativeList<int> checking = new NativeList<int>(Allocator.Temp);
                              //find all the water sources
                              for(int i = 0; i < map.gridRef.Value.cells.Length; i++)
                              {
                                  if (map.gridRef.Value.cells[i].walkSpeed > 0 &&
                                      UtilsGrid.hasWater()
                                      maps[mapId].gridRef.Value.cells[i].groundType == waterRefByte)
                                  {
                                      checking.Add(i);
                                  }
                              }

                              //find the closes object, pseudal test
                              float lowest = int.MaxValue;
                              int lowestCheckingIndex = 0;
                              float current;
                              int currentLowerstId = -1;
                              int2 dest;
                              for (int i = 0; i < checking.Length; i++)
                              {
                                  dest = new int2(maps[mapId].gridRef.Value.cells[checking[i]].pos.x,
                                                  maps[mapId].gridRef.Value.cells[checking[i]].pos.y);
                                  current = math.distance(currentMapPosition,
                                                          dest);

                                  if(current== 0)
                                  {
                                      Debug.Log("We are in water, or on water item");
                                  }

                                  if (current < lowest)
                                  {
                                      lowest = current;
                                      currentLowerstId = checking[i];
                                      lowestCheckingIndex = i;
                                  }
                              }

                              if (currentLowerstId == -1)
                              {
                                  Debug.Log($"Couldn't find food source");

                                  thirstStateCurrent.state = ThirstStates.NoWaterSource;
                                  checking.Dispose();

                                  //TODO show a negative thing, move state to higher number
                              }
                              else
                              {
                                  Debug.Log($"Found water at index: {currentMapPosition} {lowestCheckingIndex}, {checking.Length}");
                                  Debug.Log($"Indexes::: {currentLowerstId} ");
                                  //remove the lowest, after making sure its not -1


                                  checking.RemoveAt(lowestCheckingIndex);
                                  //see if we have a path
                                  destination = maps[mapId].gridRef.Value.cells[currentLowerstId].pos;



                                  path = UtilsPath.findPath(in currentMapPosition,
                                                            destination,
                                                            in groundTypeReferences,
                                                            maps[mapId]);

                                  Debug.Log($"Dring Path Valid::: {path.hasPath()}");

                              }

                              checking.Dispose();



                          }//while

                          Debug.Log($"******************** FOUND WATER SOURCE :) {destination}");

                          thirstStateCurrent.state = ThirstStates.GoToWaterSource;
                          moveActionData.loadPath(path, destination, maps[mapId].cellSize);

                      } // state  = 0
                      else if(thirstStateCurrent.state == ThirstStates.GoToWaterSource)
                      {
                          //Debug.Log($"Moving to item");
                          if (moveActionData.finished && moveActionData.success)
                          {
                              thirstStateCurrent.state = ThirstStates.Drink;
                          }
                          else if (moveActionData.finished)
                          {
                              //finisehd but failed
                              currentState.finished = true;
                              thirstStateCurrent.state = ThirstStates.NoWaterSource;
                          }
                      }
                      else if(thirstStateCurrent.state == ThirstStates.Drink)
                      {
                          Debug.Log("At water spot");
                          //reset, but need to figure out what food we are eating, how much nutritian
                          thirstState.value = 0;
                          currentState.finished = true;
                          thirstStateCurrent.state = ThirstStates.Finished;
                          moveActionData.reset();
                          Debug.Log("Thirst satisfied");
                      }
                      else if(thirstStateCurrent.state == ThirstStates.NoWaterSource)
                      {
                          Debug.Log($"No Water:  {thirstStateCurrent.state}");
                          //hungry, but no food source, show emoji, and move on
                          currentState.finished = true;
                          thirstState.skipNextStateSelection = true;
                          thirstStateCurrent.state = ThirstStates.Finished;
                          moveActionData.reset();
                      }


              }).Schedule();

              this.eventSystem
                  .AddJobHandleForProducer<AskForItemOwnershipEvent>(Dependency);
          }


  }
      
}
    */
}
