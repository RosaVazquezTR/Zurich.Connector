using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Zurich.Connector.Data.Configuration
{
    public class AsposeLicenceProvider
    {
        public static void SetAsposeWordsLicense(string licenseString)
        {
            if (string.IsNullOrEmpty(licenseString))
            {
                throw new ArgumentNullException(nameof(licenseString), "Error occurred while setting aspose license");
            }
            Aspose.Words.License license = new Aspose.Words.License();
            try
            {
                license.SetLicense(new MemoryStream(Encoding.UTF8.GetBytes(licenseString)));
            }
            catch (XmlException e)
            {
                throw new InvalidAsposeLicenseException(e, "Aspose license doesn`t comply with the XML syntax rules");
            }
            catch (InvalidOperationException e)
            {
                throw new InvalidAsposeLicenseException(e, "Aspose license is invalid");
            }
            catch (Exception e)
            {
                throw new InvalidAsposeLicenseException(e, "Error occurred while setting aspose license");
            }
        }

        public static void SetAsposePDFLicense(string licenseString)
        {
            if (string.IsNullOrEmpty(licenseString))
            {
                throw new ArgumentNullException(nameof(licenseString), "Error occurred while setting aspose license");
            }
            Aspose.Pdf.License license = new Aspose.Pdf.License();
            try
            {
                license.SetLicense(new MemoryStream(Encoding.UTF8.GetBytes(licenseString)));
            }
            catch (XmlException e)
            {
                throw new InvalidAsposeLicenseException(e, "Aspose license doesn`t comply with the XML syntax rules");
            }
            catch (InvalidOperationException e)
            {
                throw new InvalidAsposeLicenseException(e, "Aspose license is invalid");
            }
            catch (Exception e)
            {
                throw new InvalidAsposeLicenseException(e, "Error occurred while setting aspose license");
            }
        }
    }

    public class InvalidAsposeLicenseException : Exception
    {
        public InvalidAsposeLicenseException(Exception exception, string responseErrorMessage) : base(responseErrorMessage, exception)
        {
        }
    }
}
