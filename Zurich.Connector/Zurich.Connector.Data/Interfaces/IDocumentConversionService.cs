using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zurich.Connector.Data.Interfaces
{
    public interface IDocumentConversionService
    {
        /// <summary>
        /// Converts a document to a JObject.
        /// </summary>
        /// <param name="document">The document to convert.</param>
        /// <param name="transformToPdf">Specifies whether to transform the document to PDF format. Default is true.</param>
        /// <returns>The converted document as a JObject.</returns>
        public Task<JObject> ConvertDocumentToJObjectAsync(Stream document, bool transformToPdf = true);
    }
}
