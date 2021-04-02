using UnityEngine;
using UnityEngine.Serialization;

namespace Mlf.Brains.States
{
    [CreateAssetMenu(fileName = "ThirstSO", menuName = "Mlf/Brains/States/ThirstStateSO")]
    public class ThirstSo : ScriptableObject
    {

        [FormerlySerializedAs("thirstLPS")] public float thirstLps = 0.5f;
        public float thirstThreshold = 50f;

        public float startingThirst = 0f;
    }

}
