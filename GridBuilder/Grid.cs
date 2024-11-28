using DataStructures.Geometry;

namespace GridBuilder;

public class Grid
{
    private readonly LinearGridBuilder _builder = new();
    private readonly QuadraticGridBuilder _quadraticBuilder = new();
    
    public IReadOnlyList<Point>? Nodes { get; private set; }
    public IReadOnlyList<FiniteElement>? FiniteElements { get; private set; }
    public IReadOnlySet<int>? DirichletNodes { get; private set; }
    // public IReadOnlySet<Edge>? NeumannEdges { get; private set; }
    public IReadOnlySet<Edge3>? NeumannEdges { get; private set; }
    public IReadOnlySet<int>? FictitiousNodes { get; private set; }

    public void SetElement()
    {
        // Nodes = new[] { new Point(1, 1), new Point(5, 3), new(2, 5), new(4, 5) };
        Nodes = new[] { new Point(1, 1), new Point(3, 2), new Point(5, 3), new(1.5, 3), new(3, 3.5), new(4.5, 4), new(2, 5), new(3, 5), new(4, 5) };
        FiniteElements = new[] { new FiniteElement(new []{ 0, 1, 2, 3, 4, 5, 6, 7, 8 }, 1, 0) };
    }

    // public void LinearBuild(GridParameters parameters)
    // {
    //     (Nodes, FiniteElements, DirichletNodes, NeumannEdges) = _builder.BuildGrid(parameters);
    // }

    public void QuadraticBuild(GridParameters parameters)
    {
        (Nodes, FiniteElements, DirichletNodes, NeumannEdges, FictitiousNodes) = _quadraticBuilder.BuildGrid(parameters);
    }

    public string GetElementBasis(int iElem)
    {
        if (FiniteElements is null)
        {
            throw new InvalidOperationException("Finite elements cannot be null");
        }

        if (FiniteElements.Count < iElem)
        {
            throw new InvalidOperationException("Finite elements cannot be less than " + iElem);
        }

        return FiniteElements[iElem].PrintByBasis();
    }

    public void SaveGrid(string folder)
    {
        var sw = new StreamWriter($"{folder}/points");
        foreach (var point in Nodes!)
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
        
        sw = new StreamWriter($"{folder}/dirichlet");
        foreach (var node in DirichletNodes!)
        {
            sw.WriteLine(node.ToString());
        }
        sw.Close();
        
        sw = new StreamWriter($"{folder}/neumann");
        foreach (var edge in NeumannEdges!)
        {
            sw.WriteLine(edge.ToString());
        }
        sw.Close();
    }
}