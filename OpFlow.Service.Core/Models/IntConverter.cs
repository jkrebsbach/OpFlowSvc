using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpFlow.Service.Core.Models
{
    /// <summary>
    /// Allows parsing empty string as null integer - default System.Text will not allow
    /// </summary>
    public class IntConverter : JsonConverter<Int32?>
    {
        public override Int32? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Number:
                    return reader.GetInt32();
                default:
                    var stringValue = reader.GetString();
                    if (string.IsNullOrEmpty(stringValue)) return null;
                    return Int32.Parse(stringValue);
            }
        }

        public override void Write(Utf8JsonWriter writer, Int32? value, JsonSerializerOptions options)
        {
            if (value == null)
                writer.WriteNullValue();
            else
                writer.WriteNumberValue(value.Value);
        }
    }
}
