using DataStructures.Geometry;

namespace GridBuilder;

public class Grid: GridBuilder
{
    public IReadOnlyList<Point> Points { get; } = _points.AsReadOnly();
    public IReadOnlyList<FiniteElement> FiniteElements { get; } = _finiteElements.AsReadOnly();
    
    
}