using Mlf.Grid2d.Ecs;
using Mlf.Map2d;
using Unity.Mathematics;
using UnityEngine;

namespace Mlf.Map2d
{




    public class BuildingComp : MonoBehaviour
    {
        public BuildingItem item;
        public BuildingDataSO buildingSO;


        private BuildingStage buildingStage;
        private GameObject buildingComp;




        public void Init(
            BuildingItem data, BuildingDataSO so)
        {
            buildingSO = so;
            item = data;

            resetBuilding();
            SyncPosition();
        }



        private void resetBuilding()
        {
          
            if (buildingSO == null)
                Debug.LogError("Building Prefab is null");


            if (buildingComp != null)
                Destroy(buildingComp);

            buildingComp = Instantiate(buildingSO.prefab, transform);
            buildingComp.transform.position += buildingSO.placementOffset;

            SyncPosition();           
        }

        
        public void RemoveBuilding()
        {
            Destroy(this);
        }
        /*
        public void MoveBuilding(in int2 pos, MapType mapType)
        {
            GridData.pos = pos;
            SyncPosition(mapType);
        }

        public void RotateBuilding()
        {
            if (GridData.rotation == 3)
                GridData.rotation = 0;
            else
                GridData.rotation++;

        }

        */

        private void SyncPosition()
        {

            transform.position = GridSystem.getWorldPositionCellCenter(
                in item.pos, item.mapType, 0);
            //need to modify position based on size
            /*transform.position += new Vector3(
                (map.cellSize.x * buildingSO.size.x) / 2,
                 buildingSO.prefab.transform.position.y / 2 + map.cellSize.y / 2,
                (map.cellSize.z * buildingSO.size.y) / 2);
            */

        }

        public static BuildingComp PlaceBuilding(
            BuildingDataSO so, BuildingItem data, Transform parent = null)
        {
            var o = new GameObject(so.name);
            o.transform.parent = parent;
            var comp = o.AddComponent<BuildingComp>();
            comp.Init(data, so);
            return comp;
        }
    }
}
