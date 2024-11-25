using GridBuilder;

namespace FemProblem;

public static class PortraitBuilder
{
    public static void Build(Grid grid, out int[] ig, out int[] jg)
    {
        int localSize = grid.FiniteElements[0].Nodes.Count();
        
        HashSet<int>[] list = new HashSet<int>[grid.Nodes.Count].Select(_ => new HashSet<int>()).ToArray();
        foreach (var element in grid.FiniteElements.ToArray())
            foreach (var pos in element.Nodes)
                foreach (var node in element.Nodes)
                    if (pos > node)
                        list[pos].Add(node);

        list = list.Select(childList => childList.Order().ToHashSet()).ToArray();
        int count = list.Sum(childList => childList.Count);
        
        ig = new int[list.Length + 1];
        ig[0] = 0;

        for (int i = 0; i < list.Length; i++)
            ig[i + 1] = ig[i] + list[i].Count;

        jg = new int[ig[^1]];
        int k = 0;

        for (int i = 0; i < list.Length; i++)
        {
            var hashList = list[i].ToArray();
            for (int j = 0; j < hashList.Length; j++)
            {
                jg[k++] = hashList[j];
            }
        }
    } 
}