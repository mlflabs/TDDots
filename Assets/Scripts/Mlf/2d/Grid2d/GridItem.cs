using System;
using Unity.Mathematics;


namespace Mlf.Grid2d
{
    [Serializable]
    public struct GridItem
    {
        public int2 pos;
        public int typeId;
        public byte quantity;
        public int currentOwner; //if someone is going to use this....
    }
}
