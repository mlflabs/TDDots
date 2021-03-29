using UnityEngine;

namespace Mlf.Map2d
{

   
    [CreateAssetMenu(fileName = "Building List SO", menuName = "Mlf/2D/BuildingRefListSO")]
    public class BuildingReferenceListSO : ScriptableObject
    {
        [SerializeField] public BuildingDataSO[] items;
    }
}
