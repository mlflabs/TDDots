using Unity.Entities;

namespace Mlf.Brains.States
{
    [GenerateAuthoringComponent]
    public struct HungerData : IComponentData
    {
        public float HungerLps;
        public float HungerThreshold;
        //public float lastCheck; delay to check every sec, or every so many frames
    }

}
