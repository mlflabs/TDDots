using Mlf.Grid2d.Ecs;
using Mlf.Map2d;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Mlf.Map2d
{
    public class BuildingManager : MonoBehaviour
    {
        [SerializeField] public Dictionary<int, BuildingComp> MainMapBuildingDic;
        [SerializeField] public Dictionary<int, BuildingComp> SecondaryMapBuildingDic;
        public static Dictionary<byte, BuildingDataSo> BuildingSoRefDict =
            new Dictionary<byte, BuildingDataSo>();

        public GameObject mainMapBuildingContainer;
        public GameObject secondaryMapBuildingContainer;


        private static BuildingManager _instance;
        public static BuildingManager Instance { get { return _instance; } }



        public void AddBuilding(in BuildingItem item, MapType map)
        {
            
            Debug.LogWarning("Adding Building");
            //TODO: here we need to compensate for building bigger than i grid cell
            int index = GridSystem.GETIndex(in item.pos, map);

            var so = BuildingSoRefDict[item.typeId];

            if(map == MapType.main)
            {
                var comp = BuildingComp.PlaceBuilding( so, item, mainMapBuildingContainer.transform);
                MainMapBuildingDic[index] = comp;
            }
            else if(map == MapType.secondary)
            {
                SecondaryMapBuildingDic[index] = BuildingComp.PlaceBuilding(
                    so, item, secondaryMapBuildingContainer.transform);
            }

            
        }


        public void LoadBuildings(MapDataSo map, MapType mapType)
        {

            if(mapType == MapType.main)
            {
                BuildingSoRefDict = new Dictionary<byte, BuildingDataSo>();
                for (int i = 0; i < map.buildingRefList.items.Length; i++)
                {
                    BuildingSoRefDict[map.buildingRefList.items[i].typeId] =
                        map.buildingRefList.items[i];
                }
            }

           

            if(mapType == MapType.main)
            {
                //first destroy previous data
                MainMapBuildingDic = new Dictionary<int, BuildingComp>();
                if (mainMapBuildingContainer != null)
                {
                    DestroyImmediate(mainMapBuildingContainer);
                }
                CreateParentFolders();
                for (int i = 0; i < map.buildingItems.Count; i++)
                {

                    AddBuilding(map.buildingItems[i], mapType);
                }

            }
            else if(mapType == MapType.secondary)
            {
                SecondaryMapBuildingDic = new Dictionary<int, BuildingComp>();
                if (secondaryMapBuildingContainer != null)
                {
                    DestroyImmediate(secondaryMapBuildingContainer);
                }
                CreateParentFolders();
                for (int i = 0; i < map.buildingItems.Count; i++)
                {

                    AddBuilding(map.buildingItems[i], mapType);
                }


            }
            else
            {
                Debug.LogError("Map type not recognized:: " + mapType);
            }

        }



        private void Start()
        {
            CreateParentFolders();     
           
        }

        private void CreateParentFolders()
        {
            if (mainMapBuildingContainer == null)
            {
                mainMapBuildingContainer = new GameObject();
                mainMapBuildingContainer.name = "MainBuildings";
            }

            if (secondaryMapBuildingContainer == null)
            {
                secondaryMapBuildingContainer = new GameObject();
                secondaryMapBuildingContainer.name = "SecondaryBuildings";
            }
        }

        private void OnDestroy()
        {

            Destroy(mainMapBuildingContainer);
            Destroy(secondaryMapBuildingContainer);
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
