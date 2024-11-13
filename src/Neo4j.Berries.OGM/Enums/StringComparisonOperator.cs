using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neo4j.Berries.OGM.Enums
{
    public enum StringComparisonOperator
    {
        /// <summary>
        /// Translates to <c> a CONTAINS b </c>
        /// </summary>
        Contains,
        /// <summary>
        /// Translates to <c> a STARTS WITH b </c>
        /// </summary>
        StartsWith,
        /// <summary>
        /// Translates to <c> a ENDS WITH b </c>
        /// </summary>
        EndsWith,
        /// <summary>
        /// Translates to <c> a IS NORMALIZED </c>
        /// </summary>
        IsNormalized
    }
}
