namespace Neo4j.Berries.OGM.Utils;


public static class ObjectUtils
{
    public static object ToNeo4jValue(this object input)
    {
        if (input is Guid || input is Enum)
        {
            return input.ToString();
        }
        return input;
    }
}