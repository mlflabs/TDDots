using Mlf.Grid2d;
using Mlf.Npc;
using Unity.Mathematics;
using UnityEngine;

namespace Mlf.Map2d
{

    [System.Serializable]
    public struct PlantLevel
    {
        public byte level;
        public byte growthPoints;
        public byte yieldReceived;
    }


    [System.Serializable]
    public struct PlantItem : GridItem
    {
        public static byte MaxLevel = 3; //starts from 0
        public int2 pos { get => _pos; set => _pos = value; }
        [SerializeField] private int2 _pos;
        public float timeUpdated;
        public byte typeId { get => _typeId; set => _typeId = value; }
        [SerializeField] private byte _typeId;
        // public byte quantity;
        public byte level;
        public byte growth;
        public int currentOwner { get => _currentOwner; set => _currentOwner = value; }
        //if someone is going to use this....
        [SerializeField] private int _currentOwner;

        public PlantItem getSeedling(in int2 _pos)
        {
            return new PlantItem
            {
                pos = _pos,
                timeUpdated = this.timeUpdated,
                typeId = this.typeId

            };
        }

    }

    [CreateAssetMenu(fileName = "PlantData", menuName = "Mlf/2D/PlantDataSO")]
    public class PlantDataSO : ScriptableObject
    {
        public byte typeId;
        public float walkSpeedMultiplier = 1;
        
        public bool CanBuild = false;
        public bool CanGrow = false;
        public bool CanReproduce = false;


        public PlantLevel level0;
        public PlantLevel level1;
        public PlantLevel level2;
        public PlantLevel level3;//last level produces seed

        
        public Sprite sprite0;
        public Sprite sprite1;
        public Sprite sprite2;
        public Sprite sprite3;

        public Sprite getSpriteLevel(byte level)
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

        public bool CanBuild;
        public bool CanGrow;
        public bool CanReproduce;

        public PlantLevel level0;
        public PlantLevel level1;
        public PlantLevel level2;
        public PlantLevel level3;//last level produces seed


        public PlantLevel getPlantLevel(byte level)
        {
            if (level == 0) return level0;
            if (level == 1) return level1;
            if (level == 2) return level2;
            if (level == 3) return level3;
            return level3;//in future we will allow more levels, just share top sprite
        }

        public static PlantDataStruct FromSO(PlantDataSO so)
        {
            return new PlantDataStruct
            {
                typeId = so.typeId,
                walkSpeedMultiplier = so.walkSpeedMultiplier,
                CanBuild = so.CanBuild,
                CanReproduce = so.CanReproduce,
                level0 = so.level0,
                level1 = so.level1,
                level2 = so.level2,
                level3 = so.level3,
                
            };
        }

        public Cell UpdateCell(Cell c)
        {
            if (!CanBuild) c.canBuild = false;
            if (!CanGrow) c.canGrow = false;
            c.walkSpeed = c.walkSpeed * walkSpeedMultiplier;

            return c;
        }

    }
}