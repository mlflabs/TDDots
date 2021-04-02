using UnityEngine;
using UnityEngine.Serialization;

namespace Mlf.Brains.States
{

    [CreateAssetMenu(fileName = "PlaySO", menuName = "Mlf/Brains/States/PlayStateSO")]
    public class PlaySo : ScriptableObject
    {
        [FormerlySerializedAs("playLPS")] public float playLps = 0.5f;
        public float startingPlay = 0f;
    }
}