using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zurich.Common;
using Zurich.Connector.Data;
using Zurich.Connector.Data.DataMap;
using Zurich.Connector.Data.Factories;
using Zurich.Connector.Data.Model;

namespace Zurich.Connector.App.Services.DataSources
{
    public class MsGraphConnectorOperation : IConnectorDataSourceOperations
    {
        private readonly ILogger _logger;
        private readonly IDataMapping _dataMapping;
        private readonly IConfiguration _configuration;

        public MsGraphConnectorOperation(ILogger<IConnectorDataSourceOperations> logger, IDataMappingFactory dataMappingFactory, IConfiguration configuration)
        {
            _logger = logger;
            _dataMapping = dataMappingFactory.GetImplementation(AuthType.OAuth2.ToString());
            _configuration = configuration;
        }

        public bool IsCompatible(string appCode)
        {
            return appCode == KnownDataSources.msGraph;
        }

        public async Task<dynamic> SetItemLink(ConnectorEntityType entityType, dynamic item, string appCode, string hostName)
        {
            try
            {
                switch (entityType)
                {
                    case ConnectorEntityType.Search:
                        if (item is JObject result && result.ContainsKey("Documents"))
                        {
                            foreach (JObject doc in (result["Documents"] as JArray))
                            {
                                if (doc.ContainsKey(StructuredCDMProperties.Title))
                                {
                                    if (doc.ContainsKey(StructuredCDMProperties.Type))
                                    {
                                        var extension = GetExtension(doc[StructuredCDMProperties.Title].ToString());
                                        doc[StructuredCDMProperties.Type] = MapExtenstionToDocumentType(extension);
                                        doc[StructuredCDMProperties.AdditionalProperties][UnstructuredCDMProperties.Extension] = extension;
                                    }
                                    doc[StructuredCDMProperties.Title] = RemoveExtension(doc[StructuredCDMProperties.Title].ToString());
                                }
                                if (doc.ContainsKey(StructuredCDMProperties.Snippet))
                                {
                                    doc[StructuredCDMProperties.Snippet] = UpdateSnippet(doc[StructuredCDMProperties.Snippet].ToString());
                                }
                            }
                        }
                        break;
                }
            }
            catch (UriFormatException ex)
            {
                _logger.LogError("Unable to parse {entityType} web URL: {message}", entityType.ToString(), ex.Message ?? "");
            }
            return item;
        }

        /// <summary>
        /// Used to map the extension to a document type
        /// </summary>
        /// <param name="extension">The file extension</param>
        /// <returns>Document Type</returns>
        private string MapExtenstionToDocumentType(string extension)
        {
            var documentType = string.Empty;
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
            return documentType;
        }

        /// <summary>
        /// Pulls the extention form the title so can be returned on type
        /// </summary>
        /// <param name="title">Title from MsGrapoh</param>
        /// <returns>extension</returns>
        private string GetExtension(string title)
        {
            string extension = string.Empty;

            var splitString = title.Split(".");
            if (splitString.Count() > 1)
            {
                extension = splitString.Last();
            }

            return extension;
        }

        /// <summary>
        /// Removes the extension from the title
        /// </summary>
        /// <param name="title">Original title from msGraph</param>
        /// <returns>Title with no extension</returns>
        private string RemoveExtension(string title)
        {
            var splitString = title.Split(".");
            if (splitString.Count() > 1)
            {
                title = string.Join(".", splitString.SkipLast(1));
            }
            
            return title;
        }

        /// <summary>
        /// Updates the snippet to replace c0 tags with bolds and remove ddd/>
        /// </summary>
        /// <param name="snippet">Snippet from MsGraph</param>
        /// <returns>Snippet with updated tags</returns>
        internal static string UpdateSnippet(string snippet)
        {
            if(!string.IsNullOrEmpty(snippet))
            {
                snippet = snippet.Replace("<c0>", "<b>").Replace("</c0>", "</b>").Replace(" <ddd/>", ". ");
            }
            return snippet;
        }
    }
}
