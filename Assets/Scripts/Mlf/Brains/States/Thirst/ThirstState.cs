using Unity.Entities;

namespace Mlf.Brains.States
{
    public struct ThirstState : IComponentData
    {
        public float Value;
        public bool SkipNextStateSelection;
        public int LastHungerStateTime;
    }
}
