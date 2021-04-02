using System.Runtime.InteropServices;

namespace Mlf.Map2d
{

    [StructLayout(LayoutKind.Explicit)]
    public struct WorkTypesStruct
    {
        [FieldOffset(0)]
        public byte Work1;
        [FieldOffset(1)]
        public byte Work2;
        [FieldOffset(2)]
        public byte Work3;
        [FieldOffset(3)]
        public byte Work4;

        [FieldOffset(0)]
        private readonly int integer;

    }

    public struct ItemReference
    {
        public int ID;
        //public string name;
        //public string description;
        public ItemType Type;
        public WorkTypesStruct WorkTypes;

        public byte HarvestQuality;
        public byte MaxQuantity;

        public void SetWorkTypes(WorkType[] types)
        {
            if (types.Length > 1)
                WorkTypes.Work1 = (byte)types[0];
            if (types.Length > 2)
                WorkTypes.Work2 = (byte)types[1];
            if (types.Length > 3)
                WorkTypes.Work3 = (byte)types[2];
            if (types.Length > 4)
                WorkTypes.Work4 = (byte)types[3];
        }

        public bool HasWorkType(WorkType type)
        {
            if (WorkTypes.Work1 == (byte)type) return true;
            if (WorkTypes.Work2 == (byte)type) return true;
            if (WorkTypes.Work3 == (byte)type) return true;
            if (WorkTypes.Work4 == (byte)type) return true;
            return false;
        }

        //public getWorkTypes()
        //{
        //	byte[] buffer = BitConverter.GetBytes(i);
        //}

    }
}
