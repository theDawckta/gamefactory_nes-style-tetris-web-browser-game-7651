using NUnit.Framework;
using Game.Gameplay;
using OneTimeGames.CoreSystems;

namespace GameTests.EditMode
{
    public class PieceMovementControllerTests
    {
        private PlayfieldModel _model;
        private GameStateMachine _stateMachine;
        private PieceMovementController _controller;

        [SetUp]
        public void SetUp()
        {
            _model = new PlayfieldModel();
            _stateMachine = new GameStateMachine();
            _controller = new PieceMovementController(_model, _stateMachine);
        }

        [Test]
        public void Gravity_AdvancesPieceDownAfterFramesPerRow()
        {
            _controller.SpawnPiece(PieceType.I);
            int level = 0;
            int spawnRow = GameRules.PLAYFIELD_BUFFER_ROWS;
            int framesPerRow = GameRules.GetFramesPerRow(level); // 48

            for (int i = 0; i < framesPerRow - 1; i++)
                _controller.Tick(level, false, false, false, false);

            Assert.AreEqual(spawnRow, _controller.CurrentPiece.Row, "Piece should not have moved before framesPerRow ticks");

            _controller.Tick(level, false, false, false, false);
            Assert.AreEqual(spawnRow + 1, _controller.CurrentPiece.Row, "Piece should advance one row after framesPerRow ticks");
        }

        [Test]
        public void SoftDrop_AdvancesPieceOneRowPerFrame()
        {
            _controller.SpawnPiece(PieceType.I);
            int spawnRow = GameRules.PLAYFIELD_BUFFER_ROWS;

            _controller.Tick(0, false, false, false, softDropHeld: true);
            Assert.AreEqual(spawnRow + 1, _controller.CurrentPiece.Row, "Piece should advance one row per frame with soft drop");

            _controller.Tick(0, false, false, false, softDropHeld: true);
            Assert.AreEqual(spawnRow + 2, _controller.CurrentPiece.Row, "Piece should advance a second row on the next soft drop frame");
        }

        [Test]
        public void PieceLands_LocksOnNextGravityTick_NotBefore()
        {
            // Block the row below spawn to force immediate landing (I piece R0 occupies cols 3-6 at spawn row)
            int spawnRow = GameRules.PLAYFIELD_BUFFER_ROWS;
            for (int c = 3; c <= 6; c++)
                _model.SetCell(spawnRow + 1, c, 1);

            bool locked = false;
            _controller.OnPieceLocked += _ => locked = true;
            _controller.SpawnPiece(PieceType.I);

            // Level 29: framesPerRow=1 -- gravity fires every tick
            _controller.Tick(29, false, false, false, false);
            Assert.IsFalse(locked, "Piece must not lock on the same tick it lands");
            Assert.AreEqual(spawnRow, _controller.CurrentPiece.Row, "Piece row should stay at spawn row after blocked gravity");

            _controller.Tick(29, false, false, false, false);
            Assert.IsTrue(locked, "Piece must lock on the very next gravity tick after landing");
        }

        [Test]
        public void OnPieceLocked_FiresWithCorrectPieceState()
        {
            int spawnRow = GameRules.PLAYFIELD_BUFFER_ROWS;
            for (int c = 3; c <= 6; c++)
                _model.SetCell(spawnRow + 1, c, 1);

            PieceState lockedState = default;
            _controller.OnPieceLocked += ps => lockedState = ps;
            _controller.SpawnPiece(PieceType.I);

            _controller.Tick(29, false, false, false, false); // land
            _controller.Tick(29, false, false, false, false); // lock

            Assert.AreEqual(PieceType.I, lockedState.Type);
            Assert.AreEqual(spawnRow, lockedState.Row);
            Assert.AreEqual(3, lockedState.Col);
            Assert.AreEqual(0, lockedState.Rotation);
        }

        [Test]
        public void OnSpawnFailed_FiresWhenSpawnPositionIsBlocked()
        {
            _model.SetCell(GameRules.PLAYFIELD_BUFFER_ROWS, 3, 1); // block one of the I-piece spawn cells

            bool spawnFailed = false;
            _controller.OnSpawnFailed += () => spawnFailed = true;
            _controller.SpawnPiece(PieceType.I);

            Assert.IsTrue(spawnFailed, "OnSpawnFailed should fire when spawn position is occupied");
        }

        [Test]
        public void OnSpawnFailed_DoesNotFireWhenSpawnIsOpen()
        {
            bool spawnFailed = false;
            _controller.OnSpawnFailed += () => spawnFailed = true;
            _controller.SpawnPiece(PieceType.I);

            Assert.IsFalse(spawnFailed, "OnSpawnFailed must not fire when spawn position is clear");
        }

        [Test]
        public void LeftHeld_MovesImmediatelyOnFirstFrame()
        {
            _controller.SpawnPiece(PieceType.I);
            int startCol = _controller.CurrentPiece.Col; // 3

            _controller.Tick(0, leftHeld: true, false, false, false);

            Assert.AreEqual(startCol - 1, _controller.CurrentPiece.Col, "Piece should move left on the first frame leftHeld is true");
        }

        [Test]
        public void LeftHeld_NoRepeatBeforeInitialDelay()
        {
            _controller.SpawnPiece(PieceType.I);
            // First frame: moves immediately (col 3 -> 2)
            _controller.Tick(0, leftHeld: true, false, false, false);
            int colAfterFirstMove = _controller.CurrentPiece.Col; // 2

            // Frames 2-16 (hold frames 1-15): no repeat
            for (int i = 1; i < GameRules.AUTO_REPEAT_INITIAL_DELAY_FRAMES; i++)
                _controller.Tick(0, leftHeld: true, false, false, false);

            Assert.AreEqual(colAfterFirstMove, _controller.CurrentPiece.Col,
                "No additional move should occur before AUTO_REPEAT_INITIAL_DELAY_FRAMES");
        }

        [Test]
        public void LeftHeld_FirstRepeatAtInitialDelayFrame()
        {
            _controller.SpawnPiece(PieceType.I);
            // Frame 1: immediate move, col 3->2
            _controller.Tick(0, leftHeld: true, false, false, false);
            // Frames 2-16: silent (hold frames 1-15)
            for (int i = 1; i < GameRules.AUTO_REPEAT_INITIAL_DELAY_FRAMES; i++)
                _controller.Tick(0, leftHeld: true, false, false, false);

            int colBeforeRepeat = _controller.CurrentPiece.Col; // 2

            // Frame 17 (hold frame 16 = AUTO_REPEAT_INITIAL_DELAY_FRAMES): first repeat fires
            _controller.Tick(0, leftHeld: true, false, false, false);

            Assert.AreEqual(colBeforeRepeat - 1, _controller.CurrentPiece.Col,
                "First repeat should fire at AUTO_REPEAT_INITIAL_DELAY_FRAMES hold frames");
        }

        [Test]
        public void LeftHeld_RepeatsEveryRepeatRateFramesAfterDelay()
        {
            _controller.SpawnPiece(PieceType.I);

            // Frame 1: col 3->2 (immediate)
            _controller.Tick(0, leftHeld: true, false, false, false);
            // Frames 2-16: silent
            for (int i = 1; i < GameRules.AUTO_REPEAT_INITIAL_DELAY_FRAMES; i++)
                _controller.Tick(0, leftHeld: true, false, false, false);
            // Frame 17: col 2->1 (first repeat)
            _controller.Tick(0, leftHeld: true, false, false, false);

            int colAfterFirstRepeat = _controller.CurrentPiece.Col; // 1

            // Frames 18-22: no repeat (hold frames 17-21, repeatFrames 1-5)
            for (int i = 1; i < GameRules.AUTO_REPEAT_REPEAT_RATE_FRAMES; i++)
                _controller.Tick(0, leftHeld: true, false, false, false);

            Assert.AreEqual(colAfterFirstRepeat, _controller.CurrentPiece.Col,
                "No move should occur before the next repeat interval");

            // Frame 23 (hold frame 22, repeatFrames=6): second repeat fires
            _controller.Tick(0, leftHeld: true, false, false, false);

            Assert.AreEqual(colAfterFirstRepeat - 1, _controller.CurrentPiece.Col,
                "Second repeat should fire every AUTO_REPEAT_REPEAT_RATE_FRAMES after the initial delay");
        }

        [Test]
        public void RightHeld_MovesImmediatelyOnFirstFrame()
        {
            _controller.SpawnPiece(PieceType.I);
            int startCol = _controller.CurrentPiece.Col; // 3

            _controller.Tick(0, false, rightHeld: true, false, false);

            Assert.AreEqual(startCol + 1, _controller.CurrentPiece.Col, "Piece should move right on the first frame rightHeld is true");
        }

        [Test]
        public void RightHeld_NoRepeatBeforeInitialDelay()
        {
            _controller.SpawnPiece(PieceType.I);
            _controller.Tick(0, false, rightHeld: true, false, false);
            int colAfterFirstMove = _controller.CurrentPiece.Col; // 4

            for (int i = 1; i < GameRules.AUTO_REPEAT_INITIAL_DELAY_FRAMES; i++)
                _controller.Tick(0, false, rightHeld: true, false, false);

            Assert.AreEqual(colAfterFirstMove, _controller.CurrentPiece.Col,
                "No additional move should occur before AUTO_REPEAT_INITIAL_DELAY_FRAMES");
        }

        [Test]
        public void Rotation_SucceedsWhenNotBlocked()
        {
            _controller.SpawnPiece(PieceType.I);

            _controller.Tick(0, false, false, rotatePressed: true, false);

            Assert.AreEqual(1, _controller.CurrentPiece.Rotation, "Piece rotation should advance by 1 when unblocked");
        }

        [Test]
        public void Rotation_BlockedByOccupiedCell_PieceStaysInPlace()
        {
            // I R1 at row 0, col 3 occupies (0,3),(1,3),(2,3),(3,3); block (2,3) to prevent rotation
            _model.SetCell(2, 3, 1);
            _controller.SpawnPiece(PieceType.I);

            _controller.Tick(0, false, false, rotatePressed: true, false);

            Assert.AreEqual(0, _controller.CurrentPiece.Rotation, "Rotation should stay at 0 when blocked");
        }

        [Test]
        public void Rotation_BlockedByOccupiedCell_PieceDoesNotKick()
        {
            _model.SetCell(2, 3, 1);
            _controller.SpawnPiece(PieceType.I);
            int startRow = _controller.CurrentPiece.Row;
            int startCol = _controller.CurrentPiece.Col;

            _controller.Tick(0, false, false, rotatePressed: true, false);

            Assert.AreEqual(startRow, _controller.CurrentPiece.Row, "Row must not change on blocked rotation (no wall kick)");
            Assert.AreEqual(startCol, _controller.CurrentPiece.Col, "Col must not change on blocked rotation (no wall kick)");
        }
    }
}
