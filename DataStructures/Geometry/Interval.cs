using Newtonsoft.Json;

namespace DataStructures.Geometry;

public readonly record struct Interval(double LeftBorder, double RightBorder)
{
    public double Length => Math.Abs(RightBorder - LeftBorder);
    public double Center => (LeftBorder + RightBorder) / 2;
    
    public override string ToString() => $"[{LeftBorder}, {RightBorder}]";
}