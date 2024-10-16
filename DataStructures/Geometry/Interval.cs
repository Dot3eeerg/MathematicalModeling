using Newtonsoft.Json;

namespace DataStructures.Geometry;

public readonly record struct Interval(double LeftBorder, double RightBorder)
{
    [JsonIgnore] public double Length { get; } = Math.Abs(RightBorder - LeftBorder);
    [JsonIgnore] public double Center { get; } = (LeftBorder + RightBorder) / 2;
    
    public override string ToString() => $"[{LeftBorder}, {RightBorder}]";
}