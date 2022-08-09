using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zurich.Connector.App.Model
{
    public class ConnectorAdvancedSyntaxModel
    {
        /// <summary>
        /// Operators supported by the Connector
        /// </summary>
        public List<AdvancedSyntaxOperatorModel> Operators { get; set; }
    }

    public class AdvancedSyntaxOperatorModel
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
