using DataStructures.Geometry;

namespace MathFem;

public class Integration(SegmentGaussOrder9 quadratures)
{
    private Point _point = new Point(0.0, 0.0);
    
    public double Gauss2D(Func<Point, double> psi)
    {
        double result = 0;
        _point.Reset();

        for (int i = 0; i < quadratures.Size; i++)
        {
            _point.X = (quadratures.GetPoint(i) + 1) / 2.0;

            for (int j = 0; j < quadratures.Size; j++)
            {
                _point.Y = (quadratures.GetPoint(j) + 1) / 2.0;

                result += psi(_point) * quadratures.GetWeight(i) * quadratures.GetWeight(j);
            }
        }

        return result / 4.0;
    }
}