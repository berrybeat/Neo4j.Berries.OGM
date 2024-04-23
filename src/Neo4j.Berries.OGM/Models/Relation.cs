using System.Collections;

namespace Neo4j.Berries.OGM.Models;


internal interface IRelation
{
    
}


/// <summary>
/// This class defines a relation between two nodes, where it is possible to define the node properties
/// </summary>
/// <typeparam name="TRelation">The properties of the relation</typeparam>
/// <typeparam name="TNode">The node that the relation is pointing to</typeparam>
public class Relation<TRelation, TNode>(TRelation relation, TNode targetNode): IRelation
where TRelation : class
where TNode : class
{
    /// <summary>
    /// The properties of the relation
    /// </summary>
    public TRelation RelationProperties { get; set; } = relation;
    /// <summary>
    /// The node that the relation is pointing to
    /// </summary>
    public TNode TargetNode { get; set; } = targetNode;
}

/// <summary>
/// This class defines many to many relation between nodes, where it is possible to define the node properties
/// </summary>
/// <typeparam name="TRelation">The properties of the relation</typeparam>
/// <typeparam name="TNode">The node that the relation is pointing to</typeparam>
public class Relations<TRelation, TNode> : IList<Relation<TRelation, TNode>>
where TRelation: class
where TNode: class
{
    readonly IList<Relation<TRelation, TNode>> _list = new List<Relation<TRelation, TNode>>();
    public Relation<TRelation, TNode> this[int index] { 
        get {
            return _list[index];
        }
        set {
            _list[index] = value;
        }
     }

    public int Count => _list.Count;

    public bool IsReadOnly => _list.IsReadOnly;

    public void Add(Relation<TRelation, TNode> item)
    {
        _list.Add(item);
    }

    public void Clear()
    {
        _list.Clear();
    }

    public bool Contains(Relation<TRelation, TNode> item)
    {
        return _list.Contains(item);
    }

    public void CopyTo(Relation<TRelation, TNode>[] array, int arrayIndex)
    {
        _list.CopyTo(array, arrayIndex);
    }

    public IEnumerator<Relation<TRelation, TNode>> GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    public int IndexOf(Relation<TRelation, TNode> item)
    {
        return _list.IndexOf(item);
    }

    public void Insert(int index, Relation<TRelation, TNode> item)
    {
        _list.Insert(index, item);
    }

    public bool Remove(Relation<TRelation, TNode> item)
    {
        return _list.Remove(item);
    }

    public void RemoveAt(int index)
    {
        _list.RemoveAt(index);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _list.GetEnumerator();
    }
}