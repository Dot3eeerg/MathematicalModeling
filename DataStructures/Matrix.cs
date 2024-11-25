namespace DataStructures;

public class Matrix
{
    private readonly double[,] _storage;
    public int Size { get; }

    public double this[int i, int j]
    {
        get => _storage[i, j];
        set => _storage[i, j] = value;
    }

    public Matrix(int size)
    {
        _storage = new double[size, size];
        Size = size;
    }

    public void Clear() => Array.Clear(_storage, 0, _storage.Length);

    public void Copy(Matrix destination)
    {
        for (int i = 0; i < destination.Size; i++)
        {
            for (int j = 0; j < destination.Size; j++)
            {
                destination[i, j] = _storage[i, j];
            }
        }
    }

    public static Matrix operator +(Matrix fstMatrix, Matrix sndMatrix)
    {
        Matrix resultMatrix = new(fstMatrix.Size);

        for (int i = 0; i < resultMatrix.Size; i++)
        {
            for (int j = 0; j < resultMatrix.Size; j++)
            {
                resultMatrix[i, j] = fstMatrix[i, j] + sndMatrix[i, j];
            }
        }

        return resultMatrix;
    }

    public static Matrix operator *(double value, Matrix matrix)
    {
        Matrix resultMatrix = new(matrix.Size);

        for (int i = 0; i < resultMatrix.Size; i++)
        {
            for (int j = 0; j < resultMatrix.Size; j++)
            {
                resultMatrix[i, j] = value * matrix[i, j];
            }
        }

        return resultMatrix;
    }

    public static Vector<double> operator *(Matrix matrix, Vector<double> vector)
    {
        var result = new Vector<double>(matrix.Size);

        for (int i = 0; i < matrix.Size; i++)
        {
            for (int j = 0; j < matrix.Size; j++)
            {
                result[i] += matrix[i, j] * vector[j];
            }
        }

        return result;
    }
}
