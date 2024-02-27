using Aspose.Pdf.Operators;
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

        public JObject CreateDocumentJObject(Stream documentStream, bool transformToPDF = true)
        {
            JObject documentObject = new JObject();
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

            MemoryStream fileStream = new MemoryStream();
            if (transformToPDF)
            {
                document.Save(fileStream, SaveFormat.Pdf);
            }
            else
            {
                document.Save(fileStream, SaveFormat.Docx);
            }
            fileStream.Position = 0;
            byte[] fileBytes = fileStream.ToArray();
            string base64String = Convert.ToBase64String(fileBytes);
            documentObject.Add("documentContent", pageText);
            documentObject.Add("documentBase64", base64String);

            return documentObject;
        }
    }
}
