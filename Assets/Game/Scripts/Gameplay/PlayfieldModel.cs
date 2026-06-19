using System;

/// <summary>
/// Standard seven Tetromino types. The index of each enum value maps to
/// a grid cell value of (index + 1). Value 0 in the grid means "empty".
/// </summary>
public enum PieceType
{
    I = 0,
    O = 1,
    T = 2,
    S = 3,
    Z = 4,
    J = 5,
    L = 6
}

/// <summary>
/// Snapshot of a Tetromino's placement state on the playfield.
/// </summary>
public struct PieceState
{
    public PieceType Type;
    public int Row;
    public int Col;
    public int Rotation; // 0-3 representing four orientations
}

/// <summary>
/// Plain C# class that owns the logical state of the 10x22 playfield grid.
/// Dimensions: GameRules.PLAYFIELD_TOTAL_HEIGHT rows x GameRules.PLAYFIELD_WIDTH cols.
/// Row 0 and row 1 are hidden buffer rows above the visible area.
/// Row 2 is the visible top row; row 21 is the floor.
/// Cell value 0 = empty; cell value 1-7 = locked piece type (PieceType index + 1).
/// </summary>
public class PlayfieldModel
{
    /// <summary>Shorthand for total height constant.</summary>
    private readonly int _totalHeight = GameRules.PLAYFIELD_TOTAL_HEIGHT;

    /// <summary>Shorthand for width constant.</summary>
    private readonly int _width = GameRules.PLAYFIELD_WIDTH;

    /// <summary>The logical grid. [row, col] where row 0 is top buffer.</summary>
    private int[,] _grid;

    public PlayfieldModel()
    {
        _grid = new int[GameRules.PLAYFIELD_TOTAL_HEIGHT, GameRules.PLAYFIELD_WIDTH];
    }

    // -----------------------------------------------------------------------
    // Cell accessors
    // -----------------------------------------------------------------------

    /// <summary>
    /// Returns the cell value at the given row and column.
    /// Value is 0 for empty or 1-7 for a locked piece type.
    /// </summary>
    public int GetCell(int row, int col)
    {
        return _grid[row, col];
    }

    /// <summary>
    /// Returns true when the cell at (row, col) is empty (value 0).
    /// </summary>
    public bool IsEmpty(int row, int col)
    {
        return _grid[row, col] == 0;
    }

    /// <summary>
    /// Returns true when both indices are within the playfield bounds
    /// [0, PLAYFIELD_TOTAL_HEIGHT) for rows and [0, PLAYFIELD_WIDTH) for columns.
    /// </summary>
    public bool IsInBounds(int row, int col)
    {
        return row >= 0 && row < _totalHeight && col >= 0 && col < _width;
    }

    /// <summary>
    /// Sets the cell value at the given position.
    /// </summary>
    public void SetCell(int row, int col, int value)
    {
        _grid[row, col] = value;
    }

    // -----------------------------------------------------------------------
    // Piece locking
    // -----------------------------------------------------------------------

    /// <summary>
    /// Writes every cell of the active piece into the grid at its locked position.
    /// Uses (Type index + 1) as the cell value so that each piece type is distinguishable.
    /// </summary>
    public void LockPiece(PieceState piece)
    {
        int pieceValue = (int)piece.Type + 1;
        var cells = GetCellOffsets(piece.Type, piece.Rotation);

        for (int i = 0; i < cells.Length / 2; i++)
        {
            int cellRow = piece.Row + cells[i * 2];
            int cellCol = piece.Col + cells[i * 2 + 1];

            if (cellRow >= 0 && cellRow < _totalHeight && cellCol >= 0 && cellCol < _width)
            {
                _grid[cellRow, cellCol] = pieceValue;
            }
        }
    }

    // -----------------------------------------------------------------------
    // Line clearing
    // -----------------------------------------------------------------------

    /// <summary>
    /// Scans for complete rows (all 10 cells non-zero), shifts every row above
    /// each cleared line down by one position, and fills the newly vacant top rows
    /// with zeroes. Returns the number of lines cleared (0-4).
    /// </summary>
    public int ClearLines()
    {
        int linesCleared = 0;
        int dstRow = _totalHeight - 1; // Start from bottom

        for (int srcRow = _totalHeight - 1; srcRow >= 0; srcRow--)
        {
            if (IsRowFull(srcRow))
            {
                linesCleared++;
                // Skip this row -- do NOT copy it to dstRow
            }
            else
            {
                // Copy the row only when destination differs from source.
                if (dstRow != srcRow)
                {
                    for (int col = 0; col < _width; col++)
                    {
                        _grid[dstRow, col] = _grid[srcRow, col];
                    }
                }
                dstRow--;
            }
        }

        // Fill all rows above the last written row with zeroes.
        for (int row = dstRow; row >= 0; row--)
        {
            for (int col = 0; col < _width; col++)
            {
                _grid[row, col] = 0;
            }
        }

        return linesCleared;
    }

    /// <summary>
    /// Returns true when every cell in the given row is non-zero (filled).
    /// </summary>
    public bool IsRowFull(int row)
    {
        for (int col = 0; col < _width; col++)
        {
            if (_grid[row, col] == 0)
                return false;
        }
        return true;
    }

    // -----------------------------------------------------------------------
    // Utility
    // -----------------------------------------------------------------------

    /// <summary>
    /// Resets every cell in the grid to zero.
    /// </summary>
    public void Clear()
    {
        for (int row = 0; row < _totalHeight; row++)
        {
            for (int col = 0; col < _width; col++)
            {
                _grid[row, col] = 0;
            }
        }
    }

    /// <summary>
    /// Returns true if any cell in the visible top row (row index 2) is non-zero.
    /// Used for game-over / top-out detection.
    /// </summary>
    public bool IsTopOut()
    {
        int visibleTopRow = GameRules.PLAYFIELD_BUFFER_ROWS; // row 2
        for (int col = 0; col < _width; col++)
        {
            if (_grid[visibleTopRow, col] != 0)
                return true;
        }
        return false;
    }

    // -----------------------------------------------------------------------
    // NES Tetromino rotation lookup table
    //
    // Standard NES-style rotations (not SRS). Each piece has 4 rotation states.
    // Cells are stored as int[rotationIndex, cellIndex * 2 + axis] where:
    //   column 0 = row offset (dr)  ,  column 1 = column offset (dc)
    // Offsets are relative to the piece's anchor position (Row, Col).
    // -----------------------------------------------------------------------

    /// <summary>
    /// Static lookup returning cell offsets for a specific piece type and rotation.
    /// Returns an int[] of pairs: [dr0, dc0, dr1, dc1, ...].
    /// </summary>
    private static readonly int[][][] s_RotationTable = new int[][][]
    {
        // PieceType.I (4 cells per rotation)
        new int[][]
        {
            new[] { 0, 0, 0, -1, 0, 1, 0, 2 },           // rot 0: horizontal line from anchor down-right
            new[] { 0, 0, 1, 0, 2, 0, 3, 0 },            // rot 1: vertical line from anchor going down
            new[] { 0, 0, 0, -1, 0, 1, 0, 2 },           // rot 2: horizontal (same as 0)
            new[] { 0, 0, 1, 0, 2, 0, 3, 0 }             // rot 3: vertical   (same as 1)
        },

        // PieceType.O (4 cells per rotation - same in all orientations)
        new int[][]
        {
            new[] { 0, 0, 0, 1, 1, 0, 1, 1 },
            new[] { 0, 0, 0, 1, 1, 0, 1, 1 },
            new[] { 0, 0, 0, 1, 1, 0, 1, 1 },
            new[] { 0, 0, 0, 1, 1, 0, 1, 1 }
        },

        // PieceType.T (4 cells per rotation)
        new int[][]
        {
            new[] { 0, -1, 0, 0, 0, 1, 1, 0 },           // rot 0: T pointing down
            new[] { 0, 0, 1, 0, 1, -1, 0, 1 },           // rot 1: T pointing left
            new[] { 0, -1, 0, 0, 0, 1, -1, 0 },          // rot 2: T pointing up
            new[] { 0, 0, -1, 0, -1, 1, 0, -1 }          // rot 3: T pointing right
        },

        // PieceType.S (4 cells per rotation)
        new int[][]
        {
            new[] { 0, 0, 0, 1, -1, -1, -1, 0 },         // rot 0: flat S
            new[] { 0, 0, 1, 0, 1, 1, 2, 1 },            // rot 1: vertical S
            new[] { 0, -1, 0, 0, 1, 0, 1, 1 },           // rot 2: flat S (180)
            new[] { -1, 0, -1, -1, 0, -1, 0, 0 }         // rot 3: vertical S
        },

        // PieceType.Z (4 cells per rotation)
        new int[][]
        {
            new[] { -1, 0, -1, 1, 0, 0, 0, -1 },         // rot 0: flat Z
            new[] { 0, 0, 0, 1, 1, -1, 1, 0 },           // rot 1: vertical Z
            new[] { 0, 1, 0, 0, -1, 0, -1, -1 },         // rot 2: flat Z (180)
            new[] { 0, 0, 0, -1, -1, 1, -1, 0 }          // rot 3: vertical Z
        },

        // PieceType.J (4 cells per rotation)
        new int[][]
        {
            new[] { 0, 0, 0, 1, 0, -1, -1, -1 },         // rot 0: J opening to the right
            new[] { 0, 0, 1, 0, 2, 0, 2, -1 },           // rot 1
            new[] { 0, 0, 0, 1, 0, 2, -1, 2 },           // rot 2
            new[] { 0, 0, -1, 0, -2, 0, -2, 1 }          // rot 3
        },

        // PieceType.L (4 cells per rotation)
        new int[][]
        {
            new[] { 0, 0, 0, 1, 0, -1, -1, 1 },          // rot 0: L opening to the left
            new[] { 0, 0, 1, 0, 2, 0, 2, 1 },            // rot 1
            new[] { 0, 0, 0, -1, 0, -2, -1, -2 },        // rot 2
            new[] { 0, 0, -1, 0, -2, 0, -2, -1 }         // rot 3
        }
    };

    /// <summary>
    /// Returns the flattened cell-offset array for a specific piece type and rotation index.
    /// </summary>
    public static int[] GetCellOffsets(PieceType type, int rotation)
    {
        // Clamp rotation to [0, 3].
        int rot = rotation % 4;
        if (rot < 0) rot += 4;

        return s_RotationTable[(int)type][rot];
    }

    /// <summary>
    /// Returns the number of cells for a given piece type (always 4).
    /// </summary>
    public static int CellCount(PieceType type)
    {
        return GetCellOffsets(type, 0).Length / 2;
    }
}
