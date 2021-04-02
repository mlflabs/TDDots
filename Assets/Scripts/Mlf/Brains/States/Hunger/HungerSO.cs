using UnityEngine;
using UnityEngine.Serialization;

namespace Mlf.Brains.States
{
    [CreateAssetMenu(fileName = "HungerSO", menuName = "Mlf/Brains/States/HungerStateSO")]
    public class HungerSo : ScriptableObject
    {

        [FormerlySerializedAs("hungerLPS")] public float hungerLps = 0.5f;
        public float hungerThreshold = 50f;

        public float startingHunger = 0f;
    }

}
