﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zurich.Connector.App.Model;
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
                        EntityType = EntityType.Search,
                        Version = "v1"
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
                        EntityType = EntityType.Search,
                        Version = "v1"
                    },
                    DataSource = new DataSourceModel()
                    {
                        Id = "22",
                        Name = "DataSource22",
                        Description = "DataSource 22 desc"
                    }
                },
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

            result = JArray.Parse(@"[
                {
                    ""Id"":""1"",
                    ""Title"":""Secret cookie recipe 1"",
                    ""Version"":""1.0""
                },
                {
                    ""Id"":""2"",
                    ""Title"":""Secret cookie recipe 2"",
                    ""Version"":""1.0""
                },
            ]");
            return result;
        }
	}
}
