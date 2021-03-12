using Mlf.Grid2d;
using Mlf.Grid2d.Ecs;
using Mlf.Map2d;
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
        public bool finished;
        public bool success;
        public float moveSpeed;
        public float2 destination;
        public float2 nextDestinationPoint;
        public PathData path;
        public Cell currentCell;



        public void LoadFirstPoint(in GridDataStruct data)
        {
            path.index = 1;
            nextDestinationPoint = data.getWorldPositionCellCenter(in path.point1);

        }

        public void LoadNextPoint(in GridDataStruct data)
        {
            if (path.index == default) path.index = 1;
            else path.index++;
            if (path.index > 5)
            {
                Debug.Log("Path Index Out Of Range");
                return;
            }
            nextDestinationPoint = data.getWorldPositionCellCenter(path.GetCurrentPoint());

        }

        public void setFinished()
        {
            path = new PathData();
            finished = true;
            success = true;
        }

        public void setFailed()
        {
            path = new PathData();
            finished = true;
            success = false;
        }

        public void loadPath(in PathData _path, in int2 _destination, in GridDataStruct data)
        {
            path = _path;
            LoadFirstPoint(in data);
            destination = data.getWorldPositionCellCenter(in _destination);
            finished = false;
        }

        public void updatePath(in PathData _path, in GridDataStruct data)
        {
            path = _path;
            LoadFirstPoint(in data);
            finished = false;
        }

        public void reset()
        {
            finished = false;
            destination = float2.zero;
            nextDestinationPoint = float2.zero;
            path = new PathData();
        }

        public float2 pointToWorldPosition(int point, in GridDataStruct data)
        {
            if (point == 1 && !path.point1.Equals(int2.zero))
                return data.getWorldPositionCellCenter(in path.point1);
            if (point == 2 && !path.point2.Equals(int2.zero))
                return data.getWorldPositionCellCenter(in path.point2);
            if (point == 3 && !path.point3.Equals(int2.zero))
                return data.getWorldPositionCellCenter(in path.point3);
            if (point == 4 && !path.point4.Equals(int2.zero))
                return data.getWorldPositionCellCenter(in path.point4);
            if (point == 5 && !path.point5.Equals(int2.zero))
                return data.getWorldPositionCellCenter(in path.point5);
            return float2.zero;
        }

        public float3 pointToWorldPosition(int point, in GridDataStruct data, float z = 0f)
        {
            if (point == 1 && !path.point1.Equals(int2.zero))
                return data.getWorldPositionCellCenter(in path.point1, z);
            if (point == 2 && !path.point2.Equals(int2.zero))
                return data.getWorldPositionCellCenter(in path.point2, z);
            if (point == 3 && !path.point3.Equals(int2.zero))
                return data.getWorldPositionCellCenter(in path.point3, z);
            if (point == 4 && !path.point4.Equals(int2.zero))
                return data.getWorldPositionCellCenter(in path.point4, z);
            if (point == 5 && !path.point5.Equals(int2.zero))
                return data.getWorldPositionCellCenter(in path.point5, z);
            return float3.zero;
        }

    }



    public struct PathData
    {
        public sbyte index;
        public sbyte length;
        public int2 point1;
        public int2 point2;
        public int2 point3;
        public int2 point4;
        public int2 point5;

        public bool hasPath()
        {
            //in path all points should be different, so if same, they are default values
            return (!point1.Equals(point2));
        }


        public int2 GetCurrentPoint()
        {
            if (index == default)
                index = 1;

            if (index == 1) return point1;
            if (index == 2) return point2;
            if (index == 3) return point3;
            if (index == 4) return point4;
            return point5;

        }

        public void resetPathToOnePoint(int point)
        {
            if (point == 2) { point1 = point2; index = 1; }
            else if (point == 3) { point1 = point3; index = 1; }
            else if (point == 4) { point1 = point4; index = 1; }
            else if (point == 5) { point1 = point5; index = 1; }
        }

        public void setByIndex(int i, int2 value)
        {
            if (i == 1) point1 = value;
            if (i == 2) point2 = value;
            if (i == 3) point3 = value;
            if (i == 4) point4 = value;
            if (i == 5) point5 = value;


        }
    }






    class MoveActionSystem : SystemBase
    {

        //[ReadOnly]
        //public NativeArray<Cell> Grid = GridSystem.gridMap;

        protected override void OnUpdate()
        {
            //TODO here we could just use a tag to see if array updated, timestamp
            NativeArray<GridDataStruct> maps = GridSystem.Maps;

            //grid = Grid;
            //var gridSize = GridSystem.gridMapSize;
            float deltaTime = Time.DeltaTime;
            float distanceBuffer = 0.2f;

            List<MapComponentShared> mapIds = new List<MapComponentShared>();
            var groundTypeReferences = GridSystem.GroundTypeReferences;
            EntityManager.GetAllUniqueSharedComponentData<MapComponentShared>(mapIds);


            for (int m = 0; m < mapIds.Count; m++)
            {
                //int mapId = mapIds[m].mapId;
                GridDataStruct map = maps[mapIds[m].mapId];

                Dependency = Entities
                .WithName("MoveActionSystem")
                //.WithReadOnly(maps)
                .WithSharedComponentFilter(mapIds[m])
                .ForEach((Entity entity,
                            ref MoveActionData moveActionData,
                            ref Translation translation,
                            ref Rotation rotation) =>
                {

                    //Debug.Log($"DISTANCE:: {translation.Value}: {math.distance(moveActionData.nextDestinationPoint, translation.Value)}, {math.distance(moveActionData.destination, translation.Value)}");


                    //are we moving
                    if (moveActionData.finished || !moveActionData.path.hasPath())
                    {

                        //not moving, finsiehd, no action required
                        return;
                    }
                    else if (moveActionData.nextDestinationPoint.Equals(float2.zero))
                    {
                        //doesn't seem like there could be a path with 0,0,0 mid point......
                        //Debug.Log("We have a zero destination, distance:: ");
                        //UnityEngine.Debug.Log(string.Format(math.distance(moveActionData.destination, translation.Value)));
                        //UnityEngine.Debug.Log(string.Format(math.distancesq(moveActionData.destination, translation.Value)));
                        //GridData gridData = getCurrentGridData(locationData.gridId, maps);
                        //if its zero, most likely we are very close to destination, just calculate the final distance
                        //we are close enought, just move to destination
                        moveActionData.nextDestinationPoint = moveActionData.destination;



                    }
                    else
                    {
                        //we are moving
                        //are we on the same cell as destination
                        int2 gridPosition = map.getGridPosition(in translation.Value);
                        Cell cell = map.getCell(in gridPosition);

                        //======================== use cell to check position, and change speed.
                        //======================== also if we changed cells, and height is different, 
                        //======================== slow down based on height difference

                        /*Debug.Log($"Points::: {moveActionData.nextDestinationPoint}" +
                            $" {UtilsGrid.ToFloat2(translation.Value)} {translation.Value}");

                        Debug.Log($"Checking distance to next point:::: {distanceBuffer} " +
                            $"{math.distance(moveActionData.nextDestinationPoint, UtilsGrid.ToFloat2(translation.Value))}");
                        */
                        if (math.distance(moveActionData.nextDestinationPoint,
                                          UtilsGrid.ToFloat2(translation.Value)) < distanceBuffer)
                        {


                            Debug.Log($"Walked close to point: ARE WE THERE {translation.Value} {moveActionData.destination}");
                            //Debug.Log($"Translation.value:: {translation.Value}, {moveActionData.nextDestinationPoint} ");
                            //Debug.Log($"Index:: {moveActionData.path.index}");
                            //Debug.Log($"Going to next Point:: {moveActionData.path.GetCurrentPoint().x}, {moveActionData.path.GetCurrentPoint().y}");
                            if (math.distance(moveActionData.destination,
                                                UtilsGrid.ToFloat2(translation.Value)) < distanceBuffer)
                            {
                                //Debug.Log($"FINISHED!!!!!!!!!!!{moveActionData.destination.x}, {moveActionData.destination.y}");
                                //Debug.Log($" Translation:: {translation.Value.x}, {translation.Value.z} ");
                                //not moving
                                //finsih it
                                Debug.Log("Reached destination, finished");
                                moveActionData.setFinished();
                                return;
                            }
                            else
                            {
                                Debug.Log($"Loading next point: {moveActionData.path.index} {moveActionData.path.GetCurrentPoint()}");
                                moveActionData.LoadNextPoint(map);
                                Debug.Log($"Loaded point: {moveActionData.path.index}, {moveActionData.path.GetCurrentPoint()}");
                                //Debug.Log($"Loaded next Point:: {moveActionData.path.index}, {moveActionData.path.GetCurrentPoint()}");
                                return;
                            }
                        }
                        else
                        {

                            int2 pos = map.getGridPosition(in translation.Value);
                            //Debug.Log($"Current Pos:: {pos}");
                            if (moveActionData.path.index > 3)
                            {

                                int2 end = map.getGridPosition(in moveActionData.destination);


                                Debug.Log($"MoveAction point and dest:: {pos}, {end}");

                                PathData path = UtilsPath.findPath(in pos, in end,
                                    in groundTypeReferences, in map);
                                Debug.Log($"Path 1: {path.point1}, {translation.Value}");

                                moveActionData.updatePath(in path, in map);
                                //Debug.Log($"MoveActionSystem->New Path::: {path}");
                                if (moveActionData.path.hasPath())
                                {
                                    moveActionData.LoadFirstPoint(in map);
                                }
                                else
                                {
                                    Debug.Log("Couldn't reach destination, failed");
                                    moveActionData.setFailed();
                                }
                            }
                            else
                            {
                                float3 dest = new float3(moveActionData.nextDestinationPoint.x,
                                                         moveActionData.nextDestinationPoint.y,
                                                         translation.Value.z);
                                float3 moveDir = math.normalizesafe(dest - translation.Value);
                                //rotation.Value = quaternion.LookRotation(moveDir, new float2(0, 0));
                                //quaternion rot = quaternion.LookRotationSafe(moveActionData.nextDestinationPoint(),
                                //    new float3(0, 1f, 0));
                                //float rotSpeed = deltaTime * moveActionData.moveSpeed;
                                //quaternion smoothRot = math.slerp(rotation.Value, rot, rotSpeed);
                                //Debug.Log($"Move Data ADDED: {moveDir * moveActionData.moveSpeed * deltaTime}:::: speed:: {moveActionData.moveSpeed*deltaTime}");
                                translation.Value += moveDir * moveActionData.moveSpeed * deltaTime;
                                //If draw lines, 
                                Debug.DrawLine(translation.Value, dest);
                                //Debug.Log($"Moving::: {translation.Value}");
                                //draw path lines
                                ////////////////////////Debug.DrawLine(translation.Value, moveActionData.nextDestinationPoint);
                                for (int i = 2; i < 5; i++)
                                {
                                    //Debug.Log($"{moveActionData.pointToWorldPosition(i, maps[mapId].cellSize)}");
                                    if (moveActionData.pointToWorldPosition(i + 1, in map).Equals(float2.zero))
                                        break;

                                    Debug.DrawLine(
                                        moveActionData.pointToWorldPosition(i, in map, 0),
                                        moveActionData.pointToWorldPosition(i + 1, in map, 0));
                                }


                            }
                        }

                    }



                    //}).Run();
                }).Schedule(Dependency);

            }
        }





    }
}
