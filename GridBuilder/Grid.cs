using DataStructures.Geometry;

namespace GridBuilder;

public class Grid
{
    private readonly GridBuilder _builder = new();
    
    public new IReadOnlyList<Point> Points { get; }
    public new IReadOnlyList<FiniteElement> FiniteElements { get; }

    public void Build(GridParameters parameters)
    {
        _builder.BuildGrid(parameters);
    }
}