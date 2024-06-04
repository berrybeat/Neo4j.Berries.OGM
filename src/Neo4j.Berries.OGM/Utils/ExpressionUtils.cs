using System.Linq.Expressions;
using Neo4j.Berries.OGM.Contexts;

namespace Neo4j.Berries.OGM.Utils;

public static class ExpressionUtils
{
    public static string GetPropertyName<T, TProperty>(this Expression<Func<T, TProperty>> expression, bool withConversion = false)
    {
        if (expression.Body is MemberExpression memberExpression)
        {
            return withConversion ? Neo4jSingletonContext.PropertyCaseConverter(memberExpression.Member.Name) : memberExpression.Member.Name;
        }
        else if (expression.Body is UnaryExpression unaryExpression )
        {
            var propertyName = ((MemberExpression)unaryExpression.Operand).Member.Name;
            return withConversion ? Neo4jSingletonContext.PropertyCaseConverter(propertyName) : propertyName;
        }
        else
        {
            throw new ArgumentException("Invalid expression");
        }
    }
}