namespace DataStructures.Geometry;

public record struct FiniteElement(IReadOnlyList<int> Nodes, double Material)
{
    // public override string ToString() => $"{Nodes[0]} {Nodes[1]} {Nodes[2]} {Nodes[3]} {Material}";
    public override string ToString() => $"{Nodes[0]} {Nodes[1]} {Nodes[3]} {Nodes[2]} {Material}";
}