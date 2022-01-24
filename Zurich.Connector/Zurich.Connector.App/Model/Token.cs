using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zurich.Connector.App.Model
{
    public class Token
{       /// <summary>
        /// The access token type ex) bearer
        /// </summary>
        public string TokenType { get; set; }

        /// <summary>
        /// The token to be used
        /// </summary>
        public string AccessToken { get; set; }
    }
}
