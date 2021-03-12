using Mlf.Grid2d.Ecs;
using Mlf.Map2d;
using Mlf.Utils.Random;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
namespace Mlf.Brains.Actions
{


    public struct FindRandomSpotActionData : IComponentData
    {
        public int maxDistanceAllowed;
        public int minDistanceAllowed;

    }


    class GeteRandomSpotPathActionSystem : SystemBase
    {
        protected EndSimulationEntityCommandBufferSystem m_EndSimulationEcbSystem;


        //private var maps = GridSystem.Maps;

        protected override void OnCreate()
        {
            base.OnCreate();

            m_EndSimulationEcbSystem = World
                .GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        }


        protected override void OnUpdate()
        {
            //TODO here we could just use a tag to see if array updated, timestamp
            NativeArray<GridDataStruct> maps = new NativeArray<GridDataStruct>(GridSystem.Maps.Length, Allocator.TempJob);
            maps.CopyFrom(GridSystem.Maps.ToArray());

            var entityCommandBuffer =
                m_EndSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();
            var randomArray = World.GetExistingSystem<RandomSystem>().RandomArray;


            //grid = Grid;
            //var gridSize = GridSystem.gridMapSize;


            List<MapComponentShared> mapIds = new List<MapComponentShared>();
            EntityManager.GetAllUniqueSharedComponentData<MapComponentShared>(mapIds);

            for (int m = 0; m < mapIds.Count; m++)
            {
                int mapId = mapIds[m].mapId;

                Dependency = Entities
                .WithName("FindRandomSpotAction")
                .WithAll<FindRandomSpotActionData>()
                .ForEach((Entity entity,
                            int entityInQueryIndex,
                            int nativeThreadIndex,
                            in FindRandomSpotActionData findData,
                            in LocalToWorld transform) =>
                {

                    //find map index
                    int mapid = -1;
                    for (int i = 0; i < maps.Length; i++)
                    {
                        if (maps[i].id == mapId)
                        {
                            mapid = i;
                            break;
                        }
                    }

                    //if we didn't find map, bug
                    if (mapid == -1)
                    {
                        return;
                    }


                    //get random object
                    //var random = randomArray[nativeThreadIndex];
                    //int2 randomLocation = new int2(-1, -1);
                    //bool walkable = false;
                    /*while (!walkable){
                        randomLocation = locationData.gridPos + new int2(
                           random.NextInt(findData.minDistanceAllowed, findData.maxDistanceAllowed),
                           random.NextInt(findData.minDistanceAllowed, findData.maxDistanceAllowed));

                        if (randomLocation.x < 0 || randomLocation.x >= maps[mapid].gridSize.x || 
                            randomLocation.y < 0 || randomLocation.y >= maps[mapid].gridSize.y)
                        {
                            walkable = false;
                        }
                        else
                        {
                            walkable = true; // !grid[randomLocation.x + (randomLocation.y * gridSize.x)].water;
                        }    


                    }
                    */
                    //1. Find a spot to go to

                    //Debug.log("Testing.....");
                    //2. Calculate path to it
                    //var path = UtilsPath.findPath(in locationData.gridPos, in randomLocation, maps[mapid]);
                    //3. Go to it
                    //4. Finish Aciton


                    //entityCommandBuffer.AddComponent<MyReadWriteExampleComponent>(entityInQueryIndex, entity);
                }).Schedule(Dependency);

                m_EndSimulationEcbSystem.AddJobHandleForProducer(Dependency);

                //when finished add StateCompleteTag
                if (maps.IsCreated)
                    maps.Dispose();

            }

        }
    }
}
