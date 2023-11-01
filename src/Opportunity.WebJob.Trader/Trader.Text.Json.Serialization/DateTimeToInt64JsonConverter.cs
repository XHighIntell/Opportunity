using System.Text.Json;
using System.Text.Json.Serialization;
using Blite;

namespace Opportunity.WebJob.Trader.Text.Json.Serialization;

///<summary>Converts a <see cref="DateTime"/> to or from JSON.</summary>
public class DateTimeToInt64JsonConverter: JsonConverter<DateTime> {
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        var ms = reader.GetInt64();

        return ms.JSToDateTime();
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) {
        writer.WriteNumberValue(value.ToJsDateTime());
    }

}

