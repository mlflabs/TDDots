using UnityEngine;
using UnityEngine.Serialization;

namespace Mlf.Brains.States
{
    [CreateAssetMenu(fileName = "RestSO", menuName = "Mlf/Brains/States/RestStateSO")]
    public class RestSo : ScriptableObject
    {

        //rest
        [FormerlySerializedAs("restLPS")] public float restLps = 0.4f;
        public float startingRest = 0f;
    }
}
