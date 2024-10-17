namespace DataStructures.Geometry;

public record struct Point(double X, double Y)
{
    public override string ToString() => $"{X} {Y}";

    public static Point operator +(Point p1, Point p2) => new (p1.X + p2.X, p1.Y + p2.Y);
    public static Point operator -(Point p1, Point p2) => new (p1.X - p2.X, p1.Y - p2.Y);
    public static Point operator *(Point p, double num) => new (p.X * num, p.Y * num);
    public static Point operator /(Point p, double num) => new (p.X / num, p.Y / num);
}