namespace Neo4j.Berries.OGM.Interfaces;

//TODO:
//First implement this for MergeCommand
//The constructor only needs the config and the string builder
//The neo4j value normalization will be done in the NodeSet for dictionary input.
//In Merge command the properties are either Identifier or normal property. A normal property even if it is not nullable will go to SET statement
//It can happen that there is no other properties than identifiers is passed so the SET command should be set to empty.
internal interface ICommand
{
    void Reset();
    void BuildCypher(string unwindCollectionName, string rootNodeLabel, int nodeSetIndex);
    IList<object> Nodes { get; }
}