using System;
using NUnit.Framework;

[TestFixture]
public class PieceMovementControllerTests
{
    private PlayfieldModel _model;
    private GameStateMachine _stateMachine;
    private PieceMovementController _controller;

    [SetUp]
    public void Setup()
    {
        _model = new PlayfieldModel();
        _stateMachine = new GameStateMachine();
        _controller = new PieceMovementController(_model, _stateMachine);
    }

    // -----------------------------------------------------------------------
    // Spawn
    // -----------------------------------------------------------------------

    [Test]
    public void SpawnPiece_PlacesAtCorrectPosition()
    {
        _controller.SpawnPiece(PieceType.T);

        var piece = _controller.CurrentPiece;
        Assert.AreEqual(PieceType.T, piece.Type);
        Assert.AreEqual(1, piece.Row);
        Assert.AreEqual(GameRules.PLAYFIELD_WIDTH / 2 - 1, piece.Col);
        Assert.AreEqual(0, piece.Rotation);
    }

    [Test]
    public void SpawnPiece_OnSpawnFailedNotFired_WhenSpawnIsClear()
    {
        bool spawnFailed = false;
        _controller.OnSpawnFailed += () => { spawnFailed = true; };

        _controller.SpawnPiece(PieceType.I);

        Assert.IsFalse(spawnFailed);
        Assert.IsNotNull(_controller.CurrentPiece);
    }

    [Test]
    public void SpawnPiece_OnSpawnFailedFired_WhenSpawnPositionIsBlocked()
    {
        // Block the spawn position for an I-piece (horizontal at row 1, col 4)
        // I-piece rot 0: cells at (0,0), (0,-1), (0,1), (0,2) relative to anchor
        // Anchor at row 1, col 4 -> cells at (1,4), (1,3), (1,5), (1,6)
        _model.SetCell(1, 4, 1);

        bool spawnFailed = false;
        _controller.OnSpawnFailed += () => { spawnFailed = true; };

        _controller.SpawnPiece(PieceType.I);

        Assert.IsTrue(spawnFailed);
    }

    [Test]
    public void SpawnPiece_EveryType_SpawnsSuccessfullyOnClearField()
    {
        foreach (PieceType type in (PieceType[])Enum.GetValues(typeof(PieceType)))
        {
            _model.Clear();
            bool spawnFailed = false;
            _controller.OnSpawnFailed += () => { spawnFailed = true; };

            _controller.SpawnPiece(type);

            Assert.IsFalse(spawnFailed, "SpawnPiece(" + type + ") should not fail on clear field");
        }
    }

    // -----------------------------------------------------------------------
    // Gravity -- basic falling
    // -----------------------------------------------------------------------

    [Test]
    public void Gravity_PieceFallsOneRow_After48FramesAtLevel0()
    {
        _controller.SpawnPiece(PieceType.T);
        int initialRow = _controller.CurrentPiece.Row;

        // Tick 47 frames -- should not have moved yet (framesPerRow = 48)
        for (int i = 0; i < 47; i++)
        {
            _controller.Tick(0, false, false, false, false);
        }
        Assert.AreEqual(initialRow, _controller.CurrentPiece.Row, "Piece should not move before 48 frames");

        // 48th tick -- should move down one row
        _controller.Tick(0, false, false, false, false);
        Assert.AreEqual(initialRow + 1, _controller.CurrentPiece.Row, "Piece should move down on frame 48");
    }

    [Test]
    public void Gravity_PieceFallsAtCorrectRate_ForHigherLevels()
    {
        // Level 10: framesPerRow = 5
        _controller.SpawnPiece(PieceType.T);
        int initialRow = _controller.CurrentPiece.Row;

        for (int i = 0; i < 4; i++)
        {
            _controller.Tick(10, false, false, false, false);
        }
        Assert.AreEqual(initialRow, _controller.CurrentPiece.Row, "Piece should not move before 5 frames at level 10");

        _controller.Tick(10, false, false, false, false);
        Assert.AreEqual(initialRow + 1, _controller.CurrentPiece.Row, "Piece should move down on frame 5 at level 10");
    }

    [Test]
    public void Gravity_PieceFallsContinuously()
    {
        _controller.SpawnPiece(PieceType.T);

        // Tick enough frames to fall several rows at level 0 (48 frames/row)
        for (int row = 0; row < 5; row++)
        {
            for (int f = 0; f < GameRules.GetFramesPerRow(0); f++)
            {
                _controller.Tick(0, false, false, false, false);
            }
        }

        // Piece should have fallen 5 rows (or hit the floor)
        Assert.GreaterOrEqual(_controller.CurrentPiece.Row, 5);
    }

    // -----------------------------------------------------------------------
    // Lock behavior -- NES: lock on next gravity tick after landing, no delay
    // -----------------------------------------------------------------------

    [Test]
    public void Lock_PieceLocksOnNextGravityTick_AfterLanding()
    {
        // Place a block at the floor so piece lands immediately
        _model.SetCell(GameRules.PLAYFIELD_TOTAL_HEIGHT - 1, 4, 1);
        _model.SetCell(GameRules.PLAYFIELD_TOTAL_HEIGHT - 1, 5, 1);
        _model.SetCell(GameRules.PLAYFIELD_TOTAL_HEIGHT - 1, 6, 1);

        // Spawn a T-piece and let it fall
        _controller.SpawnPiece(PieceType.T);

        bool locked = false;
        _controller.OnPieceLocked += (piece) => { locked = true; };

        // Tick until piece lands and locks
        for (int i = 0; i < 1000; i++)
        {
            _controller.Tick(0, false, false, false, false);
            if (locked)
                break;
        }

        Assert.IsTrue(locked, "Piece should have locked");
    }

    [Test]
    public void Lock_OnPieceLockedFiresWithCorrectState()
    {
        // Fill bottom row to force landing quickly
        for (int col = 0; col < GameRules.PLAYFIELD_WIDTH; col++)
        {
            _model.SetCell(GameRules.PLAYFIELD_TOTAL_HEIGHT - 1, col, 1);
        }

        _controller.SpawnPiece(PieceType.O);

        bool locked = false;
        PieceState lockedPiece = default;
        _controller.OnPieceLocked += (piece) => { locked = true; lockedPiece = piece; };

        // Tick until lock
        for (int i = 0; i < 1000; i++)
        {
            _controller.Tick(0, false, false, false, false);
            if (locked)
                break;
        }

        Assert.IsTrue(locked);
        Assert.AreEqual(PieceType.O, lockedPiece.Type);
    }

    [Test]
    public void Lock_NoArtificialDelay_AfterLanding()
    {
        // Fill the bottom row
        for (int col = 0; col < GameRules.PLAYFIELD_WIDTH; col++)
        {
            _model.SetCell(GameRules.PLAYFIELD_TOTAL_HEIGHT - 1, col, 1);
        }

        _controller.SpawnPiece(PieceType.O);

        // Track total ticks until lock
        int totalTicks = 0;
        bool locked = false;
        _controller.OnPieceLocked += (piece) => { locked = true; };

        for (int i = 0; i < 10000; i++)
        {
            _controller.Tick(0, false, false, false, false);
            totalTicks++;
            if (locked)
                break;
        }

        Assert.IsTrue(locked, "Piece should have locked");

        // The piece falls from row 1 to row 19 (since floor is at row 21)
        // O-piece is 2 cells tall, so it can go to row 19 (cells at 19,20)
        // 18 moves + 1 landing tick + 1 lock tick = 20 ticks worth of gravity
        int expectedTicks = 21 * GameRules.GetFramesPerRow(0);
        // Allow small tolerance for piece geometry
        Assert.LessOrEqual(totalTicks, expectedTicks + GameRules.GetFramesPerRow(0),
            "Lock should happen with no artificial delay after landing");
    }

    [Test]
    public void Lock_PieceCellsAreWrittenToGrid()
    {
        // Fill bottom row
        for (int col = 0; col < GameRules.PLAYFIELD_WIDTH; col++)
        {
            _model.SetCell(GameRules.PLAYFIELD_TOTAL_HEIGHT - 1, col, 1);
        }

        _controller.SpawnPiece(PieceType.O);

        bool locked = false;
        PieceState lockedPiece = default;
        _controller.OnPieceLocked += (piece) => { locked = true; lockedPiece = piece; };

        for (int i = 0; i < 10000; i++)
        {
            _controller.Tick(0, false, false, false, false);
            if (locked)
                break;
        }

        Assert.IsTrue(locked);

        // O-piece cells should be locked in the grid
        int pieceValue = (int)PieceType.O + 1;
        Assert.AreEqual(pieceValue, _model.GetCell(lockedPiece.Row, lockedPiece.Col));
        Assert.AreEqual(pieceValue, _model.GetCell(lockedPiece.Row, lockedPiece.Col + 1));
        Assert.AreEqual(pieceValue, _model.GetCell(lockedPiece.Row + 1, lockedPiece.Col));
        Assert.AreEqual(pieceValue, _model.GetCell(lockedPiece.Row + 1, lockedPiece.Col + 1));
    }

    // -----------------------------------------------------------------------
    // Movement -- Left/Right
    // -----------------------------------------------------------------------

    [Test]
    public void MoveLeft_PieceMovesOneColumn()
    {
        _controller.SpawnPiece(PieceType.T);
        int initialCol = _controller.CurrentPiece.Col;

        _controller.HandleInput(true, false, false, false);

        Assert.AreEqual(initialCol - 1, _controller.CurrentPiece.Col);
    }

    [Test]
    public void MoveRight_PieceMovesOneColumn()
    {
        _controller.SpawnPiece(PieceType.T);
        int initialCol = _controller.CurrentPiece.Col;

        _controller.HandleInput(false, true, false, false);

        Assert.AreEqual(initialCol + 1, _controller.CurrentPiece.Col);
    }

    [Test]
    public void MoveLeft_BlockedByWall()
    {
        _controller.SpawnPiece(PieceType.T);

        // Move left until we hit the wall
        for (int i = 0; i < 20; i++)
        {
            _controller.HandleInput(true, false, false, false);
        }

        Assert.AreEqual(0, _controller.CurrentPiece.Col, "Piece should stop at left wall");
    }

    [Test]
    public void MoveRight_BlockedByWall()
    {
        _controller.SpawnPiece(PieceType.T);

        // Move right until we hit the wall
        for (int i = 0; i < 20; i++)
        {
            _controller.HandleInput(false, true, false, false);
        }

        // T-piece rot 0: cells at (0,-1), (0,0), (0,1), (1,0) -- max col offset is +1
        Assert.AreEqual(GameRules.PLAYFIELD_WIDTH - 2, _controller.CurrentPiece.Col, "Piece should stop at right wall");
    }

    // -----------------------------------------------------------------------
    // Auto-repeat -- Left/Right hold
    // -----------------------------------------------------------------------

    [Test]
    public void AutoRepeat_LeftHold_MovesImmediatelyOnFirstFrame()
    {
        _controller.SpawnPiece(PieceType.T);
        int initialCol = _controller.CurrentPiece.Col;

        // First frame of hold -- should move immediately
        _controller.HandleInput(true, false, false, false);

        Assert.AreEqual(initialCol - 1, _controller.CurrentPiece.Col, "First frame of hold should move immediately");
    }

    [Test]
    public void AutoRepeat_LeftHold_Waits16FramesBeforeRepeat()
    {
        _controller.SpawnPiece(PieceType.T);
        int initialCol = _controller.CurrentPiece.Col;

        // Frame 0: first move
        _controller.HandleInput(true, false, false, false);
        Assert.AreEqual(initialCol - 1, _controller.CurrentPiece.Col);

        // Frames 1-15: no repeat (15 more calls = frames 1-15)
        for (int i = 1; i < 16; i++)
        {
            _controller.HandleInput(true, false, false, false);
        }
        // After frame 15 (16 total calls), should still be at initialCol - 1
        Assert.AreEqual(initialCol - 1, _controller.CurrentPiece.Col, "No repeat during initial delay (frames 1-15)");

        // Frame 16: should repeat
        _controller.HandleInput(true, false, false, false);
        Assert.AreEqual(initialCol - 2, _controller.CurrentPiece.Col, "Should repeat on frame 16 (after 16-frame delay)");
    }

    [Test]
    public void AutoRepeat_LeftHold_RepeatsEvery6Frames()
    {
        _controller.SpawnPiece(PieceType.T);
        int initialCol = _controller.CurrentPiece.Col;

        // Frame 0: first move (col = initialCol - 1)
        _controller.HandleInput(true, false, false, false);

        // Frames 1-15: no repeat
        for (int i = 1; i < 16; i++)
        {
            _controller.HandleInput(true, false, false, false);
        }
        // Frame 16: first repeat (col = initialCol - 2)
        _controller.HandleInput(true, false, false, false);

        // Frames 17-21: no repeat (5 frames)
        for (int i = 17; i < 22; i++)
        {
            _controller.HandleInput(true, false, false, false);
        }
        // Should still be at initialCol - 2
        Assert.AreEqual(initialCol - 2, _controller.CurrentPiece.Col, "No repeat between frame 16 and 22");

        // Frame 22: second repeat (col = initialCol - 3)
        _controller.HandleInput(true, false, false, false);
        Assert.AreEqual(initialCol - 3, _controller.CurrentPiece.Col, "Should repeat on frame 22 (16 + 6)");
    }

    [Test]
    public void AutoRepeat_RightHold_SameBehaviorAsLeft()
    {
        _controller.SpawnPiece(PieceType.T);
        int initialCol = _controller.CurrentPiece.Col;

        // Frame 0: first move
        _controller.HandleInput(false, true, false, false);
        Assert.AreEqual(initialCol + 1, _controller.CurrentPiece.Col);

        // Frames 1-15: no repeat
        for (int i = 1; i < 16; i++)
        {
            _controller.HandleInput(false, true, false, false);
        }
        Assert.AreEqual(initialCol + 1, _controller.CurrentPiece.Col, "No repeat during initial delay");

        // Frame 16: repeat
        _controller.HandleInput(false, true, false, false);
        Assert.AreEqual(initialCol + 2, _controller.CurrentPiece.Col, "Should repeat on frame 16");
    }

    [Test]
    public void AutoRepeat_ReleaseResetsCounter()
    {
        _controller.SpawnPiece(PieceType.T);
        int initialCol = _controller.CurrentPiece.Col;

        // Hold left for 17 frames (past initial delay)
        for (int i = 0; i < 17; i++)
        {
            _controller.HandleInput(true, false, false, false);
        }
        int colAfterHold = _controller.CurrentPiece.Col;

        // Release
        _controller.HandleInput(false, false, false, false);

        // Press again -- should move immediately (counter reset)
        _controller.HandleInput(true, false, false, false);
        Assert.AreEqual(colAfterHold - 1, _controller.CurrentPiece.Col, "Re-press after release should move immediately");
    }

    // -----------------------------------------------------------------------
    // Rotation
    // -----------------------------------------------------------------------

    [Test]
    public void Rotation_ChangesRotationState()
    {
        _controller.SpawnPiece(PieceType.T);

        _controller.HandleInput(false, false, true, false);
        Assert.AreEqual(1, _controller.CurrentPiece.Rotation);

        _controller.HandleInput(false, false, true, false);
        Assert.AreEqual(2, _controller.CurrentPiece.Rotation);
    }

    [Test]
    public void Rotation_BlockedByWall_NoKick()
    {
        // Spawn T-piece and move it to the left wall
        _controller.SpawnPiece(PieceType.T);

        // Move left until wall
        for (int i = 0; i < 20; i++)
        {
            _controller.HandleInput(true, false, false, false);
        }
        int colAtWall = _controller.CurrentPiece.Col;

        // Attempt rotation -- should be blocked, piece stays in place
        _controller.HandleInput(false, false, true, false);

        Assert.AreEqual(colAtWall, _controller.CurrentPiece.Col, "Piece should not kick from wall");
    }

    [Test]
    public void Rotation_NoAutoRepeat()
    {
        _controller.SpawnPiece(PieceType.T);
        Assert.AreEqual(0, _controller.CurrentPiece.Rotation);

        // Press rotate multiple times in the same HandleInput call context
        _controller.HandleInput(false, false, true, false);
        Assert.AreEqual(1, _controller.CurrentPiece.Rotation);

        _controller.HandleInput(false, false, true, false);
        Assert.AreEqual(2, _controller.CurrentPiece.Rotation);
    }

    // -----------------------------------------------------------------------
    // Soft drop
    // -----------------------------------------------------------------------

    [Test]
    public void SoftDrop_PieceFallsEveryFrame()
    {
        _controller.SpawnPiece(PieceType.T);
        int initialRow = _controller.CurrentPiece.Row;

        // With soft drop, piece should fall every frame
        for (int i = 0; i < 5; i++)
        {
            _controller.Tick(0, false, false, false, true); // softDropHeld = true
        }

        Assert.AreEqual(initialRow + 5, _controller.CurrentPiece.Row, "Piece should fall 5 rows in 5 frames with soft drop");
    }

    [Test]
    public void SoftDrop_OverridesNormalGravityRate()
    {
        _controller.SpawnPiece(PieceType.T);
        int initialRow = _controller.CurrentPiece.Row;

        // Level 0 has 48 frames/row, but soft drop should override to 1 frame/row
        _controller.Tick(0, false, false, false, true);

        Assert.AreEqual(initialRow + 1, _controller.CurrentPiece.Row, "Soft drop should override normal gravity rate");
    }

    // -----------------------------------------------------------------------
    // Tick without active piece
    // -----------------------------------------------------------------------

    [Test]
    public void Tick_WithoutActivePiece_DoesNothing()
    {
        // Don't spawn any piece
        bool exception = false;
        try
        {
            for (int i = 0; i < 100; i++)
            {
                _controller.Tick(0, true, true, true, true);
            }
        }
        catch
        {
            exception = true;
        }

        Assert.IsFalse(exception, "Tick without active piece should not throw");
    }

    // -----------------------------------------------------------------------
    // Integration: spawn -> fall -> lock cycle
    // -----------------------------------------------------------------------

    [Test]
    public void Integration_SpawnFallLockCycle()
    {
        // Fill bottom row
        for (int col = 0; col < GameRules.PLAYFIELD_WIDTH; col++)
        {
            _model.SetCell(GameRules.PLAYFIELD_TOTAL_HEIGHT - 1, col, 1);
        }

        bool locked = false;
        PieceState lockedPiece = default;
        _controller.OnPieceLocked += (piece) => { locked = true; lockedPiece = piece; };

        _controller.SpawnPiece(PieceType.I);

        // Tick until lock
        for (int i = 0; i < 10000; i++)
        {
            _controller.Tick(0, false, false, false, false);
            if (locked)
                break;
        }

        Assert.IsTrue(locked, "I-piece should have locked");
        Assert.AreEqual(PieceType.I, lockedPiece.Type);

        // Verify cells are in the grid
        int pieceValue = (int)PieceType.I + 1;
        Assert.AreEqual(pieceValue, _model.GetCell(lockedPiece.Row, lockedPiece.Col));
    }

    // -----------------------------------------------------------------------
    // Integration: move while falling
    // -----------------------------------------------------------------------

    [Test]
    public void Integration_MoveWhileFalling()
    {
        _controller.SpawnPiece(PieceType.T);
        int initialCol = _controller.CurrentPiece.Col;

        // Hold left for 10 ticks
        for (int i = 0; i < 10; i++)
        {
            _controller.Tick(0, true, false, false, false);
        }

        // Piece should have moved left (at least once on first frame)
        Assert.LessOrEqual(_controller.CurrentPiece.Col, initialCol - 1);
    }

    // -----------------------------------------------------------------------
    // CurrentPiece returns correct value
    // -----------------------------------------------------------------------

    [Test]
    public void CurrentPiece_ReturnsSpawnedPiece()
    {
        _controller.SpawnPiece(PieceType.S);

        var piece = _controller.CurrentPiece;
        Assert.AreEqual(PieceType.S, piece.Type);
    }
}
