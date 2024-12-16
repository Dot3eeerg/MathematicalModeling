namespace DataStructures.Geometry;

public readonly record struct Edge(int Node1, int Node2)
{
    public override string ToString() => $"{Node1} {Node2}";
}

public readonly record struct Edge3(int Node1, int Node2, int Node3, Material Material)
{
    public override string ToString() => $"{Node1} {Node2} {Node3}";
}
