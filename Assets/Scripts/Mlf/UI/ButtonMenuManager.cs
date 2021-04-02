using UnityEngine;


namespace Mlf.UI
{
    public class ButtonMenuManager : MonoBehaviour
    {
        [Header("Panels")]
        [Tooltip("First Panel is main panel")]
        [SerializeField] public GameObject[] panels;



        public void SetActivePanel(int i = 0) // Make first the default
        {
            for (int x = 0; x < panels.Length; x++)
            {
                panels[x].SetActive(false);
            }

            if (i < panels.Length && i > -1)
                panels[i].SetActive(true);
            else
                Debug.LogError($"Panel with id not found:: {i}");
        }

        private void Start()
        {
            SetActivePanel(0);
        }

    }

}
