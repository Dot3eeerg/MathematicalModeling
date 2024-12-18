using FemProblem;
using GridBuilder;

GridParameters parameters = GridParameters.ReadFromJson("Input/GridInput.json");

Grid grid = new Grid();
// grid.LinearBuild(parameters);
grid.QuadraticBuild(parameters);
grid.SaveGrid("Grid");

// Console.WriteLine(grid.GetElementBasis(18));

FEM fem = new FEM();
fem.SetGrid(grid);
fem.SetTest(new Test2());
fem.SetBasis(new BiQuadraticBasis());
fem.Solve();

// SLAEAssembler assembler = new SLAEAssembler();
// // assembler.SetBasis(new BiLinearBasis());
// assembler.SetBasis(new QuadraticBasis());
// Grid grid = new Grid();
// grid.SetElement();
//
// assembler.Grid = grid;
// assembler.BuildLocalMatrices(0);
