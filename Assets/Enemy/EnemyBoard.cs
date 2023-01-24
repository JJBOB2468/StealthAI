using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Transform))]

public class EnemyBoard : DataBoard
{
    [HideInInspector]
    public List<Vector3> idlePosition = new ();
    [HideInInspector]
    public Vector3 detect;
    [HideInInspector]
    public bool detected = false;
    [HideInInspector]
    public Vector3 goal;
    [HideInInspector]
    public int currentIdlePos = 0;
    [HideInInspector]
    public bool seen = false;
    [HideInInspector]
    public Stack<Vector3> route = new ();
    [HideInInspector]
    public Vector3 goTo;
    [HideInInspector]
    public Rigidbody rb;
    [HideInInspector]
    public CapsuleCollider coll;
    [HideInInspector]
    public float direction;
    [HideInInspector]
    public MovementCode movement;
    [HideInInspector]
    public bool looked = false;
    public float aStarDistanceChecks = 1;
    public float moveSpeed = 1;

    private void Start()
    {
        GameObject idlePoint = transform.Find("IdlePoints").gameObject;
        if (idlePoint != null && idlePoint.transform.childCount > 0)
        {
            idlePosition.Clear();
            for (int i = 0; i < idlePoint.transform.childCount; i++)
            {
                idlePosition.Add(new Vector3(idlePoint.transform.GetChild(i).position.x,transform.position.y, idlePoint.transform.GetChild(i).position.z));
            }
        }
        else { idlePosition.Add(transform.position); }
        direction = 90;
        transform.position = idlePosition[0];
        goal = idlePosition[0];
        direction = transform.rotation.y;
        rb = GetComponent<Rigidbody>();
        goTo = transform.position;
        detect=transform.position;
        coll = GetComponent<CapsuleCollider>();
    }

    public void SetGoal(Vector3 newGoal)
    {
        goal = newGoal;
        if(goal == detect) { detect = transform.position; }
    }
}
