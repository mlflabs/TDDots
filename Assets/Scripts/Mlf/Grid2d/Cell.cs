using Mlf.Tiles2d;
using System;
using Unity.Mathematics;


namespace Mlf.Grid2d
{



    [Serializable]
    public struct Cell
    {
        //y here represents height
        public int2 pos;
        public byte tileRefIndex;
        public float walkSpeed;

        //public float speed;//speed cost multipler
        //public byte groundType;
        public bool canGrow;
        public bool canBuild;
        //public bool canSwim;
        public byte buildingId;

        //if its default values, than this cell doesn't exist
        //need some other prop so that the first cell wont default to null
        public bool IsDefault()
        {
            if (pos.Equals(int2.zero) && tileRefIndex == 0 && walkSpeed == 0)
                return true;
            return false;
        }

        public string Print()
        {
            return $"Cell, pos: {pos}, walkSpeed: {walkSpeed}, canGrow: {canGrow}, " +
                $"canBuild: {canBuild}, buildingId: {buildingId}";
        }

        public static Cell FromTileDataSo(TileDataSo so, int2 pos, byte tileRefIndex)
        {
            return new Cell
            {
                pos= pos,
                tileRefIndex = tileRefIndex,
                walkSpeed = so.walkSpeed,
                canGrow = so.canGrow,      
                canBuild = so.canBuild
            };
        }
    }
}
