using UnityEngine;

namespace Mlf.Brains.States
{
    [CreateAssetMenu(fileName = "HungerSO", menuName = "Mlf/Brains/States/HungerStateSO")]
    public class HungerSO : ScriptableObject
    {

        public float hungerLPS = 0.5f;
        public float hungerThreshold = 50f;

        public float startingHunger = 0f;
    }

}
