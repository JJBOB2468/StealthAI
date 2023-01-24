using Cinemachine.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(EnemyBoard))]
public class EnemyDefaultCode : MonoBehaviour {
    private Node mainNode;
    private EnemyBoard updateBoard;

    void Start() {
        EnemyBoard eBoard = GetComponent<EnemyBoard>();
        updateBoard = eBoard;

        Selector rootNode = new (eBoard);
        mainNode = rootNode;

        CompositeNode lookSequence = new Sequence(eBoard);
        LookDecorator mainLook = new (lookSequence, eBoard);
        lookSequence.AddToIndex(new LookFoundNode(eBoard));
        lookSequence.AddToIndex(new LookFindNode(eBoard));
        

        CompositeNode moveSequence = new Sequence(eBoard);
        moveSequence.AddToIndex(new MoveNode(eBoard));
        moveSequence.AddToIndex(new NextMoveNode(eBoard));

        rootNode.AddToIndex(mainLook);
        rootNode.AddToIndex(moveSequence);

        InvokeRepeating(("Run"), 0.1f, 0.1f);
    }

    void Run() { mainNode.Run(); }


    void Update() {
        if (updateBoard.detected) { updateBoard.SetGoal(updateBoard.detect); updateBoard.detected = false; updateBoard.looked = true; }
        if ((updateBoard.route.Count > 0 || updateBoard.goTo != updateBoard.transform.position) && updateBoard.looked && 
            (updateBoard.route.Count>0? (updateBoard.goal - updateBoard.route.ElementAt(updateBoard.route.Count-1)).magnitude <= updateBoard.aStarDistanceChecks 
            : (updateBoard.goal - updateBoard.goTo).magnitude <= updateBoard.aStarDistanceChecks)) {

            if ((updateBoard.goTo - updateBoard.transform.position).magnitude > updateBoard.aStarDistanceChecks) {
                Vector3 nextPos = transform.position + (updateBoard.goTo - transform.position).normalized * (updateBoard.moveSpeed * 10 * Time.deltaTime);
                if ((nextPos - updateBoard.transform.position).magnitude > (updateBoard.goTo - updateBoard.transform.position).magnitude)
                    { Debug.Log("balancing"); updateBoard.goTo = transform.position; }
                
                updateBoard.rb.MovePosition(nextPos);
            }
            else {
                if(updateBoard.route.Count > 0) {
                    updateBoard.goTo = updateBoard.route.Pop();
                }
                else {
                    updateBoard.transform.position = updateBoard.goal;
                    updateBoard.goTo = transform.position;
                    updateBoard.looked= false;
                }
            }

        }
    }

    public class LookFoundNode : Node {
        EnemyBoard eBoard;
        public LookFoundNode(DataBoard board) : base(board) {
            eBoard = (EnemyBoard)board;
        }

        public override Status Run() {
            if (eBoard.seen) {
            }
            return Status.COMPLETED;
        }
    }

    public class LookFindNode : Node {
        readonly EnemyBoard eBoard;
        public LookFindNode(DataBoard board) : base(board) {
            eBoard = (EnemyBoard)board;
        }

        public override Status Run()
        {
            Status rv = Status.RUNNING;
            float rotateChange = 360 * (Time.deltaTime) * eBoard.moveSpeed * 6;
            eBoard.transform.RotateAround(eBoard.transform.position, Vector3.up, rotateChange);
            eBoard.transform.position = eBoard.goTo;
            if(eBoard.transform.eulerAngles.y >= eBoard.direction - rotateChange && eBoard.transform.eulerAngles.y <= eBoard.direction + rotateChange)  {
                Debug.Log("loop");
                if ((eBoard.idlePosition[eBoard.currentIdlePos] - eBoard.transform.position).magnitude <= eBoard.aStarDistanceChecks) {
                    eBoard.currentIdlePos++;
                    if (eBoard.currentIdlePos >= eBoard.idlePosition.Count)
                    { eBoard.currentIdlePos = 0; }
                }
                eBoard.SetGoal(eBoard.idlePosition[eBoard.currentIdlePos]);
                Debug.Log("Changed1");
                rv = Status.COMPLETED;
                eBoard.looked= true;
            }
            return rv;
        }
    }

    public class LookDecorator : ConditionalDecorator {
        readonly EnemyBoard eBoard;
        public LookDecorator(Node wrappedNode, DataBoard board) : base(wrappedNode, board) {
            eBoard = (EnemyBoard)board;
        }

        public override bool CheckStatus() {
            return (!eBoard.looked && !eBoard.detected);
        }
    }

    public class MoveDecorator : ConditionalDecorator {
        public MoveDecorator(Node wrappedNode, DataBoard board) : base(wrappedNode, board) { }

        public override bool CheckStatus() {
            return true;
        }
    }

    public class NextMoveNode : Node {
        readonly EnemyBoard eBoard;
        public NextMoveNode(DataBoard board) : base(board) {
            eBoard = (EnemyBoard)board;
        }

        public override Status Run() {
            if (eBoard.detected) {
                eBoard.SetGoal(eBoard.detect);
                Debug.Log("Changed3");
            }
            return Status.COMPLETED;
        }
    }

    public class MoveNode : Node {
        readonly EnemyBoard eBoard;
        public MoveNode(DataBoard board) : base(board) {
            eBoard = (EnemyBoard)board;
            eBoard.movement = new MovementCode(eBoard);
        }

        public override Status Run() {
            Status rv = Status.RUNNING;
            eBoard.movement.mainCode();
            if (eBoard.route.Count > 0) {
                eBoard.direction = Mathf.Acos((eBoard.route.Peek().z-eBoard.transform.position.z) / (eBoard.route.Peek() - eBoard.transform.position).magnitude);
            }

            if (eBoard.route.Count > 0) {
                rv = Status.COMPLETED;
            }
            return rv;
        }
    }




}
