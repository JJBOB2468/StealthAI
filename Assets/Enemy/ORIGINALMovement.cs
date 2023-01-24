using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using Cinemachine.Utility;

public class ORIGINALMovement : MonoBehaviour
{
    //private GameObject testGameObject; //Bug Testing to find the positions the enemy found for the optimal route

    public Transform goalObject;
    public float distanceToNextPoint = 1;
    public float moveSpeed = 1;

    private List<AStarNode> aStar = new List<AStarNode>();
    private Stack<Vector3> route = new Stack<Vector3>();
    private List<Vector3> closed = new List<Vector3>();
    private Rigidbody rb;
    private CapsuleCollider coll;
    private Vector3 capsuleSize;

    void Start()
    {
        coll = GetComponent<CapsuleCollider>();
        capsuleSize.x = coll.radius * 2;
        capsuleSize.y = coll.height;
        capsuleSize.z = coll.radius * 2;
        goTo = transform.position;
        rb = GetComponent<Rigidbody>();
        if (goalObject == null) { goalObject = new GameObject("goalObject").transform; goalObject.transform.position = new Vector3(10, 0, 10); goalObject.parent = null; }
    }
    void Update()
    {
        if (((route.Count > 0) ? !(magnitude(goalObject.position, route.ElementAt(route.Count - 1)) < distanceToNextPoint) : true))
        {
            if (aStar.Count == 0)
            {
                aStar.Add(new AStarNode());
                aStar.ElementAt(0).newPos(goalObject.position, transform.position);
                /*aStar.ElementAt(0).aStarData.y = magnitude(transform.position);
                aStar.ElementAt(0).route.Push(transform.position);*/ // Code Before optimized into its own function
                iterate(transform.position);
            }
            int index = 0;
            index = lowestNodeIndex();
            findNewPositions(index);
            foreach (AStarNode node in aStar)
            {
                if (node.getMag() < distanceToNextPoint)
                {
                    Debug.Log("plop");
                    route.Clear();
                    int count = node.route.Count() - 1;
                    for (int i = 0; i < count; i++)
                    { route.Push(node.route.Pop()); }
                    aStar.RemoveAll(delegate (AStarNode a) { return a.GetType() == typeof(AStarNode); });
                    /*private static bool itis (AStarNode a) { return a.GetType() == typeof(AStarNode); }
                    Predicate<AStarNode> ISIT = delegate (AStarNode a) { return a.GetType() == typeof(AStarNode); };*/ // Code to understand how to identify all of the stored Nodes in the aStar list
                    break;
                }
            }
        }
    }
    private Vector3 goTo;
    private void FixedUpdate()
    {
        /*if (route.Count > 0)
        {
            //rb.MovePosition(transform.position + ());
            time += Time.deltaTime;
            if (time > 0.2)
            {

                transform.position = route.Pop();
                time = 0;
            }
        }*/ //First attempt at the movement portion of the code
        if (route.Count > 0)
        {
            if (goTo != transform.position)
            {
                Vector3 nextPos = transform.position + (goTo - transform.position).normalized * moveSpeed * 10 * Time.deltaTime;
                if (magnitude(nextPos, transform.position) > magnitude(goTo, transform.position))
                {
                    goTo = transform.position;
                    return;
                }
                rb.MovePosition(nextPos);
            }
            else
            { goTo = route.Pop(); }

        }
        else if (goTo != transform.position)
        { goTo = transform.position; }

    }

    private bool raycast(Vector3 origin, Vector3 normalizedDirection) { return Physics.Raycast(origin, normalizedDirection, magnitude(goalObject.position, normalizedDirection * distanceToNextPoint)); }

    List<Vector3> directions = new List<Vector3> { new Vector3(1, 0, 0), new Vector3(1, 0, 1), new Vector3(0, 0, 1), new Vector3(-1, 0, 1), new Vector3(-1, 0, 0), new Vector3(-1, 0, -1), new Vector3(0, 0, -1), new Vector3(1, 0, -1) };

    private void findNewPositions(int index)
    {
        bool found = false;
        AStarNode originNode = new AStarNode().Clone(aStar.ElementAt(index));
        Vector3 origin = originNode.route.Peek();
        foreach (Vector3 direction in directions)
        {
            Vector3 newPos = origin + direction * distanceToNextPoint;
            if (!raycast(origin, direction) && !iterate(newPos) && canIFit(origin, direction))
            {
                if (found)
                {
                    addNewNode(originNode, newPos);
                }
                else
                {
                    aStar.ElementAt(index).newPos(goalObject.position, newPos);
                    /*aStar.ElementAt(index).route.Push(newPos);
                    aStar.ElementAt(index).aStarData.x = aStar.ElementAt(index).aStarData.x + distanceToNextPoint;
                    aStar.ElementAt(index).aStarData.y = magnitude(newPos);*/ // Code Before optimized into its own function
                    found = true;
                }

            }
        }
        if (!found && aStar.ElementAt(index).route.Peek() != goalObject.position)
        {
            removeNode(index);
        }
    }
    private float magnitude(Vector3 goal, Vector3 Location)
    {
        double x2 = Math.Pow(goal.x - Location.x, 2);
        //double y2 = Math.Pow(goal.y - Location.y, 2);
        double z2 = Math.Pow(goal.z - Location.z, 2);
        return Math.Sqrt(x2 + /*y2 +*/ z2).ConvertTo<float>();
    }

    private void addNewNode(AStarNode node, Vector3 newPosition)
    {
        AStarNode newNode = new AStarNode();
        newNode.Clone(node);
        aStar.Add(new AStarNode().Clone(node));
        aStar.ElementAt(aStar.Count - 1).newPos(goalObject.position, newPosition);
        /*aStar.ElementAt(aStar.Count-1).route.Push(newPosition);
        aStar.ElementAt(aStar.Count-1).aStarData.x += distanceToNextPoint;
        aStar.ElementAt(aStar.Count-1).aStarData.y = magnitude(newPosition);*/ // Code Before optimized into its own function
    }

    private void removeNode(int index) { aStar.RemoveAt(index); }



    private int lowestNodeIndex()
    {
        float lowestValue = -1;
        int loop = 0, lowestValueLoop = 0;
        if (aStar.Count > 0)
        {
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
        //Instantiate(testGameObject, aStar.ElementAt(lowestValueLoop).route.Peek(), Quaternion.identity); //Bug Testing to find the positions the enemy found for the optimal route

        return lowestValueLoop;
    }

    private bool iterate(Vector3 position)
    {
        foreach (Vector3 i in closed)
        {
            if (position == i) { return true; }
        }
        closed.Add(position);
        return false;
    }

    List<Vector3> fitDirection = new List<Vector3> { new Vector3(1, 0, 0), new Vector3(0, 0, 1), new Vector3(-1, 0, 0), new Vector3(0, 0, -1), new Vector3(0, 1, 0), new Vector3(0, -1, 0) };

    private bool canIFit(Vector3 origin, Vector3 direction)
    {
        Vector3 gapSize = Vector3.zero;
        foreach (Vector3 size in fitDirection)
        {
            RaycastHit hit;
            Physics.Raycast(origin + direction * distanceToNextPoint, size, out hit, Vector3.Scale(size, capsuleSize).magnitude);
            gapSize += size.Abs() * magnitude(hit.point, origin + direction * distanceToNextPoint);
        }
        if (gapSize.x >= capsuleSize.x && gapSize.y >= capsuleSize.y && gapSize.z >= capsuleSize.z)
        {
            return true;
        }
        return false;
    }
}


