using Mlf.Grid2d;
using Mlf.Npc;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Mlf.Map2d
{
    [System.Serializable]
    public struct PlantLevel
    {
        public byte level;
        public byte levelUpgradeGrowthPointsRequired; //how much growth till next level
        public byte yieldReceived;
        public byte yieldEffort;
    }


    [System.Serializable]
    public struct PlantItem : IGridItem
    {
        public static byte MaxLevel = 3; //starts from 0
        public int2 Pos { get => pos; set => pos = value; }
        [SerializeField] private int2 pos;
        [FormerlySerializedAs("TimeUpdated")] public float timeUpdated;
        public byte TypeId { get => typeId; set => typeId = value; }
        [SerializeField] private byte typeId;

        [FormerlySerializedAs("Level")] public byte level;
        public byte growth;
        public int CurrentOwner { get => currentOwner; set => currentOwner = value; }
        //if someone is going to use this....
        [SerializeField] private int currentOwner;


        public PlantItem GETSeedling(in int2 position)
        {
            return new PlantItem
            {
                Pos = position,
                timeUpdated = this.timeUpdated,
                typeId = this.typeId

            };
        }

    }

    [CreateAssetMenu(fileName = "PlantData", menuName = "Mlf/2D/PlantDataSO")]
    public class PlantDataSo : ScriptableObject
    {
        public byte typeId;
        public float walkSpeedMultiplier = 1;
        
        [FormerlySerializedAs("CanBuild")] public bool canBuild = false;
        [FormerlySerializedAs("CanGrow")] public bool canGrow = false;
        [FormerlySerializedAs("CanReproduce")] public bool canReproduce = false;

        [FormerlySerializedAs("PlacementOffset")] public Vector3 placementOffset = Vector3.zero;

        public PlantLevel level0;
        public PlantLevel level1;
        public PlantLevel level2;
        public PlantLevel level3;//last level produces seed

        
        public Sprite sprite0;
        public Sprite sprite1;
        public Sprite sprite2;
        public Sprite sprite3;

        public Sprite GETSpriteLevel(byte level)
        {
            if (level == 0) return sprite0;
            if (level == 1) return sprite1;
            if (level == 2) return sprite2;
            if (level == 3) return sprite3;
            return sprite0;//make it default one
        }
    }

    [System.Serializable]
    public struct PlantDataStruct
    {
        //public string name;
        public byte typeId;
        public float walkSpeedMultiplier;

        [FormerlySerializedAs("CanBuild")] public bool canBuild;
        [FormerlySerializedAs("CanGrow")] public bool canGrow;
        [FormerlySerializedAs("CanReproduce")] public bool canReproduce;
        //after eaten, which level does it go to
        //if -1 then its destroyed
        [FormerlySerializedAs("ConsumeLevelDowngrade")] public byte consumeLevelDowngrade; 

        public PlantLevel level0;
        public PlantLevel level1;
        public PlantLevel level2;
        public PlantLevel level3;//last level produces seed


        public PlantLevel GETPlantLevel(byte level)
        {
            if (level == 0) return level0;
            if (level == 1) return level1;
            if (level == 2) return level2;
            if (level == 3) return level3;
            return level3;//in future we will allow more levels, just share top sprite
        }

        public static PlantDataStruct FromSo(PlantDataSo so)
        {
            return new PlantDataStruct
            {
                typeId = so.typeId,
                walkSpeedMultiplier = so.walkSpeedMultiplier,
                canBuild = so.canBuild,
                canReproduce = so.canReproduce,
                level0 = so.level0,
                level1 = so.level1,
                level2 = so.level2,
                level3 = so.level3,
                
            };
        }

         
       
        public static float ConsumeProgress(in PlantDataStruct data, in PlantItem item, byte workAmount)
        {
            if(workAmount >= data.GETPlantLevel(item.level).yieldEffort)
            {
                return 1;
            }
            else
            {
                return (workAmount / data.GETPlantLevel(item.level).yieldEffort);
            }
        }


        public Cell UpdateCell(Cell c)
        {
            if (!canBuild) c.canBuild = false;
            if (!canGrow) c.canGrow = false;
            c.walkSpeed = c.walkSpeed * walkSpeedMultiplier;

            return c;
        }

    }
}