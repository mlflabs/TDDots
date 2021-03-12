using UnityEngine;

namespace Mlf.Brains.States
{
    [CreateAssetMenu(fileName = "RestSO", menuName = "Mlf/Brains/States/RestStateSO")]
    public class RestSO : ScriptableObject
    {

        //rest
        public float restLPS = 0.4f;
        public float startingRest = 0f;
    }
}
