using Mlf.Grid2d.Ecs;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Mlf.Map2d
{




    public class PlantManager : MonoBehaviour
    {

        [SerializeField] public Dictionary<int, SpriteRenderer> MainMapPlantsDic;
        [SerializeField] public Dictionary<int, SpriteRenderer> SecondaryMapPlantsDic;
        public static Dictionary<byte, PlantDataSO> PlantSORefDict = 
            new Dictionary<byte, PlantDataSO>();


        public GameObject mainMapContainer;
        public GameObject secondaryMapContainer;

        [SerializeField] private GameObject spritePrefab;





        private static PlantManager _instance;
        public static PlantManager Instance { get { return _instance; } }



        public void LoadPlants(MapDataSO map, MapType mapType)
        {

            if (mapType == MapType.main)
            {
                PlantSORefDict = map.PlantRefList.GetDictionary();
            }



            if (mapType == MapType.main)
            {
                //first destroy previous data
                MainMapPlantsDic = new Dictionary<int, SpriteRenderer>();
                if (mainMapContainer != null)
                {
                    Destroy(mainMapContainer);
                }
                createParentFolders();
                for (int i = 0; i < map.PlantItems.Count; i++)
                {
                    AddPlant(map.PlantItems[i], mapType);
                }
            }
            else if (mapType == MapType.secondary)
            {
                SecondaryMapPlantsDic = new Dictionary<int, SpriteRenderer>();
                if (secondaryMapContainer != null)
                {
                    Destroy(secondaryMapContainer);
                }
                createParentFolders();
                for (int i = 0; i < map.PlantItems.Count; i++)
                {
                    AddPlant(map.PlantItems[i], mapType);
                }
            }
            else
            {
                Debug.LogError("Map type not recognized:: " + mapType);
            }
        }

        public void AddPlant(PlantItem p, MapType mapType)
        {
            
            int index;
            GameObject go;
            SpriteRenderer sr;

            index = GridSystem.getIndex(p.pos, mapType);
            Debug.LogWarning($"Adding plant: {p.pos}, {mapType}, Index: {index}");
            go = Instantiate(spritePrefab, getParentFolder(mapType).transform);
            go.transform.position = GridSystem.getWorldPositionCellCenter(p.pos, mapType, 0);
            go.name = PlantSORefDict[p.typeId].name;
            sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = PlantSORefDict[p.typeId].getSpriteLevel(p.level);


            if (mapType == MapType.main)
                MainMapPlantsDic.Add(index, sr);
            else if (mapType == MapType.secondary)
            {
                SecondaryMapPlantsDic.Add(index, sr);
            }
            else
            {
                Debug.LogError("Not Recognized Map Type:: " + mapType);
            }
        }

        public void RemovePlant(int2 pos, MapType mapType)
        {
            int index = GridSystem.getIndex(pos, mapType);
            RemovePlant(index, mapType);
        }


        public void RemovePlant(int index, MapType mapType)
        {

            

            if (mapType == MapType.main)
            {
                Destroy(MainMapPlantsDic[index].gameObject);
                MainMapPlantsDic.Remove(index);
            }
                
            else if (mapType == MapType.secondary)
            {
                Destroy(SecondaryMapPlantsDic[index].gameObject);
                SecondaryMapPlantsDic.Remove(index);
            }
            else
            {
                Debug.LogError("Not Recognized Map Type:: " + mapType);
            }
        }


        public void UpdatePlants(int[] plantKeys, MapType mapType)
        {
            PlantItem p = new PlantItem();
            for(int i = 0; i < plantKeys.Length; i++)
            {
                if (mapType == MapType.main)
                {
                    p = MapPlantManagerSystem.MainMapPlantItems[plantKeys[i]];
                    MainMapPlantsDic[plantKeys[i]].sprite = PlantSORefDict[p.typeId]
                        .getSpriteLevel(p.level);
                }
                else
                {
                    p = MapPlantManagerSystem.SecondaryMapPlantIems[plantKeys[i]];
                    SecondaryMapPlantsDic[plantKeys[i]].sprite = PlantSORefDict[p.typeId]
                        .getSpriteLevel(p.level);
                }
                    


                
                    
            }
        }

        private GameObject getParentFolder(MapType mapType)
        {
            if (mapType == MapType.main)
                return mainMapContainer;
            else if (mapType == MapType.secondary)
                return secondaryMapContainer;
            else
                Debug.LogError("MapType not recognized: " + mapType);

            return null;
        }
        private void createParentFolders()
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

        private void Start()
        {

           

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
