namespace Neo4j.Berries.OGM.Models.Config;

public class TimestampConfiguration()
{
    internal bool Enabled { get; set; }
    /// <summary>
    /// Enforce the created timestamp key. When true, ModifiedTimestampKey will be also set on creating.
    /// </summary>
    public bool EnforceModifiedTimestampKey { get; set; }
    /// <summary>
    /// The key to use for setting the created timestamp. Default: CreatedOn
    /// </summary>
    public string CreatedTimestampKey { get; set; } = "CreatedOn";
    /// <summary>
    /// The key to use for setting the updated timestamp. Default: ModifiedOn
    /// </summary>
    public string ModifiedTimestampKey { get; set; } = "ModifiedOn";
}