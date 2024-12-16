using DataStructures;
using DataStructures.Geometry;
using Newtonsoft.Json;

namespace GridBuilder;

public class GridParameters(
    double radius,
    Interval xInterval,
    int xInnerSplits,
    double xCoefficient,
    Interval yInterval,
    int yInnerSplits,
    double yCoefficient,
    int circleSplits,
    int circleRadiusSplits,
    double circleCoefficient,
    CircleFragmentation circleTear,
    byte leftBorder,
    byte bottomBorder,
    byte circleBorder,
    byte circleTearBorder,
    Material material,
    IEnumerable<CircleMaterial> circleMaterials)
{
    public double Radius { get; } = radius;
    
    [JsonProperty("X interval"), JsonRequired]
    public Interval XInterval { get; } = xInterval;
    
    [JsonProperty("X inner splits"), JsonRequired]
    public int XInnerSplits { get; } = xInnerSplits;
    
    [JsonProperty("X coefficient"), JsonRequired]
    public double XCoefficient { get; } = xCoefficient;
    
    [JsonProperty("Y interval"), JsonRequired]
    public Interval YInterval { get; } = yInterval;
    
    [JsonProperty("Y inner splits"), JsonRequired]
    public int YInnerSplits { get; } = yInnerSplits;
    
    [JsonProperty("Y coefficient"), JsonRequired]
    public double YCoefficient { get; } = yCoefficient;
    
    [JsonProperty("Circle splits"), JsonRequired]
    public int CircleSplits { get; } = circleSplits;
    
    [JsonProperty("Circle radius splits"), JsonRequired]
    public int CircleRadiusSplits { get; } = circleRadiusSplits;

    [JsonProperty("Circle coefficient"), JsonRequired]
    public double CircleCoefficient { get; } = circleCoefficient;

    [JsonProperty("Circle tear"), JsonRequired]
    public CircleFragmentation CircleTear { get; } = circleTear;
    
    [JsonProperty("Left border"), JsonRequired]
    public byte LeftBorder { get; } = leftBorder;
    
    [JsonProperty("Bottom border"), JsonRequired]
    public byte BottomBorder { get; } = bottomBorder;
    
    [JsonProperty("Circle border"), JsonRequired]
    public byte CircleBorder { get; } = circleBorder;
    
    [JsonProperty("Circle tear border"), JsonRequired]
    public byte CircleTearBorder { get; } = circleTearBorder;

    public Material Material { get; } = material;
    
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
