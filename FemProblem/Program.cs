using GridBuilder;

GridParameters parameters = GridParameters.ReadFromJson("Input/GridInput.json");

Grid grid = new Grid();
grid.Build(parameters);

Console.WriteLine("Hello, World!");