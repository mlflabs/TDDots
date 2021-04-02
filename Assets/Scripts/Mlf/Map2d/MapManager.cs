using Mlf.Grid2d;
using Mlf.Grid2d.Ecs;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace Mlf.Map2d
{

    public enum MapType: byte
    {
        //keep this order, these are used in array for indexes, needs to start from 0, secondary 1...
        main, secondary, temp, current  
    }

    public enum MapRoadType : byte
    {
        noRoad, vertical, horizontal, intersection, random
    }



    public class MapManager : MonoBehaviour
    {
        [Header("TileMap Data")]
        [SerializeField] public Tilemap tilemap;




        [FormerlySerializedAs("MapCellSize")] [Header("Map Generator Data")]
        public int2 mapCellSize;
        //public float2 mapOriginLocation;
        //public float2 mapCellSize;

        [Header("Map Debug Display")]
        public bool drawGizmos = false;
        public Color gizmoColor = Color.green;
        public Color gizmoWalkableColor = Color.green;
        public Color gizmoNotWalkable = Color.red;
        public Color gizmoBuildableColor = Color.blue;
        public int gizmoThickness = 1;

        [FormerlySerializedAs("MainMap")]
        [Header("Map Data")]
        [SerializeField] public MapDataSo mainMap;
        
        //[SerializeField] public List<MapDataSO> Maps;
        public static int DefaultMap = 0;
        [FormerlySerializedAs("MapParent")] public GameObject mapParent;




        private static MapManager _instance;
        public static MapManager Instance { get { return _instance; } }







        public void LoadMap(MapDataSo map, bool deleteOld = true)
        {


            if (deleteOld)
            {
                for (int i = mapParent.transform.childCount; i > 0; --i)
                    DestroyImmediate(mapParent.transform.GetChild(0).gameObject);
            }

            //add to grids array and make current
        }







        private void Start()
        {

            UpdateEntityMap(mainMap);

        }

        private void UpdateEntityMap(MapDataSo map)
        {
            GridSystem.UpdateMap(map, MapType.main);
            //PlantManager.Instance.LoadPlants(map, MapType.main);


            //MapItemManagerSystem.LoadPlantItems(map);
        }


        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;
            if (mainMap == null) return;//no map loaded
            MapDataSo so = mainMap;

            Gizmos.color = gizmoColor;
            Vector3 pos = new Vector3(0.02f + so.originPosition.x, so.originPosition.y + 0.02f, -2);
            //outline
            UtilsGizmo.DrawThickLine(
              pos,
              pos + new Vector3(mainMap.grid.gridSize.x * so.cellSize.x, 0, 0),
              gizmoThickness);
            UtilsGizmo.DrawThickLine(
              pos,
              pos + new Vector3(0, mainMap.grid.gridSize.y * so.cellSize.y, 0),
              gizmoThickness);
            UtilsGizmo.DrawThickLine(
              pos + new Vector3(mainMap.grid.gridSize.x * so.cellSize.x,
                                mainMap.grid.gridSize.y * so.cellSize.y, 0),
              pos + new Vector3(mainMap.grid.gridSize.y * so.cellSize.y, 0, 0),
              gizmoThickness);
            UtilsGizmo.DrawThickLine(
              pos + new Vector3(mainMap.grid.gridSize.x * so.cellSize.x,
                                mainMap.grid.gridSize.y * so.cellSize.y, 0),
              pos + new Vector3(0, mainMap.grid.gridSize.x * so.cellSize.y, 0),
              gizmoThickness);


            Vector3 walkableOffset = new Vector3(so.cellSize.x / 2, so.cellSize.y / 3, -3f);
            Vector3 buildableOffset = new Vector3(so.cellSize.x / 4, so.cellSize.y / 3, -3f);
            Vector3 boxSize = new Vector3(so.cellSize.x / 5, so.cellSize.y / 5, 1);
            Cell c;
            
            for (int x = 0; x < so.grid.gridSize.x; x++)
            {

                Gizmos.color = gizmoBuildableColor;
                UtilsGizmo.DrawThickLine(
                    pos + new Vector3(x * so.cellSize.x, 0, -2),
                    pos + new Vector3(x * so.cellSize.x, mainMap.grid.gridSize.y * so.cellSize.y, -2));

                UtilsGizmo.DrawThickLine(
                    pos + new Vector3(0, x * so.cellSize.y, -2),
                    pos + new Vector3(mainMap.grid.gridSize.x * so.cellSize.x, x * so.cellSize.y, -2));


                /*
                for (int y = 0; y < so.Grid.GridSize.y; y++)
                {
                    Vector3 worldPos = so.GetCellWorldCoordinates(new int2(x, y), 0.02f);

                    //UtilsGizmo.DrawText(worldPos,6, $"({x}:{y})");
                    /*


                    c = GridSystem.getCell(new int2(x,y));
                    //Debug.Log(data);
                    if (c.walkSpeed > 0)
                    {
                        Gizmos.color = gizmoWalkableColor;
                        UtilsGizmo.DrawBox(worldPos + walkableOffset, boxSize);
                    }
                    else
                    {
                        Gizmos.color = gizmoNotWalkable;
                        UtilsGizmo.DrawBox(worldPos + walkableOffset, boxSize);
                    }
                    if (c.canBuild)
                    {
                        Gizmos.color = gizmoBuildableColor;
                        UtilsGizmo.DrawBox(worldPos + buildableOffset, boxSize);
                    }
                    * /
                }
                */
            }

        }








        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;

            }
        }




    }
}
