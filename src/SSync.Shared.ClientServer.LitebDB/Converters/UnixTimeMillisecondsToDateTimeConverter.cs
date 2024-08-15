using SSync.Shared.ClientServer.LitebDB.Enums;
using SSync.Shared.ClientServer.LitebDB.Extensions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SSync.Shared.ClientServer.LitebDB.Converters
{
    public class UnixTimeMillisecondsToDateTimeConverter : JsonConverter<DateTime>
    {
        private readonly Time? _time;

        public UnixTimeMillisecondsToDateTimeConverter(Time? time) => _time = time;

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                if (DateTime.TryParse(stringValue, out var dateTime))
                {
                    return dateTime;
                }
                else if (long.TryParse(stringValue, out var milliseconds))
                {
                    return milliseconds.FromUnixTimestamp(_time);
                }
                else
                {
                    throw new JsonException($"Invalid DateTime or Unix time string: {stringValue}");
                }
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                var milliseconds = reader.GetInt64();
                return milliseconds.FromUnixTimestamp(_time);
            }

            throw new JsonException("Expected a number or string for Unix time conversion.");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            var milliseconds = value.ToUnixTimestamp(_time);
            writer.WriteNumberValue(milliseconds);
        }
    }
}