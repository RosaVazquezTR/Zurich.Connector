using Aspose.Words;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zurich.Connector.Data.Interfaces;

namespace Zurich.Connector.Data.Services
{
    public class AsposeWordService : IAsposeService
    {
        public AsposeWordService() 
        {

        }

        public JObject CreateJObject(Stream documentStream)
        {
            Document document = new Document(documentStream);
            JObject pageText = new JObject();
            var pageCount = document.PageCount;

            if (pageCount > 0)
            {
                for (int i = 0; i < pageCount; i++)
                {
                    Document newdoc = document.ExtractPages(i, 1);
                    pageText.Add((i + 1).ToString(), newdoc.ToString(SaveFormat.Text).Replace("\t", "").Replace("\n","").Replace("\r", ""));
                }
            }

            return pageText;
        }
    }
}
