using Mlf.Brains.Actions;
using Unity.Entities;

namespace Mlf.Brains.States
{
    public struct HungerStateCurrent : IComponentData
    {
        public HungerStates state; //defaults to first enum
        public int itemId;
        public PathData path;
    }
}
