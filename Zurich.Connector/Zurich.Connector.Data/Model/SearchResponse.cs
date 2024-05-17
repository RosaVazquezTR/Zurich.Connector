using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zurich.Connector.Data.Model
{
    public class SearchResponse
    {
        public int Count { get; set; }
        public List<SearchItemEntity> Documents { get; set; }
        public Error Error { get; set; }
    }

    public class SearchItemEntity
    {
        public string Title { get; set; }
        public string WebUrl { get; set; }
        public string Snippet { get; set; }
        public string Type { get; set; }
        public string CreationDate { get; set; }
        public string[] Snippets { get; set; }
        public Object AdditionalProperties { get; set; }
    }

    public class Error
    {
        public string Message { get; set; }
        public string StatusCode { get; set; }
    }
}
