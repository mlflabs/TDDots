using Unity.Entities;

namespace Mlf.Brains.States
{
    public struct HungerScore : IComponentData
    {
        public float value;

        public float getValue()
        {
            return value;
        }

    }
}
