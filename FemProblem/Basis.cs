using DataStructures.Geometry;

public interface IBasis2D
{
    int Size { get; }
    double GetPsi(int number, Point point);
    double GetDPsi(int number, int dNumber, Point point);
}

public readonly record struct BiLinearBasis : IBasis2D
{
    public int Size => 4;

    public BiLinearBasis() { }
    
    public double GetPsi(int number, Point point)
        => number switch
        {
            0 => GetXi(0, point.X) * GetXi(0, point.Y),
            1 => GetXi(1, point.X) * GetXi(0, point.Y),
            2 => GetXi(0, point.X) * GetXi(1, point.Y),
            3 => GetXi(1, point.X) * GetXi(1, point.Y),
            _ => throw new ArgumentOutOfRangeException(nameof(number), number, "Not expected function number")
        };
    
    public double GetDPsi(int number, int dNumber, Point point)
        => dNumber switch
        {
            0 => number switch
            {
                0 => -GetXi(0, point.Y),
                1 => GetXi(0, point.Y),
                2 => -GetXi(1, point.Y),
                3 => GetXi(1, point.Y),
                _ => throw new ArgumentOutOfRangeException(nameof(number), number, "Not expected function number")
            },
            1 => number switch
            {
                0 => -GetXi(0, point.X),
                1 => -GetXi(1, point.X),
                2 => GetXi(0, point.X),
                3 => GetXi(1, point.X),
                _ => throw new ArgumentOutOfRangeException(nameof(number), number, "Not expected function number")
            },
            _ => throw new ArgumentOutOfRangeException(nameof(dNumber), dNumber, "Not expected function number")
        };

    private double GetXi(int number, double value)
        => number switch
        {
            0 => 1 - value,
            1 => value,
            _ => throw new ArgumentOutOfRangeException(nameof(number), number, "Not expected Xi member")
        };
}

public readonly record struct BiQuadraticBasis : IBasis2D
{
    public int Size => 9;

    public BiQuadraticBasis() { }
    
    public double GetPsi(int number, Point point)
        => number switch
        {
            0 => GetXi(0, point.X) * GetXi(0, point.Y),
            1 => GetXi(1, point.X) * GetXi(0, point.Y),
            2 => GetXi(2, point.X) * GetXi(0, point.Y),
            3 => GetXi(0, point.X) * GetXi(1, point.Y),
            4 => GetXi(1, point.X) * GetXi(1, point.Y),
            5 => GetXi(2, point.X) * GetXi(1, point.Y),
            6 => GetXi(0, point.X) * GetXi(2, point.Y),
            7 => GetXi(1, point.X) * GetXi(2, point.Y),
            8 => GetXi(2, point.X) * GetXi(2, point.Y),
            _ => throw new ArgumentOutOfRangeException(nameof(number), number, "Not expected function number")
        };
    
    public double GetDPsi(int number, int dNumber, Point point)
        => dNumber switch
        {
            0 => number switch
            {
                0 => GetDXi(0, point.X) * GetXi(0, point.Y),
                1 => GetDXi(1, point.X) * GetXi(0, point.Y),
                2 => GetDXi(2, point.X) * GetXi(0, point.Y),
                3 => GetDXi(0, point.X) * GetXi(1, point.Y),
                4 => GetDXi(1, point.X) * GetXi(1, point.Y),
                5 => GetDXi(2, point.X) * GetXi(1, point.Y),
                6 => GetDXi(0, point.X) * GetXi(2, point.Y),
                7 => GetDXi(1, point.X) * GetXi(2, point.Y),
                8 => GetDXi(2, point.X) * GetXi(2, point.Y),
                _ => throw new ArgumentOutOfRangeException(nameof(number), number, "Not expected function number")
            },
            1 => number switch
            {
                0 => GetXi(0, point.X) * GetDXi(0, point.Y),
                1 => GetXi(1, point.X) * GetDXi(0, point.Y),
                2 => GetXi(2, point.X) * GetDXi(0, point.Y),
                3 => GetXi(0, point.X) * GetDXi(1, point.Y),
                4 => GetXi(1, point.X) * GetDXi(1, point.Y),
                5 => GetXi(2, point.X) * GetDXi(1, point.Y),
                6 => GetXi(0, point.X) * GetDXi(2, point.Y),
                7 => GetXi(1, point.X) * GetDXi(2, point.Y),
                8 => GetXi(2, point.X) * GetDXi(2, point.Y),
                _ => throw new ArgumentOutOfRangeException(nameof(number), number, "Not expected function number")
            },
            _ => throw new ArgumentOutOfRangeException(nameof(dNumber), dNumber, "Not expected function number")
        };

    private double GetXi(int number, double value)
        => number switch
        {
            0 => 2 * (value - 0.5) * (value - 1),
            1 => -4 * value * (value - 1),
            2 => 2 * value * (value - 0.5),
            _ => throw new ArgumentOutOfRangeException(nameof(number), number, "Not expected Xi member")
        };

    private double GetDXi(int number, double value)
        => number switch
        {
            0 => 4 * value - 3,
            1 => - 8 * value + 4,
            2 => 4 * value - 1,
            _ => throw new ArgumentOutOfRangeException(nameof(number), number, "Not expected Xi member")
        };
}
