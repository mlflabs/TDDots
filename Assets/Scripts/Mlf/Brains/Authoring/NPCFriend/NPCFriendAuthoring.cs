using Mlf.Brains.Actions;
using Mlf.Brains.States;
using Mlf.Map2d;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Mlf.Brains.Authoring.NPCFriend
{
    public class NPCFriendAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public NpcFriendSO so;


        public void Convert(Entity entity, EntityManager dstManager,
            GameObjectConversionSystem conversionSystem)
        {

            dstManager.AddComponentData(entity, new MoveActionData
            {
                destination = new float2(transform.position.x, transform.position.y),
                moveSpeed = 2f
            }); ;

            
            //Map shared object
            int currentMapId = (MapManager.Instance != null) ?
                MapManager.Instance.GetCurrentMapId() : 0;

            dstManager.AddSharedComponentData(entity, new MapComponentShared
            { mapId = currentMapId });


            //Brain Tags


            dstManager.AddComponentData(entity, new StateCompleteTag { });
            dstManager.AddComponentData(entity, new CurrentBrainState { });

            //int2 res = MapManager.Instance.GetCurrentMapPos(transform.position,
            //             MapManager.Instance.GetCurrentMapId());
            //Debug.Log($"Found Map Location::: {res.x}, {res.y} ");

            //Data
            /*
            dstManager.AddComponentData(entity, new NeedGridInfoTag
            {
                gridId = MapManager.Instance.GetCurrentMapId(),
                gridPos = MapManager.Instance.GetCurrentMapPos(transform.position,
                         MapManager.Instance.GetCurrentMapId()),
            });
            */



            // wander
            dstManager.AddComponentData(entity, new WanderDefaultSystemTag { });
            dstManager.AddComponentData(entity, new WanderScore { value = 0 });
            dstManager.AddComponentData(entity,
                new WanderState { defaultNeed = so.wander.wanderDefaultScore });
            dstManager.AddComponentData(entity,
                new WanderData { maxDistance = 5 });


            // hunger
            /*dstManager.AddComponentData(entity, new HungerDefaultSystemTag { });
            dstManager.AddComponentData(entity, new HungerScore { value = 0 });
            dstManager.AddComponentData(entity, 
                new HungerState { value = so.hunger.startingHunger });
            dstManager.AddComponentData(entity, 
                new HungerData { hungerLPS = so.hunger.hungerLPS,
                                 hungerThreshold = so.hunger.hungerThreshold});

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
