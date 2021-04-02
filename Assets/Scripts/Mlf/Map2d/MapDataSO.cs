using Mlf.Grid2d;
using Mlf.Tiles2d;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Mlf.Map2d
{

    public interface IGridItem
    {
        public int2 Pos { get; set; }
        public byte TypeId { get; set; }
        public int CurrentOwner { get; set; }
    }

    [System.Serializable]
    public struct MapItem: IGridItem
    {
        public int2 Pos { get; set; }
        public byte TypeId { get; set; }
        public byte Quantity { get; set; }
        public int CurrentOwner { get; set; } //if someone is going to use this....
    }




    [CreateAssetMenu(fileName = "New Map", menuName = "Mlf/2D/MapData")]
    public class MapDataSo : ScriptableObject
    {
        public int id;


        
        [Header("Grid")]
        [SerializeField] public GridData grid;

        [Header("References")]
        public TileReferenceListSo tileRefList;
        public PlantReferenceListSo plantRefList;
        public BuildingReferenceListSo buildingRefList;

        
        [Header("Map Data")]
        [SerializeField] public List<MapItem> items;
        [SerializeField] public List<PlantItem> plantItems;
        [SerializeField] public List<BuildingItem> buildingItems;

        [Header("Plant Data")]
        public float growthMultiplierPerSecond = 1;




        [Header("Map Display Data")]
        public float2 cellSize = new float2(1f, 1f);
        public float3 originPosition = float3.zero;


        [Header("Temp Data")]
        public string tilemapName = "Ground";
        public string tilemapSecondaryName = "UpperGround";
        public string plantTilemapName = "Plants";
        [SerializeField] public GameObject spritePrefab;


        public int2 GetCurrentMapPos(float3 worldPosition)
        {
            return new int2(
            (int)((worldPosition - originPosition).x / cellSize.x),
            (int)((worldPosition - originPosition).y / cellSize.y));
        }



        public float3 GetCellWorldCoordinates(int i, float z = 0f)
        {

            float2 f = (GetPositionByIndex(i) * cellSize);
            return new float3(f.x, f.y, z) + originPosition;
        }

        public float3 GetCellWorldCoordinatesMiddle(int i, float z = 0f)
        {

            float2 f = (GetPositionByIndex(i) * cellSize) + (cellSize/2);
            return new float3(f.x, f.y, z) + originPosition;
        }

        public float3 GetCellWorldCoordinates(int2 pos, float z = 0f)
        {
            float2 x = (pos * cellSize);
            return new float3(x.x, x.y, z) + originPosition;
        }

        public int GetGridIndex(int x, int y)
        {
            return x + (y * grid.gridSize.x);
        }
        public int GetGridIndex(in int2 pos)
        {
            return pos.x + (pos.y * grid.gridSize.x);
        }

        public int2 GetPositionByIndex(int i)
        {
            return new int2(i % grid.gridSize.x,
                            Mathf.FloorToInt(i / grid.gridSize.x));
        }


    }
}