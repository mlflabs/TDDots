using Mlf.Grid2d.Ecs;
using Mlf.Map2d;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Mlf.Map2d
{




    public class BuildingComp : MonoBehaviour
    {
        public BuildingItem item;
        [FormerlySerializedAs("buildingSO")] public BuildingDataSo buildingSo;


        private BuildingStage _buildingStage;
        private GameObject _buildingComp;




        public void Init(
            BuildingItem data, BuildingDataSo so)
        {
            buildingSo = so;
            item = data;

            ResetBuilding();
            SyncPosition();
        }



        private void ResetBuilding()
        {
          
            if (buildingSo == null)
                Debug.LogError("Building Prefab is null");


            if (_buildingComp != null)
                Destroy(_buildingComp);

            _buildingComp = Instantiate(buildingSo.prefab, transform);
            _buildingComp.transform.position += buildingSo.placementOffset;

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

            transform.position = GridSystem.GETWorldPositionCellCenter(
                in item.pos, 0, item.mapType);
            //need to modify position based on size
            /*transform.position += new Vector3(
                (map.cellSize.x * buildingSO.size.x) / 2,
                 buildingSO.prefab.transform.position.y / 2 + map.cellSize.y / 2,
                (map.cellSize.z * buildingSO.size.y) / 2);
            */

        }

        public static BuildingComp PlaceBuilding(
            BuildingDataSo so, BuildingItem data, Transform parent = null)
        {
            var o = new GameObject(so.name);
            o.transform.parent = parent;
            var comp = o.AddComponent<BuildingComp>();
            comp.Init(data, so);
            return comp;
        }
    }
}
