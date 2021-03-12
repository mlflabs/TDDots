using Unity.Entities;

namespace Mlf.Brains.States
{
    public struct HungerState : IComponentData
    {
        public float value;
        public bool skipNextStateSelection;
        public int lastHungerStateTime;
    }
}
