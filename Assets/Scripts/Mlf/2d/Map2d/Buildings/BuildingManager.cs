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
        public static Dictionary<byte, BuildingDataSO> BuildingSORefDict =
            new Dictionary<byte, BuildingDataSO>();

        public GameObject mainMapBuildingContainer;
        public GameObject secondaryMapBuildingContainer;


        private static BuildingManager _instance;
        public static BuildingManager Instance { get { return _instance; } }



        public void AddBuilding(in BuildingItem item, MapType map)
        {
            
            Debug.LogWarning("Adding Building");
            //TODO: here we need to compensate for building bigger than i grid cell
            int index = GridSystem.getIndex(in item.pos, map);

            var so = BuildingSORefDict[item.typeId];

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


        public void LoadBuildings(MapDataSO map, MapType mapType)
        {

            if(mapType == MapType.main)
            {
                BuildingSORefDict = new Dictionary<byte, BuildingDataSO>();
                for (int i = 0; i < map.BuildingRefList.items.Length; i++)
                {
                    BuildingSORefDict[map.BuildingRefList.items[i].typeId] =
                        map.BuildingRefList.items[i];
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
                createParentFolders();
                for (int i = 0; i < map.BuildingItems.Count; i++)
                {

                    AddBuilding(map.BuildingItems[i], mapType);
                }

            }
            else if(mapType == MapType.secondary)
            {
                SecondaryMapBuildingDic = new Dictionary<int, BuildingComp>();
                if (secondaryMapBuildingContainer != null)
                {
                    DestroyImmediate(secondaryMapBuildingContainer);
                }
                createParentFolders();
                for (int i = 0; i < map.BuildingItems.Count; i++)
                {

                    AddBuilding(map.BuildingItems[i], mapType);
                }


            }
            else
            {
                Debug.LogError("Map type not recognized:: " + mapType);
            }

        }



        private void Start()
        {
            createParentFolders();     
           
        }

        private void createParentFolders()
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
