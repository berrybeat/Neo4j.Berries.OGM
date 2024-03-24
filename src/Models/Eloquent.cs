using System.Linq.Expressions;
using berrybeat.Neo4j.OGM.Enums;
using berrybeat.Neo4j.OGM.Utils;

namespace berrybeat.Neo4j.OGM.Models;

public class Eloquent<TQueryable> where TQueryable : class
{
    private IEnumerable<ConjunctionGroup> RawClauses = [];
    public readonly int Index;
    internal Dictionary<string, object> QueryParameters { get; set; } = [];
    public Eloquent(int index)
    {
        RawClauses = RawClauses.Append(new ConjunctionGroup { Conjunction = "AND" });
        this.Index = index;
    }
    private string CurrentQueryParameterName => $"$qp_{Index}_{QueryParameters.Count}";
    /// <summary>
    /// Will create a new conjunction group with the AND conjunction
    /// </summary>
    public Eloquent<TQueryable> AND
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
    public Eloquent<TQueryable> OR
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
    public Eloquent<TQueryable> XOR
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
    /// <param name="expression">The property to compare</param>
    /// <param name="value">The value to compare the property to</param>
    public Eloquent<TQueryable> Where<TProperty>(Expression<Func<TQueryable, TProperty>> expression, TProperty value)
    {
        return Where(expression, ComparisonOperator.Equals, value);
    }
    /// <summary>
    /// Will add a where clause to the current conjunction group
    /// </summary>
    /// <param name="expression">The property to compare</param>
    /// <param name="comparisonOperator">The comparison operator to use</param>
    /// <param name="value">The value to compare the property to</param>
    public Eloquent<TQueryable> Where<TProperty>(Expression<Func<TQueryable, TProperty>> expression, ComparisonOperator comparisonOperator, TProperty value)
    {
        if (value is null)
        {
            if (comparisonOperator == ComparisonOperator.Equals)
            {
                return WhereIsNull(expression);
            }
            else if (comparisonOperator == ComparisonOperator.NotEquals)
            {
                return WhereIsNotNull(expression);
            }
        }

        var opt = OperatorMaps.ComparisonOperatorMap[comparisonOperator];
        return AddWhereClause(expression, opt, value);
    }
    /// <summary>
    /// Will add a where clause to the current conjunction group. This where clause checks if the given property <c>IS NULL</c>
    /// </summary>
    /// <param name="expression">The property to compare</param>
    public Eloquent<TQueryable> WhereIsNull<TProperty>(Expression<Func<TQueryable, TProperty>> expression)
    {
        return AddWhereClause(expression, "{0} IS NULL", null, true);
    }
    /// <summary>
    /// Will add a where clause to the current conjunction group. This where clause checks if the given property <c>IS NOT NULL</c>
    /// </summary>
    /// <param name="expression">The property to compare</param>
    public Eloquent<TQueryable> WhereIsNotNull<TProperty>(Expression<Func<TQueryable, TProperty>> expression)
    {
        return AddWhereClause(expression, "{0} IS NOT NULL", null, true);
    }
    /// <summary>
    /// Will add a where clause to the current conjunction group. This where clause checks if the given property <c>IS IN</c> the given values
    /// </summary>
    /// <param name="expression">The property to compare</param>
    /// <param name="values">The values to compare the property to</param>
    public Eloquent<TQueryable> WhereIsIn<TProperty>(Expression<Func<TQueryable, TProperty>> expression, IEnumerable<TProperty> values)
    {
        return AddWhereClause(expression, "IN", values);
    }
    /// <summary>
    /// Will add a where clause to the current conjunction group. This where clause checks if the given property <c>IS NOT IN</c> the given values
    /// </summary>
    /// <param name="expression">The property to compare</param>
    /// <param name="values">The values to compare the property to</param>
    public Eloquent<TQueryable> WhereIsNotIn<TProperty>(Expression<Func<TQueryable, TProperty>> expression, IEnumerable<TProperty> values)
    {
        return AddWhereClause(expression, "NOT {0} IN {1}", values, true);
    }
    private Eloquent<TQueryable> AddWhereClause<TProperty>(Expression<Func<TQueryable, TProperty>> expression, string opt, object value, bool OverwriteOperatorFormat = false)
    {
        string prop = expression.GetPropertyName();
        if (value is null)
        {
            //There is no value to parameterize
            RawClauses.Last().Members = RawClauses.Last().Members.Append(new(prop, opt, null, OverwriteOperatorFormat));
        }
        else
        {
            value = value is Guid ? value.ToString() : value;
            if(value is IEnumerable<Guid> enumerable)
            {
                value = enumerable.Select(x => x.ToString()).ToArray();
            }
            var queryParameterName = CurrentQueryParameterName;
            QueryParameters.Add(queryParameterName.Replace("$", ""), value);
            RawClauses.Last().Members = RawClauses.Last().Members.Append(new(prop, opt, queryParameterName, OverwriteOperatorFormat));
        }
        return this;
    }

    internal string ToCypher(string alias) {
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