using Enums.Sql.Tokens;
using Sql.Expressions;
using Sql.Tokens;

namespace Sql;

internal static class Extensions
{
    public static bool IsNumeric(this string s)
    {
        return double.TryParse(s, out _);
    }
    
    public static int IndexOf(this SqlToken[] tokens, SqlToken? token) => Array.IndexOf(tokens, token);

    public static int IndexOf(this SqlToken[] tokens, Keyword kw) =>
        tokens.IndexOf(tokens.FirstOrDefault(t => t.IsKeyword(kw)));

    public static SqlToken[][] Split(this SqlToken[] collection, OperatorType op, out SqlToken[] operators)
    {
        int operatorsCount = collection.Count(t => t.IsOperator(op));

        List<ConditionExpression> conditions = [];
        List<SqlToken> oneConditionTokens = [];
        List<SqlToken> logicalOperators = [];

        for (int i = 0; i < collection.Length; i++)
        {
            oneConditionTokens.Add(collection[i]);

            if (collection[i].IsOperator(OperatorType.LogicalOperator))
            {
                conditions.Add(parseCondition(oneConditionTokens));
                oneConditionTokens.Clear();
                logicalOperators.Add(collection[i]);
            }
            else if (i == collection.Length - 1)
            {
                conditions.Add(parseCondition(oneConditionTokens));
            }
        }

        ConditionExpression parseCondition(IReadOnlyList<SqlToken> tokens) =>
            new(
                tokens[0],
                tokens[2],
                tokens[1]);
    }
}