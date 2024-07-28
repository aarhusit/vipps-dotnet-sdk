using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vipps.net.Helpers
{
    internal static class VippsRequestSerializer
    {
        private static readonly JsonSerializerOptions JsonSerializerSettings =
            new()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
            };

        internal static string SerializeVippsRequest<T>(T vippsRequest)
            where T : class
        {
            string serializedRequest = JsonSerializer.Serialize(vippsRequest, JsonSerializerSettings);

            return serializedRequest;
        }

        internal static T DeserializeVippsResponse<T>(string vippsResponse)
            where T : class
        {
            try
            {
                var deserializedTyped = JsonSerializer.Deserialize<T>(
                    vippsResponse,
                    JsonSerializerSettings
                );
                return deserializedTyped ?? throw new Exceptions.VippsTechnicalException($"Response could not be deserialized to {nameof(T)}");
            }
            catch (Exceptions.VippsBaseException)
            {
                throw;
            }
            catch (System.Exception ex)
            {
                throw new Exceptions.VippsTechnicalException($"Error deserializing response of type {nameof(T)}",ex);
            }
        }
    }
}
