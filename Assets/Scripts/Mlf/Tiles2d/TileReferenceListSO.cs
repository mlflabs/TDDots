using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Mlf.Tiles2d
{
    [Serializable]
    public class TileReference
    {
        public bool isLowerGround;
        public TileBase tile;
        public TileDataSo data;
        //load this underneeth, only if its uper ground tile
        public TileBase lowerGround;
    }

    [CreateAssetMenu(fileName = "TileReferenceList", menuName = "Mlf/2D/Tiles/TileReferenceList")]
    public class TileReferenceListSo : ScriptableObject
    {
        public TileReference[] list;


        public TileDataSo GETData(TileBase tile)
        {
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i].tile == tile) return list[i].data;
            }
            return null;
        }



        public int GETRefIndex(TileBase tile)
        {
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i].tile == tile) return i;
            }
            return -1;
        }

    }
}