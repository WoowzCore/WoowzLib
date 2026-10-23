using System.Linq.Expressions;
using WLI;
using WLO;

namespace WL;

public static class SQL{
        public static (string Sql, Dictionary<string, object?> Parameters) Translate<T>(Expression<Func<T, bool>> Predicate, Database Owner){
        Dictionary<string, object?> Parameters = [];
        string Sql = __Visit(Predicate.Body, Owner, Parameters);
        return (Sql, Parameters);
    }

    public static string GetColumnName<T>(Expression<Func<T, object>> Selector, Database Owner){
        Expression Body = Selector.Body;
        if(Body is UnaryExpression Unary && Unary.NodeType == ExpressionType.Convert){ Body = Unary.Operand; }

        if(Body is MemberExpression Member){ return Owner.QuoteIdentifier(Member.Member.Name); }

        throw new ExceptionWL($"Не удалось получить имя колонки из выражения: {Selector}");
    }

    private static string __Visit(Expression Node, Database Owner, Dictionary<string, object?> Parameters){
        switch(Node){
            case BinaryExpression Binary:
                return __VisitBinary(Binary, Owner, Parameters);

            case UnaryExpression Unary when Unary.NodeType == ExpressionType.Not:
                return $"NOT ({__Visit(Unary.Operand, Owner, Parameters)})";

            case UnaryExpression Unary when Unary.NodeType == ExpressionType.Convert:
                return __Visit(Unary.Operand, Owner, Parameters);

            case MemberExpression Member when Member.Expression?.NodeType == ExpressionType.Parameter:
                return Owner.QuoteIdentifier(Member.Member.Name);

            case MethodCallExpression Call:
                return __VisitMethodCall(Call, Owner, Parameters);

            case ConstantExpression:
            case MemberExpression:{
                object? Value = __EvaluateValue(Node);
                string ParamName = $"p{Parameters.Count}";
                Parameters[ParamName] = Value;
                return Owner.ParameterPrefix + ParamName;
            }

            default:
                throw new ExceptionWL($"Выражение не поддерживается: {Node.NodeType}");
        }
    }

    private static string __VisitBinary(BinaryExpression Binary, Database Owner, Dictionary<string, object?> Parameters){
        if(Binary.NodeType == ExpressionType.AndAlso){
            return $"({__Visit(Binary.Left, Owner, Parameters)} AND {__Visit(Binary.Right, Owner, Parameters)})";
        }
        if(Binary.NodeType == ExpressionType.OrElse){
            return $"({__Visit(Binary.Left, Owner, Parameters)} OR {__Visit(Binary.Right, Owner, Parameters)})";
        }

        if(Binary.NodeType == ExpressionType.Equal && __IsNullConstant(Binary.Right)){
            return $"{__Visit(Binary.Left, Owner, Parameters)} IS NULL";
        }
        if(Binary.NodeType == ExpressionType.NotEqual && __IsNullConstant(Binary.Right)){
            return $"{__Visit(Binary.Left, Owner, Parameters)} IS NOT NULL";
        }

        string Left  = __Visit(Binary.Left, Owner, Parameters);
        string Right = __Visit(Binary.Right, Owner, Parameters);

        string Op = Binary.NodeType switch{
            ExpressionType.Equal              => "=",
            ExpressionType.NotEqual           => "!=",
            ExpressionType.GreaterThan        => ">",
            ExpressionType.GreaterThanOrEqual => ">=",
            ExpressionType.LessThan           => "<",
            ExpressionType.LessThanOrEqual    => "<=",
            var _ => throw new ExceptionWL($"Оператор не поддерживается: {Binary.NodeType}")
        };

        return $"{Left} {Op} {Right}";
    }

    private static bool __IsNullConstant(Expression Node) => Node is ConstantExpression{ Value: null };

    private static string __VisitMethodCall(MethodCallExpression Call, Database Owner, Dictionary<string, object?> Parameters){
        if(Call.Object != null && Call.Method.DeclaringType == typeof(string)){
            string Column = __Visit(Call.Object, Owner, Parameters);
            object? Arg = __EvaluateValue(Call.Arguments[0]);
            string ParamName = $"p{Parameters.Count}";

            string Pattern = Call.Method.Name switch{
                "Contains"   => $"%{Arg}%",
                "StartsWith" => $"{Arg}%",
                "EndsWith"   => $"%{Arg}",
                var _ => throw new ExceptionWL($"Метод строки не поддерживается: {Call.Method.Name}")
            };

            Parameters[ParamName] = Pattern;
            return $"{Column} LIKE {Owner.ParameterPrefix + ParamName}";
        }

        throw new ExceptionWL($"Вызов метода не поддерживается: {Call.Method.Name}");
    }

    private static object? __EvaluateValue(Expression Node){
        if(Node is ConstantExpression Constant){ return Constant.Value; }

        UnaryExpression Boxed = Expression.Convert(Node, typeof(object));
        return Expression.Lambda<Func<object?>>(Boxed).Compile().Invoke();
    }
}