using Unity.Entities;
using Unity.Mathematics;

namespace Mlf.Brains.States
{
    public struct ThirstStateCurrent : IComponentData
    {
        public ThirstStates state;
        public int3 destination;
    }
}
