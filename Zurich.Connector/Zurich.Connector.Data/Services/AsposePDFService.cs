using Aspose.Pdf;
using Aspose.Pdf.Text;
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
    public class AsposePDFService : IAsposeService
    {
        public AsposePDFService()
        {

        }

        public JObject CreateJObject(Stream documentStream)
        {
            Document document = new Document(documentStream);
            JObject pageText = new JObject();
            
            int i = 0;

            if(document.Pages.Count > 0)
            {
                foreach (var page in document.Pages)
                {
                    TextAbsorber textAbsorber = new TextAbsorber();
                    page.Accept(textAbsorber);
                    pageText.Add((i + 1).ToString(), textAbsorber.Text.Replace("\t", "").Replace("\n", "").Replace("\r", ""));
                    i++;
                }
            }

            return pageText;
        }
    }
}
