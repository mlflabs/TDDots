using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Mlf.Tiles2d
{
    [Serializable]
    public class TileReference
    {
        public TileBase tile;
        public TileDataSO data;
    }

    [CreateAssetMenu(fileName = "TileReferenceList", menuName = "Mlf/2D/Tiles/TileReferenceList")]
    public class TileReferenceListSO : ScriptableObject
    {
        public TileReference[] list;


        public TileDataSO getData(TileBase tile)
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