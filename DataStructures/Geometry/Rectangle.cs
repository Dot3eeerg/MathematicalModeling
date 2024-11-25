namespace DataStructures.Geometry;

public readonly record struct Rectangle(Point LeftBottom, Point RightTop)
{
    public Point LeftTop { get; } = new(LeftBottom.X, RightTop.Y);
    public Point RightBottom { get; } = new(RightTop.X, LeftBottom.Y);
}