using GridBuilder;

GridParameters parameters = GridParameters.ReadFromJson("Input/GridInput.json");

Grid grid = new Grid();
grid.Build(parameters);
grid.SaveGrid("Grid");

Console.WriteLine("Hello, World!");