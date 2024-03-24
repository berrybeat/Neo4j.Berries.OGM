using System.Linq.Expressions;

namespace berrybeat.Neo4j.OGM.Utils;

public static class ExpressionUtils
{
    public static string GetPropertyName<T, TProperty>(this Expression<Func<T, TProperty>> expression)
    {
        if (expression.Body is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }
        else if (expression.Body is UnaryExpression unaryExpression )
        {
            return ((MemberExpression)unaryExpression.Operand).Member.Name;
        }
        else
        {
            throw new ArgumentException("Invalid expression");
        }
    }
}