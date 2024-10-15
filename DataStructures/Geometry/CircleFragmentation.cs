using System.Globalization;
using Newtonsoft.Json;

namespace DataStructures.Geometry;

public class CircleFragmentationJsonConverter : JsonConverter
{
    public override bool CanConvert(Type typeToConvert) =>  typeof(Interval) == typeToConvert;

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        string? value = reader.Value?.ToString();
        
        if (value != null && CircleFragmentation.TryParse(value, out var interval))
        {
            return interval;
        }

        throw new JsonSerializationException($"Cannot convert value '{value}' to CircleFragmentation.");
    }
    
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        value ??= new CircleFragmentation();
        var circleFragmentation = (CircleFragmentation)value;
        writer.WriteRawValue($"\"[{circleFragmentation.Offset}, {circleFragmentation.Split}]\"");
    } 
}

[JsonConverter(typeof(CircleFragmentationJsonConverter))]
public record struct CircleFragmentation(int Offset, int Split)
{
    public static bool TryParse(string value, out CircleFragmentation circleFragmentation)
    {
        var words = value.Split(new[] { ' ', ',', '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length != 2 || !int.TryParse(words[0], CultureInfo.InvariantCulture, out var offset) ||
            !int.TryParse(words[1], CultureInfo.InvariantCulture, out var split))
        {
            circleFragmentation = default;
            return false;
        }
        
        circleFragmentation = new CircleFragmentation(offset, split);
        return true;
    }
}