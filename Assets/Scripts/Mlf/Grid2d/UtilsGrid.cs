using Mlf.Grid2d.Ecs;
using Unity.Collections;
using Unity.Mathematics;


namespace Mlf.Grid2d
{
    public static class UtilsGrid
    {


        /*
        public static void GetPositionByIndex(int i, int width, out int x, out int y)
        {
           
            y = Mathf.FloorToInt(i / width);
            x = i % width;
            Debug.Log(x + ":: " + y);
        }

        



        public static int2 GetCurrentMapPos(
            in float2 worldPosition, in float2 cellSize, in float2 originPosition)
        {
            return new int2(
            (int)((worldPosition - originPosition).x / cellSize.x),
            (int)((worldPosition - originPosition).y / cellSize.y));
        }

        public static int2 GetCurrentMapPos(
            in float3 worldPosition, in float2 cellSize, in float2 originPosition)
        {
            float2 wp = ToFloat2(worldPosition);
            return new int2(
            (int)((wp - originPosition).x / cellSize.x),
            (int)((wp - originPosition).y / cellSize.y));
        }



        public static float2 GetMiddleCellWorldCoordinates(
            int2 pos, float2 cellSize)
        {
            return (pos * cellSize) +
                new float2(cellSize.x / 2, cellSize.y / 2);
        }

        */

        public static float2 ToFloat2(float3 value)
        {
            return new float2(value.x, value.y);
        }


        public static bool CanWalk(
            in Cell cell, in NativeHashMap<byte, GroundTypeStruct> groundTypes)
        {
            return groundTypes[cell.tileRefIndex].CanWalk && cell.buildingId == 0;
        }

        public static bool CanWalk(
            in Cell cell, GroundTypeStruct groundType)
        {
            return groundType.CanWalk && cell.buildingId == 0;
        }

        public static bool CanBuild(
            in Cell cell, in NativeHashMap<byte, GroundTypeStruct> groundTypes)
        {
            return groundTypes[cell.tileRefIndex].CanWalk && cell.buildingId == 0;
        }

        public static bool HasWater(in Cell cell, in NativeHashMap<byte, GroundTypeStruct> groundTypes)
        {
            return groundTypes[cell.tileRefIndex].HasFreshWater;

        }


    }
}
