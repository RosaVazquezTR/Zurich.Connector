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
        public List<AdvancedSyntaxOperator> Operators { get; set; }
    }

    public class AdvancedSyntaxOperator
    {
        /// <summary>
        /// Name of the operator
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// String representation of the connectors operator
        /// </summary>
        public string Value { get; set; }
    }
}

