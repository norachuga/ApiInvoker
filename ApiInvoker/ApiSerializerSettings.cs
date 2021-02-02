using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ApiInvoker
{
    // Sets JSON serializer options that we need to be common between all services
    // to ensure smooth communication with any caller. 
    public static class ApiSerializerSettings
    {
        public static JsonSerializerSettings Configure() => Configure(new JsonSerializerSettings());

        public static JsonSerializerSettings Configure(JsonSerializerSettings jsonSerializerSettings)
        {
            // Include the type name in the JSON if the object's actual type does  
            // not match the declared type. A common example of this is sending
            // a List<BaseType> that contains items of DerivedTypeA, DerivedTypeB, etc.
            // This setting ensures the items will deserialize to the derived type
            // and not the base type.
            jsonSerializerSettings.TypeNameHandling = TypeNameHandling.Auto;

            // Properties with null values will be omitted in the JSON. (no functional impact)
            jsonSerializerSettings.NullValueHandling = NullValueHandling.Ignore;

            // Enums will serialize to strings instead of integers. (no functional impact)
            jsonSerializerSettings.Converters.Add(new StringEnumConverter());

            return jsonSerializerSettings;
        }

    }
}
