namespace MathFem;

public interface IQuadrature
{
    int Size { get; }
    double GetPoint(int number);
    double GetWeight(int number);
}

public readonly record struct SegmentGaussOrder9 : IQuadrature
{
    public int Size => 5;

    public SegmentGaussOrder9() { }

    public double GetPoint(int number)
        => number switch
        {
            0 => 0.0,
            1 => 1.0 / 3.0 * Math.Sqrt(5 - 2 * Math.Sqrt(10.0 / 7.0)),
            2 => -1.0 / 3.0 * Math.Sqrt(5 - 2 * Math.Sqrt(10.0 / 7.0)),
            3 => 1.0 / 3.0 * Math.Sqrt(5 + 2 * Math.Sqrt(10.0 / 7.0)),
            4 => -1.0 / 3.0 * Math.Sqrt(5 + 2 * Math.Sqrt(10.0 / 7.0)),
            _ => throw new ArgumentOutOfRangeException(nameof(number), number, "Not expected point number")
        };
    
    public double GetWeight(int number)
        => number switch
        {
            0 => 128.0 / 225.0,
            1 => (322.0 + 13.0 * Math.Sqrt(70.0)) / 900.0,
            2 => (322.0 + 13.0 * Math.Sqrt(70.0)) / 900.0,
            3 => (322.0 - 13.0 * Math.Sqrt(70.0)) / 900.0,
            4 => (322.0 - 13.0 * Math.Sqrt(70.0)) / 900.0,
            _ => throw new ArgumentOutOfRangeException(nameof(number), number, "Not expected weight number")
        };
}