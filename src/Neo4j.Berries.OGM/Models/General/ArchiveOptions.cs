namespace Neo4j.Berries.OGM.Models.General;

internal class ArchiveOptions
{
    public bool StartNode { get; set; }
    public bool Edges { get; set; }
}

public class ArchiveOptionsBuilder
{
    internal ArchiveOptions Options { get; } = new ArchiveOptions();
    /// <summary>
    /// If true, the matched node in the root query will be archived.
    /// </summary>
    public ArchiveOptionsBuilder StartNode()
    {
        Options.StartNode = true;
        return this;
    }
    /// <summary>
    /// If true, the matched relation(s) in the result of WithRelation query will be archived.
    /// </summary>
    public ArchiveOptionsBuilder Edges()
    {
        Options.Edges = true;
        return this;
    }
    /// <summary>
    /// Everything matched with queries prior to the archive method will be archived.
    /// </summary>
    public ArchiveOptionsBuilder All()
    {
        Options.StartNode = true;
        Options.Edges = true;
        return this;
    }
}