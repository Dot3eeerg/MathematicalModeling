using DataStructures.Geometry;

namespace GridBuilder;

public class Grid
{
    private readonly GridBuilder _builder = new();
    
    public new IReadOnlyList<Point>? Points { get; private set; }
    public new IReadOnlyList<FiniteElement>? FiniteElements { get; private set; }

    public void Build(GridParameters parameters)
    {
        (Points, FiniteElements) = _builder.BuildGrid(parameters);
    }

    public void SaveGrid(string folder)
    {
        var sw = new StreamWriter($"{folder}/points");
        foreach (var point in Points!)
        {
            sw.WriteLine(point.ToString());
        }
        sw.Close();
        
        sw = new StreamWriter($"{folder}/finite_elements");
        foreach (var element in FiniteElements!)
        {
            sw.WriteLine(element.ToString());
        }
        sw.Close();
    }
}