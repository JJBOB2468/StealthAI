using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Status
{
    COMPLETED,
    RUNNING,
    FAILED
}

public abstract class Node
{
    private DataBoard board;
    public Node(DataBoard board)
    {
        this.board = board;
    }

    public abstract Status Run();
}

public abstract class CompositeNode : Node
{
    protected List<Node> nodes;
    protected int executeCount = 0;
    public CompositeNode(DataBoard board) : base(board)
    {
        nodes = new List<Node>();
    }
    public void AddToIndex(Node node)
        { nodes.Add(node); }
}

public class Selector : CompositeNode
{
    public Selector(DataBoard board) : base(board)
    {
    }

    public override Status Run()
    {
        Status rv = Status.FAILED;
        for (; executeCount < nodes.Count; executeCount++)
        {
            rv = nodes[executeCount].Run();
            if (rv != Status.FAILED)
                { break; }
        }
        if (rv != Status.RUNNING)
            { executeCount = 0; }
        return rv;
    }
}

public class Sequence : CompositeNode
{
    public Sequence(DataBoard board) : base(board)
    {
    }

    public override Status Run()
    {
        Status rv = Status.COMPLETED;
        for (; executeCount < nodes.Count; executeCount++)
        {
            rv = nodes[executeCount].Run();
            if (rv != Status.COMPLETED)
                { break; }
        }
        if (rv != Status.RUNNING)
            { executeCount = 0; }
        return rv;
    }
}

public abstract class DecoratorNode : Node
{
    protected Node WrappedNode;
    public DecoratorNode(Node WrappedNode, DataBoard board) : base(board)
    {
        this.WrappedNode = WrappedNode;
    }

    public Node GetWrappedNode()
    {
        return WrappedNode;
    }
}

public abstract class ConditionalDecorator : DecoratorNode
{
    public ConditionalDecorator(Node WrappedNode, DataBoard board) : base(WrappedNode, board)
    {
    }

    public abstract bool CheckStatus();
    public override Status Run()
    {
        Status rv = Status.FAILED;

        if (CheckStatus())
            rv = WrappedNode.Run();

        return rv;
    }
}
