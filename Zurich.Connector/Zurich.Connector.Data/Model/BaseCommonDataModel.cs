using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zurich.Connector.Data.Model
{
    /// <summary>
    /// Minimal requirement to be considered a CommonDataModel
    /// </summary>
    public class BaseCommonDataModel
    {
        /// <summary>
        /// Friendly Name of the object
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The Id used to reference
        /// </summary>
        public string Id { get; set; }
    }

    /// <summary>
    /// Additional Information needed for WebModels
    /// </summary>
    public class BaseCommonDataWebModel : BaseCommonDataModel
    {
        /// <summary>
        /// The link for the object
        /// </summary>
        public string WebLink { get; set; }
    }
}
