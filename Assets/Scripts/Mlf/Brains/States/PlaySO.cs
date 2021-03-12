using UnityEngine;

namespace Mlf.Brains.States
{

    [CreateAssetMenu(fileName = "PlaySO", menuName = "Mlf/Brains/States/PlayStateSO")]
    public class PlaySO : ScriptableObject
    {
        public float playLPS = 0.5f;
        public float startingPlay = 0f;
    }
}