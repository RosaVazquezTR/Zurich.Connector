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
        Task<DataMappingClass> GetMap(string appCode, DataType dataType);
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

        public async Task<DataMappingClass> GetMap(string appCode, DataType dataType)
        {
            DataMappingFile file = await ReadFile();
            if (file == null)
            {
                return null;
            }

            DataMappingConnection appMapping = GetDataMap(file, appCode, dataType);
            if (appMapping == null)
            {
                return null;
            }

            DataMappingDataType dataTypeMap = appMapping.DataTypes.FirstOrDefault();

            if (dataTypeMap == null)
            {
                return null;
            }

            return new DataMappingClass()
            {
                AppCode = appCode,
                DataType = (DataType)Enum.Parse(typeof(DataType), dataTypeMap.Name, true),
                Api = new DataMappingApiRequest() { Url = dataTypeMap.Api.Endpoint, MethodType = dataTypeMap.Api.MethodType, AuthHeader = appMapping.OAuth.AuthHeader },
                Mapping = dataTypeMap.Mapping,
                ResultLocation = dataTypeMap.ResultLocation
            };
        }

        internal DataMappingConnection GetDataMap(DataMappingFile file, string appCode, DataType dataType)
        {
            DataMappingConnection currentAppType = null;
            if (file != null)
            {
                currentAppType = file.Connections.FirstOrDefault(x => x.AppCode.Equals(appCode, StringComparison.OrdinalIgnoreCase));
                if (currentAppType != null)
                {
                    currentAppType.DataTypes = currentAppType.DataTypes.Where(x => x.Name.Equals(dataType.ToString(), StringComparison.OrdinalIgnoreCase)).ToList();
                }
            }
            return currentAppType;
        }

        internal async Task<DataMappingFile> ReadFile()
        {
            DataMappingFile file = null;
            // D:\\home\\site\\wwwroot\\DataMapping\\DataMapping.json
            //var filePath = _configuration.GetValue<string>(AppConfigKeys.DataMappingFilePath);
            var filePath = "./DataMapping.json";

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
    }
}
