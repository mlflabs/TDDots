using Mlf.Grid2d;
using Mlf.Grid2d.Ecs;
using Mlf.UI;
using System;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Mlf.Map2d
{

    public class MouseBuildingPlacementSystem : MonoBehaviour
    {
        

        private static MouseBuildingPlacementSystem _instance;
        public static MouseBuildingPlacementSystem instance { get { return _instance; } }



        [FormerlySerializedAs("PositiveColor")] public Color positiveColor = new Color();
        [FormerlySerializedAs("NegativeColor")] public Color negativeColor = new Color(1, 1, 1);
        [FormerlySerializedAs("OverlayPrefab")] [SerializeField] private GameObject overlayPrefab;
        public float zLevel = -1;

        [FormerlySerializedAs("MenuManager")] [Header("Menu Data")]
        public ButtonMenuManager menuManager;
        [FormerlySerializedAs("BuildButton")] public Button buildButton;
        [FormerlySerializedAs("BuildActionButtonMenuId")] public int buildActionButtonMenuId = 1;
        [Tooltip("This works with ButtonMenuManager, once start building is initiated, which menu to show")]
        /// For Debuging
        [Header("For Debuging")]
        [SerializeField] bool placeBuilding = false;
        float3 _placeBuildingMouseStartPos;
        float3 _placeBuildingInitialPos; // at start of mouse move, button down
        float3 _placeBuildingPos;
        public int2 placeBuildingGridPos;
        public bool canPlaceBuilding;
        [FormerlySerializedAs("placeBuildingSO")] public BuildingDataSo placeBuildingSo;
        private GameObject _overlayContainer;
        private SpriteRenderer[] _overlayCells;
        //[SerializeField] private GameObject buildingPrefab;
        [SerializeField] private BuildingComp buildingComp;

        //EVENTS
        public event Action<bool> ONCanBuildChanged;


        
        

        
        //public GameObject buildingShadowPrefab;
        //TESTING
        

        /*
        public GridBuildingItem gridBuildingItem { get {
                return buildingComp.item;
        } }
        */
        
        public void StartBuildSetup(BuildingDataSo buildingSo)
        {

            //Move to action button menu
            menuManager.SetActivePanel(buildActionButtonMenuId);

            //create our objects
            _overlayContainer = new GameObject("Building Overlay Container");
            
            GameObject go;
            float2 cellSize = GridSystem.GETCellSize();
            _overlayCells = new SpriteRenderer[buildingSo.size.x * buildingSo.size.y];
            for(int x = 0; x < buildingSo.size.x; x++)
                for(int y = 0; y < buildingSo.size.y; y++)
                {
                    int index = x + (y * buildingSo.size.x);
                    _overlayCells[index] = Instantiate(overlayPrefab, _overlayContainer.transform)
                        .GetComponent<SpriteRenderer>();
                    _overlayCells[index].transform.position = new float3(
                        x * cellSize.x,
                        y * cellSize.y, 0);
                }

            
            placeBuilding = true;

            placeBuildingSo = buildingSo;
            
            //we go thourgh this process to snap to grid
            _placeBuildingPos = GETMiddleScreenPosition();
            placeBuildingGridPos = GridSystem.GETGridPosition(_placeBuildingPos);
            _placeBuildingPos = GridSystem.GETWorldPosition(placeBuildingGridPos, 0, MapType.current);
            _placeBuildingInitialPos = _placeBuildingPos;

            _placeBuildingPos.z = zLevel;
            _overlayContainer.transform.position = _placeBuildingPos;
            

            /// get the middle closes cell pos.....
            placeBuildingGridPos = GridSystem.GETGridPosition(_placeBuildingPos);


            //BUILD ITEM
            BuildingItem data = new BuildingItem
            {
                pos = placeBuildingGridPos,
                typeId = buildingSo.typeId,
                rotation = 0,
                mapType = MapType.current,
                stage = BuildingStage.placingStage
            };

            buildingComp = BuildingComp.PlaceBuilding(buildingSo, data, _overlayContainer.transform);
            buildingComp.transform.localPosition = Vector3.zero; //buildingSO.placementOffset;
            /*
            buildingPrefab = new GameObject(buildingSO.buildingName);
            buildingPrefab.transform.position = placeBuildingPos; // middle closest cell
            buildingComp = buildingPrefab.AddComponent<BuildingComp>();
            buildingComp.buildingSO = buildingSO;
            
            buildingComp.Init(data,buildingSO);
            */
            //buildingShadowPrefab = Instantiate(buildingSO.clearPrefab);
            //buildingShadowPrefab.transform.position = placeBuildingPos;

            Debug.Log($"Started building process::: {_placeBuildingPos}");
            DrawBuildingOverlay();

            //buildingGridSize = buildingSize;  
        }

        public void FinishBuild()
        {

            BuildingItem data = new BuildingItem
            {
                pos = placeBuildingGridPos,
                typeId = placeBuildingSo.typeId,
                rotation = 0,
                mapType = GridSystem.CurrentMapType,
                stage = BuildingStage.placingStage
            };

            MapBuildingManagerSystem.AddBuilding(data, GridSystem.CurrentMapType);
            //finally stop everything
            StopBuildSetup();
        }


        public void StopBuildSetup()
        {
            placeBuilding = false;
            Debug.Log($"Stopped Building setup at position: {placeBuildingGridPos}");
            //Destroy(buildingComp);
            buildingComp.RemoveBuilding();
            Destroy(_overlayContainer);

            menuManager.SetActivePanel();
        }

        /*
        public void rotateBuilding()
        {
            buildingComp.RotateBuilding();
            //buildingShadowPrefab.transform.Rotate(0, 90, 0);
        }
        */


        
        private void Update()
        {


            if (placeBuilding)
            {

                if (Input.GetMouseButtonDown(0))
                {
                    _placeBuildingInitialPos = _placeBuildingPos;
                    _placeBuildingMouseStartPos = GETMousePosition();
                }

                if (Input.GetMouseButton(0))
                {
                   
                    _placeBuildingPos = _placeBuildingInitialPos +
                        (GETMousePosition() - _placeBuildingMouseStartPos);
                    DrawBuildingOverlay();
                }

                if (Input.GetMouseButtonUp(0))
                {
                    PlaceItem();
                    _placeBuildingMouseStartPos = 0;
                }

            }
                
        }


        public void PlaceItem()
        {
            float3 mousePos = GETMousePosition();

            


        }


        
        public void DrawBuildingOverlay()
        {
            //Debug.Log("Drawing Select Overlay");
            //float3 worldPos = getMousePosition(); //UtilsInput.GetMouseWorld3DPosition();


            //get grid position
            int2 gridPos = GridSystem.GETGridPosition(_placeBuildingPos);

            
            //if (gridPos.Equals(placeBuildingGridPos))
            //    return; //currently this will prevent the first go though, so keep it out



            placeBuildingGridPos = gridPos;


            

            _placeBuildingPos = GridSystem.GETWorldPositionCellCenter(gridPos, 0, MapType.current);
            _placeBuildingPos.z = zLevel;
            _overlayContainer.transform.position = _placeBuildingPos;

            //draw overlay
            //Debug.Log($"Overlay Possitions: {gridPos}, {placeBuildingPos}");

            Cell cell;
            canPlaceBuilding = true;
            bool canPlaceBuildingCell = true;
            for(int x = 0; x < placeBuildingSo.size.x; x++)
                for(int y = 0; y < placeBuildingSo.size.y; y++)
                {
                    canPlaceBuildingCell = true;


                    //Debug.Log($"Finding Cell::: {x}, {y}, {placeBuildingGridPos}");
                    cell = GridSystem.GETCell(
                        new int2(x + placeBuildingGridPos.x, y+placeBuildingGridPos.y));


                  

                    if (!cell.canBuild)
                    {
                        canPlaceBuilding = false;
                        canPlaceBuildingCell = false;
                    }
                    
                    
                    if (cell.IsDefault())
                    {
                        //Debug.Log("############################ No data found");
                        canPlaceBuilding = false;
                        canPlaceBuildingCell = false;
                    }
                    int index = x + (y * placeBuildingSo.size.x);
                    //Debug.Log($"INDE#S: {index}");
                    if (canPlaceBuildingCell)
                        _overlayCells[index].color = positiveColor;
                    else
                        _overlayCells[index].color = negativeColor;


                }

            ONCanBuildChanged?.Invoke(canPlaceBuilding);

            if(buildButton != null)
            {
                buildButton.interactable = canPlaceBuilding;
            }

            //Debug.Log($"Can build: {canPlaceBuilding}");

        }




        public float distance = 50f;
        public float3 GETMousePosition()
        {
            //Debug.Log(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            return Camera.main.ScreenToWorldPoint(Input.mousePosition);

           
            //return float3.zero;
        }

        public float3 GETMiddleScreenPosition()
        {
           return Camera.main.ViewportToWorldPoint(
                new Vector3(0.5f, 0.5f, 10f));
        }


        public struct GetSelectedItemsJob : IJob
        {
           
            

            public void Execute()
            {
                //go thought all selectable items, with transform



                /*
                if (entityPosition.x >= lowerLeftPosition.x &&
                            entityPosition.y >= lowerLeftPosition.y &&
                            entityPosition.x <= upperRightPosition.x &&
                            entityPosition.y <= upperRightPosition.y)
                */
            }
        }


        private void Awake()
        {
            DontDestroyOnLoad(this);
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