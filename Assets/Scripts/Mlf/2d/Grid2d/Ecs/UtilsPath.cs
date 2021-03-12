using Mlf.Brains.Actions;
using Mlf.Map2d;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Mlf.Grid2d.Ecs
{

    public static class UtilsPath
    {

        private const int MOVE_STRAIGHT_COST = 10;
        private const int MOVE_DIAGONAL_COST = 14;

        public struct NodeDetails
        {
            public int index;
            public byte step; //how many steps to here
            public byte quality; //how valuable this destination is
        }

        public struct Node
        {
            public int2 pos;
            public int index;
            public float speed;
            public int gCost { get; set; } // cost - from start
            public int hCost { get; set; } //quick calculation,  - to destination
            public int fCost { get; set; } //combination of g and h cost
            public int cameFromNodeIndex { get; set; }

            public bool canMove { get { return (speed != 0); } }


            public Node(int2 _pos, int _index, float _speed)
            {
                pos = _pos;
                index = _index;
                speed = _speed;
                gCost = int.MaxValue;
                hCost = 0;
                fCost = 0;
                cameFromNodeIndex = -1;
            }

            public Node(in Cell cell, int _index)
            {
                pos = cell.pos;
                index = _index;
                speed = cell.walkSpeed;
                gCost = int.MaxValue;
                hCost = 0;
                fCost = 0;
                cameFromNodeIndex = -1;
            }

            //see if we are straight, up,down, or are we diagnol
            public void CalculateGCost(in Node prevNode)
            {
                //TODO
                //Here we can also put into consideration, speed boost
                if (pos.x == prevNode.pos.x || pos.y == prevNode.pos.y)
                {
                    gCost = prevNode.gCost + MOVE_STRAIGHT_COST;
                }
                else
                {
                    gCost = prevNode.gCost + MOVE_DIAGONAL_COST;
                }
            }

            //int2 y, is int3 z //2d to 3d graph
            public void CalculateHCost(in int2 endPos)
            {
                int xDistance = math.abs(pos.x - endPos.x);
                int yDistance = math.abs(pos.y - endPos.y);
                int remaining = math.abs(xDistance - yDistance);
                hCost = MOVE_DIAGONAL_COST * math.min(xDistance, yDistance) +
                        MOVE_STRAIGHT_COST * remaining;
            }


            public void CalculateFCost()
            {
                fCost = gCost + hCost;
            }
        }


        public static PathData findPath(in int2 startPosition,
                                    in int2 endPosition,
                                    in NativeHashMap<byte, GroundTypeStruct> groundTypes,
                                    in GridDataStruct gridData)
        {

            //if we are already here
            if (startPosition.x == endPosition.x && startPosition.y == endPosition.y)
            {
                Debug.Log("UtilsPath => Already at destination");
                PathData data = new PathData();
                int i = gridData.getIndex(in startPosition);
                data.point1 = gridData.gridRef.Value.cells[i].pos;
                return data;
            }

            Debug.Log($"Path finding start: {startPosition} end: {endPosition}");

            //lets make sure start and end are not same cell
            int distance = math.abs(startPosition - endPosition).x + math.abs(startPosition - endPosition).y;
            Debug.Log($"Path Calculation, distance apart:: {distance} ");



            NativeArray<Node> pathNodeArray = new NativeArray<Node>(
                gridData.gridRef.Value.cells.Length, Allocator.Temp);
            NativeList<int> openList = new NativeList<int>(Allocator.Temp);
            NativeList<int> closedList = new NativeList<int>(Allocator.Temp);

            NativeArray<int2> neighbourOffsetArray = new NativeArray<int2>(8, Allocator.Temp)
            {
                [0] = new int2(-1, 0),
                [1] = new int2(+1, 0),
                [2] = new int2(0, +1),
                [3] = new int2(0, -1),
                [4] = new int2(-1, -1),
                [5] = new int2(-1, +1),
                [6] = new int2(+1, -1),
                [7] = new int2(+1, +1)
            };

            int endNodeIndex = gridData.getIndex(in endPosition);
            int startNodeIndex = gridData.getIndex(in startPosition);


            // Prep the start note
            Node startNode = new Node(
                in gridData.gridRef.Value.cells[startNodeIndex],
                startNodeIndex);
            startNode.gCost = 0;
            startNode.CalculateFCost();
            pathNodeArray[startNode.index] = startNode;



            openList.Add(startNode.index);

            //placeholders
            int2 neighbourPosition;
            int2 neighbourOffset;
            Node neighbourNode;
            Node currentNode;
            while (openList.Length > 0)
            {
                int currentNodeIndex = GetLowestCostFNodeIndex(openList, pathNodeArray);
                currentNode = pathNodeArray[currentNodeIndex];

                if (currentNodeIndex == endNodeIndex)
                {
                    // Reached our destination!
                    break;
                }

                // Remove current node from Open List
                for (int i = 0; i < openList.Length; i++)
                {
                    if (openList[i] == currentNodeIndex)
                    {
                        openList.RemoveAtSwapBack(i);
                        break;
                    }
                }

                closedList.Add(currentNodeIndex);

                for (int i = 0; i < neighbourOffsetArray.Length; i++)
                {
                    neighbourOffset = neighbourOffsetArray[i];
                    neighbourPosition = currentNode.pos + neighbourOffset;


                    if (!IsPositionInsideGrid(in neighbourPosition, in gridData.gridSize))
                    {
                        // Neighbour not valid position
                        continue;
                    }

                    int neighbourNodeIndex = CalculateIndex(neighbourPosition, gridData.gridSize.x);

                    if (closedList.Contains(neighbourNodeIndex))
                    {
                        // Already searched this node
                        continue;
                    }



                    neighbourNode = new Node(
                        in gridData.gridRef.Value.cells[neighbourNodeIndex],
                        neighbourNodeIndex);


                    if (!neighbourNode.canMove)
                        continue;


                    neighbourNode.CalculateGCost(in currentNode);
                    neighbourNode.CalculateHCost(in endPosition);
                    neighbourNode.CalculateFCost();
                    neighbourNode.cameFromNodeIndex = currentNodeIndex;
                    pathNodeArray[neighbourNodeIndex] = neighbourNode;
                    // Debug.Log($"Node Calc::: {neighbourNode.pos.x},{neighbourNode.pos.z}");
                    if (!openList.Contains(neighbourNode.index))
                    {
                        openList.Add(neighbourNode.index);
                    }
                    //int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNodePosition, neighbourPosition);
                    //if (tentativeGCost < neighbourNode.gCost)
                    //{





                    //}
                } //for
            } //while



            Node endNode = pathNodeArray[endNodeIndex];
            PathData pathData = new PathData();
            //See if we found the path
            if (endNode.fCost == 0 && endNode.index == 0 && endNode.cameFromNodeIndex == 0)
            {
                //Didn't find path
                Debug.Log("Couldn't find path");

            } 
            else if (endNode.cameFromNodeIndex == -1)
            {
                //Didn't find path
                Debug.Log("End node is the same as start node");
            }
            else
            {
                // Found a path
                NativeList<int2> path = new NativeList<int2>(Allocator.Temp);
                currentNode = endNode;
                Node cameFromNode;
                while (currentNode.cameFromNodeIndex != -1)
                {
                    cameFromNode = pathNodeArray[currentNode.cameFromNodeIndex];
                    //Debug.Log($"Path Data::: {path}, {path.Length},   node: {currentNode.pos}");
                    path.Add(currentNode.pos);
                    currentNode = cameFromNode;
                }

                for (int i = 1; i < 6; i++)
                {
                    if (path.Length >= i)
                    {
                        pathData.setByIndex(i, path[path.Length - i]);
                    }
                    else
                        break;
                }
                path.Dispose();
            }

            neighbourOffsetArray.Dispose();
            openList.Dispose();
            closedList.Dispose();
            pathNodeArray.Dispose();

            return pathData;

        }





        //-------------------------------------------------------------------------







        public static void FindItemQuality(in int2 startPosition,
                                                in MapItemData mapItemData,
                                                in NativeHashMap<int, ItemReference> itemReferences,
                                                int maxDistance,
                                                in NativeHashMap<byte, GroundTypeStruct> groundTypes,
                                                in GridDataStruct gridData)
        {


            NativeList<NodeDetails> nodeDetailList = new NativeList<NodeDetails>(Allocator.Temp);

            NativeArray<Node> pathNodeArray = new NativeArray<Node>(
                gridData.gridRef.Value.cells.Length, Allocator.Temp);

            NativeArray<int2> neighbourOffsetArray = new NativeArray<int2>(8, Allocator.Temp)
            {
                [0] = new int2(-1, -1),
                [1] = new int2(-1, +1),
                [2] = new int2(+1, -1),
                [3] = new int2(+1, +1),
                [4] = new int2(-1, 0),
                [5] = new int2(+1, 0),
                [6] = new int2(0, +1),
                [7] = new int2(0, -1)
            };


            int startNodeIndex = gridData.getIndex(in startPosition);


            // Prep the start note
            Node startNode = new Node(
                in gridData.gridRef.Value.cells[startNodeIndex],
                startNodeIndex);
            startNode.gCost = 0;
            startNode.CalculateFCost();
            pathNodeArray[startNode.index] = startNode;

            NativeList<int> openList = new NativeList<int>(Allocator.Temp);
            NativeList<int> closedList = new NativeList<int>(Allocator.Temp);

            openList.Add(startNode.index);

            //placeholders
            int2 neighbourPosition;
            int2 neighbourOffset;
            while (openList.Length > 0)
            {
                int currentNodeIndex = GetLowestCostFNodeIndex(openList, pathNodeArray);
                Node currentNode = pathNodeArray[currentNodeIndex];



                // Remove current node from Open List
                for (int i = 0; i < openList.Length; i++)
                {
                    if (openList[i] == currentNodeIndex)
                    {
                        openList.RemoveAtSwapBack(i);
                        break;
                    }
                }

                closedList.Add(currentNodeIndex);

                for (int i = 0; i < neighbourOffsetArray.Length; i++)
                {
                    neighbourOffset = neighbourOffsetArray[i];
                    neighbourPosition = currentNode.pos + neighbourOffset;

                    if (!IsPositionInsideGrid(in neighbourPosition, in gridData.gridSize))
                    {
                        // Neighbour not valid position
                        continue;
                    }

                    int neighbourNodeIndex = CalculateIndex(neighbourPosition, gridData.gridSize.x);

                    if (closedList.Contains(neighbourNodeIndex))
                    {
                        // Already searched this node
                        continue;
                    }


                    Node neighbourNode = new Node(
                        in gridData.gridRef.Value.cells[neighbourNodeIndex],
                        neighbourNodeIndex);


                    //pathNodeArray[neighbourNodeIndex];
                    if (!neighbourNode.canMove)
                    {
                        // Not walkable
                        continue;
                    }

                    //does this node have desired item
                    /*mapItemData.items

                    int2 currentNodePosition = new int2(currentNode.pos.x, currentNode.pos.z);

                    int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNodePosition, neighbourPosition);
                    if (tentativeGCost < neighbourNode.gCost)
                    {
                        neighbourNode.cameFromNodeIndex = currentNodeIndex;
                        neighbourNode.gCost = tentativeGCost;
                        neighbourNode.CalculateFCost();
                        pathNodeArray[neighbourNodeIndex] = neighbourNode;

                        if (!openList.Contains(neighbourNode.index))
                        {
                            openList.Add(neighbourNode.index);
                        }
                    }
                    */
                } //for
            } //while




            neighbourOffsetArray.Dispose();
            openList.Dispose();
            closedList.Dispose();
            nodeDetailList.Dispose();



        }




        private static int CalculateIndex(int2 pos, int gridWidth)
        {
            return pos.x + (pos.y * gridWidth);
        }

        private static int GetLowestCostFNodeIndex(
            in NativeList<int> openList, in NativeArray<Node> pathNodeArray)
        {
            Node lowestCostPathNode = pathNodeArray[openList[0]];
            for (int i = 1; i < openList.Length; i++)
            {
                Node testPathNode = pathNodeArray[openList[i]];
                if (testPathNode.fCost < lowestCostPathNode.fCost)
                {
                    lowestCostPathNode = testPathNode;
                }
            }
            return lowestCostPathNode.index;
        }

        private static bool IsPositionInsideGrid(in int2 gridPosition, in int2 gridSize)
        {
            return
                gridPosition.x >= 0 &&
                gridPosition.y >= 0 &&
                gridPosition.x < gridSize.x &&
                gridPosition.y < gridSize.y;
        }


    }


}
