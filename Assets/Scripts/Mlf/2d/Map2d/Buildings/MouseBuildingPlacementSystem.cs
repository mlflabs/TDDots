using Mlf.Grid2d;
using Mlf.Grid2d.Ecs;
using Mlf.UI;
using System;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace Mlf.Map2d
{

    public class MouseBuildingPlacementSystem : MonoBehaviour
    {
        

        private static MouseBuildingPlacementSystem _instance;
        public static MouseBuildingPlacementSystem instance { get { return _instance; } }



        public Color PositiveColor = new Color();
        public Color NegativeColor = new Color(1, 1, 1);
        [SerializeField] private GameObject OverlayPrefab;
        public float zLevel = -1;

        [Header("Menu Data")]
        public ButtonMenuManager MenuManager;
        public Button BuildButton;
        public int BuildActionButtonMenuId = 1;
        [Tooltip("This works with ButtonMenuManager, once start building is initiated, which menu to show")]
        /// For Debuging
        [Header("For Debuging")]
        [SerializeField] bool placeBuilding = false;
        float3 placeBuildingMouseStartPos;
        float3 placeBuildingInitialPos; // at start of mouse move, button down
        float3 placeBuildingPos;
        public int2 placeBuildingGridPos;
        public bool canPlaceBuilding;
        public BuildingDataSO placeBuildingSO;
        private GameObject overlayContainer;
        private SpriteRenderer[] overlayCells;
        //[SerializeField] private GameObject buildingPrefab;
        [SerializeField] private BuildingComp buildingComp;

        //EVENTS
        public event Action<bool> onCanBuildChanged;


        
        

        
        //public GameObject buildingShadowPrefab;
        //TESTING
        

        /*
        public GridBuildingItem gridBuildingItem { get {
                return buildingComp.item;
        } }
        */
        
        public void startBuildSetup(BuildingDataSO buildingSO)
        {

            //Move to action button menu
            MenuManager.setActivePanel(BuildActionButtonMenuId);

            //create our objects
            overlayContainer = new GameObject("Building Overlay Container");
            
            GameObject go;
            float2 cellSize = GridSystem.getCellSize();
            overlayCells = new SpriteRenderer[buildingSO.size.x * buildingSO.size.y];
            for(int x = 0; x < buildingSO.size.x; x++)
                for(int y = 0; y < buildingSO.size.y; y++)
                {
                    int index = x + (y * buildingSO.size.x);
                    overlayCells[index] = Instantiate(OverlayPrefab, overlayContainer.transform)
                        .GetComponent<SpriteRenderer>();
                    overlayCells[index].transform.position = new float3(
                        x * cellSize.x,
                        y * cellSize.y, 0);
                }

            
            placeBuilding = true;

            placeBuildingSO = buildingSO;
            
            //we go thourgh this process to snap to grid
            placeBuildingPos = getMiddleScreenPosition();
            placeBuildingGridPos = GridSystem.getGridPosition(placeBuildingPos);
            placeBuildingPos = GridSystem.getWorldPosition(placeBuildingGridPos, MapType.current, 0);
            placeBuildingInitialPos = placeBuildingPos;

            placeBuildingPos.z = zLevel;
            overlayContainer.transform.position = placeBuildingPos;
            

            /// get the middle closes cell pos.....
            placeBuildingGridPos = GridSystem.getGridPosition(placeBuildingPos);


            //BUILD ITEM
            BuildingItem data = new BuildingItem
            {
                pos = placeBuildingGridPos,
                typeId = buildingSO.typeId,
                rotation = 0,
                mapType = MapType.current,
                stage = BuildingStage.placingStage
            };

            buildingComp = BuildingComp.PlaceBuilding(buildingSO, data, overlayContainer.transform);
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

            Debug.Log($"Started building process::: {placeBuildingPos}");
            drawBuildingOverlay();

            //buildingGridSize = buildingSize;  
        }

        public void finishBuild()
        {

            BuildingItem data = new BuildingItem
            {
                pos = placeBuildingGridPos,
                typeId = placeBuildingSO.typeId,
                rotation = 0,
                mapType = GridSystem.CurrentMapType,
                stage = BuildingStage.placingStage
            };

            MapBuildingManagerSystem.AddBuilding(data, GridSystem.CurrentMapType);
            //finally stop everything
            stopBuildSetup();
        }


        public void stopBuildSetup()
        {
            placeBuilding = false;
            Debug.Log($"Stopped Building setup at position: {placeBuildingGridPos}");
            //Destroy(buildingComp);
            buildingComp.RemoveBuilding();
            Destroy(overlayContainer);

            MenuManager.setActivePanel();
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
                    placeBuildingInitialPos = placeBuildingPos;
                    placeBuildingMouseStartPos = getMousePosition();
                }

                if (Input.GetMouseButton(0))
                {
                   
                    placeBuildingPos = placeBuildingInitialPos +
                        (getMousePosition() - placeBuildingMouseStartPos);
                    drawBuildingOverlay();
                }

                if (Input.GetMouseButtonUp(0))
                {
                    placeItem();
                    placeBuildingMouseStartPos = 0;
                }

            }
                
        }


        public void placeItem()
        {
            float3 mousePos = getMousePosition();

            


        }


        
        public void drawBuildingOverlay()
        {
            //Debug.Log("Drawing Select Overlay");
            //float3 worldPos = getMousePosition(); //UtilsInput.GetMouseWorld3DPosition();


            //get grid position
            int2 gridPos = GridSystem.getGridPosition(placeBuildingPos);

            
            //if (gridPos.Equals(placeBuildingGridPos))
            //    return; //currently this will prevent the first go though, so keep it out



            placeBuildingGridPos = gridPos;


            

            placeBuildingPos = GridSystem.getWorldPositionCellCenter(gridPos, MapType.current, 0);
            placeBuildingPos.z = zLevel;
            overlayContainer.transform.position = placeBuildingPos;

            //draw overlay
            //Debug.Log($"Overlay Possitions: {gridPos}, {placeBuildingPos}");

            Cell cell;
            canPlaceBuilding = true;
            bool canPlaceBuildingCell = true;
            for(int x = 0; x < placeBuildingSO.size.x; x++)
                for(int y = 0; y < placeBuildingSO.size.y; y++)
                {
                    canPlaceBuildingCell = true;


                    //Debug.Log($"Finding Cell::: {x}, {y}, {placeBuildingGridPos}");
                    cell = GridSystem.getCell(
                        new int2(x + placeBuildingGridPos.x, y+placeBuildingGridPos.y));


                  

                    if (!cell.canBuild)
                    {
                        canPlaceBuilding = false;
                        canPlaceBuildingCell = false;
                    }
                    
                    
                    if (cell.isDefault())
                    {
                        //Debug.Log("############################ No data found");
                        canPlaceBuilding = false;
                        canPlaceBuildingCell = false;
                    }
                    int index = x + (y * placeBuildingSO.size.x);
                    //Debug.Log($"INDE#S: {index}");
                    if (canPlaceBuildingCell)
                        overlayCells[index].color = PositiveColor;
                    else
                        overlayCells[index].color = NegativeColor;


                }

            onCanBuildChanged?.Invoke(canPlaceBuilding);

            if(BuildButton != null)
            {
                BuildButton.interactable = canPlaceBuilding;
            }

            //Debug.Log($"Can build: {canPlaceBuilding}");

        }




        public float distance = 50f;
        public float3 getMousePosition()
        {
            //Debug.Log(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            return Camera.main.ScreenToWorldPoint(Input.mousePosition);

           
            //return float3.zero;
        }

        public float3 getMiddleScreenPosition()
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