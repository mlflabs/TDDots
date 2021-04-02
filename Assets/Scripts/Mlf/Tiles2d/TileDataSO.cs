using UnityEngine;

namespace Mlf.Tiles2d
{



    [CreateAssetMenu(fileName = "New TileData", menuName = "Mlf/2D/Tiles/TileData")]
    public class TileDataSo : ScriptableObject
    {

        public float walkSpeed = 1;
        public float swimSpeed = 0;

        public bool canBuild = true;
        public bool canGrow = true;

        public bool hasFreshWater;
    }
}