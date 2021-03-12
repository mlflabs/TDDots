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
        public Sprite sprite;
    }


    [CreateAssetMenu(fileName = "PlantData", menuName = "Mlf/2D/PlantDataSO")]
    public class PlantDataSO : ScriptableObject
    {

        public half walkSpeedMultiplier = (half)1;
        
        public bool CanBuild = false;
        public bool CanGrow = false;
        public bool CanReproduce = false;

        public PlantLevel level1;
        public PlantLevel level2;
        public PlantLevel level3;
        public PlantLevel level4;//last level produces seed


    }
}