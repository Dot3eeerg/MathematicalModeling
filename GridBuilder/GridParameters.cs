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
    byte circleBorder)
{
    [JsonProperty("Radius"), JsonRequired]
    public double Radius { get; } = radius;
    
    [JsonProperty("XInterval"), JsonRequired]
    public Interval XInterval { get; } = xInterval;
    
    [JsonProperty("XSplits"), JsonRequired]
    public int XSplits { get; } = xSplits;
    
    [JsonProperty("XInnerSplits"), JsonRequired]
    public int XInnerSplits { get; } = xInnerSplits;
    
    [JsonProperty("XCoefficient"), JsonRequired]
    public double XCoefficient { get; } = xCoefficient;
    
    [JsonProperty("YInterval"), JsonRequired]
    public Interval YInterval { get; } = yInterval;
    
    [JsonProperty("YSplits"), JsonRequired]
    public int YSplits { get; } = ySplits;
    
    [JsonProperty("YInnerSplits"), JsonRequired]
    public int YInnerSplits { get; } = yInnerSplits;
    
    [JsonProperty("YCoefficient"), JsonRequired]
    public double YCoefficient { get; } = yCoefficient;
    
    [JsonProperty("Circle tear"), JsonRequired]
    public CircleFragmentation CircleTear { get; } = circleTear;
    
    [JsonProperty("Left border"), JsonRequired]
    public byte LeftBorder { get; } = leftBorder;
    
    [JsonProperty("Bottom border"), JsonRequired]
    public byte BottomBorder { get; } = bottomBorder;
    
    [JsonProperty("Circle border"), JsonRequired]
    public byte CircleBorder { get; } = circleBorder;

    public static GridParameters ReadFromJson(string json)
    {
        if (!File.Exists(json)) throw new FileNotFoundException($"File {json} not found");
        
        using var stream = new StreamReader(json);
        return JsonConvert.DeserializeObject<GridParameters>(stream.ReadToEnd()) ??
               throw new NullReferenceException("Can't deserialize");
    }
}
