using Neo4j.Berries.OGM.Models.Config;

namespace Neo4j.Berries.OGM.Interfaces;

public interface INodeConfiguration<TNode>
where TNode: class {
    ///<summary>
    /// Configures the node type
    ///</summary>
    void Configure(NodeTypeBuilder<TNode> builder);
}