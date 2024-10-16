using DataStructures.Geometry;
using Newtonsoft.Json;

namespace GridBuilder;

public class GridParameters(
    double radius,
    Interval xInterval,
    int xSplits,
    int xInnerSplits,
    double xCoefficient,
    Interval yInterval,
    int ySplits,
    int yInnerSplits,
    double yCoefficient,
    CircleFragmentation circleTear,
    byte leftBorder,
    byte bottomBorder,
    byte circleBorder,
    double material,
    IEnumerable<CircleMaterial> circleMaterials)
{
    public double Radius { get; } = radius;
    
    [JsonProperty("X interval"), JsonRequired]
    public Interval XInterval { get; } = xInterval;
    
    [JsonProperty("X splits"), JsonRequired]
    public int XSplits { get; } = xSplits;
    
    [JsonProperty("X inner splits"), JsonRequired]
    public int XInnerSplits { get; } = xInnerSplits;
    
    [JsonProperty("X coefficient"), JsonRequired]
    public double XCoefficient { get; } = xCoefficient;
    
    [JsonProperty("Y interval"), JsonRequired]
    public Interval YInterval { get; } = yInterval;
    
    [JsonProperty("Y splits"), JsonRequired]
    public int YSplits { get; } = ySplits;
    
    [JsonProperty("Y inner splits"), JsonRequired]
    public int YInnerSplits { get; } = yInnerSplits;
    
    [JsonProperty("Y coefficient"), JsonRequired]
    public double YCoefficient { get; } = yCoefficient;
    
    [JsonProperty("Circle tear"), JsonRequired]
    public CircleFragmentation CircleTear { get; } = circleTear;
    
    [JsonProperty("Left border"), JsonRequired]
    public byte LeftBorder { get; } = leftBorder;
    
    [JsonProperty("Bottom border"), JsonRequired]
    public byte BottomBorder { get; } = bottomBorder;
    
    [JsonProperty("Circle border"), JsonRequired]
    public byte CircleBorder { get; } = circleBorder;

    public double Material { get; } = material;
    
    [JsonProperty("Circle materials"), JsonRequired]
    public IReadOnlyList<CircleMaterial> CircleMaterials { get; } = circleMaterials.ToList();

    public static GridParameters ReadFromJson(string json)
    {
        if (!File.Exists(json)) throw new FileNotFoundException($"File {json} not found");
        
        using var stream = new StreamReader(json);
        return JsonConvert.DeserializeObject<GridParameters>(stream.ReadToEnd()) ??
               throw new NullReferenceException("Can't deserialize");
    }
}
