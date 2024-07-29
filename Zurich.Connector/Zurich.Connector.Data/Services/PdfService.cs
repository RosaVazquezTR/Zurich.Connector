using GroupDocs.Parser;
using GroupDocs.Parser.Options;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Layout;
using Microsoft.Rest.Azure;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zurich.Connector.Data.Interfaces;

namespace Zurich.Connector.Data.Services
{
    public class PdfService : IDocumentConversionService
    {
        public async Task<JObject> ConvertDocumentToJObjectAsync(Stream documentStream, bool transformToPdf = true)
        {
            JObject documentObject = [];

            using (PdfDocument document = new(new PdfReader(documentStream)))
            {
                JObject pageObjects = GetDocumentPages(document);
                documentObject.Add("documentContent", pageObjects);

                if (transformToPdf)
                {
                    byte[] fileBytes = ConvertDocumentToPdf(documentStream);
                    documentObject.Add("documentBase64", Convert.ToBase64String(fileBytes));
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

        private static byte[] ConvertDocumentToPdf(Stream documentStream)
        {
            long fileLenght = documentStream.Length;
            byte[] bytes = new byte[fileLenght];
            documentStream.Read(bytes, 0, (int)fileLenght);
            return bytes;
        }
    }
}
