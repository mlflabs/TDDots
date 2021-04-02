using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Mlf.Map2d
{
    [Serializable]
    public class PlantReference
    {
        public TileBase tile;
        public PlantDataSo data;
    }

    [CreateAssetMenu(fileName = "PlantReferenceList", menuName = "Mlf/2D/PlantReferenceList")]
    public class PlantReferenceListSo : ScriptableObject
    {
        public PlantReference[] list;


        public PlantDataSo GETData(TileBase tile)
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

        public Dictionary<byte, PlantDataSo> GetDictionary()
        {
            var dic = new Dictionary<byte, PlantDataSo>();
            for (int i = 0; i < list.Length; i++)
            {
                if (dic.ContainsKey(list[i].data.typeId))
                    Debug.LogError("Duplicate Plant Reference TypeId: " + list[i].data.typeId);
                dic[list[i].data.typeId] = list[i].data;
            }

            return dic;
        }

    }
}