using Mlf.Grid2d.Ecs;
using UnityEngine;
using UnityEngine.Serialization;

namespace Mlf.Map2d
{
    public class PlantComp : MonoBehaviour
    {
        public PlantItem item;
        public PlantDataSo plantSo;
        public bool showWorkProgress = false;
        public SpriteRenderer spritePlant;

        public void Init(PlantDataSo so, PlantItem data, MapType mapType)
        {
            plantSo = so;

            //add the spriterendere
            var o = new GameObject("PlantSprite");
            spritePlant = o.AddComponent<SpriteRenderer>(); 
            o.transform.parent = transform;
            o.transform.localPosition = so.placementOffset;
            spritePlant.sortingOrder = PlantManager.Instance.spriteSortingLayer;

            //setup sprite
            UpdatePlant(in data);

            //get world position
            transform.position = GridSystem.GETWorldPositionCellCenter(
                data.Pos, 0, mapType);
        }


        public void UpdatePlant(in PlantItem p)
        {
           item = p; 
           spritePlant.sprite =  plantSo.GETSpriteLevel(item.level);
            //for debug, change name of plant
            this.gameObject.name = $"Plant: {plantSo.name} Level: {item.level.ToString()}";
        }


        public void RemovePlant()
        {
            //Here we can play some sort of animation
            Destroy(this);
        }
        
        

        public static PlantComp PlacePlant(
            PlantDataSo so, PlantItem data, MapType mapType, Transform parent = null)
        {
            var o = new GameObject(so.name);
            o.transform.parent = parent;
            var comp = o.AddComponent<PlantComp>();
            comp.Init(so, data, mapType);
            return comp;
        }
    }
}
