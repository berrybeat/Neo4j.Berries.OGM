namespace Neo4j.Berries.OGM.Models.Config;

public class TimestampConfiguration()
{
    internal bool Enabled { get; set; }
    /// <summary>
    /// Enforce the created timestamp key. When true, ModifiedTimestampKey will be also set on creating.
    /// </summary>
    public bool EnforceModifiedTimestampKey { get; set; }
    /// <summary>
    /// The key to use for setting the created timestamp. Default: createdOn
    /// </summary>
    public string CreatedTimestampKey { get; set; } = "createdOn";
    /// <summary>
    /// The key to use for setting the updated timestamp. Default: modifiedOn
    /// </summary>
    public string ModifiedTimestampKey { get; set; } = "modifiedOn";
    /// <summary>
    /// The key to use for setting the archived timestamp. Default: archivedOn
    /// </summary>
    public string ArchivedTimestampKey { get; set; } = "archivedOn";
}