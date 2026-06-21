using NUnit.Framework;
using Game.Gameplay;

namespace GameTests.EditMode
{
    public class PlayfieldModelTests
    {
        private PlayfieldModel _model;

        [SetUp]
        public void SetUp()
        {
            _model = new PlayfieldModel();
        }

        [Test]
        public void FreshGrid_AllCellsEmpty()
        {
            for (int r = 0; r < 22; r++)
                for (int c = 0; c < 10; c++)
                    Assert.AreEqual(0, _model.GetCell(r, c));
        }

        [Test]
        public void SetCell_GetCell_RoundTrip()
        {
            _model.SetCell(5, 3, 4);
            Assert.AreEqual(4, _model.GetCell(5, 3));
        }

        [Test]
        public void IsEmpty_ReturnsTrueForZeroAndFalseAfterSet()
        {
            Assert.IsTrue(_model.IsEmpty(0, 0));
            _model.SetCell(0, 0, 1);
            Assert.IsFalse(_model.IsEmpty(0, 0));
        }

        [Test]
        public void IsInBounds_ValidCoordinates_ReturnsTrue()
        {
            Assert.IsTrue(_model.IsInBounds(0, 0));
            Assert.IsTrue(_model.IsInBounds(21, 9));
            Assert.IsTrue(_model.IsInBounds(10, 5));
        }

        [Test]
        public void IsInBounds_OutOfRange_ReturnsFalse()
        {
            Assert.IsFalse(_model.IsInBounds(-1, 0));
            Assert.IsFalse(_model.IsInBounds(0, -1));
            Assert.IsFalse(_model.IsInBounds(22, 0));
            Assert.IsFalse(_model.IsInBounds(0, 10));
        }

        [Test]
        public void LockPiece_IPiece_SetsCorrectCells()
        {
            var piece = new PieceState { Type = PieceType.I, Row = 5, Col = 3, Rotation = 0 };
            _model.LockPiece(piece);
            // I R0 offsets: (0,0),(0,1),(0,2),(0,3) -> row 5, cols 3-6
            Assert.AreEqual(1, _model.GetCell(5, 3));
            Assert.AreEqual(1, _model.GetCell(5, 4));
            Assert.AreEqual(1, _model.GetCell(5, 5));
            Assert.AreEqual(1, _model.GetCell(5, 6));
        }

        [Test]
        public void LockPiece_OPiece_SetsCorrectCells()
        {
            var piece = new PieceState { Type = PieceType.O, Row = 10, Col = 4, Rotation = 0 };
            _model.LockPiece(piece);
            // O offsets: (0,0),(0,1),(1,0),(1,1) -> rows 10-11, cols 4-5
            Assert.AreEqual(2, _model.GetCell(10, 4));
            Assert.AreEqual(2, _model.GetCell(10, 5));
            Assert.AreEqual(2, _model.GetCell(11, 4));
            Assert.AreEqual(2, _model.GetCell(11, 5));
        }

        [Test]
        public void LockPiece_TPiece_SetsCorrectCells()
        {
            var piece = new PieceState { Type = PieceType.T, Row = 8, Col = 3, Rotation = 0 };
            _model.LockPiece(piece);
            // T R0 offsets: (0,0),(0,1),(0,2),(1,1)
            Assert.AreEqual(3, _model.GetCell(8, 3));
            Assert.AreEqual(3, _model.GetCell(8, 4));
            Assert.AreEqual(3, _model.GetCell(8, 5));
            Assert.AreEqual(3, _model.GetCell(9, 4));
        }

        [Test]
        public void LockPiece_CellValueIsPieceTypeIndexPlusOne()
        {
            var pieceI = new PieceState { Type = PieceType.I, Row = 0, Col = 0, Rotation = 0 };
            _model.LockPiece(pieceI);
            Assert.AreEqual(1, _model.GetCell(0, 0)); // I index=0, value=1

            _model.Clear();

            // L index=6, value=7; R0 offsets include (1,0)
            var pieceL = new PieceState { Type = PieceType.L, Row = 0, Col = 0, Rotation = 0 };
            _model.LockPiece(pieceL);
            Assert.AreEqual(7, _model.GetCell(1, 0));
        }

        [Test]
        public void IsRowFull_EmptyRow_ReturnsFalse()
        {
            Assert.IsFalse(_model.IsRowFull(21));
        }

        [Test]
        public void IsRowFull_OneHole_ReturnsFalse()
        {
            for (int c = 0; c < 9; c++)
                _model.SetCell(21, c, 1);
            Assert.IsFalse(_model.IsRowFull(21));
        }

        [Test]
        public void IsRowFull_AllFilled_ReturnsTrue()
        {
            for (int c = 0; c < 10; c++)
                _model.SetCell(21, c, 1);
            Assert.IsTrue(_model.IsRowFull(21));
        }

        [Test]
        public void ClearLines_NoFullRows_ReturnsZero()
        {
            _model.SetCell(21, 5, 1);
            Assert.AreEqual(0, _model.ClearLines());
        }

        [Test]
        public void ClearLines_OneFullRow_ReturnsOne_AndShiftsDown()
        {
            for (int c = 0; c < 10; c++)
                _model.SetCell(21, c, 1);
            _model.SetCell(20, 3, 5); // marker above the full row

            int cleared = _model.ClearLines();

            Assert.AreEqual(1, cleared);
            // Row 20's marker should have shifted down to row 21
            Assert.AreEqual(5, _model.GetCell(21, 3));
            Assert.AreEqual(0, _model.GetCell(20, 3));
        }

        [Test]
        public void ClearLines_FourFullRows_ReturnsFour()
        {
            for (int r = 18; r <= 21; r++)
                for (int c = 0; c < 10; c++)
                    _model.SetCell(r, c, 1);

            Assert.AreEqual(4, _model.ClearLines());
        }

        [Test]
        public void ClearLines_ScatteredFullRows_ReturnsCorrectCount()
        {
            for (int c = 0; c < 10; c++)
            {
                _model.SetCell(19, c, 1);
                _model.SetCell(21, c, 1);
            }
            _model.SetCell(20, 5, 1); // row 20 is not full

            Assert.AreEqual(2, _model.ClearLines());
        }

        [Test]
        public void IsTopOut_FreshGrid_ReturnsFalse()
        {
            Assert.IsFalse(_model.IsTopOut());
        }

        [Test]
        public void IsTopOut_CellInRow2_ReturnsTrue()
        {
            _model.SetCell(2, 5, 1);
            Assert.IsTrue(_model.IsTopOut());
        }

        [Test]
        public void IsTopOut_CellsInBufferRowsOnly_ReturnsFalse()
        {
            // Rows 0 and 1 are buffer rows; only row 2 triggers top-out
            _model.SetCell(0, 0, 1);
            _model.SetCell(1, 0, 1);
            Assert.IsFalse(_model.IsTopOut());
        }

        [Test]
        public void Clear_ResetsAllCells()
        {
            for (int r = 0; r < 22; r++)
                for (int c = 0; c < 10; c++)
                    _model.SetCell(r, c, r + c + 1);
            _model.Clear();
            for (int r = 0; r < 22; r++)
                for (int c = 0; c < 10; c++)
                    Assert.AreEqual(0, _model.GetCell(r, c));
        }
    }
}
