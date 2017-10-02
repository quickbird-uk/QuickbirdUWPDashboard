namespace Quickbird.Internet
{
    using System;
    using Newtonsoft.Json;
    using Util;

    public class SensorReadingsJsConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) { return objectType == typeof(BroadcasterService.SensorReading); }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            BroadcasterService.SensorReading it;
            TimeSpan Duration;
            DateTimeOffset Timestamp;
            var Value = double.NegativeInfinity;
            Guid SensorId;

            while (reader.Read())
            {
                if (reader.TokenType != JsonToken.PropertyName)
                    break;

                var propertyName = (string) reader.Value;

                if (!reader.Read())
                    continue;

                if (propertyName == nameof(it.Duration))
                {
                    Duration = serializer.Deserialize<TimeSpan>(reader);
                }
                if (propertyName == nameof(it.Timestamp))
                {
                    Timestamp = serializer.Deserialize<DateTimeOffset>(reader);
                }
                if (propertyName == nameof(it.Value))
                {
                    Value = serializer.Deserialize<double>(reader);
                }
                if (propertyName == nameof(it.SensorId))
                {
                    SensorId = serializer.Deserialize<Guid>(reader);
                }
            }

            return new BroadcasterService.SensorReading(SensorId, Value, Timestamp, Duration);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var reading = (BroadcasterService.SensorReading) value;

            writer.WriteStartObject();
            writer.WritePropertyName(nameof(reading.SensorId));
            serializer.Serialize(writer, reading.SensorId);
            writer.WritePropertyName(nameof(reading.Value));
            serializer.Serialize(writer, reading.Value);
            writer.WritePropertyName(nameof(reading.Timestamp));
            serializer.Serialize(writer, reading.Timestamp);
            writer.WritePropertyName(nameof(reading.Duration));
            serializer.Serialize(writer, reading.Duration);

            writer.WriteEndObject();
        }
    }
}
