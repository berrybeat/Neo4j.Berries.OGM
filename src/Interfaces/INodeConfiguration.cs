using berrybeat.Neo4j.OGM.Models;
using berrybeat.Neo4j.OGM.Models.Config;

namespace berrybeat.Neo4j.OGM.Interfaces;

public interface INodeConfiguration<TNode>
where TNode: class {
    ///<summary>
    /// Configures the node type
    ///</summary>
    void Configure(NodeTypeBuilder<TNode> builder);
}