using Mlf.Brains.Actions;
using Mlf.Brains.States;
using Mlf.Grid2d.Ecs;
using Mlf.Map2d;
using Mlf.Npc;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Mlf.Brains.Authoring.NPCFriend
{
    public class NpcPlantEaterAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public NpcPlantEaterSo so;


        public void Convert(Entity entity, EntityManager dstManager,
            GameObjectConversionSystem conversionSystem)
        {

            MapType map = MapType.main;

            dstManager.AddComponentData(entity, new MoveActionData
            {
                Destination = new float2(transform.position.x, transform.position.y),
                MoveSpeed = 2f
            }); ;

            
            //Map shared object
            //int currentMapId = (MapManager.Instance != null) ?
            //    MapManager.Instance.GetCurrentMapId() : 0;

            dstManager.AddSharedComponentData(entity, new MapComponentShared
            { type = map });


            //Brain Tags
            dstManager.AddComponentData(entity, new StateCompleteTag { });
            dstManager.AddComponentData(entity, new CurrentBrainState { });




            // wander
            dstManager.AddComponentData(entity, new WanderDefaultSystemTag { });
            dstManager.AddComponentData(entity, new WanderScore { Value = 0 });
            dstManager.AddComponentData(entity,
                new WanderState { DefaultNeed = so.wander.wanderDefaultScore });
            dstManager.AddComponentData(entity,
                new WanderData { MAXDistance = 5 });



            //see if we need to generate a unique id
            int userid = NpcManagerSystem.AddNpc(entity, so.userId, map);
            so.userId = userid;

            dstManager.AddComponentData(entity, new NpcData { UserId = so.userId });
            //dstManager.AddComponentData(entity, new MapItemOwned { });



            // hunger
            dstManager.AddComponentData(entity, new HungerDefaultSystemTag { });
            dstManager.AddComponentData(entity, new HungerScore { Value = 0 });
            dstManager.AddComponentData(entity, 
                new HungerState { Value = so.hunger.startingHunger });
            dstManager.AddComponentData(entity, 
                new HungerData { HungerLps = so.hunger.hungerLps,
                                 HungerThreshold = so.hunger.hungerThreshold});

            /*
            // thirst
            dstManager.AddComponentData(entity, new ThirstDefaultSystemTag { });
            dstManager.AddComponentData(entity, new ThirstScore { value = 0 });
            dstManager.AddComponentData(entity,
                new ThirstState { value = so.thirst.startingThirst });
            dstManager.AddComponentData(entity,
                new ThirstData
                {
                    thirstLPS = so.hunger.hungerLPS,
                    thirstThreshold = so.hunger.hungerThreshold
                });
            */


            // rest
            //dstManager.AddComponentData(entity, new RestDefaultSystemTag { });
            //dstManager.AddComponentData(entity, new RestScore { value = 0 });
            //dstManager.AddComponentData(entity,
            //    new RestState { value = so.rest.startingRest });
            //dstManager.AddComponentData(entity,
            //    new RestData { restLPS = so.rest.restLPS });


            // play
            //dstManager.AddComponentData(entity, new PlayDefaultSystemTag { });
            //dstManager.AddComponentData(entity, new PlayScore { });
            //dstManager.AddComponentData(entity,
            //    new PlayState { value = so.play.startingPlay });
            //dstManager.AddComponentData(entity,
            //    new PlayData { playLPS = so.play.playLPS });




            //dstManager.AddBuffer<ChunkMeshIndices>(entity);
            //dstManager.AddBuffer<ChunkMeshUVs>(entity);
            //dstManager.AddBuffer<ChunkMeshVerts>(entity);
        }
    }
}
