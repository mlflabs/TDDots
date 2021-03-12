using Unity.Entities;

namespace Mlf.Brains.States
{
    [GenerateAuthoringComponent]
    public struct HungerData : IComponentData
    {
        public float hungerLPS;
        public float hungerThreshold;
        //public float lastCheck; delay to check every sec, or every so many frames
    }

}
