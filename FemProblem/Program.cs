using FemProblem;
using GridBuilder;

GridParameters parameters = GridParameters.ReadFromJson("Input/GridInput.json");

Grid grid = new Grid();
// grid.Build(parameters);
grid.QuadraticBuild(parameters);
grid.SaveGrid("Grid");

// Console.WriteLine(grid.GetElementBasis(18));

FEM fem = new FEM();
fem.SetGrid(grid);
fem.SetTest(new Test2());
fem.Solve();

Console.WriteLine("Hello, World!");