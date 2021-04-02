using Unity.Entities;

namespace Mlf.Brains.States
{
    public struct HungerState : IComponentData
    {
        public float Value;
        public bool SkipNextStateSelection;
        public int LastHungerStateTime;
    }
}
