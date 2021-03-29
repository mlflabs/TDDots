using Mlf.Brains.Actions;
using Mlf.Npc;
using Unity.Entities;

namespace Mlf.Brains.States
{
    public struct HungerStateCurrent : IComponentData
    {
        public HungerStates state; //defaults to first enum
        public int itemPosIndex; // position of item
        public ItemType itemType;
        public PathData path;
    }
}
