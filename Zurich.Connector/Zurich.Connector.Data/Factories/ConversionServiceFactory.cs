using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zurich.Connector.Data.Enum;
using Zurich.Connector.Data.Interfaces;
using Zurich.Connector.Data.Services;
using Zurich.Connector.Data.Utils;

namespace Zurich.Connector.Data.Factories
{
    public class ConversionServiceFactory
    {
        public static IDocumentConversionService GetConversionImplementation(AsposeFileFormat fileFormat)
        {
            return fileFormat switch
            {
                AsposeFileFormat.Pdf => new PdfService(),
                AsposeFileFormat.Docx or AsposeFileFormat.Doc or AsposeFileFormat.Rtf => new WordService(),
                _ => throw new NotSupportedException($"{fileFormat} format is not supported!"),
            };
        }
    }
}
