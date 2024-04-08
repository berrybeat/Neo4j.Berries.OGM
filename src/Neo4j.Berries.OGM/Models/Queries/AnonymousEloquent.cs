using System.Data;
using Neo4j.Berries.OGM.Enums;
using Neo4j.Berries.OGM.Utils;

namespace Neo4j.Berries.OGM.Models.Queries;

public class Eloquent
{
    private IEnumerable<ConjunctionGroup> RawClauses = [];
    public readonly int Index;
    internal Dictionary<string, object> QueryParameters { get; set; } = [];
    private string CurrentQueryParameterName => $"$qp_{Index}_{QueryParameters.Count}";
    public Eloquent(int index)
    {
        RawClauses = RawClauses.Append(new ConjunctionGroup { Conjunction = "AND" });
        this.Index = index;
    }

    /// <summary>
    /// Will create a new conjunction group with the AND conjunction
    /// </summary>
    public Eloquent AND
    {
        get
        {
            RawClauses = RawClauses.Append(new ConjunctionGroup { Conjunction = "AND" });
            return this;
        }
    }
    /// <summary>
    /// Will create a new conjunction group with the OR conjunction
    /// </summary>
    public Eloquent OR
    {
        get
        {
            RawClauses = RawClauses.Append(new ConjunctionGroup { Conjunction = "OR" });
            return this;
        }
    }
    /// <summary>
    /// Will create a new conjunction group with the XOR conjunction
    /// </summary>
    public Eloquent XOR
    {
        get
        {
            RawClauses = RawClauses.Append(new ConjunctionGroup { Conjunction = "XOR" });
            return this;
        }
    }
    /// <summary>
    /// Will add a where clause to the current conjunction group. The default comparison operator is equals
    /// </summary>
    /// <param name="property">The property to compare</param>
    /// <param name="value">The value to compare the property to</param>
    public Eloquent Where(string property, object value)
    {
        return Where(property, ComparisonOperator.Equals, value);
    }
    /// <summary>
    /// Will add a where clause to the current conjunction group
    /// </summary>
    /// <param name="property">The property to compare</param>
    /// <param name="comparisonOperator">The comparison operator to use</param>
    /// <param name="value">The value to compare the property to</param>
    public Eloquent Where(string property, ComparisonOperator comparisonOperator, object value)
    {
        if (value is null)
        {
            if (comparisonOperator == ComparisonOperator.Equals)
            {
                return WhereIsNull(property);
            }
            else if (comparisonOperator == ComparisonOperator.NotEquals)
            {
                return WhereIsNotNull(property);
            }
        }

        var opt = OperatorMaps.ComparisonOperatorMap[comparisonOperator];
        return AddWhereClause(property, opt, value);
    }
    /// <summary>
    /// Will add a where clause to the current conjunction group. This where clause checks if the given property <c>IS NULL</c>
    /// </summary>
    /// <param name="property">The property to compare</param>
    public Eloquent WhereIsNull(string property)
    {
        return AddWhereClause(property, "{0} IS NULL", null, true);
    }
    /// <summary>
    /// Will add a where clause to the current conjunction group. This where clause checks if the given property <c>IS NOT NULL</c>
    /// </summary>
    /// <param name="property">The property to compare</param>
    public Eloquent WhereIsNotNull(string property)
    {
        return AddWhereClause(property, "{0} IS NOT NULL", null, true);
    }
    /// <summary>
    /// Will add a where clause to the current conjunction group. This where clause checks if the given property <c>IS IN</c> the given values
    /// </summary>
    /// <param name="property">The property to compare</param>
    /// <param name="values">The values to compare the property to</param>
    public Eloquent WhereIsIn(string property, IEnumerable<object> values)
    {
        return AddWhereClause(property, "IN", values);
    }
    /// <summary>
    /// Will add a where clause to the current conjunction group. This where clause checks if the given property <c>IS NOT IN</c> the given values
    /// </summary>
    /// <param name="property">The property to compare</param>
    /// <param name="values">The values to compare the property to</param>
    public Eloquent WhereIsNotIn(string property, IEnumerable<object> values)
    {
        return AddWhereClause(property, "NOT {0} IN {1}", values, true);
    }
    protected Eloquent AddWhereClause(string property, string opt, object value, bool OverwriteOperatorFormat = false)
    {
        if (value is null)
        {
            //There is no value to parameterize
            RawClauses.Last().Members = RawClauses
                .Last()
                .Members
                .Append(
                    new(property, opt, null, OverwriteOperatorFormat)
                    );
        }
        else
        {
            value = value.ToNeo4jValue();
            if (value is IEnumerable<Guid> enumerable)
            {
                value = enumerable.Select(x => x.ToString()).ToArray();
            }
            var queryParameterName = CurrentQueryParameterName;
            QueryParameters.Add(queryParameterName.Replace("$", ""), value);
            RawClauses.Last().Members = RawClauses
                .Last()
                .Members
                .Append(
                    new(property, opt, queryParameterName, OverwriteOperatorFormat)
                    );
        }
        return this;
    }
    internal string ToCypher(string alias)
    {
        var whereClauseSegments = RawClauses
        .Where(x => x.Members.Any())
        .SelectMany((clause, index) =>
        {
            if (index == 0)
            {
                return new List<string> { clause.ToString(alias) };
            }
            return new List<string> { clause.Conjunction, clause.ToString(alias) };
        });
        return string.Join(" ", whereClauseSegments);
    }
}