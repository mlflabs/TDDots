using Mlf.Grid2d;
using Mlf.Grid2d.Ecs;
using Mlf.Map2d;
using Mlf.MyTime;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Mlf.Brains.Actions
{

    public struct MoveActionData : IComponentData
    {
        public bool Finished;
        public bool Success;
        public float MoveSpeed;
        public float2 Destination;
        public float2 NextDestinationPoint;
        public PathData Path;
        public Cell CurrentCell;



        public void LoadFirstPoint(in GridDataStruct data)
        {
            Path.Index = 1;
            NextDestinationPoint = data.GETWorldPositionCellCenter(in Path.Point1);

        }

        public void LoadNextPoint(in GridDataStruct data)
        {
            if (Path.Index == default) Path.Index = 1;
            else Path.Index++;
            if (Path.Index > 5)
            {
                Debug.Log("Path Index Out Of Range");
                return;
            }
            NextDestinationPoint = data.GETWorldPositionCellCenter(Path.GetCurrentPoint());

        }

        public void SetFinished()
        {
            Path = new PathData();
            Finished = true;
            Success = true;
        }

        public void SetFailed()
        {
            Path = new PathData();
            Finished = true;
            Success = false;
        }

        public void LoadPath(in PathData path, in GridDataStruct data)
        {
            Path = path;
            LoadFirstPoint(in data);
            Destination = data.GETWorldPositionCellCenter(in Path.FinalDestination);
            Finished = false;
        }

        public void UpdatePath(in PathData path, in GridDataStruct data)
        {
            Path = path;
            LoadFirstPoint(in data);
            Finished = false;
        }

        public void Reset()
        {
            Finished = false;
            Destination = float2.zero;
            NextDestinationPoint = float2.zero;
            Path = new PathData();
        }

        public float2 PointToWorldPosition(int point, in GridDataStruct data)
        {
            if (point == 1 && !Path.Point1.Equals(int2.zero))
                return data.GETWorldPositionCellCenter(in Path.Point1);
            if (point == 2 && !Path.Point2.Equals(int2.zero))
                return data.GETWorldPositionCellCenter(in Path.Point2);
            if (point == 3 && !Path.Point3.Equals(int2.zero))
                return data.GETWorldPositionCellCenter(in Path.Point3);
            if (point == 4 && !Path.Point4.Equals(int2.zero))
                return data.GETWorldPositionCellCenter(in Path.Point4);
            if (point == 5 && !Path.Point5.Equals(int2.zero))
                return data.GETWorldPositionCellCenter(in Path.Point5);
            return float2.zero;
        }

        public float3 PointToWorldPosition(int point, in GridDataStruct data, float z = 0f)
        {
            if (point == 1 && !Path.Point1.Equals(int2.zero))
                return data.GETWorldPositionCellCenter(in Path.Point1, z);
            if (point == 2 && !Path.Point2.Equals(int2.zero))
                return data.GETWorldPositionCellCenter(in Path.Point2, z);
            if (point == 3 && !Path.Point3.Equals(int2.zero))
                return data.GETWorldPositionCellCenter(in Path.Point3, z);
            if (point == 4 && !Path.Point4.Equals(int2.zero))
                return data.GETWorldPositionCellCenter(in Path.Point4, z);
            if (point == 5 && !Path.Point5.Equals(int2.zero))
                return data.GETWorldPositionCellCenter(in Path.Point5, z);
            return float3.zero;
        }

    }



    public struct PathData
    {
        public sbyte Index;
        public sbyte Length;
        public int2 FinalDestination;
        public int2 Point1;
        public int2 Point2;
        public int2 Point3;
        public int2 Point4;
        public int2 Point5;

        public bool HasPath()
        {
            //in path all points should be different, so if same, they are default values
            return (!Point1.Equals(Point2));
        }


        public int2 GetCurrentPoint()
        {
            if (Index == default)
                Index = 1;

            if (Index == 1) return Point1;
            if (Index == 2) return Point2;
            if (Index == 3) return Point3;
            if (Index == 4) return Point4;
            return Point5;

        }

        public void resetPathToOnePoint(int point)
        {
            if (point == 2) { Point1 = Point2; Index = 1; }
            else if (point == 3) { Point1 = Point3; Index = 1; }
            else if (point == 4) { Point1 = Point4; Index = 1; }
            else if (point == 5) { Point1 = Point5; Index = 1; }
        }

        public void SetByIndex(int i, int2 value)
        {
            if (i == 1) Point1 = value;
            if (i == 2) Point2 = value;
            if (i == 3) Point3 = value;
            if (i == 4) Point4 = value;
            if (i == 5) Point5 = value;


        }
    }






    class MoveActionSystem : SystemBase
    {

        //[ReadOnly]
        //public NativeArray<Cell> Grid = GridSystem.gridMap;

        protected override void OnUpdate()
        {
            //Debug.Log("Move000000000000000000000");
            float deltaTime = TimeSystem.DeltaTime; // Time.DeltaTime;
            float distanceBuffer = 0.2f;

            List<MapComponentShared> mapIds = new List<MapComponentShared>();
            //var groundTypeReferences = GridSystem.GroundTypeReferences;
            EntityManager.GetAllUniqueSharedComponentData<MapComponentShared>(mapIds);


            for (int m = 0; m < mapIds.Count; m++)
            {
                GridDataStruct map;

                NativeArray<Cell> cells;

                if (mapIds[m].type == MapType.main)
                {
                    map = GridSystem.MainMap;
                    cells = GridSystem.MainMapCells;
                }
                else
                {
                    map = GridSystem.SecondaryMap;
                    cells = GridSystem.SecondaryMapCells;
                }

                Entities
                .WithName("MoveActionSystem")
                //.WithReadOnly(maps)
                .WithReadOnly(cells)
                .WithSharedComponentFilter(mapIds[m])
                .ForEach((Entity entity,
                            ref MoveActionData moveActionData,
                            ref Translation translation,
                            ref Rotation rotation) =>
                {

                    //Debug.Log($"DISTANCE:: {translation.Value}: {math.distance(moveActionData.nextDestinationPoint, translation.Value)}, {math.distance(moveActionData.destination, translation.Value)}");


                    //are we moving
                    if (moveActionData.Finished || !moveActionData.Path.HasPath())
                    {
                        //Debug.Log("Move1");
                        //not moving, finsiehd, no action required
                        return;
                    }
                    else if (moveActionData.NextDestinationPoint.Equals(float2.zero))
                    {
                        //Debug.Log("Move2");
                        //doesn't seem like there could be a path with 0,0,0 mid point......
                        //Debug.Log("We have a zero destination, distance:: ");
                        //UnityEngine.Debug.Log(string.Format(math.distance(moveActionData.destination, translation.Value)));
                        //UnityEngine.Debug.Log(string.Format(math.distancesq(moveActionData.destination, translation.Value)));
                        //GridData gridData = getCurrentGridData(locationData.gridId, maps);
                        //if its zero, most likely we are very close to destination, just calculate the final distance
                        //we are close enought, just move to destination
                        moveActionData.NextDestinationPoint = moveActionData.Destination;



                    }
                    else
                    {
                        //Debug.Log("Move3");
                        //we are moving
                        //are we on the same cell as destination
                        int2 gridPosition = map.GETGridPosition(in translation.Value);
                        Cell cell = map.GETCell(in gridPosition, in cells);

                        //======================== use cell to check position, and change speed.
                        //======================== also if we changed cells, and height is different, 
                        //======================== slow down based on height difference

                        /*Debug.Log($"Points::: {moveActionData.nextDestinationPoint}" +
                            $" {UtilsGrid.ToFloat2(translation.Value)} {translation.Value}");

                        Debug.Log($"Checking distance to next point:::: {distanceBuffer} " +
                            $"{math.distance(moveActionData.nextDestinationPoint, UtilsGrid.ToFloat2(translation.Value))}");
                        */
                        if (math.distance(moveActionData.NextDestinationPoint,
                                          UtilsGrid.ToFloat2(translation.Value)) < distanceBuffer)
                        {


                            //Debug.Log($"Walked close to point: ARE WE THERE {translation.Value} {moveActionData.destination}");
                            //Debug.Log($"Translation.value:: {translation.Value}, {moveActionData.nextDestinationPoint} ");
                            //Debug.Log($"Index:: {moveActionData.path.index}");
                            //Debug.Log($"Going to next Point:: {moveActionData.path.GetCurrentPoint().x}, {moveActionData.path.GetCurrentPoint().y}");
                            if (math.distance(moveActionData.Destination,
                                                UtilsGrid.ToFloat2(translation.Value)) < distanceBuffer)
                            {
                                //Debug.Log($"FINISHED!!!!!!!!!!!{moveActionData.destination.x}, {moveActionData.destination.y}");
                                //Debug.Log($" Translation:: {translation.Value.x}, {translation.Value.z} ");
                                //not moving
                                //finsih it
                                //Debug.Log("Reached destination, finished");
                                moveActionData.SetFinished();
                                return;
                            }
                            else
                            {
                                //Debug.Log($"Loading next point: {moveActionData.path.index} {moveActionData.path.GetCurrentPoint()}");
                                moveActionData.LoadNextPoint(map);
                                //Debug.Log($"Loaded point: {moveActionData.path.index}, {moveActionData.path.GetCurrentPoint()}");
                                //Debug.Log($"Loaded next Point:: {moveActionData.path.index}, {moveActionData.path.GetCurrentPoint()}");
                                return;
                            }
                        }
                        else
                        {

                            int2 pos = map.GETGridPosition(in translation.Value);
                            //Debug.Log($"Current Pos:: {pos}");
                            if (moveActionData.Path.Index > 3)
                            {

                                int2 end = map.GETGridPosition(in moveActionData.Destination);


                                //Debug.Log($"MoveAction point and dest:: {pos}, {end}");

                                PathData path = UtilsPath.FindPath(
                                    in pos, 
                                    in end,
                                    //in groundTypeReferences, 
                                    in cells, 
                                    in map);
                                //Debug.Log($"Path 1: {path.point1}, {translation.Value}");

                                moveActionData.UpdatePath(in path, in map);
                                //Debug.Log($"MoveActionSystem->New Path::: {path}");
                                if (moveActionData.Path.HasPath())
                                {
                                    moveActionData.LoadFirstPoint(in map);
                                }
                                else
                                {
                                    Debug.Log("Couldn't reach destination, failed");
                                    moveActionData.SetFailed();
                                }
                            }
                            else
                            {
                                float3 dest = new float3(moveActionData.NextDestinationPoint.x,
                                                         moveActionData.NextDestinationPoint.y,
                                                         translation.Value.z);
                                float3 moveDir = math.normalizesafe(dest - translation.Value);
                                //rotation.Value = quaternion.LookRotation(moveDir, new float2(0, 0));
                                //quaternion rot = quaternion.LookRotationSafe(moveActionData.nextDestinationPoint(),
                                //    new float3(0, 1f, 0));
                                //float rotSpeed = deltaTime * moveActionData.moveSpeed;
                                //quaternion smoothRot = math.slerp(rotation.Value, rot, rotSpeed);
                                //Debug.Log($"Move Data ADDED: {moveDir * moveActionData.moveSpeed * deltaTime}:::: speed:: {moveActionData.moveSpeed*deltaTime}");
                                translation.Value += moveDir * moveActionData.MoveSpeed * deltaTime;
                                //If draw lines, 
                                Debug.DrawLine(translation.Value, dest);
                                //Debug.Log($"Moving::: {translation.Value}");
                                //draw path lines
                                ////////////////////////Debug.DrawLine(translation.Value, moveActionData.nextDestinationPoint);
                                for (int i = 2; i < 5; i++)
                                {
                                    //Debug.Log($"{moveActionData.pointToWorldPosition(i, maps[mapId].cellSize)}");
                                    if (moveActionData.PointToWorldPosition(i + 1, in map).Equals(float2.zero))
                                        break;

                                    Debug.DrawLine(
                                        moveActionData.PointToWorldPosition(i, in map, 0),
                                        moveActionData.PointToWorldPosition(i + 1, in map, 0));
                                }


                            }
                        }

                    }



                    //}).Run();
                }).Schedule();

            }
        }





    }
}
