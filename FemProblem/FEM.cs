using DataStructures;
using GridBuilder;

namespace FemProblem;

public class FEM
{
    private Grid _grid = default!;
    private ITest _test = default!;
    private IterativeSolver _iterativeSolver = default!;
    private Vector<double> _localVector = default!;
    private Vector<double> _globalVector = default!;
    private SLAEAssembler _slaeAssembler = default!;
    
    public void SetGrid(Grid grid) => _grid = grid;
    
    public void SetTest(ITest test) => _test = test;

    public void Solve()
    {
        Initialize();
        AssemblySlae();
        AccountDirichletBoundaries();
        AccountFictitiousNodes();
        
        _iterativeSolver.SetMatrix(_slaeAssembler.GlobalMatrix);
        _iterativeSolver.SetVector(_globalVector);
        _iterativeSolver.Compute();
    }
    
    private void Initialize()
    {
        _slaeAssembler = new SLAEAssembler();
        
        PortraitBuilder.Build(_grid, out var ig, out var jg);
        _slaeAssembler.GlobalMatrix = new(ig.Length - 1, jg.Length)
        {
            Ig = ig,
            Jg = jg
        };
        _slaeAssembler.SetBasis(new BiQuadraticBasis());
        _slaeAssembler.Grid = _grid;

        _globalVector = new(ig.Length - 1);
        _localVector = new(_slaeAssembler.Basis.Size);

        _iterativeSolver = new CGMCholesky(5000, 1e-14);
    }

    private void AssemblySlae()
    {
        for (int iElem = 0; iElem < _grid.FiniteElements.Count; iElem++)
        {
            var element = _grid.FiniteElements[iElem];

            _slaeAssembler.BuildLocalMatrices(iElem);
            BuildLocalVector(iElem);

            for (int i = 0; i < _slaeAssembler.Basis.Size; i++)
            {
                _globalVector[element.Nodes[i]] += _localVector[i];

                for (int j = 0; j < _slaeAssembler.Basis.Size; j++)
                {
                    _slaeAssembler.FillGlobalMatrix(element.Nodes[i], element.Nodes[j],
                        _slaeAssembler.StiffnessMatrix[i, j]);
                }
            }
        } 
    }
    
    private void BuildLocalVector(int iElem)
    {
        _localVector.Fill(0.0);

        for (int i = 0; i < _slaeAssembler.Basis.Size; i++)
        {
            for (int j = 0; j < _slaeAssembler.Basis.Size; j++)
            {
                _localVector[i] += _slaeAssembler.MassMatrix[i, j] *
                                   _test.F(_grid.Nodes[_grid.FiniteElements[iElem].Nodes[j]]);
            }
        }
    }

    private void AccountDirichletBoundaries()
    {
        foreach (var node in _grid.DirichletNodes!)
        {
            _slaeAssembler.GlobalMatrix.Di[node] = 1;
            var value = _test.U(_grid.Nodes![node]);
            _globalVector[node] = value;

            for (int i = _slaeAssembler.GlobalMatrix.Ig[node]; i < _slaeAssembler.GlobalMatrix.Ig[node + 1]; i++)
            {
                _globalVector[_slaeAssembler.GlobalMatrix.Jg[i]] -= value * _slaeAssembler.GlobalMatrix.Gg[i];
                _slaeAssembler.GlobalMatrix.Gg[i] = 0;
            }

            for (int i = node + 1; i < _slaeAssembler.GlobalMatrix.Size; i++)
            {
                for (int j = _slaeAssembler.GlobalMatrix.Ig[i]; j < _slaeAssembler.GlobalMatrix.Ig[i + 1]; j++)
                {
                    if (_slaeAssembler.GlobalMatrix.Jg[j] == node)
                    {
                        _globalVector[i] -= value * _slaeAssembler.GlobalMatrix.Gg[j];
                        _slaeAssembler.GlobalMatrix.Gg[j] = 0;
                    }
                }
            }
        }
    }

    private void AccountFictitiousNodes()
    {
        foreach (var node in _grid.FictitiousNodes!)
        {
            _slaeAssembler.GlobalMatrix.Di[node] = 1;
        }
    }
}