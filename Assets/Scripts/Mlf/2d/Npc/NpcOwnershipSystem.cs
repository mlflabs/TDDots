using BovineLabs.Event.Jobs;
using BovineLabs.Event.Systems;
using Mlf.Grid2d.Ecs;
using Mlf.Map2d;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
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
        public MapType map;
        public int indexPos;
        public int userId;
        public ItemType type;
        public Entity entity;
    }






    [UpdateAfter(typeof(EndSimulationEntityCommandBufferSystem))]
    public class NpcOwnershipSystem : SystemBase //ConsumeSingleEventSystemBase<AskForItemOwnershipEvent>
    {
        EventSystem eventSystem;
        /*
        protected override void OnEvent(AskForItemOwnershipEvent e)
        {

            if (e.map == MapType.main)// which map
            {
                if (e.type == ItemType.plant)
                {
                    if (MapPlantManagerSystem.MainMapPlantItems[e.indexPos].currentOwner > 0)
                        return;

                    var item = MapPlantManagerSystem.MainMapPlantItems[e.indexPos];
                    item.currentOwner = e.userId;
                    MapPlantManagerSystem.MainMapPlantItems[e.indexPos] = item;
                }
            }
            else
            {
                Debug.LogError($"Map type not recognized:: {e.map}");
            }
        }
        */


        protected override void OnUpdate()
        {
            //var mainPlantItems = MapPlantManagerSystem.MainMapPlantItems;
            //var secondaryPlantItems = MapPlantManagerSystem.SecondaryMapPlantIems;

            Dependency = new OwnershipEventJob
            {

            }.Schedule<OwnershipEventJob, AskForItemOwnershipEvent>(eventSystem);
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

        

        [BurstCompile]
        private struct OwnershipEventJob : IJobEvent<AskForItemOwnershipEvent>
        {
            [ReadOnly]
            public NativeArray<int> test;

            public void Execute(AskForItemOwnershipEvent e)
            {
                Debug.Log($"===============Reading onwership requests from: {e.userId}, {e.indexPos} ");
            }
        }


    }
}

