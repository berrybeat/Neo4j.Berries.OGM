using System.Linq.Expressions;
using Neo4j.Berries.OGM.Enums;
using Neo4j.Berries.OGM.Utils;

namespace Neo4j.Berries.OGM.Models.Queries;

public class Eloquent<TQueryable>(int index) : Eloquent(index)
where TQueryable : class
{
    /// <summary>
    /// Will create a new conjunction group with the AND conjunction
    /// </summary>
    public new Eloquent<TQueryable> AND
    {
        get
        {
            _ = base.AND;
            return this;
        }
    }
    /// <summary>
    /// Will create a new conjunction group with the OR conjunction
    /// </summary>
    public new Eloquent<TQueryable> OR
    {
        get
        {
            _ = base.OR;
            return this;
        }
    }
    /// <summary>
    /// Will create a new conjunction group with the XOR conjunction
    /// </summary>
    public new Eloquent<TQueryable> XOR
    {
        get
        {
            _ = base.XOR;
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
        AddWhereClause(expression.GetPropertyName(true), opt, value);
        return this;
    }
    /// <summary>
    /// Will add a where clause to the current conjunction group. This where clause checks if the given property <c>IS NULL</c>
    /// </summary>
    /// <param name="expression">The property to compare</param>
    public Eloquent<TQueryable> WhereIsNull<TProperty>(Expression<Func<TQueryable, TProperty>> expression)
    {
        AddWhereClause(expression.GetPropertyName(true), "{0} IS NULL", null, true);
        return this;
    }
    /// <summary>
    /// Will add a where clause to the current conjunction group. This where clause checks if the given property <c>IS NOT NULL</c>
    /// </summary>
    /// <param name="expression">The property to compare</param>
    public Eloquent<TQueryable> WhereIsNotNull<TProperty>(Expression<Func<TQueryable, TProperty>> expression)
    {
        AddWhereClause(expression.GetPropertyName(true), "{0} IS NOT NULL", null, true);
        return this;
    }
    /// <summary>
    /// Will add a where clause to the current conjunction group. This where clause checks if the given property <c>IS IN</c> the given values
    /// </summary>
    /// <param name="expression">The property to compare</param>
    /// <param name="values">The values to compare the property to</param>
    public Eloquent<TQueryable> WhereIsIn<TProperty>(Expression<Func<TQueryable, TProperty>> expression, IEnumerable<TProperty> values)
    {
        AddWhereClause(expression.GetPropertyName(true), "IN", values);
        return this;
    }
    /// <summary>
    /// Will add a where clause to the current conjunction group. This where clause checks if the given property <c>IS NOT IN</c> the given values
    /// </summary>
    /// <param name="expression">The property to compare</param>
    /// <param name="values">The values to compare the property to</param>
    public Eloquent<TQueryable> WhereIsNotIn<TProperty>(Expression<Func<TQueryable, TProperty>> expression, IEnumerable<TProperty> values)
    {
        AddWhereClause(expression.GetPropertyName(true), "NOT {0} IN {1}", values, true);
        return this;
    }
}