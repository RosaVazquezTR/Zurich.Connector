using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Zurich.Common;
using Zurich.Connector.Data.Model;

namespace Zurich.Connector.Data.Repositories
{
    /// <summary>
    /// Repo to get information around DataMapping
    /// </summary>
    public interface IDataMappingRepository
    {
        /// <summary>
        /// Passed information about the map type this returns the corresponding data around how
        /// </summary>
        /// <param name="appCode">The current code to use to get the app</param>
        /// <param name="dataType">Type of data we want to retrieve</param>
        /// <returns>Mapping information for the call</returns>
        Task<DataMappingClass> GetMap(string connectionId);

        /// <summary>
        /// Gets all connections from the data mapping file
        /// </summary>
        /// <param name="appCode">The current code to use to get the app</param>
        /// <param name="dataType">Type of data we want to retrieve</param>
        /// <returns>List of Data Mapping Connections</returns>
        Task<List<DataMappingConnection>> GetConnectors();

        /// <summary>
        /// Makes a Get call to fetch the configuration of connector
        /// </summary>
        /// <param name="connectorid">Holds id of connector</param>
        /// <returns>a string</returns>
        Task<ConnectorsConfigResponseEntity> GetConnectorConfiguration(string ConnectorId);
    }

    public class DataMappingRepository : IDataMappingRepository
    {
        private readonly ILogger<DataMappingRepository> _logger;
        private readonly IConfiguration _configuration;

        public DataMappingRepository(ILogger<DataMappingRepository> logger, IConfiguration configuration)
        {
            this._logger = logger;
            _configuration = configuration;
        }

        public async Task<DataMappingClass> GetMap(string connectionId)
        {
            DataMappingFile file = await ReadFile();
            if (file == null)
            {
                return null;
            }

            DataMappingConnection appMapping = GetDataMap(file, connectionId);
            if (appMapping == null)
            {
                return null;
            }

            DataMappingEndpoint endpointMap = appMapping.Endpoint;

            if (endpointMap == null)
            {
                return null;
            }

            return new DataMappingClass()
            {
                Id = appMapping.Id,
                AppCode = appMapping.AppCode,
                AuthType = appMapping.Auth.Type,
                DataType = (DataType)Enum.Parse(typeof(DataType), endpointMap.Name, true),
                Api = new DataMappingApiRequest() { Url = endpointMap.Api.Path, MethodType = endpointMap.Api.MethodType, AuthHeader = appMapping.Auth.OAuth?.AuthHeader },
                Mapping = endpointMap.Mapping,
                ResultLocation = endpointMap.ResultLocation
            };

        }

        public async Task<List<DataMappingConnection>> GetConnectors()
        {
            DataMappingFile file = await ReadFile();
            if (file == null)
            {
                return null;
            }

            return file.Connections;
        }

        internal DataMappingConnection GetDataMap(DataMappingFile file, string connectionId)
        {
            DataMappingConnection currentAppType = null;
            if (file != null)
            {
                currentAppType = file.Connections.FirstOrDefault(x => x.Id.Equals(connectionId, StringComparison.OrdinalIgnoreCase));
                if (currentAppType != null)
                {
                    currentAppType.Endpoint = currentAppType.Endpoint;
                }
            }
            return currentAppType;
        }

        internal async Task<DataMappingFile> ReadFile()
        {
            DataMappingFile file = null;

            var filePath = _configuration.GetValue<string>(AppConfigKeys.DataMappingFilePath);

            if (File.Exists(filePath))
            {
                using (StreamReader r = new StreamReader(filePath))
                {
                    string json = await r.ReadToEndAsync();
                    file = JsonConvert.DeserializeObject<DataMappingFile>(json);
                }
            }
            else
            {
                _logger.LogError($"Could not find dataMapping file at {filePath}");
            }

            return file;
        }

        public async Task<ConnectorsConfigResponseEntity> GetConnectorConfiguration(string connectorId)
        {
            ConnectorsConfigEntity connectorconfigType = null;
            //below are Temporary third party api response object for connector
            var listofConnectors = new ConnectorsEntity
            {
                connectors = new List<ConnectorsConfigEntity>
                  {
                     new ConnectorsConfigEntity
                     {
                         Id="101",
                         AppCode="Office",
                         Auth= new DataMappingAuth
                         {
                             Type= AuthType.OAuth,

                             OAuth= new DataMappingOAuthType
                             {
                                 AuthUrl="https://ci.login.legalplatform.thomsonreuters.com/is/connect/authorize",
                                 TokenUrl="https://ci.login.legalplatform.thomsonreuters.com/is/connect/token",
                                 AuthHeader="id"
                             }
                         },
                         Endpoint= new DataMappingEndpoint
                         {
                             Name="GetConfiguration",
                             Api = new DataMappingApi
                             {
                                 Path="https://graph.microsoft.com/v1.0/me",
                                 MethodType="Get"
                             },
                             Mapping = new List<DataMappingProperty>
                             {
                                 new DataMappingProperty
                                 {
                                     CDMProperty="",
                                     APIProperty=""
                                 }
                             },
                             ResultLocation="",
                         },
                         redirectionURL="",
                         filters = new string[]{"sort:true"}

                     }
                  }

            };
            if (listofConnectors.connectors != null)
            {
                connectorconfigType = listofConnectors.connectors.FirstOrDefault(x => x.Id.Equals(connectorId, StringComparison.OrdinalIgnoreCase));
            }
            return new ConnectorsConfigResponseEntity
            {
                Id = connectorconfigType.Id,
                Api = new DataMappingApiRequest
                {
                    MethodType = connectorconfigType.Endpoint.Api.MethodType,
                    Url = connectorconfigType.redirectionURL,
                    Hostname = connectorconfigType.AppCode,
                    AuthHeader = connectorconfigType.Endpoint.Api.Path
                },
                AppCode = connectorconfigType.AppCode,
                AuthType = connectorconfigType.Auth,
                CDMData = connectorconfigType.Endpoint.Mapping,
                filters = connectorconfigType.filters

            };
        }
    }
}
