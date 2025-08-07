//=====================================================
// Copyright (c) 2024 Francis-Black EWANE
//=====================================================

using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Xpandables.Net.Repositories.SqlBuilder;

/// <summary>
/// Parses LINQ expressions into SQL expressions.
/// </summary>
internal static class ExpressionParser
{
    /// <summary>
    /// Parses a predicate expression into a SQL WHERE condition.
    /// </summary>
    /// <param name="expression">The predicate expression.</param>
    /// <param name="queryModel">The query model for parameter management.</param>
    /// <returns>The SQL WHERE condition.</returns>
    public static string ParseWhereExpression(LambdaExpression expression, QueryModel queryModel)
    {
        ArgumentNullException.ThrowIfNull(expression);
        ArgumentNullException.ThrowIfNull(queryModel);
        
        return ParseExpression(expression.Body, queryModel);
    }
    
    /// <summary>
    /// Parses a select expression for column selection.
    /// </summary>
    /// <param name="expression">The select expression.</param>
    /// <returns>The SQL column expression.</returns>
    public static string ParseSelectExpression(LambdaExpression expression)
    {
        ArgumentNullException.ThrowIfNull(expression);
        
        return ParseColumnExpression(expression.Body);
    }
    
    /// <summary>
    /// Parses a GROUP BY expression.
    /// </summary>
    /// <param name="expression">The GROUP BY expression.</param>
    /// <returns>The SQL GROUP BY expression.</returns>
    public static string ParseGroupByExpression(LambdaExpression expression)
    {
        ArgumentNullException.ThrowIfNull(expression);
        
        if (expression.Body is NewExpression newExpression)
        {
            var columns = newExpression.Arguments
                .Select(ParseColumnExpression)
                .ToList();
            return string.Join(", ", columns);
        }
        
        return ParseColumnExpression(expression.Body);
    }
    
    /// <summary>
    /// Parses an ORDER BY expression.
    /// </summary>
    /// <param name="expression">The ORDER BY expression.</param>
    /// <returns>The SQL ORDER BY expression.</returns>
    public static string ParseOrderByExpression(LambdaExpression expression)
    {
        ArgumentNullException.ThrowIfNull(expression);
        
        return ParseColumnExpression(expression.Body);
    }
    
    private static string ParseExpression(Expression expression, QueryModel queryModel)
    {
        return expression switch
        {
            BinaryExpression binaryExp => ParseBinaryExpression(binaryExp, queryModel),
            UnaryExpression unaryExp => ParseUnaryExpression(unaryExp, queryModel),
            MemberExpression memberExp => ParseMemberExpression(memberExp),
            ConstantExpression constantExp => ParseConstantExpression(constantExp, queryModel),
            MethodCallExpression methodExp => ParseMethodCallExpression(methodExp, queryModel),
            ConditionalExpression condExp => ParseConditionalExpression(condExp, queryModel),
            NewArrayExpression newArrayExp => ParseNewArrayExpression(newArrayExp, queryModel),
            _ => throw new NotSupportedException($"Expression type {expression.NodeType} is not supported")
        };
    }
    
    private static string ParseColumnExpression(Expression expression)
    {
        return expression switch
        {
            MemberExpression memberExp => ParseMemberExpression(memberExp),
            UnaryExpression { Operand: MemberExpression memberExp2 } => ParseMemberExpression(memberExp2),
            NewExpression newExp => ParseNewExpression(newExp),
            _ => throw new NotSupportedException($"Column expression type {expression.NodeType} is not supported")
        };
    }
    
    private static string ParseBinaryExpression(BinaryExpression expression, QueryModel queryModel)
    {
        var left = ParseExpression(expression.Left, queryModel);
        var right = ParseExpression(expression.Right, queryModel);
        
        var operatorString = expression.NodeType switch
        {
            ExpressionType.Equal => HandleNullComparison(right, "=", "IS"),
            ExpressionType.NotEqual => HandleNullComparison(right, "!=", "IS NOT"),
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
            ExpressionType.Modulo => "%",
            _ => throw new NotSupportedException($"Binary operator {expression.NodeType} is not supported")
        };
        
        return expression.NodeType is ExpressionType.AndAlso or ExpressionType.OrElse
            ? $"({left}) {operatorString} ({right})"
            : $"{left} {operatorString} {right}";
    }
    
    private static string HandleNullComparison(string right, string normalOp, string nullOp)
    {
        return right == "NULL" ? nullOp : normalOp;
    }
    
    private static string ParseUnaryExpression(UnaryExpression expression, QueryModel queryModel)
    {
        return expression.NodeType switch
        {
            ExpressionType.Not => $"NOT ({ParseExpression(expression.Operand, queryModel)})",
            ExpressionType.Convert => ParseExpression(expression.Operand, queryModel),
            ExpressionType.Quote => ParseExpression(expression.Operand, queryModel),
            _ => throw new NotSupportedException($"Unary operator {expression.NodeType} is not supported")
        };
    }
    
    private static string ParseMemberExpression(MemberExpression expression)
    {
        var memberName = expression.Member.Name;
        
        if (expression.Expression is ParameterExpression parameter)
        {
            return $"[{parameter.Name}].[{memberName}]";
        }
        
        if (expression.Expression is MemberExpression parentMember)
        {
            var parentExpression = ParseMemberExpression(parentMember);
            return $"{parentExpression}.{memberName}";
        }
        
        return $"[{memberName}]";
    }
    
    private static string ParseConstantExpression(ConstantExpression expression, QueryModel queryModel)
    {
        if (expression.Value is null)
        {
            return "NULL";
        }
        
        var paramName = queryModel.AddParameter($"p{queryModel.Parameters.Count}", expression.Value);
        return paramName;
    }
    
    private static string ParseMethodCallExpression(MethodCallExpression expression, QueryModel queryModel)
    {
        var methodName = expression.Method.Name;
        var objectExpression = expression.Object;
        
        // Handle static SQL functions
        if (expression.Method.DeclaringType == typeof(SqlFunction))
        {
            return ParseSqlFunctionMethod(expression, queryModel);
        }
        
        return methodName switch
        {
            "Contains" => ParseContainsMethod(expression, queryModel),
            "StartsWith" => ParseStartsWithMethod(expression, queryModel),
            "EndsWith" => ParseEndsWithMethod(expression, queryModel),
            "ToUpper" => ParseToUpperMethod(expression),
            "ToLower" => ParseToLowerMethod(expression),
            "Trim" => ParseTrimMethod(expression),
            "IsNullOrEmpty" => ParseIsNullOrEmptyMethod(expression, queryModel),
            "HasValue" => ParseHasValueMethod(expression),
            _ => throw new NotSupportedException($"Method {methodName} is not supported")
        };
    }
    
    private static string ParseSqlFunctionMethod(MethodCallExpression expression, QueryModel queryModel)
    {
        var methodName = expression.Method.Name;
        
        return methodName switch
        {
            "Count" when expression.Arguments.Count == 0 => "COUNT(*)",
            "Count" when expression.Arguments.Count == 1 => 
                $"COUNT({ParseExpression(expression.Arguments[0], queryModel)})",
            "Sum" => $"SUM({ParseExpression(expression.Arguments[0], queryModel)})",
            "Avg" => $"AVG({ParseExpression(expression.Arguments[0], queryModel)})",
            "Min" => $"MIN({ParseExpression(expression.Arguments[0], queryModel)})",
            "Max" => $"MAX({ParseExpression(expression.Arguments[0], queryModel)})",
            _ => throw new NotSupportedException($"SQL function {methodName} is not supported")
        };
    }
    
    private static string ParseContainsMethod(MethodCallExpression expression, QueryModel queryModel)
    {
        if (expression.Object is not null)
        {
            // String.Contains
            var target = ParseExpression(expression.Object, queryModel);
            var value = ParseExpression(expression.Arguments[0], queryModel);
            return $"{target} LIKE '%' + {value} + '%'";
        }
        
        // Collection.Contains (IN operator)
        var member = ParseExpression(expression.Arguments[0], queryModel);
        
        // Handle array/list contains
        if (expression.Arguments.Count > 1)
        {
            var collection = expression.Arguments[1];
            return ParseInExpression(member, collection, queryModel);
        }
        
        // Handle extension method Contains (e.g., someArray.Contains(x))
        if (expression.Method.DeclaringType == typeof(System.Linq.Enumerable))
        {
            var collection = expression.Arguments[0];
            var item = ParseExpression(expression.Arguments[1], queryModel);
            return ParseInExpression(item, collection, queryModel);
        }
        
        throw new NotSupportedException("Contains with non-constant collections is not supported");
    }
    
    private static string ParseInExpression(string member, Expression collection, QueryModel queryModel)
    {
        if (collection is ConstantExpression { Value: System.Collections.IEnumerable values })
        {
            var parameters = new List<string>();
            foreach (var value in values)
            {
                var paramName = queryModel.AddParameter($"p{queryModel.Parameters.Count}", value);
                parameters.Add(paramName);
            }
            
            return parameters.Count == 0 ? "1=0" : $"{member} IN ({string.Join(", ", parameters)})";
        }
        
        // Handle member access to collections
        if (collection is MemberExpression memberAccess)
        {
            // For now, we can't evaluate complex member expressions at compile time
            throw new NotSupportedException("Dynamic collection evaluation is not supported. Use constant collections only.");
        }
        
        throw new NotSupportedException("Contains with non-constant collections is not supported");
    }
    
    private static string ParseStartsWithMethod(MethodCallExpression expression, QueryModel queryModel)
    {
        var target = ParseExpression(expression.Object!, queryModel);
        var value = ParseExpression(expression.Arguments[0], queryModel);
        return $"{target} LIKE {value} + '%'";
    }
    
    private static string ParseEndsWithMethod(MethodCallExpression expression, QueryModel queryModel)
    {
        var target = ParseExpression(expression.Object!, queryModel);
        var value = ParseExpression(expression.Arguments[0], queryModel);
        return $"{target} LIKE '%' + {value}";
    }
    
    private static string ParseToUpperMethod(MethodCallExpression expression)
    {
        var target = ParseColumnExpression(expression.Object!);
        return $"UPPER({target})";
    }
    
    private static string ParseToLowerMethod(MethodCallExpression expression)
    {
        var target = ParseColumnExpression(expression.Object!);
        return $"LOWER({target})";
    }
    
    private static string ParseTrimMethod(MethodCallExpression expression)
    {
        var target = ParseColumnExpression(expression.Object!);
        return $"LTRIM(RTRIM({target}))";
    }
    
    private static string ParseIsNullOrEmptyMethod(MethodCallExpression expression, QueryModel queryModel)
    {
        var target = ParseExpression(expression.Arguments[0], queryModel);
        return $"({target} IS NULL OR {target} = '')";
    }
    
    private static string ParseHasValueMethod(MethodCallExpression expression)
    {
        var target = ParseColumnExpression(expression.Object!);
        return $"{target} IS NOT NULL";
    }
    
    private static string ParseConditionalExpression(ConditionalExpression expression, QueryModel queryModel)
    {
        var test = ParseExpression(expression.Test, queryModel);
        var ifTrue = ParseExpression(expression.IfTrue, queryModel);
        var ifFalse = ParseExpression(expression.IfFalse, queryModel);
        
        return $"CASE WHEN {test} THEN {ifTrue} ELSE {ifFalse} END";
    }
    
    private static string ParseNewArrayExpression(NewArrayExpression expression, QueryModel queryModel)
    {
        var values = expression.Expressions
            .Select(expr => ParseExpression(expr, queryModel))
            .ToList();
            
        return string.Join(", ", values);
    }
    
    private static string ParseNewExpression(NewExpression expression)
    {
        var columns = expression.Arguments
            .Select(ParseColumnExpression)
            .ToList();
            
        return string.Join(", ", columns);
    }
}