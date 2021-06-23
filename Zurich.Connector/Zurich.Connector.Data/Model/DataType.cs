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
        UserInfo,
        History,
        Favorites
    }

    /// <summary>
    /// Used to describe the available Authorization Types from the DataMapping Repo
    /// </summary>
    public enum AuthType
    {
        None,
        TransferToken,
        OAuth2
    }

    /// <summary>
    /// Used to describe the available Entity Types from the DataMapping Repo
    /// </summary>
    public enum EntityType
    {
        Document,
        History,
        Matters,
        UserProfile,
        Favorites,
        Search
    }

    /// <summary>
    /// /Represents Registration Mode
    /// </summary>
    public enum RegistrationEntityMode
    {
        Registered,
        AutoRegistered,
        ManualRegistered,
        None
    }
}
