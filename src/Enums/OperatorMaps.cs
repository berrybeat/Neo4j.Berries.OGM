namespace berrybeat.Neo4j.OGM.Enums;

public class OperatorMaps {
#pragma warning disable CA2211 // Non-constant fields should not be visible
    public static Dictionary<ComparisonOperator, string> ComparisonOperatorMap = new() {
#pragma warning restore CA2211 // Non-constant fields should not be visible
        { ComparisonOperator.Equals, "=" },
        { ComparisonOperator.NotEquals, "<>" },
        { ComparisonOperator.GreaterThan, ">" },
        { ComparisonOperator.GreaterThanOrEquals, ">=" },
        { ComparisonOperator.LessThan, "<" },
        { ComparisonOperator.LessThanOrEquals, "<=" }
    };
}