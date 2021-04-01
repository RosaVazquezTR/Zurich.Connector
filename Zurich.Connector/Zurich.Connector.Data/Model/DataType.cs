using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zurich.Connector.Data.Model
{
    /// <summary>
    /// Used to describe the available DataTypes from the DataMapping Repo
    /// </summary>
    public enum DataType
    {
        None,
        Document,
        Matters,
        Search,
        UserInfo
    }
}
