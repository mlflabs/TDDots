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
        public byte buildingId;

        //if its default values, than this cell doesn't exist
        //need some other prop so that the first cell wont default to null
        public bool notNull()
        {
            if (pos.Equals(int2.zero) && tileRefIndex == 0 && walkSpeed == 0)
                return false;
            return true;
        }
    }
}
