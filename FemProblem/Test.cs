using DataStructures.Geometry;

namespace FemProblem;

public interface ITest
{
    double U(Point p);
    
    double F(Point p);
}

public class Test1 : ITest
{
    public double U(Point p) => 2;

    public double F(Point p) => 0;
}

public class Test2 : ITest
{
    public double U(Point p) => p.X;

    public double F(Point p) => 0;
}
