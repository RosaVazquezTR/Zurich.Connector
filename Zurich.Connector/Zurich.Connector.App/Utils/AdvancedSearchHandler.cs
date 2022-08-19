﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Zurich.Connector.App.Model;

namespace Zurich.Connector.App.Utils
{
    public class AdvancedSearchHandler
    {
        private static readonly AdvancedSyntaxOperatorModel _federatedSearchOperators = new()
        {
            And = "&",
            Or = "OR",
            Wildcard = "*",
            TextualOcurrence = "\"",
            Not = "%",
            TermGrouping = "(",
            Proximity = "/"
        };
        private static MatchCollection modifyedQuery;

        public static bool validateQuery(string query)
        {
            // There is no nesting of any operator. The text inside () or " " must be query terms.
            List<string> forbiddenOperators = new List<string>() { "%", "/", "*", "?", "\"", "(", ")" };
            MatchCollection enclosedTexts = Regex.Matches(query, @"\((.+?)\)|\" + (char)34 + @"(.+?)\" + (char)34); //Gets the enclosed text in () and ""
            foreach (Match text in enclosedTexts)
            {
                if (forbiddenOperators.Any(s => text.Groups[1].Value.Contains(s)) || forbiddenOperators.Any(s => text.Groups[2].Value.Contains(s)))
                    return false;
            }

            // The () and "" operators strictly open and close. 
            if (query.Count(f => (f == '(')) != query.Count(f => (f == ')')) || query.Count(f => (f == '"')) % 2 != 0)
                return false;

            // Not operator % must be followed by a term with no space
            int numOfNots = query.Count(f => (f == '%'));
            if (numOfNots > 0 & (numOfNots != Regex.Matches(query, @"\%[a-zA-Z\d]+").Count) )
                return false;

            // Proximity operator must be followed by an integer  @
            int numOfProx = query.Count(f => (f == '/'));
            if (numOfProx > 0 & (numOfProx != Regex.Matches(query, @"\/\d+").Count))
                return false;

            return true;
        }
        public static string HandleOperator(string query, ConnectorModel connectorModel)
        {
            // MsGraph connector adds some system parameters through query, we should not modify them
            string systemQuery = "";

            if (connectorModel.Id == "14")
            {
                modifyedQuery = Regex.Matches(query, @"\((.+?)\)");
                if (modifyedQuery.Count > 0)
                {
                    systemQuery = modifyedQuery[1].Value;
                    query = query.Replace(systemQuery, "");
                    query = query.Substring(1, query.Length - 3);
                }
            }
            if (validateQuery(query))
            {
                string newQuery = query;
                foreach (var fedOperator in _federatedSearchOperators.GetType().GetProperties())
                {
                    //Our own operator, used in FedSearch
                    String fedSearchOperator = _federatedSearchOperators.GetType().GetProperty(fedOperator.Name).GetValue(_federatedSearchOperators,null).ToString().ToUpper();
                    //Operator used in connector, this will be the replace
                    String connectorOperator = connectorModel.AdvancedSearchSyntax.Operators.GetType().GetProperty(fedOperator.Name).GetValue(connectorModel.AdvancedSearchSyntax.Operators, null).ToString();
                    String operatorName = fedOperator.Name;

                    if ((fedSearchOperator.ToUpper() != (connectorOperator.ToUpper())) || fedSearchOperator == " & ")
                    {
                        // A simple replace of the fedSearch operator with the connector operator must work (with some considerations): 
                        newQuery = newQuery.Replace(fedSearchOperator, connectorOperator);

                        // At this first version, we follow the Westlaw/PracticalLaw convention of replacing the "&" chars by operators
                        // when its quoted and keeping it when its parentheses grouped:
                        // i.e. "Elephant & Castle" -> "Elephant AND castle"
                        //      (Elephant & Castle) ->  (Elephant & Castle)

                        if (operatorName == "And")
                        {
                            MatchCollection enclosedTexts = Regex.Matches(query, @"\" + (char)34 + @"(.+?)\" + (char)34); //Gets the enclosed text in ""
                            foreach (Match text in enclosedTexts)
                            {
                                newQuery = newQuery.Replace(text.Value.Replace("&", "AND"), text.Value);
                            }
                        }

                        // When the connector proximity operator is in format NEAR(n) 
                        if(operatorName == "Proximity" & connectorOperator.Contains("(") )
                        {
                            string pattern = Regex.Escape(connectorOperator) + @"(\d+)";
                            MatchCollection ocurrance = Regex.Matches(newQuery,pattern);

                            foreach (Match text in ocurrance)
                            {
                                string newoperator = connectorOperator.Replace("n", text.Groups[1].Value);
                                newQuery = newQuery.Replace(text.Value, newoperator);
                            }
                        }
                    }      
                }
                if (connectorModel.Id == "14") 
                    if (modifyedQuery.Count > 0)
                        return "(" + newQuery + ") " + systemQuery;
                return newQuery;
            }
            else
            {
                throw new Exception("Invalid query, unsupported operators.");
            }
        }
    }
}
