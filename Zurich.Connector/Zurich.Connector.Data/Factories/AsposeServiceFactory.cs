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
    public class AsposeServiceFactory
    {
        public static IAsposeService GetAsposeImplementation(AsposeFileFormat fileFormat)
        {
            switch (fileFormat)
            {
                case AsposeFileFormat.Pdf:
                    return new AsposePDFService();
                case AsposeFileFormat.Docx: case AsposeFileFormat.Doc: case AsposeFileFormat.Rtf:
                    return new AsposeWordService();
                default:
                    throw new NotSupportedException($"{fileFormat} format is not supported!");
            }
        }
    }
}
