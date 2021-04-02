using Unity.Entities;
using Unity.Mathematics;

namespace Mlf.Brains.States
{
    public struct ThirstStateCurrent : IComponentData
    {
        public ThirstStates State;
        public int3 Destination;
    }
}
