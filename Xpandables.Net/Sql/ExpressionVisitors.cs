using System.Data;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using Microsoft.Data.SqlClient;

namespace Xpandables.Net.Sql;
/// <summary>
/// Expression visitor that converts LINQ expressions to SQL fragments and parameters.
/// </summary>
internal sealed class SqlExpressionVisitor : ExpressionVisitor
{
    private readonly StringBuilder _sql = new();
    private readonly List<IDbDataParameter> _parameters = [];
    private readonly Dictionary<ParameterExpression, string> _parameterAliases = [];
    private int _parameterIndex;

    /// <summary>
    /// Gets the generated SQL string.
    /// </summary>
    public string Sql => _sql.ToString();

    /// <summary>
    /// Gets the collection of SQL parameters.
    /// </summary>
    public IReadOnlyList<IDbDataParameter> Parameters => _parameters.AsReadOnly();

    /// <summary>
    /// Registers parameter aliases based on expression parameters.
    /// </summary>
    /// <param name="expression">The lambda expression containing parameters.</param>
    public void RegisterParameterAliases(LambdaExpression expression)
    {
        for (int i = 0; i < expression.Parameters.Count; i++)
        {
            var param = expression.Parameters[i];
            var alias = GetDefaultAlias(param.Type, i);
            _parameterAliases[param] = alias;
        }
    }

    /// <summary>
    /// Visits the expression and generates SQL.
    /// </summary>
    /// <param name="expression">The expression to visit.</param>
    /// <returns>The SQL string.</returns>
    public string VisitAndGenerateSql(Expression expression)
    {
        _sql.Clear();
        Visit(expression);
        return _sql.ToString();
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
        _sql.Append('(');
        Visit(node.Left);

        string operatorSql = GetSqlOperator(node.NodeType);
        _sql.Append(CultureInfo.InvariantCulture, $" {operatorSql} ");

        Visit(node.Right);
        _sql.Append(')');

        return node;
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Expression is ParameterExpression parameter)
        {
            var columnName = GetColumnName(node.Member);
            var tableAlias = _parameterAliases.GetValueOrDefault(parameter, GetDefaultAlias(parameter.Type, 0));
            _sql.Append(CultureInfo.InvariantCulture, $"[{tableAlias}].[{columnName}]");
        }
        else if (node.Expression is ConstantExpression ||
                 (node.Expression is MemberExpression))
        {
            // Handle constant member access (local variables, properties)
            var value = GetMemberValue(node);
            AddParameter(value);
        }
        else
        {
            // Handle other member expressions by evaluating them
            var value = GetMemberValue(node);
            AddParameter(value);
        }

        return node;
    }

    protected override Expression VisitConstant(ConstantExpression node)
    {
        AddParameter(node.Value);
        return node;
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        // Handle string methods
        if (node.Method.DeclaringType == typeof(string))
        {
            HandleStringMethod(node);
        }
        // Handle LINQ aggregate methods
        else if (IsLinqAggregateMethod(node))
        {
            HandleLinqAggregateMethod(node);
        }
        else
        {
            // For other methods, try to evaluate the result
            var value = GetExpressionValue(node);
            AddParameter(value);
        }

        return node;
    }

    protected override Expression VisitNew(NewExpression node)
    {
        // Handle anonymous type creation (for SELECT clauses)
        for (int i = 0; i < node.Arguments.Count; i++)
        {
            if (i > 0) _sql.Append(", ");

            Visit(node.Arguments[i]);

            // Add alias if available
            if (node.Members?[i] != null)
            {
                _sql.Append(CultureInfo.InvariantCulture, $" AS [{node.Members[i].Name}]");
            }
        }

        return node;
    }

    protected override Expression VisitMemberInit(MemberInitExpression node)
    {
        // Handle object initialization (for SELECT clauses)
        for (int i = 0; i < node.Bindings.Count; i++)
        {
            if (i > 0) _sql.Append(", ");

            if (node.Bindings[i] is MemberAssignment assignment)
            {
                Visit(assignment.Expression);
                _sql.Append(CultureInfo.InvariantCulture, $" AS [{assignment.Member.Name}]");
            }
        }

        return node;
    }

    private void HandleStringMethod(MethodCallExpression node)
    {
        switch (node.Method.Name)
        {
            case nameof(string.Contains):
                Visit(node.Object!);
                _sql.Append(" LIKE ");
                var containsValue = GetExpressionValue(node.Arguments[0]);
                AddParameter($"%{containsValue}%");
                break;

            case nameof(string.StartsWith):
                Visit(node.Object!);
                _sql.Append(" LIKE ");
                var startsWithValue = GetExpressionValue(node.Arguments[0]);
                AddParameter($"{startsWithValue}%");
                break;

            case nameof(string.EndsWith):
                Visit(node.Object!);
                _sql.Append(" LIKE ");
                var endsWithValue = GetExpressionValue(node.Arguments[0]);
                AddParameter($"%{endsWithValue}");
                break;

            default:
                var value = GetExpressionValue(node);
                AddParameter(value);
                break;
        }
    }

    private void HandleLinqAggregateMethod(MethodCallExpression node)
    {
        switch (node.Method.Name)
        {
            case "Count":
                _sql.Append("COUNT(*)");
                break;
            case "Sum":
                _sql.Append("SUM(");
                if (node.Arguments.Count > 1)
                    Visit(node.Arguments[1]);
                else
                    _sql.Append('*');
                _sql.Append(')');
                break;
            case "Max":
                _sql.Append("MAX(");
                if (node.Arguments.Count > 1)
                    Visit(node.Arguments[1]);
                _sql.Append(')');
                break;
            case "Min":
                _sql.Append("MIN(");
                if (node.Arguments.Count > 1)
                    Visit(node.Arguments[1]);
                _sql.Append(')');
                break;
            case "Average":
                _sql.Append("AVG(");
                if (node.Arguments.Count > 1)
                    Visit(node.Arguments[1]);
                _sql.Append(')');
                break;
        }
    }

    private static bool IsLinqAggregateMethod(MethodCallExpression node)
    {
        var supportedMethods = new[] { "Count", "Sum", "Max", "Min", "Average" };
        return node.Method.DeclaringType == typeof(Queryable) &&
               supportedMethods.Contains(node.Method.Name);
    }

    private void AddParameter(object? value)
    {
        var parameterName = $"@p{_parameterIndex++}";
        _parameters.Add(new SqlParameter(parameterName, value ?? DBNull.Value));
        _sql.Append(parameterName);
    }

    private static object? GetMemberValue(MemberExpression memberExpression)
    {
#pragma warning disable CA1031 // Do not catch general exception types
        try
        {
            if (memberExpression.Expression is ConstantExpression constant)
            {
                return memberExpression.Member switch
                {
                    FieldInfo field => field.GetValue(constant.Value),
                    PropertyInfo property => property.GetValue(constant.Value),
                    _ => null
                };
            }

            var lambda = Expression.Lambda(memberExpression);
            var compiled = lambda.Compile();
            return compiled.DynamicInvoke();
        }
        catch
        {
            return null;
        }
#pragma warning restore CA1031 // Do not catch general exception types
    }

    private static object? GetExpressionValue(Expression expression)
    {
#pragma warning disable CA1031 // Do not catch general exception types
        try
        {
            if (expression is ConstantExpression constant)
                return constant.Value;

            var lambda = Expression.Lambda(expression);
            var compiled = lambda.Compile();
            return compiled.DynamicInvoke();
        }
        catch
        {
            return null;
        }
#pragma warning restore CA1031 // Do not catch general exception types
    }

    private static string GetSqlOperator(ExpressionType nodeType) => nodeType switch
    {
        ExpressionType.Equal => "=",
        ExpressionType.NotEqual => "<>",
        ExpressionType.GreaterThan => ">",
        ExpressionType.GreaterThanOrEqual => ">=",
        ExpressionType.LessThan => "<",
        ExpressionType.LessThanOrEqual => "<=",
        ExpressionType.AndAlso => "AND",
        ExpressionType.OrElse => "OR",
        ExpressionType.Add => "+",
        ExpressionType.Subtract => "-",
        ExpressionType.Multiply => "*",
        ExpressionType.Divide => "/",
        _ => throw new NotSupportedException($"Operator {nodeType} is not supported")
    };

#pragma warning disable CA1308 // Normalize strings to uppercase
    private static string GetDefaultAlias(Type type, int parameterIndex)
    {
        // Generate single character aliases: u, o, c, etc.
        if (parameterIndex == 0) return type.Name.ToLowerInvariant()[0].ToString();
        return $"{type.Name.ToLowerInvariant()[0]}{parameterIndex}";
    }

#pragma warning restore CA1308 // Normalize strings to uppercase
    private static string GetColumnName(MemberInfo member)
    {
        var columnAttribute = member.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.ColumnAttribute>();
        return columnAttribute?.Name ?? member.Name;
    }
}