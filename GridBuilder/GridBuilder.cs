using DataStructures.Geometry;

namespace GridBuilder;

public abstract class GridBuilder
{
    private GridParameters? Parameters { get; set; }

    protected static List<Point> _points = null!;
    protected static List<FiniteElement> _finiteElements = null!;

    protected void SetParameters(GridParameters parameters) => Parameters = parameters;
    
    public void BuildGrid()
    {
        if (Parameters == null)
            throw new ArgumentNullException(nameof(Parameters), "Grid parameters cannot be null.");
        
        CreatePoints();
        CreateElements();
        AccountBoundaryConditions();
    }

    private void CreatePoints()
    {
        
    }
    
    private void CreateElements()
    {
        
    }

    private void AccountBoundaryConditions()
    {
        
    }
}