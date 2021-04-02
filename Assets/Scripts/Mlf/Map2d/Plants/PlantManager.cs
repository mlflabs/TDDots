using Mlf.Grid2d.Ecs;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Mlf.Map2d
{




    public class PlantManager : MonoBehaviour
    {

        [SerializeField] public Dictionary<int, PlantComp> MainMapPlantsDic;
        [SerializeField] public Dictionary<int, PlantComp> SecondaryMapPlantsDic;
        public static Dictionary<byte, PlantDataSo> PlantSoRefDict = 
            new Dictionary<byte, PlantDataSo>();


        public GameObject mainMapContainer;
        public GameObject secondaryMapContainer;

        [SerializeField] public int spriteSortingLayer = 0;

        //[SerializeField] private GameObject spritePrefab;





        private static PlantManager _instance;
        public static PlantManager Instance { get { return _instance; } }



        public void LoadPlants(MapDataSo map, MapType mapType)
        {

            if (mapType == MapType.main)
            {
                PlantSoRefDict = map.plantRefList.GetDictionary();
            }

            if (mapType == MapType.main)
            {
                
                if (mainMapContainer != null)
                {
                    Destroy(mainMapContainer);
                }
                
                CreateParentFolders();
                
                for (int i = 0; i < map.plantItems.Count; i++)
                {
                    AddPlant(map.plantItems[i], mapType);
                }
            }
            else if (mapType == MapType.secondary)
            {
                if (secondaryMapContainer != null)
                {
                    Destroy(secondaryMapContainer);
                }
                CreateParentFolders();
                for (int i = 0; i < map.plantItems.Count; i++)
                {
                    AddPlant(map.plantItems[i], mapType);
                }
            }
            else
            {
                Debug.LogError("Map type not recognized:: " + mapType);
            }
        }

        public void AddPlant(PlantItem p, MapType mapType)
        {
            
            int index = GridSystem.GETIndex(p.Pos, mapType);
            
            GETPlantDic(mapType).Add(index,
                    PlantComp.PlacePlant(
                        PlantSoRefDict[p.TypeId],p, mapType, GETParentFolder(mapType)));

        }

        public void RemovePlant(int2 pos, MapType mapType)
        {
            int index = GridSystem.GETIndex(pos, mapType);
            RemovePlant(index, mapType);
        }


        public void RemovePlant(int index, MapType mapType)
        {

            

            if (mapType == MapType.main)
            {
                MainMapPlantsDic[index].RemovePlant();
                MainMapPlantsDic.Remove(index);
            }
                
            else if (mapType == MapType.secondary)
            {
                SecondaryMapPlantsDic[index].RemovePlant();
                SecondaryMapPlantsDic.Remove(index);
            }
            else
            {
                Debug.LogError("Not Recognized Map Type:: " + mapType);
            }
        }


        public void UpdatePlant(in PlantItem plant, int plantIndex, MapType mapType)
        {
            GETPlantDic(mapType)[plantIndex].UpdatePlant(plant);
        }


        public Dictionary<int, PlantComp> GETPlantDic(MapType map)
        {
            if (map == MapType.main)
                return MainMapPlantsDic;
            else if (map == MapType.secondary)
                return SecondaryMapPlantsDic;
            else
            {
                Debug.LogError("Not Recognized Map Type:: " + map);
                return MainMapPlantsDic;
            }
        }

        private Transform GETParentFolder(MapType mapType)
        {
            if (mapType == MapType.main)
                return mainMapContainer.transform;
            else if (mapType == MapType.secondary)
                return secondaryMapContainer.transform;
            else
                Debug.LogError("MapType not recognized: " + mapType);

            return null;
        }
        private void CreateParentFolders()
        {
            if (mainMapContainer == null)
            {
                mainMapContainer = new GameObject();
                mainMapContainer.name = "MainBuildings";
            }

            if (secondaryMapContainer == null)
            {
                secondaryMapContainer = new GameObject();
                secondaryMapContainer.name = "SecondaryBuildings";
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
            MainMapPlantsDic =  new Dictionary<int, PlantComp>();
            SecondaryMapPlantsDic = new Dictionary<int, PlantComp>();
        }




    }
}
