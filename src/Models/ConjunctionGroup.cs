namespace berrybeat.Neo4j.OGM.Models;

internal class ConjunctionGroup
{
    /// <summary>
    /// The conjunction can be AND, OR or XOR
    /// </summary>
    public string Conjunction { get; set; }
    public IEnumerable<ConjunctionGroupMember> Members { get; set; } = [];
    public string ToString(string alias)
    {
        var members = Members.Select(m =>
        {
            if (m.OverwriteOperatorFormat)
            {
                return string.Format(m.Operator, $"{alias}.{m.Operand1}", m.Operand2);
            }
            return $"{alias}.{m.Operand1} {m.Operator} {m.Operand2}";
        });
        if (members.Count() > 1)
        {
            return $"({string.Join($" {Conjunction} ", members)})";
        }
        return string.Join($" {Conjunction} ", members);
    }
}

internal record ConjunctionGroupMember(string Operand1, string Operator, string Operand2, bool OverwriteOperatorFormat = false);