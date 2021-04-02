using Unity.Entities;

namespace Mlf.Brains.States
{

    public struct ThirstData : IComponentData
    {
        public float ThirstLps;
        public float ThirstThreshold;
        //public float lastCheck; delay to check every sec, or every so many frames
    }

}
