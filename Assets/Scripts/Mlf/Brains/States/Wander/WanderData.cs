using Unity.Entities;

namespace Mlf.Brains.States
{

    public struct WanderData : IComponentData
    {

        public sbyte maxDistance; //it foes from negative this to positive this
        //public float hungerLPS;
        //public float lastCheck; delay to check every sec, or every so many frames
    }

}
