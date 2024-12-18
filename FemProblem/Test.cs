using DataStructures.Geometry;

namespace FemProblem;

public interface ITest
{
    double U(Point p);

    double F(Point p, Material material);
    
    double Theta(Point p, Material material);
}

public class Test1 : ITest
{
    public double U(Point p) => 2;
    
    public double F(Point p, Material material) => 2;

    public double Theta(Point p, Material material) => 0;
}

public class Test2 : ITest
{
    public double U(Point p) => 4 * p.X;
    
    public double F(Point p, Material material) => material.Gamma * 4 * p.X;
    
    public double Theta(Point p, Material material) => -4 * material.Lambda;
}

public class Test3 : ITest
{
    public double U(Point p) => p.Y * p.Y;
    
    public double F(Point p, Material material) => -2 * material.Lambda + p.Y * p.Y * material.Gamma;
    
    public double Theta(Point p, Material material) => -2 * p.Y;
}

public class Test4 : ITest
{
    public double U(Point p) => p.Y * p.Y * p.Y;
    
    public double F(Point p, Material material) => -6 * p.Y + p.Y * p.Y * p.Y;
    
    public double Theta(Point p, Material material) => -3 * p.Y * p.Y;
}

public class Test5 : ITest
{
    public double U(Point p) => p.Y * p.Y * p.X + p.X * p.X;
    
    public double F(Point p, Material material) => -2 - 2 * p.X + U(p);
    
    public double Theta(Point p, Material material) => -3 * p.Y * p.Y;
}

public class Test6 : ITest
{
    public double U(Point p) => Math.Sin(p.X);
    
    public double F(Point p, Material material) => -Math.Sin(p.X);
    
    public double Theta(Point p, Material material) => -3 * p.Y * p.Y;
}
