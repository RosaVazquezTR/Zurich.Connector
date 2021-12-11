using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace Zurich.Connector.App.Serializers
{
    /// <summary>
    /// Custom Cosmos serializer which allows passing Json serializer settings
    /// </summary>
    public sealed class CosmosJsonSerializer : CosmosSerializer
    {
        private static readonly Encoding DefaultEncoding = new UTF8Encoding(false, true);
        private readonly JsonSerializerSettings _serializerSettings;

        public CosmosJsonSerializer(JsonSerializerSettings serializerSettings)
        {
            _serializerSettings = serializerSettings ?? throw new ArgumentNullException(nameof(serializerSettings));
        }

        public override T FromStream<T>(Stream stream)
        {
            using (stream)
            {
                if (typeof(Stream).IsAssignableFrom(typeof(T)))
                {
                    return (T)(object)stream;
                }

                using (var sr = new StreamReader(stream))
                {
                    using (var jsonTextReader = new JsonTextReader(sr))
                    {
                        var serializer = GetSerializer();
                        return serializer.Deserialize<T>(jsonTextReader);
                    }
                }
            }
        }

        public override Stream ToStream<T>(T input)
        {
            var payload = new MemoryStream();
            using (var streamWriter = new StreamWriter(payload, encoding: DefaultEncoding, bufferSize: 1024, leaveOpen: true))
            {
                using (JsonWriter jsonWriter = new JsonTextWriter(streamWriter))
                {
                    jsonWriter.Formatting = Formatting.None;
                    var serializer = GetSerializer();
                    serializer.Serialize(jsonWriter, input);
                    jsonWriter.Flush();
                    streamWriter.Flush();
                }
            }

            payload.Position = 0;
            return payload;
        }

        private JsonSerializer GetSerializer()
        {
            return JsonSerializer.Create(_serializerSettings);
        }
    }
}
