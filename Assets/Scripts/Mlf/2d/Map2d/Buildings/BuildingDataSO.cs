using Mlf.Grid2d;
using Unity.Mathematics;
using UnityEngine;


namespace Mlf.Map2d
{

    public enum BuildingTypes : byte
    {
        decoration,
    }

    public enum BuildingStage : byte
    {
        placingStage, buildStage
    }


    [System.Serializable]
    public struct BuildingItem
    {
        public MapType mapType;
        //public int mapid;
        public int2 pos;
        public byte typeId { get; set; }

        public byte rotation;
        public BuildingStage stage;

    }

    public struct BuildingDataStruct
    {
        public byte typeId;
        public int2 size;
        public int2 entranceTile;
        public BuildingTypes type;


        public static BuildingDataStruct fromBuildingDataSO (BuildingDataSO so)
        {
            return new BuildingDataStruct
            {
                typeId = so.typeId,
                entranceTile = so.entranceTile,
                size = so.size,
                type = so.type
            };
        }

        public Cell updateCell(Cell c)
        {
            //Here we can implement each building cell has different properties
            c.canBuild = false;
            c.canGrow = false;
            c.walkSpeed = 0;

            return c;
        }
    }





    [CreateAssetMenu(fileName = "New Building", menuName = "Mlf/2D/BuildingDataSO")]
    public class BuildingDataSO : ScriptableObject
    {
        public byte typeId;
        public string buildingName = "Default";
        [TextArea(5, 20)] public string description;

        public int2 size = new int2(1, 1);
        public int2 entranceTile = new int2(1, 1);
        public BuildingTypes type = BuildingTypes.decoration;

        public Sprite icon;
        public GameObject prefab;

        public Vector3 placementOffset = new Vector3(0, 0, 0);

    }
}
