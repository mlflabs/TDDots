using Mlf.Brains.Actions;
using Mlf.Grid2d;
using Mlf.Grid2d.Ecs;
using Mlf.Map2d;
using Mlf.Utils.Random;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Mlf.Brains.States
{

    public struct WanderStateCurrent : IComponentData
    {
        public byte State;

        public static WanderStateCurrent GETNewState()
        {
            return new WanderStateCurrent { State = 0 };
        }
    }



    class WanderStateSystem : SystemBase
    {
        protected EndSimulationEntityCommandBufferSystem MEndSimulationEcbSystem;

        protected override void OnCreate()
        {
            base.OnCreate();

            MEndSimulationEcbSystem = World
                .GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        }

        protected override void OnUpdate()
        {

            //Debug.Log("Wander Update");
            var randomArray = World.GetExistingSystem<RandomSystem>().RandomArray;
            //var groundTypeReferences = GridSystem.GroundTypeReferences;

            List<MapComponentShared> mapIds = new List<MapComponentShared>();
            EntityManager.GetAllUniqueSharedComponentData(mapIds);

            for (int m = 0; m < mapIds.Count; m++)
            {
                //int mapId = mapIds[m].mapId;
                GridDataStruct map;
                NativeArray<Cell> cells;
                
                if(mapIds[m].type == MapType.main)
                {
                    map = GridSystem.MainMap;
                    cells = GridSystem.MainMapCells;
                }
                else
                {
                    map = GridSystem.SecondaryMap;
                    cells = GridSystem.SecondaryMapCells;
                }

                Entities
                    .WithSharedComponentFilter(mapIds[m])
                    .WithName("DefaultWanderStateSystem")
                    .WithReadOnly(cells)
                    .ForEach((
                             int nativeThreadIndex,
                             ref WanderStateCurrent wanderState,
                             ref MoveActionData moveActionData,
                             ref CurrentBrainState currentState,
                             in LocalToWorld transform,
                             in WanderData wanderData) =>

                             {
                                 //Debug.Log("-wander000000");
                                 //Debug.Log($"Wander State: {wanderState.state} Finished: {completeProgressData.finished}");
                                 if (wanderState.State == 0)
                                 {
                                     //Debug.Log("Wander State 2");

                                     var random = randomArray[nativeThreadIndex];
                                     int2 randomLocation = new int2(-1, -1);
                                     int gridIndex = 0;
                                     int2 currentMapPosition = map.GETGridPosition(transform.Position);
                                     moveActionData.Reset();

                                     while (!moveActionData.Path.HasPath())
                                     {
                                         //Debug.Log($"MinMax: {wanderData.maxDistance} ");
                                         //Debug.Log($"Current Map Position:: {currentMapPosition.x}, {currentMapPosition.y} ");
                                         //Debug.Log($"Transform:: {transform.Position}, {transform.Position.x}, {transform.Position.z}");
                                         randomLocation = currentMapPosition + new int2(
                                             random.NextInt(wanderData.MAXDistance * -1, wanderData.MAXDistance),
                                             random.NextInt(wanderData.MAXDistance * -1, wanderData.MAXDistance));


                                         if (randomLocation.x < 1 || randomLocation.x >= map.GridSize.x - 1 ||
                                             randomLocation.y < 1 || randomLocation.y >= map.GridSize.y - 1)
                                         {

                                         }
                                         else
                                         {
                                             //Debug.Log("Wander State Found Path*****************");
                                             gridIndex = map.GETIndex(randomLocation);
                                             PathData path = UtilsPath.FindPath(
                                                 in currentMapPosition,
                                                 in randomLocation,
                                                 //in groundTypeReferences,
                                                 in cells,
                                                 in map);
                                             //Debug.Log($"Random Location:: {randomLocation.x},{randomLocation.y} ");
                                             moveActionData.LoadPath(in path, in map);
                                         }
                                     }


                                     //Debug.Log($"****************** New Wander Destination::: {moveActionData.destination} ");

                                     wanderState.State = 1;

                                 } // state  = 0
                                 else if (wanderState.State == 1)
                                 {
                                     //Debug.Log($"Wander State 3 {moveActionData.finished}");
                                     if (moveActionData.Finished)
                                         currentState.Finished = true;
                                 }


                                 //1. Find a spot to go to

                                 //2. Calculate path to it

                                 //3. Go to it
                                 //4. Finish Aciton






                                 // *  ecb.AddComponent<FindRandomSpotActionData>(entityInQueryIndex, entity,
                                 //    new FindRandomSpotActionData
                                 //    {
                                 //        minDistanceAllowed = 0,
                                 //        maxDistanceAllowed = 5
                                 //    });
                                 //    wanderState.state = 1;
                                 // * 





                                 //entityCommandBuffer.AddComponent<MyReadWriteExampleComponent>(entityInQueryIndex, entity);
                             })
                        .Schedule();
            }


        }




    }
}
