using System.Text.Json;
using System.Text.Json.Serialization;

namespace HuvrApiClient.JsonConverters
{
    /// <summary>
    /// JSON converter that handles both string and numeric ID values
    /// </summary>
    public class FlexibleIdConverter : JsonConverter<string?>
    {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    return reader.GetString();

                case JsonTokenType.Number:
                    // Handle both integer and decimal numbers
                    if (reader.TryGetInt64(out long longValue))
                    {
                        return longValue.ToString();
                    }
                    if (reader.TryGetDouble(out double doubleValue))
                    {
                        return doubleValue.ToString();
                    }
                    throw new JsonException($"Unable to convert number to string for ID");

                case JsonTokenType.Null:
                    return null;

                default:
                    throw new JsonException($"Unexpected token type {reader.TokenType} for ID field. Expected String or Number.");
            }
        }

        public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                writer.WriteStringValue(value);
            }
        }
    }
}
