using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Mlf.Map2d
{
    [Serializable]
    public class PlantReference
    {
        public TileBase tile;
        public PlantDataSO data;
    }

    [CreateAssetMenu(fileName = "PlantReferenceList", menuName = "Mlf/2D/PlantReferenceList")]
    public class PlantReferenceListSO : ScriptableObject
    {
        public PlantReference[] list;


        public PlantDataSO getData(TileBase tile)
        {
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i].tile == tile) return list[i].data;
            }
            return null;
        }



        public int getRefIndex(TileBase tile)
        {
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i].tile == tile) return i;
            }
            return -1;
        }

    }
}