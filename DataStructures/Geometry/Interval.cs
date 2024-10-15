using System.Globalization;
using Newtonsoft.Json;

namespace DataStructures.Geometry;

public class IntervalJsonConverter : JsonConverter
{
    public override bool CanConvert(Type typeToConvert) =>  typeof(Interval) == typeToConvert;

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        string? value = reader.Value?.ToString();
        
        if (value != null && Interval.TryParse(value, out var interval))
        {
            return interval;
        }

        throw new JsonSerializationException($"Cannot convert value '{value}' to Interval.");
    }
    
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        value ??= new Interval();
        var interval = (Interval)value;
        writer.WriteRawValue($"\"[{interval.LeftBorder}, {interval.RightBorder}]\"");
    } 
}

[JsonConverter(typeof(IntervalJsonConverter))]
public readonly record struct Interval(double LeftBorder, double RightBorder)
{
    [JsonIgnore] public double Length { get; } = Math.Abs(RightBorder - LeftBorder);
    [JsonIgnore] public double Center { get; } = (LeftBorder + RightBorder) / 2;
    
    public override string ToString() => $"[{LeftBorder}, {RightBorder}]";

    public static bool TryParse(string value, out Interval interval)
    {
        var words = value.Split(new[] { ' ', ',', '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length != 2 || !float.TryParse(words[0], CultureInfo.InvariantCulture, out var x) ||
            !float.TryParse(words[1], CultureInfo.InvariantCulture, out var y))
        {
            interval = default;
            return false;
        }
        
        interval = new Interval(x, y);
        return true;
    }
}