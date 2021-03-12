using UnityEngine;

namespace Mlf.Brains.States
{
    [CreateAssetMenu(fileName = "ThirstSO", menuName = "Mlf/Brains/States/ThirstStateSO")]
    public class ThirstSO : ScriptableObject
    {

        public float thirstLPS = 0.5f;
        public float thirstThreshold = 50f;

        public float startingThirst = 0f;
    }

}
