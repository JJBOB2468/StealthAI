using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Runtime.CompilerServices;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEngine;

public class AStarNode
{
    private Vector2 aStarData;
    public Stack<Vector3> route = new();
    public AStarNode()
    {
        aStarData = Vector2.zero;
    }

    public float GetSum()
    {
        return aStarData.x + aStarData.y;
    }

    public AStarNode Clone(AStarNode clone)
    {
        aStarData = clone.aStarData;
        int v1 = 1;
        foreach(Vector3 v in clone.route)
        {
            route.Push(clone.route.ElementAt(clone.route.Count()-v1));
            v1++;
        }
        return this;
    }

    private float Magnitude(Vector3 goal, Vector3 Location)
    {
        double x2 = Math.Pow(goal.x - Location.x, 2);
        //double y2 = Math.Pow(goal.y - Location.y, 2);
        double z2 = Math.Pow(goal.z - Location.z, 2);
        return Math.Sqrt(x2 + /*y2 +*/ z2).ConvertTo<float>();
    }

    public void newPos(Vector3 goal, Vector3 newPos)
    {
        Vector3 t;
        aStarData.x = (newPos - (route.TryPeek(out t) ? t : newPos)).magnitude;
        aStarData.y = (goal - newPos).magnitude;
        route.Push(newPos);
    }

    public float getMag() { return aStarData.y; }
}
