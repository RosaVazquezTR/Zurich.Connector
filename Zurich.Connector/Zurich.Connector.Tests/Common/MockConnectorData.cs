using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zurich.Connector.App.Model;
using Zurich.Connector.App.Utils;
using Zurich.Connector.Data.Model;

namespace Zurich.Connector.Tests.Common
{
	/// <summary>
	/// class responsible for mocking connector and data source data.
	/// </summary>
    internal static class MockConnectorData
    {
		/// <summary>
		/// Setup connector Model
		/// </summary>
		/// <returns></returns>
		internal static IEnumerable<ConnectorModel> SetupConnectorModel()
		{
            return new List<ConnectorModel>()
            {
                new ConnectorModel()
                {
                    Id = "1",
                    Info = new ConnectorInfoModel()
                    {
                        Title = "Connector1",
                        DataSourceId = "11",
                        Description ="Connector 1 desc",
                        EntityType = ConnectorEntityType.Search,
                        Version = "v1"
                    },
                    Request = new ConnectorRequestModel()
                    {
                        Parameters = new List<ConnectorRequestParameterModel>()
                        {
                            new ConnectorRequestParameterModel() { CdmName = "Query", Name = "searchTerm", DefaultValue = "*"},
                            new ConnectorRequestParameterModel() { CdmName = "Offset", Name = "resultsStartIndex", DefaultValue = "1"},
                            new ConnectorRequestParameterModel() { CdmName = "ResultSize", Name = "resultsCount", DefaultValue = "25"},
                        },
                        Sorting = new ConnectorRequestSortingModel(){Properties = new List<ConnectorRequestSortingPropertiesModel>(){ }}
                    },
                    DataSource = new DataSourceModel()
                    {
                        Id = "11",
                        Name = "DataSource11",
                        Description = "DataSource 11 desc"
                    }
                },
                new ConnectorModel()
                {
                    Id = "2",
                    Info = new ConnectorInfoModel()
                    {
                        Title = "Connector2",
                        DataSourceId = "22",
                        Description ="Connector 2 desc",
                        EntityType = ConnectorEntityType.Search,
                        Version = "v1"
                    },
                    Pagination = new PaginationModel()
                    {
                        Available = true,
                        IsZeroBasedOffset = true,
                    },
                    Request = new ConnectorRequestModel()
                    {
                        Parameters = new List<ConnectorRequestParameterModel>()
                        {
                            new ConnectorRequestParameterModel() { CdmName = "Query", Name = "searchTerm", DefaultValue = "*"},
                            new ConnectorRequestParameterModel() { CdmName = "Offset", Name = "resultsStartIndex", DefaultValue = "1"},
                            new ConnectorRequestParameterModel() { CdmName = "ResultSize", Name = "resultsCount", DefaultValue = "25"},
                        },
                        Sorting = new ConnectorRequestSortingModel(){Properties = new List<ConnectorRequestSortingPropertiesModel>(){ }}
                    },
                    DataSource = new DataSourceModel()
                    {
                        Id = "22",
                        Name = "DataSource22",
                        Description = "DataSource 22 desc",
                        
                        RegistrationInfo = new RegistrationInfoModel()
                        {
                            RegistrationMode = Zurich.Common.Models.Connectors.RegistrationEntityMode.Manual
                        }
                        

                    }
                },
                new ConnectorModel()
                {
                    Id = "3",
                    Info = new ConnectorInfoModel()
                    {
                        Title = "Connector3",
                        DataSourceId = "33",
                        Description ="Connector 3 desc",
                        EntityType = ConnectorEntityType.Search,
                        Version = "v1"
                    },
                    Pagination = new PaginationModel()
                    {
                        Available = true,
                        IsZeroBasedOffset = true,
                    },
                    Request = new ConnectorRequestModel()
                    {
                        Parameters = new List<ConnectorRequestParameterModel>()
                        {
                            new ConnectorRequestParameterModel() { CdmName = "Query", Name = "searchTerm", DefaultValue = "*"},
                            new ConnectorRequestParameterModel() { CdmName = "Offset", Name = "resultsStartIndex", DefaultValue = "1"},
                            new ConnectorRequestParameterModel() { CdmName = "ResultSize", Name = "resultsCount", DefaultValue = "25"},
                        },
                        Sorting = new ConnectorRequestSortingModel(){
                                                                        Properties = new List<ConnectorRequestSortingPropertiesModel>()
                                                                        { 
                                                                            new ConnectorRequestSortingPropertiesModel()
                                                                            {
                                                                                 Element ="sortOrder",
                                                                                 ElementValue="DATE",
                                                                                 Name ="Date",
                                                                                 Type = "date"
                                                                            }
                                                                        }
                                                                    }
                    },
                    DataSource = new DataSourceModel()
                    {
                        Id = "33",
                        Name = "DataSource33",
                        Description = "DataSource 33 desc"
                    }
                },
                new ConnectorModel()
                {
                    Id = "4",
                    Info = new ConnectorInfoModel()
                    {
                        Title = "Connector4",
                        DataSourceId = "22",
                        Description ="Connector 4 desc",
                        EntityType = ConnectorEntityType.Document,
                        Version = "v1"
                    },
                    Pagination = new PaginationModel()
                    {
                        Available = true,
                        IsZeroBasedOffset = true,
                    },
                    Request = new ConnectorRequestModel()
                    {
                        Parameters = new List<ConnectorRequestParameterModel>()
                        {
                            new ConnectorRequestParameterModel() { CdmName = "docTypes", Name = "$filter", Key = "resourceVisualization/type", DefaultValue = "", InClause = ODataConstants.OData},
                            new ConnectorRequestParameterModel() { CdmName = "referenceType", Name = "$filter", Key = "referenece/type", DefaultValue = "testValue", InClause = ODataConstants.OData},
                            new ConnectorRequestParameterModel() { CdmName = "resultSize", Name = "$top", DefaultValue = "", InClause = ODataConstants.OData},
                        },
                        Sorting = new ConnectorRequestSortingModel(){Properties = new List<ConnectorRequestSortingPropertiesModel>(){ }}
                    },
                    DataSource = new DataSourceModel()
                    {
                        Id = "22",
                        Name = "DataSource22",
                        Description = "DataSource 22 desc",
                        RegistrationInfo= new RegistrationInfoModel()
                        {
                            RegistrationMode = Zurich.Common.Models.Connectors.RegistrationEntityMode.Automatic
                        }
                    }
                }
            };
		}

        internal static IEnumerable<ConnectorModel> SetupConnectorModel_Version2()
        {
            return new List<ConnectorModel>()
            {
                new ConnectorModel()
                {
                    Id = "1",
                    Info = new ConnectorInfoModel()
                    {
                        Title = "Connector1",
                        DataSourceId = "11",
                        Description ="Connector 1 desc",
                        EntityType = ConnectorEntityType.Search,
                        Version = "v1"
                    },
                    Request = new ConnectorRequestModel()
                    {
                        Parameters = new List<ConnectorRequestParameterModel>()
                        {
                            new ConnectorRequestParameterModel() { CdmName = "Query", Name = "searchTerm", DefaultValue = "*"},
                            new ConnectorRequestParameterModel() { CdmName = "Offset", Name = "resultsStartIndex", DefaultValue = "1"},
                            new ConnectorRequestParameterModel() { CdmName = "ResultSize", Name = "resultsCount", DefaultValue = "25"},
                        }
                    },
                    DataSource = new DataSourceModel()
                    {
                        Id = "11",
                        Name = "DataSource11",
                        Description = "DataSource 11 desc"
                    }
                },
                new ConnectorModel()
                {
                    Id = "2",
                    Info = new ConnectorInfoModel()
                    {
                        Title = "Connector2",
                        DataSourceId = "22",
                        Description ="Connector 2 desc",
                        EntityType = ConnectorEntityType.Search,
                        Version = "v1"
                    },
                    Pagination = new PaginationModel()
                    {
                        Available = true,
                        IsZeroBasedOffset = true,
                    },
                    Request = new ConnectorRequestModel()
                    {
                        Sorting = new ConnectorRequestSortingModel(){Properties = new List<ConnectorRequestSortingPropertiesModel>(){ }}
                    },
                    DataSource = new DataSourceModel()
                    {
                        Id = "22",
                        Name = "DataSource22",
                        Description = "DataSource 22 desc"
                    }
                },
                new ConnectorModel()
                {
                    Id = "3",
                    Info = new ConnectorInfoModel()
                    {
                        Title = "Connector3",
                        DataSourceId = "33",
                        Description ="Connector 3 desc",
                        EntityType = ConnectorEntityType.Search,
                        Version = "v1"
                    },
                    Pagination = new PaginationModel()
                    {
                        Available = true,
                        IsZeroBasedOffset = true,
                    },
                    Request = new ConnectorRequestModel()
                    {
                    },
                    DataSource = new DataSourceModel()
                    {
                        Id = "33",
                        Name = "DataSource33",
                        Description = "DataSource 33 desc"
                    }
                }
            };
        }

        /// <summary>
        /// Setup data source model.
        /// </summary>
        /// <returns></returns>
        internal static IEnumerable<DataSourceModel> SetupDataSourceModel()
		{
			return new List<DataSourceModel>()
			{
                new DataSourceModel()
                {
                    Id = "11",
                    Name = "DataSource11",
                    Description = "DataSource 11 desc"
                },
                new DataSourceModel()
				{
                    Id = "22",
                    Name = "DataSource22",
                    Description = "DataSource 22 desc"
                },
			};
		}

        internal static IEnumerable<dynamic> SetupDocumentsModel()
        {
            dynamic result;

            result = JObject.Parse(@"{
                ""TotalCount"": 2,
                ""Items"": [
                    {
                        ""Id"":""1"",
                        ""Title"":""Secret cookie recipe 1"",
                        ""Version"":""1.0"",
                        ""DesktopUrl"":""iwl:dms=2fdb2-dmobility.imanage.work&&lib=TestLibrary&&num=118&&ver=1"",
                        ""lib"":""TestLibrary"",

                    },
                    {
                        ""Id"":""2"",
                        ""Title"":""Secret cookie recipe 2"",
                        ""Version"":""1.0"",
                        ""DesktopUrl"":""iwl:dms=3fdb2-dmobility.imanage.work&&lib=TestLibrary1&&num=119&&ver=1"",
                        ""lib"":""TestLibrary1"",

                    }
                ]
            }");
            return result;
        }

        internal static IEnumerable<dynamic> SetupSearchDocumentsModel()
        {
            dynamic result;

            result = JObject.Parse(@"{
                ""TotalCount"": 2,
                ""Documents"": [
                    {
                        ""Id"":""1"",
                        ""Title"":""Secret cookie recipe 1"",
                        ""WebUrl"":"""",
                        ""Snippet"":""abc"",
                        ""PlcReference"":""4-000-4131"",

                    },
                    {
                        ""Id"":""2"",
                        ""Title"":""Secret cookie recipe 2"",
                        ""WebUrl"":"""",
                        ""Snippet"":""xyc"",
                        ""PlcReference"":""4-000-4121"",

                    }
                ]
            }");
            return result;
        }
    }
}
