using Newtonsoft.Json;
using Quickbird.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quickbird.Internet
{
    public class SensorReadingsJsConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Messenger.SensorReading); 
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Messenger.SensorReading it;
            TimeSpan Duration;
            DateTimeOffset Timestamp;
            Double Value = double.NegativeInfinity;
            Guid SensorId; 

            while (reader.Read())
            {
                if (reader.TokenType != JsonToken.PropertyName)
                    break;

                var propertyName = (string)reader.Value;

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

            return new Messenger.SensorReading(SensorId, Value, Timestamp, Duration);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Messenger.SensorReading reading = (Messenger.SensorReading) value; 

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
