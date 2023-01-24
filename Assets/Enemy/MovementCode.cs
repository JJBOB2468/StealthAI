using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using Cinemachine.Utility;
using UnityEditor.Rendering;

public class MovementCode {
    readonly private float distanceToNextPoint = 1;

    private List<AStarNode> aStar = new();
    private Stack<Vector3> route = new();
    private List<Vector3> closed = new();
    private Vector3 capsuleSize = Vector3.zero;
    EnemyBoard eBoard;

    public MovementCode(DataBoard board) {
        eBoard = (EnemyBoard)board;
        distanceToNextPoint = eBoard.aStarDistanceChecks;
    }

    public void mainCode() {
        if (capsuleSize == Vector3.zero) { capsuleSize.x = eBoard.coll.radius * 2; capsuleSize.y = eBoard.coll.height; capsuleSize.z = eBoard.coll.radius * 2; }
        if (eBoard.route.Count <= 0 || ((eBoard.goal - eBoard.route.ElementAt(eBoard.route.Count - 1)).magnitude > distanceToNextPoint)) {
            if (aStar.Count == 0) {
                aStar.Add(new AStarNode());
                aStar.ElementAt(0).newPos(eBoard.goal, eBoard.transform.position);
                iterate(eBoard.transform.position);
            }
            int index = 0;
            index = lowestNodeIndex();
            findNewPositions(index);
            foreach (AStarNode node in aStar) {
                if (node.getMag() < distanceToNextPoint) {
                    /////////JUST AN IDEA TO MAKE SMALLER ROUTES
                    /*
                    List<Vector3> tempRoute = node.route.ToList<Vector3>();
                    Vector3 lastVec = new();
                    int loop = 0;
                    Vector3 lastNorm = new();
                    foreach (Vector3 v in tempRoute)
                    {
                        if (lastVec != null)
                        {
                            if (lastNorm != null)
                            {
                                if(lastNorm == (lastVec - v).normalized)
                                {
                                    tempRoute.RemoveAt(loop-1);
                                }
                            }
                            lastNorm = (lastVec - v).normalized;
                        }
                        lastVec = v;
                        loop++;
                    }
                    node.route = tempRoute;*/
                    eBoard.route.Clear();
                    closed.Clear();
                    route.Clear();
                    int count = node.route.Count;
                    for(int i = 0; i < count-1; i++)
                    { eBoard.route.Push(node.route.Pop()); }
                    aStar.RemoveAll(delegate (AStarNode a) { return a.GetType() == typeof(AStarNode); });
                    break;
                }
            }
        }
    }

    private bool Raycast(Vector3 origin, Vector3 normalizedDirection) { return Physics.Raycast(origin, normalizedDirection, (eBoard.goal - normalizedDirection * distanceToNextPoint).magnitude); }

    readonly List<Vector3> directions = new() { new Vector3(1, 0, 0), new Vector3(1, 0, 1), new Vector3(0, 0, 1), new Vector3(-1, 0, 1), new Vector3(-1, 0, 0), new Vector3(-1, 0, -1), new Vector3(0, 0, -1), new Vector3(1, 0, -1) };
    private void findNewPositions(int index) {
        bool found = false;
        AStarNode originNode = new AStarNode().Clone(aStar.ElementAt(index));
        Vector3 origin = originNode.route.Peek();
        foreach (Vector3 direction in directions) {
            Vector3 newPos = origin + direction * distanceToNextPoint;
            if (!Raycast(origin, direction) && !iterate(newPos) && canIFit(origin, direction)) {
                if (found) {
                    addNewNode(originNode, newPos);
                }
                else {
                    aStar.ElementAt(index).newPos(eBoard.goal, newPos);
                    found = true;
                }

            }
        }
        if (!found && aStar.ElementAt(index).route.Peek() != eBoard.goal) {
            removeNode(index);
        }
    }

    private void addNewNode(AStarNode node, Vector3 newPosition) {
        AStarNode newNode = new();
        newNode.Clone(node);
        aStar.Add(new AStarNode().Clone(node));
        aStar.ElementAt(aStar.Count - 1).newPos(eBoard.goal, newPosition);
    }

    private void removeNode(int index) { aStar.RemoveAt(index); }

    private int lowestNodeIndex() {
        float lowestValue = -1;
        int loop = 0, lowestValueLoop = 0;
        if (aStar.Count > 0) {
            float nodeSum = 0;
            foreach (AStarNode node in aStar)
            {
                nodeSum = node.GetSum();
                if (nodeSum < lowestValue)
                {
                    lowestValue = nodeSum;
                    lowestValueLoop = loop;
                }
                else if (lowestValue == -1)
                {
                    lowestValue = nodeSum;
                }
                loop++;
            }
        }
        return lowestValueLoop;
    }

    private bool iterate(Vector3 position) {
        foreach (Vector3 i in closed)
        {
            if (position == i) { return true; }
        }
        closed.Add(position);
        return false;
    }

    readonly List<Vector3> fitDirection = new() { new Vector3(1, 0, 0), new Vector3(0, 0, 1), new Vector3(-1, 0, 0), new Vector3(0, 0, -1), new Vector3(0, 1, 0), new Vector3(0, -1, 0) };

    private bool canIFit(Vector3 origin, Vector3 direction) {
        Vector3 gapSize = Vector3.zero;
        foreach (Vector3 size in fitDirection)
        {
            RaycastHit hit;
            Physics.Raycast(origin + direction * distanceToNextPoint, size, out hit, Vector3.Scale(size, capsuleSize).magnitude);
            gapSize += size.Abs() * (hit.point - (origin + direction * distanceToNextPoint)).magnitude;
        }
        if (gapSize.x >= capsuleSize.x && gapSize.y >= capsuleSize.y && gapSize.z >= capsuleSize.z)
        {
            return true;
        }
        return false;
    }
}