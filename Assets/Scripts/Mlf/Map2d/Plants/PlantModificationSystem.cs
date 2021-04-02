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



    public struct PlantModificationEvent
    {
        public MapType Map;
        public PlantItem Plant;
        public bool NewPlant;
    }






    //[UpdateAfter(typeof(EndSimulationEntityCommandBufferSystem))]
    //[UpdateAfter(typeof(PlantGrowthSystem))]
    //[UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public class PlantModificationSystem : ConsumeSingleEventSystemBase<PlantModificationEvent>
    {
        //EventSystem eventSystem;

        protected override void OnEvent(PlantModificationEvent e)
        {
           // Debug.LogWarning($"Plant Modification New: {e.newPlant}");

            if (e.NewPlant)
            {
                MapPlantManagerSystem.AddPlantItem(e.Plant, e.Map);
            }
            else
            {
                MapPlantManagerSystem.UpdatePlantItem(e.Plant, e.Map);
            }
        }

    }
}

