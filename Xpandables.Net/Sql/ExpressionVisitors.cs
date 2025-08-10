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
    /// Registers parameter aliases based on expression parameters, using their actual names.
    /// </summary>
    /// <param name="expression">The lambda expression containing parameters.</param>
    public void RegisterParameterAliases(LambdaExpression expression)
    {
        foreach (var param in expression.Parameters)
        {
            // Use the actual parameter name from the expression
            _parameterAliases[param] = param.Name!;
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

        // Handle standalone boolean expressions
        if (expression.Type == typeof(bool) && ShouldConvertBooleanToEquality(expression))
        {
            var booleanValue = DetermineBooleanValue(expression, out var memberExpression);
            if (memberExpression != null)
            {
                Visit(memberExpression);
                _sql.Append(" = ");
                AddParameter(booleanValue);
            }
            else
            {
                Visit(expression);
            }
        }
        else
        {
            Visit(expression);
        }

        return _sql.ToString();
    }

    /// <summary>
    /// Adds a parameter to the SQL query.
    /// </summary>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="value">The value of the parameter.</param>
    public void AddParameter(string name, object? value) =>
        _parameters.Add(new SqlParameter(name, value ?? DBNull.Value));

    private static bool ShouldConvertBooleanToEquality(Expression expression) =>
        expression switch
        {
            // Direct boolean property access: u.IsActive
            MemberExpression member when member.Expression is ParameterExpression => true,

            // Negated boolean property: !u.IsActive
            UnaryExpression { NodeType: ExpressionType.Not } unary
                when unary.Operand is MemberExpression member &&
                     member.Expression is ParameterExpression => true,

            // Boolean binary expressions should not be converted
            BinaryExpression => false,

            _ => false
        };

    private static bool DetermineBooleanValue(Expression expression, out MemberExpression? memberExpression)
    {
        memberExpression = null;

        switch (expression)
        {
            // Direct boolean property access: u.IsActive -> u.IsActive = true
            case MemberExpression member when member.Expression is ParameterExpression:
                memberExpression = member;
                return true;

            // Negated boolean property: !u.IsActive -> u.IsActive = false
            case UnaryExpression { NodeType: ExpressionType.Not } unary
                when unary.Operand is MemberExpression mem &&
                     mem.Expression is ParameterExpression:
                memberExpression = (MemberExpression)unary.Operand;
                return false;

            default:
                return true;
        }
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
            // Use the actual parameter name as the alias
            var tableAlias = _parameterAliases.TryGetValue(parameter, out var value) ? value : parameter.Name;
            _sql.Append(CultureInfo.InvariantCulture, $"[{tableAlias}].[{columnName}]");
        }
        else if (node.Expression is ConstantExpression or MemberExpression)
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

            // Store the current SQL length to get the column expression
            var startLength = _sql.Length;
            Visit(node.Arguments[i]);
            var columnExpression = _sql.ToString(startLength, _sql.Length - startLength);

            // Add alias only if it's different from the column name or if it's needed
            if (node.Members?[i] != null)
            {
                var aliasName = node.Members[i].Name;

                // Check if we need an alias by comparing with the actual column name
                if (ShouldAddAlias(node.Arguments[i], aliasName, columnExpression))
                {
                    _sql.Append(CultureInfo.InvariantCulture, $" AS [{aliasName}]");
                }
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
                // Store the current SQL length to get the column expression
                var startLength = _sql.Length;
                Visit(assignment.Expression);
                var columnExpression = _sql.ToString(startLength, _sql.Length - startLength);

                var aliasName = assignment.Member.Name;

                // Check if we need an alias
                if (ShouldAddAlias(assignment.Expression, aliasName, columnExpression))
                {
                    _sql.Append(CultureInfo.InvariantCulture, $" AS [{aliasName}]");
                }
            }
        }

        return node;
    }

    private static bool ShouldAddAlias(Expression expression, string aliasName, string _)
    {
        // If it's a member expression accessing a property/field
        if (expression is MemberExpression memberExpr &&
            memberExpr.Expression is ParameterExpression)
        {
            // Get the actual column name from the member
            var actualColumnName = GetColumnName(memberExpr.Member);

            // Only add alias if the alias name is different from the actual column name
            return !string.Equals(aliasName, actualColumnName, StringComparison.OrdinalIgnoreCase);
        }

        // For complex expressions, method calls, etc., always add alias
        return true;
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

    private static string GetColumnName(MemberInfo member)
    {
        var columnAttribute = member.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.ColumnAttribute>();
        return columnAttribute?.Name ?? member.Name;
    }

    protected override Expression VisitUnary(UnaryExpression node)
    {
        switch (node.NodeType)
        {
            case ExpressionType.Not:
                // Check if this is a boolean property negation that should be handled at a higher level
                if (node.Operand is MemberExpression member &&
                    member.Expression is ParameterExpression &&
                    node.Type == typeof(bool))
                {
                    // This case is handled by VisitAndGenerateSql for standalone boolean expressions
                    // If we reach here, it means we're in a nested context
                    _sql.Append("(NOT ");
                    Visit(node.Operand);
                    _sql.Append(')');
                }
                else
                {
                    _sql.Append("(NOT ");
                    Visit(node.Operand);
                    _sql.Append(')');
                }
                break;
            case ExpressionType.Negate:
                _sql.Append("(-");
                Visit(node.Operand);
                _sql.Append(')');
                break;
            case ExpressionType.Convert:
            case ExpressionType.ConvertChecked:
                // For type conversions, just visit the operand
                Visit(node.Operand);
                break;
            default:
                // For other unary expressions, try to evaluate them
                var value = GetExpressionValue(node);
                AddParameter(value);
                break;
        }

        return node;
    }
}