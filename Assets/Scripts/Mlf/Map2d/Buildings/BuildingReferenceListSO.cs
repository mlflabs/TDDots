using UnityEngine;

namespace Mlf.Map2d
{

   
    [CreateAssetMenu(fileName = "Building List SO", menuName = "Mlf/2D/BuildingRefListSO")]
    public class BuildingReferenceListSo : ScriptableObject
    {
        [SerializeField] public BuildingDataSo[] items;
    }
}
