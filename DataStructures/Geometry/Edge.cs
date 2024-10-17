namespace DataStructures.Geometry;

public readonly record struct Edge(int Node1, int Node2)
{
    public override string ToString() => $"({Node1} {Node2})";
}