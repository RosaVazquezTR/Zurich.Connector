using Aspose.Pdf.Operators;
using Aspose.Words;
using GroupDocs.Parser;
using GroupDocs.Parser.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Words.NET;
using Zurich.Connector.Data.Interfaces;

namespace Zurich.Connector.Data.Services
{
    public class WordService : IDocumentConversionService
    {
        public async Task<JObject> ConvertDocumentToJObjectAsync(Stream documentStream, bool transformToPdf = true)
        {
            Document document = new(documentStream);
            JObject pageText = ProcessDocumentPages(document);

            string documentBase64 = ConvertDocumentToBase64(document, transformToPdf ? SaveFormat.Pdf : SaveFormat.Docx);

            JObject documentObject = new()
            {
                { "documentContent", pageText },
                { "documentBase64", documentBase64 }
            };

            return await Task.FromResult(documentObject);
        }

        private static JObject ProcessDocumentPages(Document document)
        {
            JObject pageText = [];

            for (int i = 0; i < document.PageCount; i++)
            {
                Document page = document.ExtractPages(i, 1);
                pageText.Add((i + 1).ToString(), page.ToString(SaveFormat.Text).Replace("\t", "").Replace("\n", "").Replace("\r", ""));
            }

            return pageText;
        }

        private static string ConvertDocumentToBase64(Document document, SaveFormat format)
        {
            using MemoryStream memoryStream = new();
            document.Save(memoryStream, format);
            memoryStream.Position = 0;
            byte[] fileBytes = memoryStream.ToArray();
            return Convert.ToBase64String(fileBytes);
        }
    }
}
