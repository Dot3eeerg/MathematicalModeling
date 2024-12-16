namespace DataStructures.Geometry;

public enum FiniteElementType
{
    Linear,
    Quadratic,
}

public record struct Material(double Lambda, double Gamma);

public readonly record struct FiniteElement(IReadOnlyList<int> Nodes, Material ElementMaterial, FiniteElementType FiniteElementType)
{
    public string PrintByBasis()
    {
        switch (FiniteElementType)
        {
            case FiniteElementType.Linear:
                return
                    $"1 - {Nodes[0]}\n" +
                    $"2 - {Nodes[1]}\n" +
                    $"3 - {Nodes[2]}\n" +
                    $"4 - {Nodes[3]}\n";
            
            case FiniteElementType.Quadratic:
                return
                    $"1 - {Nodes[0]}\n" +
                    $"2 - {Nodes[1]}\n" +
                    $"3 - {Nodes[2]}\n" +
                    $"4 - {Nodes[3]}\n" +
                    $"5 - {Nodes[4]}\n" +
                    $"6 - {Nodes[5]}\n" +
                    $"7 - {Nodes[6]}\n" +
                    $"8 - {Nodes[7]}\n" +
                    $"9 - {Nodes[8]}";
            
            default:
                throw new ArgumentOutOfRangeException(nameof(FiniteElementType), FiniteElementType, null);
        }
    }

    public override string ToString()
    {
        switch (FiniteElementType)
        {
            case FiniteElementType.Linear:
                return $"{Nodes[0]} {Nodes[1]} {Nodes[3]} {Nodes[2]} {ElementMaterial.Lambda}";
            
            case FiniteElementType.Quadratic:
                return $"{Nodes[0]} {Nodes[1]} {Nodes[2]} {Nodes[5]} {Nodes[8]} {Nodes[7]} {Nodes[6]} {Nodes[3]} {ElementMaterial.Lambda} {Nodes[4]}";
            
            default:
                throw new ArgumentOutOfRangeException($"Cannot find finite element type");
        }
    }
}