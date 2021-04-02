using Mlf.Brains.Actions;
using Mlf.Npc;
using Unity.Entities;

namespace Mlf.Brains.States
{
    public struct HungerStateCurrent : IComponentData
    {
        public HungerStates State; //defaults to first enum
        public int ItemPosIndex; // position of item
        public ItemType ItemType;
        public PathData Path;
    }
}
