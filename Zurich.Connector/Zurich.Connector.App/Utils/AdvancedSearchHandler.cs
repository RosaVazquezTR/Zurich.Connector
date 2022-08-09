using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Zurich.Connector.App.Utils
{
    public static class AdvancedSearchHandler
    {
        public static string HandleAnd(string query,string advancedOperator, string replacement)
        {
            string pattern = @"(?<!\(|\"")\b\s?" + advancedOperator + @"\s?\b(?![\w\s]*[\)|\""])";

            return Regex.Replace(query, pattern, " " + replacement + " ");
        }
    }
}
