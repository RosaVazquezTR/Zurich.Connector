using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Zurich.Connector.Data.Interfaces;
using Zurich.Connector.Data.Utils;

namespace Zurich.Connector.Data.Services
{
    public class PdfService : IDocumentConversionService
    {
        public async Task<JObject> ConvertDocumentToJObjectAsync(Stream documentStream, bool transformToPdf = true)
        {
            JObject documentObject = [];

            using (PdfDocument document = new(new PdfReader(documentStream)))
            {
                documentObject.Add("documentContent", GetDocumentPages(document));

                if (transformToPdf)
                {
                    documentObject.Add("documentBase64", Base64Utils.EncodeStreamToBase64(documentStream));
                }
            }

            return await Task.FromResult(documentObject);
        }

        private static JObject GetDocumentPages(PdfDocument document)
        {
            JObject documentPages = [];

            ConcurrentDictionary<int, string> pageTexts = new();

            Parallel.ForEach(Enumerable.Range(1, document.GetNumberOfPages()), (index) =>
            {
                lock (pageTexts)
                {
                    string pageText = GetPageText(document, index);
                    pageTexts.TryAdd(index, pageText.Replace("\n", " ").Replace("\t", ""));
                }
            });

            foreach (KeyValuePair<int, string> page in pageTexts.OrderBy(page => page.Key))
            {
                documentPages.Add(page.Key.ToString(), page.Value);
            }

            return documentPages;
        }

        private static string GetPageText(PdfDocument pdfDocument, int pageNumber)
        {
            PdfPage page = pdfDocument.GetPage(pageNumber);
            return PdfTextExtractor.GetTextFromPage(page);
        }
    }
}
