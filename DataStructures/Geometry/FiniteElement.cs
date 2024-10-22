namespace DataStructures.Geometry;

public enum FiniteElementType
{
    Linear,
    Quadratic,
    Cubic
}

public record struct FiniteElement(IReadOnlyList<int> Nodes, double Material, FiniteElementType FiniteElementType)
{
    // public override string ToString() => $"{Nodes[0]} {Nodes[1]} {Nodes[2]} {Nodes[3]} {Material}";
    // public override string ToString() => $"{Nodes[0]} {Nodes[1]} {Nodes[3]} {Nodes[2]} {Material}";

    public override string ToString()
    {
        switch (FiniteElementType)
        {
            case FiniteElementType.Linear:
                return $"{Nodes[0]} {Nodes[1]} {Nodes[3]} {Nodes[2]} {Material}";
            
            case FiniteElementType.Quadratic:
                return $"{Nodes[0]} {Nodes[2]} {Nodes[8]} {Nodes[6]} {Material}";
            
            case FiniteElementType.Cubic:
                return $"{Nodes[0]} {Nodes[3]} {Nodes[15]} {Nodes[12]} {Material}";
            
            default:
                throw new ArgumentOutOfRangeException($"Cannot find finite element type");
        }
    }
}