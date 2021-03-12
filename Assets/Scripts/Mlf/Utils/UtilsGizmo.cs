using Mlf.Utils;
using UnityEngine;

public class UtilsGizmo
{

    public static void DrawThickLine(Vector3 start, Vector3 end, int lineCount = 1)
    {

        Vector3 offset = new Vector3(0.01f, 0.01f, 0.01f);
        for (int i = 0; i < lineCount; i++)
        {
            Gizmos.DrawLine(start + (offset * i), end + (offset * i));
        }

    }

    public static void DrawGridOutline(Vector3 startLocation,
                                       int widthCellCount,
                                       int heightCellCount,
                                       float cellWidth,
                                       float cellHeight,
                                       float gridHeight)
    {
        Vector3 start;
        Vector3 end;
        for (int x = 0; x < widthCellCount; x++)
        {
            start = startLocation + new Vector3(x * cellWidth, gridHeight, 0);
            end = startLocation + new Vector3(x * cellWidth,
                    gridHeight, cellHeight * heightCellCount);
            DrawThickLine(start, end, 1);
        }
        for (int y = 0; y < widthCellCount; y++)
        {
            start = startLocation + new Vector3(0, gridHeight, y * cellHeight);
            end = startLocation + new Vector3(cellWidth * widthCellCount,
                    gridHeight, y * cellHeight);
            DrawThickLine(start, end, 1);
        }
    }

    public static void DrawBox(Vector3 center, Vector3 size)
    {

        Gizmos.DrawCube(center, size);
    }

    public static void DrawBoxWithText(Vector3 center,
                                        Vector3 size,
                                        string text)
    {
        DrawBox(center, size);
        UtilsText.CreateWorldText(text, null, center, 10);
    }






}
