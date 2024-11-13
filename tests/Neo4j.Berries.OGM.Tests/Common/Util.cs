using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Neo4j.Berries.OGM.Tests.Common
{
    public static class Util
    {
        public static string NormalizeWhitespace(this string input)
        {
            return input.Trim().Replace("\r\n", "\n").Replace(" ", "");
        }
    }
}
