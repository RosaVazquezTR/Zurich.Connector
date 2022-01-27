namespace Zurich.Connector.App.Services.DataSources
{
    /// <summary>
    /// Has some methods that can be used by multiple ConnectorOperations
    /// </summary>
    public static class ConnectorOperationsUtility
    {
        /// <summary>
        /// Used to map the extension to a document type
        /// </summary>
        /// <param name="extension">The file extension</param>
        /// <returns>Document Type</returns>
        public static string MapExtensionToDocumentType(string extension)
        {
            var documentType = string.Empty;
            if (!string.IsNullOrEmpty(extension))
            {
                switch (extension.ToUpper())
                {
                    case "ACCDB":
                        documentType = "Access";
                        break;

                    case "DAT":
                    case "GED":
                    case "SDF":
                        documentType = "AnalyticsView";
                        break;

                    case "AIF":
                    case "IFF":
                    case "M3U":
                    case "M4A":
                    case "MID":
                    case "MP3":
                    case "MPA":
                    case "WAV":
                    case "WMA":
                        documentType = "Audio";
                        break;

                    case "KEY":
                    case "KEYCHAIN":
                        documentType = "AzureKeyVault";
                        break;

                    case "ICS":
                        documentType = "Calendar";
                        break;

                    case "CSV":
                        documentType = "CSV";
                        break;

                    case "VCF":
                        documentType = "ContactCard";
                        break;

                    case "CS":
                        documentType = "CSharp";
                        break;

                    case "DB":
                    case "DBF":
                    case "MDB":
                    case "PDB":
                    case "SQL":
                        documentType = "Database";
                        break;

                    case "LOG":
                        documentType = "DocumentManagement";
                        break;

                    case "INDD":
                    case "PCT":
                        documentType = "EditPhoto";
                        break;

                    case "XLR":
                    case "XLS":
                    case "XLSX":
                        documentType = "Excel";
                        break;

                    case "3DM":
                    case "3DS":
                    case "DWG":
                    case "DXF":
                    case "MAX":
                    case "OBJ":
                        documentType = "File3d";
                        break;

                    case "ASP":
                    case "ASPX":
                        documentType = "FileASPX";
                        break;

                    case "APK":
                    case "APP":
                    case "BAT":
                    case "C":
                    case "CGI":
                    case "CLASS":
                    case "COM":
                    case "CPP":
                    case "EXE":
                    case "GADGET":
                    case "JAR":
                    case "SH":
                    case "SLN":
                    case "SWIFT":
                    case "VCXPROJ":
                    case "WSF":
                    case "XCODEPROJ":
                        documentType = "FileCode";
                        break;

                    case "CFM":
                    case "CSS":
                        documentType = "FileCSS";
                        break;

                    case "DCR":
                    case "HTM":
                    case "HTML":
                    case "XHT":
                        documentType = "FileHTML";
                        break;

                    case "JAVA":
                    case "JSP":
                        documentType = "FileJAVA";
                        break;

                    case "FNT":
                    case "FON":
                    case "OTF":
                    case "TTF":
                        documentType = "Font";
                        break;

                    case "BAK":
                    case "TMP":
                        documentType = "HardDriveGroup";
                        break;

                    case "AI":
                    case "BMP":
                    case "DDS":
                    case "EPS":
                    case "GIF":
                    case "JPG":
                    case "PNG":
                    case "PS":
                    case "PSD":
                    case "PSPIMAGE":
                    case "SVG":
                    case "TGA":
                    case "THM":
                    case "TIF":
                    case "TIFF":
                    case "YUV":
                        documentType = "Image";
                        break;

                    case "JS":
                        documentType = "JavaScriptLanguage";
                        break;

                    case "CER":
                    case "CSR":
                        documentType = "LaptopSecure";
                        break;

                    case "EML":
                    case "MSG":
                        documentType = "Mail";
                        break;

                    case "TAX2016":
                    case "TAX2017":
                        documentType = "Money";
                        break;

                    case "ONE":
                        documentType = "OneNote";
                        break;

                    case "PDF":
                        documentType = "PDF";
                        break;

                    case "PPS":
                    case "PPT":
                    case "PPTX":
                        documentType = "PowerPoint";
                        break;

                    case "B":
                    case "DEM":
                    case "GAM":
                    case "NES":
                    case "ROM":
                    case "SAV":
                        documentType = "Save";
                        break;

                    case "LUA":
                    case "PHP":
                    case "PL":
                    case "PY":
                        documentType = "Script";
                        break;

                    case "CAB":
                    case "CPL":
                    case "CUR":
                    case "DLL":
                    case "DMP":
                    case "DRV":
                    case "ICNS":
                    case "ICO":
                    case "LNK":
                    case "SYS":
                        documentType = "System";
                        break;

                    case "ODT":
                    case "PAGES":
                    case "RTF":
                    case "TEX":
                    case "TXT":
                    case "WPD":
                    case "WPS":
                        documentType = "Text";
                        break;

                    case "3G2":
                    case "3GP":
                    case "AFG":
                    case "ASF":
                    case "AVI":
                    case "AVCHD":
                    case "FLV":
                    case "M4V":
                    case "MOV":
                    case "MP4":
                    case "MPG":
                    case "RM":
                    case "SRT":
                    case "SWF":
                    case "VOB":
                    case "WMV":
                        documentType = "Video";
                        break;

                    case "VB":
                        documentType = "VisualBasicLanguage";
                        break;

                    case "DOC":
                    case "DOCX":
                        documentType = "Word";
                        break;

                    case "GPX":
                    case "KML":
                    case "KMZ":
                        documentType = "World";
                        break;

                    case "XML":
                        documentType = "XML";
                        break;

                    case "7Z":
                    case "CBR":
                    case "DEB":
                    case "GZ":
                    case "PKG":
                    case "RAR":
                    case "RPM":
                    case "SITX":
                    case "TAR":
                    case "ZIP":
                    case "ZIPX":
                        documentType = "ZipFolder";
                        break;
                }
            }
            return documentType;
        }
    }
}
