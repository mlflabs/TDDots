using Unity.Entities;

namespace Mlf.Brains.States
{
    public struct HungerScore : IComponentData
    {
        public float Value;

        public float GETValue()
        {
            return Value;
        }

    }
}
