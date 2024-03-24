using System.Globalization;
using System.Text;

namespace Neo4j.Berries.OGM.Utils;

public static class StringUtils {
    /// <summary>
    /// Append the given inputs to the builder, each on a new line. The returned string builder is a clone of the passed string builder.
    /// </summary>
    public static StringBuilder AppendLines(this StringBuilder builder, params string[] inputs) {
        var clone = builder.Clone();
        foreach (var input in inputs) {
            clone.AppendLine(input);
        }
        return clone;
    }

    public static StringBuilder Clone(this StringBuilder input) {
        return new StringBuilder(input.ToString());
    }
}