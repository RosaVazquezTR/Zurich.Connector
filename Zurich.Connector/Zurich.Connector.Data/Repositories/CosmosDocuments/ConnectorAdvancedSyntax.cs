using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zurich.Connector.Data.Repositories.CosmosDocuments
{
    public class ConnectorAdvancedSyntax
    {
        /// <summary>
        /// Operators supported by the Connector
        /// </summary>
        public AdvancedSyntaxOperator Operators { get; set; }
    }

    public class AdvancedSyntaxOperator
    {
        /// <summary>
        /// String representation of the or operator
        /// </summary>
        public string And { get; set; }
        /// <summary>
        /// String representation of the or operator
        /// </summary>
        public string Or { get; set; }
        /// <summary>
        /// String representation of the textual ocurrence operator
        /// </summary>
        public string TextualOcurrence { get; set; }
        /// <summary>
        /// String representation of the NOT operator
        /// </summary>
        public string Not { get; set; }
        /// <summary>
        /// String representation of the Wildcard operator
        /// </summary>
        public string Wildcard { get; set; }
        /// <summary>
        /// String representation of the Proximity operator
        /// </summary>
        public string Proximity { get; set; }
        /// <summary>
        /// String representation of the TermGrouping operator
        /// </summary>
        public string TermGrouping { get; set; }
    }
}

