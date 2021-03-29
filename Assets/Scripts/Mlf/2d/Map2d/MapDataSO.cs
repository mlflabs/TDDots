using Mlf.Grid2d;
using Mlf.Tiles2d;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Mlf.Map2d
{

    public interface GridItem
    {
        public int2 pos { get; set; }
        public byte typeId { get; set; }
        public int currentOwner { get; set; }
    }

    [System.Serializable]
    public struct MapItem: GridItem
    {
        public int2 pos { get; set; }
        public byte typeId { get; set; }
        public byte quantity { get; set; }
        public int currentOwner { get; set; } //if someone is going to use this....
    }




    [CreateAssetMenu(fileName = "New Map", menuName = "Mlf/2D/MapData")]
    public class MapDataSO : ScriptableObject
    {
        public int id;


        [Header("Grid")]
        [SerializeField] public GridData Grid;

        [Header("References")]
        public TileReferenceListSO TileRefList;
        public PlantReferenceListSO PlantRefList;
        public BuildingReferenceListSO BuildingRefList;

        [Header("Map Data")]
        [SerializeField] public List<MapItem> Items;
        [SerializeField] public List<PlantItem> PlantItems;
        [SerializeField] public List<BuildingItem> BuildingItems;

        [Header("Plant Data")]
        public float growthMultiplierPerSecond = 1;




        [Header("Map Display Data")]
        public float2 CellSize = new float2(1f, 1f);
        public float3 OriginPosition = float3.zero;


        [Header("Temp Data")]
        public string tilemapName = "Ground";
        public string tilemapSecondaryName = "UpperGround";
        public string plantTilemapName = "Plants";
        [SerializeField] public GameObject spritePrefab;


        public int2 GetCurrentMapPos(float3 worldPosition)
        {
            return new int2(
            (int)((worldPosition - OriginPosition).x / CellSize.x),
            (int)((worldPosition - OriginPosition).y / CellSize.y));
        }



        public float3 GetCellWorldCoordinates(int i, float z = 0f)
        {

            float2 f = (GetPositionByIndex(i) * CellSize);
            return new float3(f.x, f.y, z) + OriginPosition;
        }

        public float3 GetCellWorldCoordinatesMiddle(int i, float z = 0f)
        {

            float2 f = (GetPositionByIndex(i) * CellSize) + (CellSize/2);
            return new float3(f.x, f.y, z) + OriginPosition;
        }

        public float3 GetCellWorldCoordinates(int2 pos, float z = 0f)
        {
            float2 x = (pos * CellSize);
            return new float3(x.x, x.y, z) + OriginPosition;
        }

        public int GetGridIndex(int x, int y)
        {
            return x + (y * Grid.GridSize.x);
        }
        public int GetGridIndex(in int2 pos)
        {
            return pos.x + (pos.y * Grid.GridSize.x);
        }

        public int2 GetPositionByIndex(int i)
        {
            return new int2(i % Grid.GridSize.x,
                            Mathf.FloorToInt(i / Grid.GridSize.x));
        }


    }
}