using System;
using NUnit.Framework;

[TestFixture]
public class PlayfieldModelTests
{
    private PlayfieldModel _model;

    [SetUp]
    public void Setup()
    {
        _model = new PlayfieldModel();
    }

    // -----------------------------------------------------------------------
    // GetCell / SetCell / IsEmpty
    // -----------------------------------------------------------------------

    [Test]
    public void FreshGrid_AllCellsAreEmpty()
    {
        for (int row = 0; row < GameRules.PLAYFIELD_TOTAL_HEIGHT; row++)
        {
            for (int col = 0; col < GameRules.PLAYFIELD_WIDTH; col++)
            {
                Assert.AreEqual(0, _model.GetCell(row, col), $"Cell ({row},{col}) should be 0");
                Assert.IsTrue(_model.IsEmpty(row, col), $"IsEmpty({row},{col}) should be true");
            }
        }
    }

    [Test]
    public void SetCell_SetsCorrectValue()
    {
        _model.SetCell(5, 3, 42);
        Assert.AreEqual(42, _model.GetCell(5, 3));
    }

    [Test]
    public void SetCell_NonEmptyAfterSet()
    {
        _model.SetCell(10, 5, 3);
        Assert.IsFalse(_model.IsEmpty(10, 5));
    }

    // -----------------------------------------------------------------------
    // IsInBounds
    // -----------------------------------------------------------------------

    [Test]
    public void IsInBounds_ValidCoordinates_ReturnsTrue()
    {
        Assert.IsTrue(_model.IsInBounds(0, 0));
        Assert.IsTrue(_model.IsInBounds(21, 9));
        Assert.IsTrue(_model.IsInBounds(10, 5));
    }

    [Test]
    public void IsInBounds_NegativeRow_ReturnsFalse()
    {
        Assert.IsFalse(_model.IsInBounds(-1, 0));
    }

    [Test]
    public void IsInBounds_RowEqualToHeight_ReturnsFalse()
    {
        Assert.IsFalse(_model.IsInBounds(GameRules.PLAYFIELD_TOTAL_HEIGHT, 0));
    }

    [Test]
    public void IsInBounds_NegativeCol_ReturnsFalse()
    {
        Assert.IsFalse(_model.IsInBounds(0, -1));
    }

    [Test]
    public void IsInBounds_ColEqualToWidth_ReturnsFalse()
    {
        Assert.IsFalse(_model.IsInBounds(0, GameRules.PLAYFIELD_WIDTH));
    }

    [Test]
    public void IsInBounds_FarOutOfBounds_ReturnsFalse()
    {
        Assert.IsFalse(_model.IsInBounds(100, 500));
    }

    // -----------------------------------------------------------------------
    // LockPiece / GetCell integration
    // -----------------------------------------------------------------------

    [Test]
    public void LockPiece_I_PointsCorrectCells()
    {
        var piece = new PieceState
        {
            Type = PieceType.I,
            Row = GameRules.PLAYFIELD_BUFFER_ROWS,
            Col = 3,
            Rotation = 0 // horizontal
        };

        _model.LockPiece(piece);

        // I-piece rotation 0 is horizontal: cells at (row, col), (row, col-1), (row, col+1), (row, col+2)
        int expectedValue = (int)PieceType.I + 1; // 1

        Assert.AreEqual(expectedValue, _model.GetCell(2, 3));
        Assert.AreEqual(expectedValue, _model.GetCell(2, 2));
        Assert.AreEqual(expectedValue, _model.GetCell(2, 4));
        Assert.AreEqual(expectedValue, _model.GetCell(2, 5));

        // Verify no other cells are touched
        Assert.AreEqual(0, _model.GetCell(3, 3));
        Assert.AreEqual(0, _model.GetCell(1, 3));
    }

    [Test]
    public void LockPiece_O_PointsCorrectCells()
    {
        var piece = new PieceState
        {
            Type = PieceType.O,
            Row = 5,
            Col = 4,
            Rotation = 0
        };

        _model.LockPiece(piece);

        int expectedValue = (int)PieceType.O + 1; // 2

        Assert.AreEqual(expectedValue, _model.GetCell(5, 4));
        Assert.AreEqual(expectedValue, _model.GetCell(5, 5));
        Assert.AreEqual(expectedValue, _model.GetCell(6, 4));
        Assert.AreEqual(expectedValue, _model.GetCell(6, 5));

        Assert.AreEqual(0, _model.GetCell(5, 3)); // adjacent empty
    }

    [Test]
    public void LockPiece_T_PointsCorrectCells()
    {
        var piece = new PieceState
        {
            Type = PieceType.T,
            Row = 10,
            Col = 5,
            Rotation = 0 // T pointing down
        };

        _model.LockPiece(piece);

        int expectedValue = (int)PieceType.T + 1; // 3

        Assert.AreEqual(expectedValue, _model.GetCell(10, 4)); // 0,-1
        Assert.AreEqual(expectedValue, _model.GetCell(10, 5)); // 0,0
        Assert.AreEqual(expectedValue, _model.GetCell(10, 6)); // 0,1
        Assert.AreEqual(expectedValue, _model.GetCell(11, 5)); // 1,0

        Assert.AreEqual(0, _model.GetCell(9, 5)); // not occupied
    }

    [Test]
    public void LockPiece_OverwritesExistingCells()
    {
        _model.SetCell(5, 5, 1); // Set I-piece value

        var piece = new PieceState
        {
            Type = PieceType.O,
            Row = 5,
            Col = 5,
            Rotation = 0
        };

        _model.LockPiece(piece);

        Assert.AreEqual(2, _model.GetCell(5, 5)); // now O-piece value (type index + 1)
    }

    [Test]
    public void LockPiece_OnlyWritesWithinBounds()
    {
        // Lock an I piece partially outside bounds on the left.
        var piece = new PieceState
        {
            Type = PieceType.I,
            Row = 15,
            Col = 0,
            Rotation = 0 // horizontal: cells at (0,0), (0,-1), (0,1), (0,2)
        };

        _model.LockPiece(piece);

        int expectedValue = (int)PieceType.I + 1;

        Assert.AreEqual(0, _model.GetCell(15, 0));    // offset 0,0 is valid but let's check
        // Actually I rot 0: 0,0 0,-1 0,1 0,2 -- so (15,0) gets set from the first cell [0,0]
        Assert.AreEqual(expectedValue, _model.GetCell(15, 0));
        Assert.AreEqual(expectedValue, _model.GetCell(15, 1)); // offset 0,1
        Assert.AreEqual(expectedValue, _model.GetCell(15, 2)); // offset 0,2
        // col -1 is out of bounds, should be ignored
        Assert.IsFalse(_model.IsInBounds(15, -1));
    }

    [Test]
    public void LockPiece_EachTypeUsesCorrectValue()
    {
        foreach (PieceType type in (PieceType[])Enum.GetValues(typeof(PieceType)))
        {
            _model.Clear();
            var piece = new PieceState
            {
                Type = type,
                Row = 10,
                Col = 5,
                Rotation = 0
            };

            _model.LockPiece(piece);

            int expectedValue = (int)type + 1;
            // Check the anchor cell (offset 0,0 which every piece has due to rotation table definition)
            Assert.AreEqual(expectedValue, _model.GetCell(10, 5), $"Type {type} should produce value {expectedValue}");
        }
    }

    // -----------------------------------------------------------------------
    // IsRowFull
    // -----------------------------------------------------------------------

    [Test]
    public void IsRowFull_EmptyRow_ReturnsFalse()
    {
        Assert.IsFalse(_model.IsRowFull(10));
    }

    [Test]
    public void IsRowFull_OneEmptyCell_ReturnsFalse()
    {
        for (int col = 0; col < GameRules.PLAYFIELD_WIDTH; col++)
        {
            _model.SetCell(10, col, 1);
        }
        // row is full now

        _model.SetCell(10, 5, 0); // empty one cell
        Assert.IsFalse(_model.IsRowFull(10));
    }

    [Test]
    public void IsRowFull_AllNonZero_ReturnsTrue()
    {
        for (int col = 0; col < GameRules.PLAYFIELD_WIDTH; col++)
        {
            _model.SetCell(10, col, col % 7 + 1); // values 1-7, never 0
        }

        Assert.IsTrue(_model.IsRowFull(10));
    }

    // -----------------------------------------------------------------------
    // ClearLines
    // -----------------------------------------------------------------------

    [Test]
    public void ClearLines_NoFullRows_ReturnsZero()
    {
        _model.SetCell(15, 3, 1);
        _model.SetCell(10, 7, 2);

        int linesCleared = _model.ClearLines();
        Assert.AreEqual(0, linesCleared);
    }

    [Test]
    public void ClearLines_OneFullRow_ReturnsOne()
    {
        for (int col = 0; col < GameRules.PLAYFIELD_WIDTH; col++)
        {
            _model.SetCell(15, col, 1); // fill row 15
        }

        // Put something above row 15
        _model.SetCell(14, 3, 5);

        int linesCleared = _model.ClearLines();
        Assert.AreEqual(1, linesCleared);

        // Row 15 should be empty now (it was cleared)
        for (int col = 0; col < GameRules.PLAYFIELD_WIDTH; col++)
        {
            Assert.AreEqual(0, _model.GetCell(21, col), "Row 21 should be empty (was below row 15 and nothing shifted there)");
        }

        // Row 14 content should have shifted to row 14 (stayed in place since cleared row was above it)
        Assert.AreEqual(5, _model.GetCell(14, 3));
    }

    [Test]
    public void ClearLines_SingleRow_RowAboveShiftsDown()
    {
        // Fill row 10 completely.
        for (int col = 0; col < GameRules.PLAYFIELD_WIDTH; col++)
        {
            _model.SetCell(10, col, 2); // all type-O values in row 10
        }

        // Put a piece in row 9 (just above the full row).
        for (int col = 0; col < GameRules.PLAYFIELD_WIDTH; col++)
        {
            _model.SetCell(9, col, 3); // all type-T values in row 9
        }

        // Row 11 should be untouched.
        _model.SetCell(11, 5, 7);

        int linesCleared = _model.ClearLines();
        Assert.AreEqual(1, linesCleared);

        // Row 9 contents should shift down to row 10.
        for (int col = 0; col < GameRules.PLAYFIELD_WIDTH; col++)
        {
            Assert.AreEqual(3, _model.GetCell(10, col), $"Row 10 col {col} should now have type-T value from shifted row 9");
        }

        // Row 11 stays in place.
        Assert.AreEqual(7, _model.GetCell(11, 5));

        // Rows above row 9 should be cleared to 0 because there was nothing above row 9
        for (int col = 0; col < GameRules.PLAYFIELD_WIDTH; col++)
        {
            Assert.AreEqual(0, _model.GetCell(9, col), $"Row 9 col {col} should be empty after shift");
        }
    }

    [Test]
    public void ClearLines_FourFullRows_ReturnsFour()
    {
        // Fill rows 10-13 completely.
        for (int row = 10; row <= 13; row++)
        {
            for (int col = 0; col < GameRules.PLAYFIELD_WIDTH; col++)
            {
                _model.SetCell(row, col, 1);
            }
        }

        // Put content in row 9.
        for (int col = 0; col < GameRules.PLAYFIELD_WIDTH; col++)
        {
            _model.SetCell(9, col, 4);
        }

        int linesCleared = _model.ClearLines();
        Assert.AreEqual(4, linesCleared);

        // Row 9 content should shift to row 13.
        for (int col = 0; col < GameRules.PLAYFIELD_WIDTH; col++)
        {
            Assert.AreEqual(4, _model.GetCell(13, col));
        }

        // Rows 10-12 should now be empty (nothing was above row 9).
        for (int row = 10; row <= 12; row++)
        {
            for (int col = 0; col < GameRules.PLAYFIELD_WIDTH; col++)
            {
                Assert.AreEqual(0, _model.GetCell(row, col), $"Row {row} should be empty");
            }
        }
    }

    [Test]
    public void ClearLines_ScatteredRows_MultipleCleared()
    {
        // Fill rows 5, 12, and 20.
        foreach (int row in new[] { 5, 12, 20 })
        {
            for (int col = 0; col < GameRules.PLAYFIELD_WIDTH; col++)
            {
                _model.SetCell(row, col, 1);
            }
        }

        // Put a marker at row 3.
        _model.SetCell(3, 0, 7);

        int linesCleared = _model.ClearLines();
        Assert.AreEqual(3, linesCleared);

        // The marker from row 3 should have shifted down by 3 rows (to row 6).
        // Rows 5,12,20 were cleared so everything above shifts down.
        // Row 3 -> after clearing rows at 5,12,20, it lands at row 6.
        Assert.AreEqual(7, _model.GetCell(6, 0), "Marker should have shifted down 3 rows");

        // Top rows should be zeroed
        for (int col = 0; col < GameRules.PLAYFIELD_WIDTH; col++)
        {
            Assert.AreEqual(0, _model.GetCell(0, col));
            Assert.AreEqual(0, _model.GetCell(1, col));
            Assert.AreEqual(0, _model.GetCell(2, col));
        }
    }

    // -----------------------------------------------------------------------
    // IsTopOut
    // -----------------------------------------------------------------------

    [Test]
    public void IsTopOut_FreshGrid_ReturnsFalse()
    {
        Assert.IsFalse(_model.IsTopOut());
    }

    [Test]
    public void IsTopOut_CellInRow2_ReturnsTrue()
    {
        _model.SetCell(2, 5, 1);
        Assert.IsTrue(_model.IsTopOut(), "Setting a cell in row 2 (visible top) should trigger top-out");
    }

    [Test]
    public void IsTopOut_CellInRow0_ReturnsFalse()
    {
        _model.SetCell(0, 5, 1); // buffer row - not visible
        Assert.IsFalse(_model.IsTopOut(), "Buffer rows should not count as top-out");
    }

    [Test]
    public void IsTopOut_CellInRow1_ReturnsFalse()
    {
        _model.SetCell(1, 5, 1); // buffer row - still hidden
        Assert.IsFalse(_model.IsTopOut());
    }

    [Test]
    public void IsTopOut_AnyColumnInRow2_TriggersTrue()
    {
        for (int col = 0; col < GameRules.PLAYFIELD_WIDTH; col++)
        {
            _model.Clear();
            _model.SetCell(2, col, 1);
            Assert.IsTrue(_model.IsTopOut(), $"SetCell at column {col} in row 2 should trigger top-out");
        }
    }

    // -----------------------------------------------------------------------
    // Clear
    // -----------------------------------------------------------------------

    [Test]
    public void Clear_ResetsAllCellsToZero()
    {
        // Fill the entire grid with non-zero values.
        for (int row = 0; row < GameRules.PLAYFIELD_TOTAL_HEIGHT; row++)
        {
            for (int col = 0; col < GameRules.PLAYFIELD_WIDTH; col++)
            {
                _model.SetCell(row, col, 1);
            }
        }

        _model.Clear();

        for (int row = 0; row < GameRules.PLAYFIELD_TOTAL_HEIGHT; row++)
        {
            for (int col = 0; col < GameRules.PLAYFIELD_WIDTH; col++)
            {
                Assert.AreEqual(0, _model.GetCell(row, col), $"Cell ({row},{col}) should be 0 after Clear");
            }
        }
    }

    // -----------------------------------------------------------------------
    // CellCount
    // -----------------------------------------------------------------------

    [Test]
    public void CellCount_EveryType_HasFourCells()
    {
        foreach (PieceType type in (PieceType[])Enum.GetValues(typeof(PieceType)))
        {
            Assert.AreEqual(4, PlayfieldModel.CellCount(type), $"PieceType.{type} should have 4 cells");
        }
    }

    // -----------------------------------------------------------------------
    // PieceState struct
    // -----------------------------------------------------------------------

    [Test]
    public void PieceState_CanBeConstructed()
    {
        var state = new PieceState
        {
            Type = PieceType.T,
            Row = 5,
            Col = 3,
            Rotation = 2
        };

        Assert.AreEqual(PieceType.T, state.Type);
        Assert.AreEqual(5, state.Row);
        Assert.AreEqual(3, state.Col);
        Assert.AreEqual(2, state.Rotation);
    }
}
