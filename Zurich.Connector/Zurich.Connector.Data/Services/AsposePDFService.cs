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
            JObject documentObject = new JObject();
            Document document = new Document(documentStream);
            JObject pageText = new JObject();
            List<Document> documents = new List<Document>();

            Document firstHalfDocument = new Document();
            Document secondHalfDocument = new Document();

            int total = document.Pages.Count;
            int firstHalf = total / 2;
            int secondHalf = total - firstHalf;

            if (document.Pages.Count > 0)
            {
                for (int i = 1; i < firstHalf; i++)
                {
                    firstHalfDocument.Pages.Add(document.Pages[i]);
                }
                for (int j = firstHalf; j < total + 1; j++)
                {
                    secondHalfDocument.Pages.Add(document.Pages[j]);
                }
            }

            JObject firstHalfObjects = new JObject();
            JObject secondHalfObjects = new JObject();

            Parallel.Invoke(
                () => ExtractText(firstHalfDocument, firstHalfObjects, 1),
                () => ExtractText(secondHalfDocument, secondHalfObjects, secondHalf)
             );

            firstHalfObjects.Merge(secondHalfObjects, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Union,
                MergeNullValueHandling = MergeNullValueHandling.Merge
            });

            MemoryStream pdfStream = new MemoryStream();
            document.Save(pdfStream, SaveFormat.Pdf);
            byte[] pdfBytes = pdfStream.ToArray();
            string base64String = Convert.ToBase64String(pdfBytes);
            documentObject.Add("documentContent", firstHalfObjects);
            documentObject.Add("documentBase64", base64String);

            return documentObject;
        }

        public void ExtractText(Document document, JObject jObject, int startingCount)
        {
            TextAbsorber textAbsorber = new TextAbsorber();

            for (int i = 0; i < document.Pages.Count; i++)
            {
                textAbsorber = new TextAbsorber();
                document.Pages[i + 1].Accept(textAbsorber);
                jObject.Add(startingCount.ToString(), textAbsorber.Text.Replace("\t", "").Replace("\n", "").Replace("\r", ""));
                startingCount++;
            }

            
        }
    }
}
