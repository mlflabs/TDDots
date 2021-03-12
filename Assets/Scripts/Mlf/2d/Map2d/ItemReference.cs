using System.Runtime.InteropServices;

namespace Mlf.Map2d
{

    [StructLayout(LayoutKind.Explicit)]
    public struct WorkTypesStruct
    {
        [FieldOffset(0)]
        public byte work1;
        [FieldOffset(1)]
        public byte work2;
        [FieldOffset(2)]
        public byte work3;
        [FieldOffset(3)]
        public byte work4;

        [FieldOffset(0)]
        public int integer;

    }

    public struct ItemReference
    {
        public int id;
        //public string name;
        //public string description;
        public Type type;
        public WorkTypesStruct workTypes;

        public byte harvestQuality;
        public byte maxQuantity;

        public void setWorkTypes(WorkType[] types)
        {
            if (types.Length > 1)
                workTypes.work1 = (byte)types[0];
            if (types.Length > 2)
                workTypes.work2 = (byte)types[1];
            if (types.Length > 3)
                workTypes.work3 = (byte)types[2];
            if (types.Length > 4)
                workTypes.work4 = (byte)types[3];
        }

        public bool hasWorkType(WorkType type)
        {
            if (workTypes.work1 == (byte)type) return true;
            if (workTypes.work2 == (byte)type) return true;
            if (workTypes.work3 == (byte)type) return true;
            if (workTypes.work4 == (byte)type) return true;
            return false;
        }

        //public getWorkTypes()
        //{
        //	byte[] buffer = BitConverter.GetBytes(i);
        //}

    }
}
