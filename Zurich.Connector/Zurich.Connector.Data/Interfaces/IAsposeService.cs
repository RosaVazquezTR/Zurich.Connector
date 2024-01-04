using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zurich.Connector.Data.Interfaces
{
    public interface IAsposeService
    {
        //Define any other needed operation in here, ie: Convert
        public JObject CreateJObject(Stream documentStream);
    }
}
