using BovineLabs.Event.Jobs;
using BovineLabs.Event.Systems;
using Mlf.Grid2d.Ecs;
using Mlf.Map2d;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Mlf.Npc
{

    public enum ItemType : byte
    {
        standard, plant, building
    }

    public struct AskForItemOwnershipEvent
    {
        public MapType Map;
        public int IndexPos;
        public int UserId;
        public ItemType Type;
        public bool RemoveOwnership;
        //public Entity entity;
    }






    //[UpdateAfter(typeof(EndSimulationEntityCommandBufferSystem))]
    //[UpdateAfter(typeof(PlantGrowthSystem))]
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public class NpcOwnershipSystem : ConsumeSingleEventSystemBase<AskForItemOwnershipEvent>
    {
        //EventSystem eventSystem;

        protected override void OnEvent(AskForItemOwnershipEvent e)
        {

            if (e.Map == MapType.main)// which map
            {
                if (e.Type == ItemType.plant)
                {

                    if (e.RemoveOwnership)
                    {
                        if (MapPlantManagerSystem.MainMapPlantItems[e.IndexPos].CurrentOwner == e.UserId)
                        {
                            var item = MapPlantManagerSystem.MainMapPlantItems[e.IndexPos];
                            item.CurrentOwner = 0;
                            MapPlantManagerSystem.MainMapPlantItems[e.IndexPos] = item;
                        }
                    }
                    else
                    {
                        //We are adding ownership
                        if (MapPlantManagerSystem.MainMapPlantItems[e.IndexPos].CurrentOwner > 0)
                            return;

                        var item = MapPlantManagerSystem.MainMapPlantItems[e.IndexPos];
                        item.CurrentOwner = e.UserId;
                        MapPlantManagerSystem.MainMapPlantItems[e.IndexPos] = item;
                    }
                }
            }
            else
            {
                Debug.LogError($"Map type not recognized:: {e.Map}");
            }
        }


        /*
        protected override void OnUpdate()
        {
            //var mainPlantItems = MapPlantManagerSystem.MainMapPlantItems;
            //var secondaryPlantItems = MapPlantManagerSystem.SecondaryMapPlantIems;
            //var test = new NativeArray<int>(4, Allocator.TempJob);
            dependencyJobHandle = new OwnershipEventJob
            {
                mainMapPlantItems = MapPlantManagerSystem.MainMapPlantItems, 
            }.Schedule<OwnershipEventJob, AskForItemOwnershipEvent>(eventSystem);

            //dependencyJobHandle.Complete();
            //endSimulationEcbSystem.AddJobHandleForProducer(events);
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            //world = World;
            //endSimulationEcbSystem =
            //  World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            eventSystem = World.GetOrCreateSystem<EventSystem>();
        }
        */
        

        [BurstCompile]
        private struct OwnershipEventJob : IJobEvent<AskForItemOwnershipEvent>
        {

            public NativeHashMap<int, PlantItem> MainMapPlantItems;

            public void Execute(AskForItemOwnershipEvent e)
            {
                Debug.Log($"===============Reading onwership requests from: {e.UserId}, {e.IndexPos} ");
            }
        }


        public JobHandle GetDependency()
        {
            return Dependency;
        }

        public void AddDependency(JobHandle inputDependency)
        {
            Dependency = JobHandle.CombineDependencies(Dependency, inputDependency);
        }


    }
}

