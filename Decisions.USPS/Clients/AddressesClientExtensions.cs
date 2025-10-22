using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Decisions.USPS.Clients
{
    /// <summary>
    /// Custom JSON converter to handle the Source field which can be either a string or an object
    /// </summary>
    public class SourceConverter : JsonConverter<Source>
    {
        public override Source ReadJson(JsonReader reader, Type objectType, Source existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                // If the source is a string (like "Access Token"), create a Source object with it as the parameter
                string sourceString = (string)reader.Value;
                return new Source { Parameter = sourceString };
            }
            else if (reader.TokenType == JsonToken.StartObject)
            {
                // If it's an object, deserialize normally
                JObject jsonObject = JObject.Load(reader);
                return jsonObject.ToObject<Source>(serializer);
            }
            else if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            
            throw new JsonSerializationException($"Unexpected token type {reader.TokenType} when deserializing Source");
        }

        public override void WriteJson(JsonWriter writer, Source value, JsonSerializer serializer)
        {
            // For writing, just serialize the object normally
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                serializer.Serialize(writer, value);
            }
        }
    }

    /// <summary>
    /// Partial class to customize JSON serialization settings
    /// </summary>
    public partial class AddressesClient
    {
        static partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
        {
            // Add our custom converter for the Source type
            settings.Converters.Add(new SourceConverter());
            
            // Set error handling to continue on error to be more resilient
            settings.Error = (sender, args) =>
            {
                // Log the error but continue processing
                System.Diagnostics.Debug.WriteLine($"JSON Deserialization Error: {args.ErrorContext.Error.Message}");
                args.ErrorContext.Handled = true;
            };
        }
    }
}