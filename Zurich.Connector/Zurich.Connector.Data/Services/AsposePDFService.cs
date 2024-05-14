using Aspose.Pdf;
using Aspose.Pdf.Operators;
using Aspose.Pdf.Text;
using Microsoft.Rest.Azure;
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

        public JObject CreateDocumentJObject(Stream documentStream, bool transformToPDF = true)
        {
            JObject documentObject = new();

            using (Document document = new(documentStream))
            {
                int total = document.Pages.Count;
                int firstHalf = total / 2;

                Document firstHalfDocument = new();
                firstHalfDocument.Pages.Add(document.Pages.Take(firstHalf).ToArray());

                Document secondHalfDocument = new();
                secondHalfDocument.Pages.Add(document.Pages.Skip(firstHalf).ToArray());

                JObject firstHalfObjects = new();
                JObject secondHalfObjects = new();

                Parallel.Invoke(
                    () => ExtractText(firstHalfDocument, firstHalfObjects, 1),
                    () => ExtractText(secondHalfDocument, secondHalfObjects, firstHalf + 1)
                );

                firstHalfObjects.Merge(secondHalfObjects, new JsonMergeSettings
                {
                    MergeArrayHandling = MergeArrayHandling.Union,
                    MergeNullValueHandling = MergeNullValueHandling.Merge
                });

                documentObject.Add("documentContent", firstHalfObjects);

                if (transformToPDF)
                {
                    byte[] pdfBytes = ConvertDocumentToPdf(document);
                    documentObject.Add("documentBase64", Convert.ToBase64String(pdfBytes));
                }
            }

            return documentObject;
        }

        private static void ExtractText(Document document, JObject jObject, int startPage)
        {
            for (int i = 0; i < document.Pages.Count; i++)
            {
                TextAbsorber textAbsorber = new();
                document.Pages[i + 1].Accept(textAbsorber);
                jObject.Add(startPage.ToString(), textAbsorber.Text.Replace("\t", "").Replace("\n", "").Replace("\r", ""));
                startPage++;
            }
        }

        private static byte[] ConvertDocumentToPdf(Document document)
        {
            using MemoryStream pdfStream = new();
            document.Save(pdfStream, SaveFormat.Pdf);
            return pdfStream.ToArray();
        }
    }
}
