using Mlf.Grid2d;
using Mlf.Grid2d.Ecs;
using Mlf.Map2d;
using Mlf.MyTime;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Mlf.Brains.Actions
{


    
    public struct WorkActionData : IComponentData
    {
        public bool Finished;
        public bool Success;
    }

    class WorkActionSystem : SystemBase
    {

        protected override void OnUpdate()
        {
            var deltaTime = TimeSystem.DeltaTime; // Time.DeltaTime;


            var mapComps = new List<MapComponentShared>();
            EntityManager.GetAllUniqueSharedComponentData<MapComponentShared>(mapComps);


            GridDataStruct map;
            NativeArray<Cell> cells;
            for (int m = 0; m < mapComps.Count; m++)
            {
                if (mapComps[m].type == MapType.main)
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
                .WithName("WorkActionSystem")
                //.WithReadOnly(maps)
                //.WithReadOnly(cells)
                .WithSharedComponentFilter(mapComps[m])
                .ForEach((Entity entity,
                            ref WorkActionData workActionData) =>
                { 

                    
                }).Schedule();

            }
        }





    }
}
