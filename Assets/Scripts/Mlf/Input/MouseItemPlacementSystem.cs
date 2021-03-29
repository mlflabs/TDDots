using System;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Mlf.Inputs
{

    //add this to all selectable entites
    public struct SelectableTag: IComponentData { };

    public class MouseItemPlacementSystem : MonoBehaviour
    {
        

        private static MouseItemPlacementSystem _instance;
        public static MouseItemPlacementSystem instance { get { return _instance; } }





        //place building settings
        [SerializeField] bool placeBuilding = false;
        [SerializeField] private int2 buildingGridSize = new int2(1, 1);
        float2 placeBuildingMouseStartPos;
        float2 placeBuildingInitialPos; // at start of mouse move, button down
        float2 placeBuildingPos;
        [SerializeField] private float2 startPosition;
        [SerializeField] private float2 endPosition;
        [SerializeField] public float3 selectionAreaSize;
        [SerializeField] private float selectionAreaMinSize = 1f;
        [SerializeField] bool selectOnlyOneEntity;


        [SerializeField] private GameObject selectOverlay;


        //EVENTS
        public event Action<bool> onCanBuildChanged;

        /// <summary>
        /// ///////////Old
        /// </summary>
        //[SerializeField] private Transform selectionAreaTransform;
        
        [SerializeField] private MeshFilter selectMeshFilter;




        //place building settings
        
        
        public int2 placeBuildingGridPos;
        public bool canPlaceBuilding;
        //////////////////public BuildingSO placeBuildingSO;
        //public GameObject buildingShadowPrefab;
        //TESTING
        [SerializeField] private GameObject buildingPrefab;
        ////////////////[SerializeField] private BuildingComp buildingComp;

        /*
        public GridBuildingItem gridBuildingItem { get {
                return buildingComp.item;
        } }
        */
        
        /*
        public void startBuildSetup(BuildingSO buildingSO)
        {
            
            placeBuilding = true;
            selectOverlay.SetActive(true);

            placeBuildingSO = buildingSO;
            buildingGridSize = buildingSO.size;
            placeBuildingPos = getMiddleScreenPosition();
            placeBuildingGridPos = GridSystem.currentMapData.getGridPosition(placeBuildingPos);

            //building shadow
            buildingPrefab = new GameObject(buildingSO.buildingName);
            buildingPrefab.transform.position = placeBuildingPos;
            buildingComp = buildingPrefab.AddComponent<BuildingComp>();
            buildingComp.buildingSO = buildingSO;
            GridBuildingItem item = new GridBuildingItem
            {
                pos = placeBuildingGridPos,
                buildingId = buildingSO.id,
                rotation = 0
            };
            buildingComp.Init(
                item, BuildingStage.placingStage,buildingSO, in GridSystem.currentMapData);

            //buildingShadowPrefab = Instantiate(buildingSO.clearPrefab);
            //buildingShadowPrefab.transform.position = placeBuildingPos;

            Debug.Log($"Started building process::: {placeBuildingPos}");
            drawBuildingOverlay();

            //buildingGridSize = buildingSize;
        }
        */



        public void stopBuildSetup()
        {
            placeBuilding = false;
            selectOverlay.SetActive(false);
            Debug.Log($"Stopped Building setup at position: {placeBuildingGridPos}");
            Destroy(buildingPrefab);
        }

        /*
        public void rotateBuilding()
        {
            buildingComp.RotateBuilding();
            //buildingShadowPrefab.transform.Rotate(0, 90, 0);
        }
        */


        /*
        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (placeBuilding)
                {
                    placeBuildingInitialPos = placeBuildingPos;
                    placeBuildingMouseStartPos = getMousePosition();

                }
                else
                {
                    // Mouse Pressed
                    selectMeshFilter.mesh.Clear();
                    selectOverlay.SetActive(true);
                    startPosition = getMousePosition();
                }
               
            }

            if (Input.GetMouseButton(0))
            {
                if (placeBuilding)
                {
                                       
                    placeBuildingPos = placeBuildingInitialPos +
                        (getMousePosition() - placeBuildingMouseStartPos);
                    drawBuildingOverlay();
                }
                else
                {
                    drawOverlay();
                }
                
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (placeBuilding)
                {
                    placeItem();
                    //drawBuildingOverlay();
                }
                else
                {
                    // Mouse Released
                    selectOverlay.gameObject.SetActive(false);
                    //drawOverlay();

                    if (!selectOnlyOneEntity)
                    {

                    }
                }
                

               

                ////////// Deselect all selected Entities
                //Entities.WithAll<UnitSelected>().ForEach((Entity entity) => {
                //    PostUpdateCommands.RemoveComponent<UnitSelected>(entity);
                //});

                /////// Select Entities inside selection area
                / *int selectedEntityCount = 0;
                Entities.ForEach((Entity entity, ref Translation translation) => {
                    if (selectOnlyOneEntity == false || selectedEntityCount < 1)
                    {
                        float3 entityPosition = translation.Value;
                        if (entityPosition.x >= lowerLeftPosition.x &&
                            entityPosition.y >= lowerLeftPosition.y &&
                            entityPosition.x <= upperRightPosition.x &&
                            entityPosition.y <= upperRightPosition.y)
                        {
                            // Entity inside selection area
                            PostUpdateCommands.AddComponent(entity, new UnitSelected());
                            selectedEntityCount++;
                        }
                    }
                });* /
            }

            if (Input.GetMouseButtonDown(1))
            {
                /////// Right mouse button down
                / *float3 targetPosition = UtilsClass.GetMouseWorldPosition();
                List<float3> movePositionList = GetPositionListAround(targetPosition, new float[] { 10f, 20f, 30f }, new int[] { 5, 10, 20 });
                int positionIndex = 0;
                Entities.WithAll<UnitSelected>().ForEach((Entity entity, ref MoveTo moveTo) => {
                    moveTo.position = movePositionList[positionIndex];
                    positionIndex = (positionIndex + 1) % movePositionList.Count;
                    moveTo.move = true;
                });
                * /
            }


        }

*/
        public void placeItem()
        {
            float3 mousePos = getMousePosition();

            


        }


        /*
        public void drawBuildingOverlay()
        {
            //Debug.Log("Drawing Select Overlay");
            //float3 worldPos = getMousePosition(); //UtilsInput.GetMouseWorld3DPosition();


            //get grid position
            int2 gridPos = GridSystem.currentMapData.getGridPosition(placeBuildingPos);

            //Debug.Log($"Draw Building Overlay:: {gridPos}, {buildingGridSize}");

            //if (gridPos.Equals(placeBuildingGridPos))
            //    return;

            placeBuildingGridPos = gridPos;

            //draw overlay
            MeshData meshData = new MeshData();
            Cell cell;
            //Debug.Log("------");
            int startHeight = -1;
            int textureColor = 6;//blue
            canPlaceBuilding = true;
            for(int x = 0; x < buildingGridSize.x; x++)
                for(int y = 0; y < buildingGridSize.y; y++)
                {
                    textureColor = 6;
                    //Debug.Log($"Finding Cell::: {x}, {y}, {placeBuildingGridPos}");
                    cell = GridSystem.currentMapData.getCell(
                        new int2(x + placeBuildingGridPos.x, y+placeBuildingGridPos.y));
                    if (startHeight == -1)
                        startHeight = cell.pos.y;

                    if(startHeight != cell.pos.y || !cell.canMove)
                    {
                        textureColor = 4;
                        canPlaceBuilding = false;
                    }

                    if (!cell.canMove || cell.buildingId != 0)
                    {
                        textureColor = 4;
                        canPlaceBuilding = false;
                    }
                    
                    
                    if (!cell.notNull())
                    {
                        //Debug.Log("############################ No data found");
                        return;
                    }

                    //Debug.Log($"Cell pos: {cell.pos} ");
                    UtilsChunk.AddVoxelDataToOverlay(cell, meshData, GridSystem.currentMapData, textureColor);
                }

            onCanBuildChanged?.Invoke(canPlaceBuilding);
            Debug.Log($"Can build: {canPlaceBuilding}");
            selectMeshFilter.mesh.Clear();
            selectMeshFilter.mesh = UtilsChunk.CreateMesh(meshData);

            buildingComp.MoveBuilding(placeBuildingGridPos, in GridSystem.currentMapData);

            //buildingShadowPrefab.transform.position = placeBuildingPos;
            //buildingPrefab.transform.position = placeBuildingPos;
        }

        */


        /*
        public void drawOverlay()
        {

            //Debug.Log("Drawing Select Overlay");
            endPosition = getMousePosition(); //UtilsInput.GetMouseWorld3DPosition();
            float3 lowerLeftPosition = new float3(
                      math.min(startPosition.x, endPosition.x),
                   0, math.min(startPosition.z, endPosition.z));
            float3 upperRightPosition = new float3(
                   math.max(startPosition.x, endPosition.x),
                0, math.max(startPosition.z, endPosition.z));

            float selectionAreaSize = math.distance(lowerLeftPosition, upperRightPosition);

            //Debug.Log($"Positions:: {startPosition}, {endPosition}");
            //Debug.Log($"Select Size:: {selectionAreaSize}, {selectionAreaMinSize}, {lowerLeftPosition}, {upperRightPosition}");
            if (selectionAreaSize < selectionAreaMinSize)
            {
                //Debug.Log("Too Small, not drawing select overlay");
                // Selection area too small
                lowerLeftPosition += new float3(-1, -1, 0) * (selectionAreaMinSize - selectionAreaSize) * .5f;
                upperRightPosition += new float3(+1, +1, 0) * (selectionAreaMinSize - selectionAreaSize) * .5f;
                selectOnlyOneEntity = true;
            }
            else
            {
                //Debug.Log($"Drawing overlay {lowerLeftPosition}, {upperRightPosition}");
                selectOnlyOneEntity = false;
                selectMeshFilter.mesh.Clear();
                selectMeshFilter.mesh = UtilsChunk.GenerateOverlay(lowerLeftPosition, 
                                           upperRightPosition, 
                                           GridSystem.currentMapData);
            }
        }

        */



        public float distance = 50f;
        public float3 getMousePosition()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, distance))
            {
                //draw invisible ray cast/vector
                //Debug.DrawLine(ray.origin, hit.point);
                //log hit area to the console
                //Debug.Log(hit.point);
                return hit.point;

            }

            return float3.zero;
        }

        public float3 getMiddleScreenPosition()
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, distance))
            {
                //draw invisible ray cast/vector
                //Debug.DrawLine(ray.origin, hit.point);
                //log hit area to the console
                //Debug.Log(hit.point);
                return hit.point;

            }

            return float3.zero;
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

        private void Start()
        {
            if (selectOverlay)
            {
                selectMeshFilter = selectOverlay.AddComponent<MeshFilter>();
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