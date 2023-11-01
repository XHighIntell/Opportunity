using System.Text.Json;
using System.Text.Json.Serialization;

namespace Opportunity.WebJob.Trader.Text.Json.Serialization;

///<summary>Converts a <see cref="TimeSpan"/> to or from JSON.</summary>
public class TimeSpanToInt64JsonConverter: JsonConverter<TimeSpan> {
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        var ms = reader.GetInt64();
            
        return TimeSpan.FromMilliseconds(ms);
    }

    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options) {
        writer.WriteNumberValue(value.Ticks / TimeSpan.TicksPerMillisecond);
    }
}

