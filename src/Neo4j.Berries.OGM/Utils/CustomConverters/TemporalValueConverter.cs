using System.Text.Json;
using System.Text.Json.Serialization;
using Neo4j.Driver;

namespace Neo4j.Berries.OGM.Utils.CustomConverters;

public class ZonedDateTimeConverter : JsonConverter<ZonedDateTime>
{
    public override ZonedDateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        if(string.IsNullOrEmpty(str)) return null;
        var dateTime = DateTime.Parse(str);
        return new ZonedDateTime(dateTime);
    }

    public override void Write(Utf8JsonWriter writer, ZonedDateTime value, JsonSerializerOptions options)
    {
        if(value is null)
        {
            writer.WriteNullValue();
            return;
        }
        writer.WriteStringValue(value.ToString());
    }
}

public class LocalDateTimeConverter : JsonConverter<LocalDateTime>
{
    public override LocalDateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        if(string.IsNullOrEmpty(str)) return null;
        var dateTime = DateTime.Parse(str);
        return new LocalDateTime(dateTime);
    }

    public override void Write(Utf8JsonWriter writer, LocalDateTime value, JsonSerializerOptions options)
    {
        if(value is null)
        {
            writer.WriteNullValue();
            return;
        }
        writer.WriteStringValue(value.ToString());
    }
}