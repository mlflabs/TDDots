using Mlf.Brains.Actions;
using Mlf.Brains.States;
using Mlf.Map2d;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Mlf.Sprite2d
{
    public struct SpriteTag: IComponentData{};

    public class SpriteAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        

        public void Convert(Entity entity, EntityManager dstManager,
            GameObjectConversionSystem conversionSystem)
        {
            Debug.Log("8888888888888888 Converting Sprite");

            //dstManager.AddComponentData(entity, new SpriteTag { });


            //MapItemManagerSystem.SpritePrefab = entity;
        }
    }
}
