using UnityEngine;


namespace Mlf.Map2d

{
    public enum Type
    {
        Plant,
        Food,
        Other
    }

    public enum WorkType : byte
    {
        None,
        Cut,
        Plant,
        Harvest,
        Build
    }

    public enum ToolTypeNeeded
    {
        None,
        Hammer,
        Ax,
        PickAx
    }


    [System.Serializable]
    public class MapItemDetail
    {
        public int id;
        public string name;
        [TextArea(5, 20)] public string description;
        public Sprite icon;
        public GameObject prefab;
        public Type type;
        public WorkType[] workTypes;
        public ToolTypeNeeded[] toolTypes;
        public byte harvestQuality;
        public byte maxQuantity;


    }

    [CreateAssetMenu(fileName = "Item List SO", menuName = "Mlf/Map Items/MapItemRefListSO")]
    public class MapItemReferenceListSO : ScriptableObject
    {

        [SerializeField] public MapItemDetail[] items;

        public MapItemDetail getItem(int id)
        {
            if (items == null)
            {
                Debug.LogError("Items is null");
            }
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].id == id)
                    return items[id];
            }

            //not found, write a warrning
            Debug.LogError($"Reference Item not found, id: {id} ");
            return null;
        }

    }
}