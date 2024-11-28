using DataStructures.Geometry;

namespace FemProblem;

public interface ITest
{
    double U(Point p);
    
    double F(Point p);
    
    double Theta(Point p);
}

public class Test1 : ITest
{
    public double U(Point p) => 2;

    public double F(Point p) => 0;

    public double Theta(Point p) => 0;
}

public class Test2 : ITest
{
    public double U(Point p) => 4 * p.Y;

    public double F(Point p) => 0;
    
    public double Theta(Point p) => -4;
}

public class Test3 : ITest
{
    public double U(Point p) => p.Y * p.Y;

    public double F(Point p) => -2;
    
    public double Theta(Point p) => -2 * p.X;
}

public class Test4 : ITest
{
    public double U(Point p) => p.Y * p.Y * p.Y;

    public double F(Point p) => -6 * p.Y;
    
    public double Theta(Point p) => -2 * p.X;
}
