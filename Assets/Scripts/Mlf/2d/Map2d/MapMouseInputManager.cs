using Mlf.Grid2d;
using Mlf.Grid2d.Ecs;
using Mlf.Tiles2d;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Mlf.Map2d
{




    public class MapMouseInputManager : MonoBehaviour
    {


        [Header("Map Display Data")]
        [SerializeField] public Tilemap tilemap;

        [Header("Map Data")]
        public MapDataSO mapData;


        [Header("Debug")]
        public bool highlightTileCell;
        public bool onClickPrintTileData;


        private static MapMouseInputManager _instance;
        public static MapMouseInputManager Instance { get { return _instance; } }




        private void Start()
        {


        }

        private void Update()
        {
            if (highlightTileCell && Input.GetMouseButtonDown(0))
            {
                //are we on map
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                Cell c = GridSystem.getCell(mousePos);

                if (!c.isDefault())
                {
                    Debug.LogWarning(c.print());
                }
                else
                {
                    Debug.LogWarning($"Didn't get cell MousePos: {mousePos}, {c.pos}");
                }
            }


            if (onClickPrintTileData)
                if (Input.GetMouseButtonDown(0))
                {
                    Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                    Vector3Int gridpos = tilemap.WorldToCell(mousePos);

                    TileBase tile = tilemap.GetTile(gridpos);

                    TileDataSO data = mapData.TileRefList.getData(tile);

                    if (data != null)
                    {
                        Debug.LogWarning($"TileData: {data.name}, Walkable: {data.walkSpeed}, Pos: {gridpos}");
                    }
                    else
                    {
                        Debug.LogWarning($"No Tile found pos: {gridpos}");
                    }

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
