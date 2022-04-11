using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Runtime.Serialization;

namespace Zurich.Connector.Web.Enum
{
    /// <summary>
    /// Enum that represents different sorting type options
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SortType
    {
        Date,
        Alphabetical,
        Relevance
    }
}
