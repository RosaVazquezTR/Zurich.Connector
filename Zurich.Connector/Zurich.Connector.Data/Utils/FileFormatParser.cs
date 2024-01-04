using FileSignatures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zurich.Connector.Data.Enum;

namespace Zurich.Connector.Data.Utils
{
    public class FileFormatParser
    {
        public static AsposeFileFormat GetFileFormat(string extension)
        {
            return extension switch
            {
                "doc" => AsposeFileFormat.Doc,
                "docx" => AsposeFileFormat.Docx,
                "pdf" => AsposeFileFormat.Pdf,
                "rtf" => AsposeFileFormat.Rtf,
                _ => throw new NotImplementedException()
            };
        }

        public static string FindDocumentTypeFromStream(Stream documentStream)
        {
            var inspector = new FileFormatInspector();

            var format = inspector.DetermineFileFormat(documentStream);

            if (format == null)
            {
                throw new ApplicationException("Document type not determined");
            }
            else
            {
                return format.Extension;
            }
        }
    }

    
}
