using System.Globalization;
using Newtonsoft.Json;

namespace DataStructures.Geometry;

public class CircleShapeJsonConverter : JsonConverter
{
    public override bool CanConvert(Type typeToConvert) =>  typeof(Interval) == typeToConvert;

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        string? value = reader.Value?.ToString();
        
        if (value != null && CircleShape.TryParse(value, out var circleShape))
        {
            return circleShape;
        }

        throw new JsonSerializationException($"Cannot convert value '{value}' to CircleShape.");
    }
    
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        value ??= new CircleShape();
        var circleShape = (CircleShape)value;
        writer.WriteRawValue($"\"[{circleShape.Degrees}, {circleShape.Material}]\"");
    } 
}

[JsonConverter(typeof(CircleShapeJsonConverter))]
public record struct CircleShape(int Degrees, double Material)
{
    public static bool TryParse(string value, out CircleShape circleShape)
    {
        var words = value.Split(new[] { ' ', ',', '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length != 2 || !int.TryParse(words[0], CultureInfo.InvariantCulture, out var degrees) ||
            !int.TryParse(words[1], CultureInfo.InvariantCulture, out var material))
        {
            circleShape = default;
            return false;
        }
        
        circleShape = new CircleShape(degrees, material);
        return true;
    }
}