using Mlf.Map2d;
using System;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Mlf.Inventory
{


    public struct InventoryBufferElement : IBufferElementData
    {
        public int ItemType;
    }


}