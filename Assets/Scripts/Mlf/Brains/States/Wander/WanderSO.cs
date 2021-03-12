using UnityEngine;

namespace Mlf.Brains.States
{
    [CreateAssetMenu(fileName = "WanderSO", menuName = "Mlf/Brains/States/WanderStateSO")]
    public class WanderSO : ScriptableObject
    {

        //hungerData LPS LossPerSecond
        public float wanderDefaultScore = 50f;
    }

}
