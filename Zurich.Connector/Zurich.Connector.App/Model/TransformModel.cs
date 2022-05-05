using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zurich.Connector.App.Model
{
    /// <summary>
    /// Store transform details.
    /// </summary>
    public class TransformModel
    {
        /// <summary>
        /// Transform ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Transform Type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Data source id
        /// </summary>
        public string DataSourceId { get; set; }

        /// <summary>
        /// Transform details
        /// </summary>
        public string Transform { get; set; }
    }
}
