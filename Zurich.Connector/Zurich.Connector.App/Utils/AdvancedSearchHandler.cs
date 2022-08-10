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
            if (advancedOperator.ToUpper()==(replacement.ToUpper())) {
                return query;
            }
            string pattern = @"(?<!\(|\"")\b\s?" + advancedOperator + @"\s?\b(?![\w\s]*[\)|\""])";

            return Regex.Replace(query, pattern, " " + replacement + " ");
        }
        public static string HandleOperator(string query, string advancedOperator, string replacement, string operatorName)
        {
            if (advancedOperator.ToUpper() == (replacement.ToUpper()))
            {
                return query;
            }
            String pattern = ""; //default pattern xxabcd
            if (operatorName == "And" || operatorName == "Or") { //xx and xx - xx or xx - xx)and(xx - xx)or(xx
                pattern = @"(?<!\(|\"")\b\s?" + advancedOperator + @"\s?\b(?![\w\s]*[\)|\""])";
            }
            else if (operatorName == "Proximity") {
                pattern = @"(?<!\(|\"")\b\s?" + advancedOperator + @"\s?\b(?![\w\s]*[\)|\""])";
            }
            return Regex.Replace(query, pattern, " " + replacement + " ");
        }
    }
}
