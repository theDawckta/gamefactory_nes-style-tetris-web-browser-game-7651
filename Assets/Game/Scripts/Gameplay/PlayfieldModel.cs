namespace Game.Gameplay
{
    public enum PieceType
    {
        I, O, T, S, Z, J, L
    }

    public struct PieceState
    {
        public PieceType Type;
        public int Row;
        public int Col;
        public int Rotation;
    }

    public class PlayfieldModel
    {
        private readonly int[,] _grid;

        // [pieceTypeIndex, rotation, cellIndex*2+0=rowOffset / cellIndex*2+1=colOffset]
        // NES Tetris rotation tables (non-SRS)
        public static readonly int[,,] CellOffsets = new int[7, 4, 8]
        {
            { // I (index 0)
                { 0,0, 0,1, 0,2, 0,3 }, // R0: horizontal
                { 0,0, 1,0, 2,0, 3,0 }, // R1: vertical
                { 0,0, 0,1, 0,2, 0,3 }, // R2: same as R0
                { 0,0, 1,0, 2,0, 3,0 }, // R3: same as R1
            },
            { // O (index 1)
                { 0,0, 0,1, 1,0, 1,1 },
                { 0,0, 0,1, 1,0, 1,1 },
                { 0,0, 0,1, 1,0, 1,1 },
                { 0,0, 0,1, 1,0, 1,1 },
            },
            { // T (index 2)
                { 0,0, 0,1, 0,2, 1,1 }, // R0: flat top (XXX / .X.)
                { 0,1, 1,0, 1,1, 2,1 }, // R1: stem right
                { 0,1, 1,0, 1,1, 1,2 }, // R2: flat bottom (.X. / XXX)
                { 0,0, 1,0, 1,1, 2,0 }, // R3: stem left
            },
            { // S (index 3)
                { 0,1, 0,2, 1,0, 1,1 }, // R0: .XX / XX.
                { 0,0, 1,0, 1,1, 2,1 }, // R1: vertical
                { 0,1, 0,2, 1,0, 1,1 }, // R2: same as R0
                { 0,0, 1,0, 1,1, 2,1 }, // R3: same as R1
            },
            { // Z (index 4)
                { 0,0, 0,1, 1,1, 1,2 }, // R0: XX. / .XX
                { 0,1, 1,0, 1,1, 2,0 }, // R1: vertical
                { 0,0, 0,1, 1,1, 1,2 }, // R2: same as R0
                { 0,1, 1,0, 1,1, 2,0 }, // R3: same as R1
            },
            { // J (index 5)
                { 0,0, 1,0, 1,1, 1,2 }, // R0: X.. / XXX
                { 0,0, 0,1, 1,0, 2,0 }, // R1: XX / X. / X.
                { 0,0, 0,1, 0,2, 1,2 }, // R2: XXX / ..X
                { 0,1, 1,1, 2,0, 2,1 }, // R3: .X / .X / XX
            },
            { // L (index 6)
                { 0,2, 1,0, 1,1, 1,2 }, // R0: ..X / XXX
                { 0,0, 1,0, 2,0, 2,1 }, // R1: X. / X. / XX
                { 0,0, 0,1, 0,2, 1,0 }, // R2: XXX / X..
                { 0,0, 0,1, 1,1, 2,1 }, // R3: XX / .X / .X
            },
        };

        public PlayfieldModel()
        {
            _grid = new int[GameRules.PLAYFIELD_TOTAL_HEIGHT, GameRules.PLAYFIELD_WIDTH];
        }

        public int GetCell(int row, int col)
        {
            return _grid[row, col];
        }

        public bool IsEmpty(int row, int col)
        {
            return _grid[row, col] == 0;
        }

        public bool IsInBounds(int row, int col)
        {
            return row >= 0 && row < GameRules.PLAYFIELD_TOTAL_HEIGHT
                && col >= 0 && col < GameRules.PLAYFIELD_WIDTH;
        }

        public void SetCell(int row, int col, int value)
        {
            _grid[row, col] = value;
        }

        public void LockPiece(PieceState piece)
        {
            int typeIndex = (int)piece.Type;
            int cellValue = typeIndex + 1;
            int rot = piece.Rotation % 4;

            for (int i = 0; i < 4; i++)
            {
                int r = piece.Row + CellOffsets[typeIndex, rot, i * 2];
                int c = piece.Col + CellOffsets[typeIndex, rot, i * 2 + 1];
                if (IsInBounds(r, c))
                    _grid[r, c] = cellValue;
            }
        }

        public int ClearLines()
        {
            int cleared = 0;
            int writeRow = GameRules.PLAYFIELD_TOTAL_HEIGHT - 1;

            for (int readRow = GameRules.PLAYFIELD_TOTAL_HEIGHT - 1; readRow >= 0; readRow--)
            {
                if (IsRowFull(readRow))
                {
                    cleared++;
                }
                else
                {
                    if (writeRow != readRow)
                        for (int c = 0; c < GameRules.PLAYFIELD_WIDTH; c++)
                            _grid[writeRow, c] = _grid[readRow, c];
                    writeRow--;
                }
            }

            for (int r = writeRow; r >= 0; r--)
                for (int c = 0; c < GameRules.PLAYFIELD_WIDTH; c++)
                    _grid[r, c] = 0;

            return cleared;
        }

        public bool IsRowFull(int row)
        {
            for (int c = 0; c < GameRules.PLAYFIELD_WIDTH; c++)
                if (_grid[row, c] == 0)
                    return false;
            return true;
        }

        public void Clear()
        {
            for (int r = 0; r < GameRules.PLAYFIELD_TOTAL_HEIGHT; r++)
                for (int c = 0; c < GameRules.PLAYFIELD_WIDTH; c++)
                    _grid[r, c] = 0;
        }

        public bool IsTopOut()
        {
            // Row index 2 is the visible top row (rows 0-1 are buffer rows)
            for (int c = 0; c < GameRules.PLAYFIELD_WIDTH; c++)
                if (_grid[GameRules.PLAYFIELD_BUFFER_ROWS, c] != 0)
                    return true;
            return false;
        }
    }
}
