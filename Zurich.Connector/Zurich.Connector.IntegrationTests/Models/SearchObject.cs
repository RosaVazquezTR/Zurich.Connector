using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zurich.Common.Models.CommonDataModels;

namespace Zurich.Connector.IntegrationTests.Models
{
    public class SearchObject
    {
        public int Count { get; set; }

        public List<SearchEntity> Documents { get; set; }
    }
}
