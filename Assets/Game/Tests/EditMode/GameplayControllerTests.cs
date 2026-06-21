using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

[TestFixture]
public class GameplayControllerTests
{
    // =========================================================================
    // PieceMovementController spawn tests
    // =========================================================================

    [Test]
    public void PieceMovementController_SpawnPiece_SucceedsForAllPieceTypes()
    {
        // Verifies that every piece type can be placed at the initial spawn
        // position on a clear board. Pieces with negative row offsets (S, Z, J, L)
        // previously failed when spawned at row 0; the fix is row 1.
        var playfield = new PlayfieldModel();
        var stateMachine = new GameStateMachine();
        var controller = new PieceMovementController(playfield, stateMachine);

        bool spawnFailed = false;
        controller.OnSpawnFailed += () => { spawnFailed = true; };

        foreach (PieceType type in Enum.GetValues(typeof(PieceType)))
        {
            spawnFailed = false;
            playfield.Clear();
            controller.SpawnPiece(type);
            Assert.IsFalse(spawnFailed, $"Piece type {type} failed to spawn on a clear board -- spawn position is out of bounds");
        }
    }

    // =========================================================================
    // PieceRandomizer tests
    // =========================================================================

    [Test]
    public void PieceRandomizer_Next_ReturnsValidPieceType()
    {
        var randomizer = new PieceRandomizer(42);
        for (int i = 0; i < 100; i++)
        {
            var piece = randomizer.Next();
            Assert.IsTrue(Enum.IsDefined(typeof(PieceType), piece), "Should return valid PieceType");
        }
    }

    [Test]
    public void PieceRandomizer_7Bag_ContainsAllTypes()
    {
        var randomizer = new PieceRandomizer(42);
        var pieces = new HashSet<PieceType>();

        for (int i = 0; i < 7; i++)
        {
            pieces.Add(randomizer.Next());
        }

        Assert.AreEqual(7, pieces.Count, "First 7 pieces should contain all 7 types (one each)");
    }

    [Test]
    public void PieceRandomizer_7Bag_NoDuplicatesInFirstBag()
    {
        var randomizer = new PieceRandomizer(123);
        var pieces = new List<PieceType>();

        for (int i = 0; i < 7; i++)
        {
            pieces.Add(randomizer.Next());
        }

        Assert.AreEqual(pieces.Count, pieces.Distinct().Count(), "No duplicates in first bag");
    }

    [Test]
    public void PieceRandomizer_Peek_ReturnsSameAsNext()
    {
        var randomizer = new PieceRandomizer(42);
        var peeked = randomizer.Peek();
        var nexted = randomizer.Next();

        Assert.AreEqual(peeked, nexted, "Peek should return same type as Next");
    }

    [Test]
    public void PieceRandomizer_Peek_DoesNotConsume()
    {
        var randomizer = new PieceRandomizer(42);
        var peeked1 = randomizer.Peek();
        var peeked2 = randomizer.Peek();

        Assert.AreEqual(peeked1, peeked2, "Multiple peeks should return same value");
    }

    [Test]
    public void PieceRandomizer_SecondBag_AfterFirstBagExhausted()
    {
        var randomizer = new PieceRandomizer(42);
        // Consume first bag
        for (int i = 0; i < 7; i++)
        {
            randomizer.Next();
        }

        // Second bag should work
        var piece = randomizer.Next();
        Assert.IsTrue(Enum.IsDefined(typeof(PieceType), piece), "Second bag should produce valid piece");
    }

    // =========================================================================
    // GameRules scoring tests
    // =========================================================================

    [Test]
    public void Scoring_SingleLine_AtLevel0_Adds40()
    {
        int score = GameRules.CalculateScore(1, 0);
        Assert.AreEqual(40, score, "Single line at level 0 should add 40 points");
    }

    [Test]
    public void Scoring_DoubleLine_AtLevel0_Adds100()
    {
        int score = GameRules.CalculateScore(2, 0);
        Assert.AreEqual(100, score, "Double line at level 0 should add 100 points");
    }

    [Test]
    public void Scoring_TripleLine_AtLevel0_Adds300()
    {
        int score = GameRules.CalculateScore(3, 0);
        Assert.AreEqual(300, score, "Triple line at level 0 should add 300 points");
    }

    [Test]
    public void Scoring_Tetris_AtLevel0_Adds1200()
    {
        int score = GameRules.CalculateScore(4, 0);
        Assert.AreEqual(1200, score, "Tetris at level 0 should add 1200 points");
    }

    [Test]
    public void Scoring_Tetris_AtLevel1_Adds2400()
    {
        int score = GameRules.CalculateScore(4, 1);
        Assert.AreEqual(2400, score, "Tetris at level 1 should add 2400 points (1200 * 2)");
    }

    [Test]
    public void Scoring_SingleLine_AtLevel9_Adds400()
    {
        int score = GameRules.CalculateScore(1, 9);
        Assert.AreEqual(400, score, "Single line at level 9 should add 400 points (40 * 10)");
    }

    [Test]
    public void Scoring_ZeroLines_Returns0()
    {
        int score = GameRules.CalculateScore(0, 0);
        Assert.AreEqual(0, score, "Zero lines should return 0 score");
    }

    // =========================================================================
    // Level progression tests
    // =========================================================================

    [Test]
    public void LevelProgression_0Lines_IsLevel0()
    {
        Assert.AreEqual(0, GameRules.GetLevel(0));
    }

    [Test]
    public void LevelProgression_9Lines_IsLevel0()
    {
        Assert.AreEqual(0, GameRules.GetLevel(9));
    }

    [Test]
    public void LevelProgression_10Lines_IsLevel1()
    {
        Assert.AreEqual(1, GameRules.GetLevel(10));
    }

    [Test]
    public void LevelProgression_19Lines_IsLevel1()
    {
        Assert.AreEqual(1, GameRules.GetLevel(19));
    }

    [Test]
    public void LevelProgression_20Lines_IsLevel2()
    {
        Assert.AreEqual(2, GameRules.GetLevel(20));
    }

    [Test]
    public void LevelProgression_50Lines_IsLevel5()
    {
        Assert.AreEqual(5, GameRules.GetLevel(50));
    }

    // =========================================================================
    // Gravity lookup table tests
    // =========================================================================

    [Test]
    public void Gravity_Level0_Is48FramesPerRow()
    {
        Assert.AreEqual(48, GameRules.GetFramesPerRow(0));
    }

    [Test]
    public void Gravity_Level1_Is43FramesPerRow()
    {
        Assert.AreEqual(43, GameRules.GetFramesPerRow(1));
    }

    [Test]
    public void Gravity_Level9_Is6FramesPerRow()
    {
        Assert.AreEqual(6, GameRules.GetFramesPerRow(9));
    }

    [Test]
    public void Gravity_Level10_Is5FramesPerRow()
    {
        Assert.AreEqual(5, GameRules.GetFramesPerRow(10));
    }

    [Test]
    public void Gravity_Level29_Is1FramePerRow()
    {
        Assert.AreEqual(1, GameRules.GetFramesPerRow(29));
    }

    [Test]
    public void Gravity_Level30_ClampedToLevel29()
    {
        Assert.AreEqual(1, GameRules.GetFramesPerRow(30), "Level 30 should clamp to level 29 value");
    }

    // =========================================================================
    // GameStateMachine tests
    // =========================================================================

    [Test]
    public void GameStateMachine_DefaultState_IsIdle()
    {
        var machine = new GameStateMachine();
        Assert.AreEqual(GameStateMachine.GameState.Idle, machine.CurrentState);
    }

    [Test]
    public void GameStateMachine_ChangeState_FiresEvent()
    {
        var machine = new GameStateMachine();
        bool eventFired = false;
        GameStateMachine.GameState receivedState = default;

        machine.OnStateChanged += (state) =>
        {
            eventFired = true;
            receivedState = state;
        };

        machine.ChangeState(GameStateMachine.GameState.Spawning);

        Assert.IsTrue(eventFired, "OnStateChanged should fire");
        Assert.AreEqual(GameStateMachine.GameState.Spawning, receivedState);
    }

    [Test]
    public void GameStateMachine_NewStates_Exist()
    {
        // Verify new states are accessible
        var idle = GameStateMachine.GameState.Idle;
        var spawning = GameStateMachine.GameState.Spawning;
        var playing = GameStateMachine.GameState.Playing;
        var lineClear = GameStateMachine.GameState.LineClear;

        Assert.IsTrue(Enum.IsDefined(typeof(GameStateMachine.GameState), idle));
        Assert.IsTrue(Enum.IsDefined(typeof(GameStateMachine.GameState), spawning));
        Assert.IsTrue(Enum.IsDefined(typeof(GameStateMachine.GameState), playing));
        Assert.IsTrue(Enum.IsDefined(typeof(GameStateMachine.GameState), lineClear));
    }

    // =========================================================================
    // GameplayController integration logic tests (without Unity)
    // =========================================================================

    /// <summary>
    /// Simulates a GameplayController game loop using the underlying components
    /// directly (without Unity MonoBehaviour lifecycle).
    /// </summary>
    private class SimulatedGame
    {
        public PlayfieldModel Playfield { get; private set; }
        public GameStateMachine StateMachine { get; private set; }
        public PieceMovementController PieceController { get; private set; }
        public PieceRandomizer Randomizer { get; private set; }

        public int CurrentScore { get; private set; }
        public int TotalLinesCleared { get; private set; }
        public int CurrentLevel { get { return GameRules.GetLevel(TotalLinesCleared); } }
        public bool GameOver { get; private set; }
        public int GameOverScore { get; private set; }
        public int StateChangeCount { get; private set; }

        public SimulatedGame(int seed)
        {
            Playfield = new PlayfieldModel();
            StateMachine = new GameStateMachine();
            PieceController = new PieceMovementController(Playfield, StateMachine);
            Randomizer = new PieceRandomizer(seed);

            PieceController.OnPieceLocked += OnPieceLocked;
            PieceController.OnSpawnFailed += OnSpawnFailed;
            StateMachine.OnStateChanged += OnStateChanged;
        }

        public void StartGame()
        {
            CurrentScore = 0;
            TotalLinesCleared = 0;
            GameOver = false;
            GameOverScore = 0;
            StateChangeCount = 0;
            Playfield.Clear();
            StateMachine.ChangeState(GameStateMachine.GameState.Spawning);
        }

        public void Tick(bool leftHeld = false, bool rightHeld = false, bool rotatePressed = false, bool softDropHeld = false)
        {
            if (GameOver) return;

            switch (StateMachine.CurrentState)
            {
                case GameStateMachine.GameState.Idle:
                    break;
                case GameStateMachine.GameState.Spawning:
                    PieceController.SpawnPiece(Randomizer.Next());
                    // Only transition to Playing if spawn succeeded (state is still Spawning)
                    if (StateMachine.CurrentState == GameStateMachine.GameState.Spawning)
                    {
                        StateMachine.ChangeState(GameStateMachine.GameState.Playing);
                    }
                    break;
                case GameStateMachine.GameState.Playing:
                    PieceController.Tick(CurrentLevel, leftHeld, rightHeld, rotatePressed, softDropHeld);
                    break;
                case GameStateMachine.GameState.LineClear:
                    int levelAtClear = CurrentLevel;
                    int linesCleared = Playfield.ClearLines();
                    if (linesCleared > 0)
                    {
                        CurrentScore += GameRules.CalculateScore(linesCleared, levelAtClear);
                        TotalLinesCleared += linesCleared;
                    }
                    StateMachine.ChangeState(GameStateMachine.GameState.Spawning);
                    break;
                case GameStateMachine.GameState.GameOver:
                    break;
            }
        }

        private void OnPieceLocked(PieceState piece)
        {
            StateMachine.ChangeState(GameStateMachine.GameState.LineClear);
        }

        private void OnSpawnFailed()
        {
            StateMachine.ChangeState(GameStateMachine.GameState.GameOver);
        }

        private void OnStateChanged(GameStateMachine.GameState newState)
        {
            StateChangeCount++;
            if (newState == GameStateMachine.GameState.GameOver)
            {
                GameOver = true;
                GameOverScore = CurrentScore;
            }
        }
    }

    [Test]
    public void SimulatedGame_StartGame_ResetsScoreAndLines()
    {
        var game = new SimulatedGame(42);
        game.StartGame();

        Assert.AreEqual(0, game.CurrentScore, "Score should be 0 after StartGame");
        Assert.AreEqual(0, game.TotalLinesCleared, "Lines should be 0 after StartGame");
        Assert.AreEqual(0, game.CurrentLevel, "Level should be 0 after StartGame");
    }

    [Test]
    public void SimulatedGame_ClearingOneLine_AtLevel0_Adds40()
    {
        var game = new SimulatedGame(42);
        game.StartGame();

        // Fill a row near the bottom to force a line clear
        // First let a piece fall and lock, then fill a row manually
        // Actually, let's fill a row and let a piece land on top to clear it
        for (int col = 0; col < GameRules.PLAYFIELD_WIDTH; col++)
        {
            game.Playfield.SetCell(GameRules.PLAYFIELD_TOTAL_HEIGHT - 2, col, 1);
        }

        // Spawn and let piece fall -- when it locks, line clear should process
        // But we need to simulate the full cycle. Let's just tick with soft drop.
        for (int i = 0; i < 5000; i++)
        {
            game.Tick(softDropHeld: true);
            if (game.GameOver) break;
        }

        // After the piece locks and the line clears, score should be 40
        Assert.AreEqual(40, game.CurrentScore, "Clearing 1 line at level 0 should add 40");
    }

    [Test]
    public void SimulatedGame_ClearingFourLines_AtLevel0_Adds1200()
    {
        var game = new SimulatedGame(42);
        game.StartGame();

        // Fill 4 rows near the bottom
        for (int row = GameRules.PLAYFIELD_TOTAL_HEIGHT - 5; row < GameRules.PLAYFIELD_TOTAL_HEIGHT - 1; row++)
        {
            for (int col = 0; col < GameRules.PLAYFIELD_WIDTH; col++)
            {
                game.Playfield.SetCell(row, col, 1);
            }
        }

        // Tick with soft drop until the piece locks and lines clear
        for (int i = 0; i < 10000; i++)
        {
            game.Tick(softDropHeld: true);
            if (game.GameOver) break;
        }

        Assert.AreEqual(1200, game.CurrentScore, "Clearing 4 lines at level 0 should add 1200");
    }

    [Test]
    public void SimulatedGame_After10Lines_LevelBecomes1()
    {
        var game = new SimulatedGame(42);
        game.StartGame();

        // Fill rows and let pieces clear them -- need 10 line clears
        // Fill 10 rows near the bottom
        for (int row = GameRules.PLAYFIELD_TOTAL_HEIGHT - 11; row < GameRules.PLAYFIELD_TOTAL_HEIGHT - 1; row++)
        {
            for (int col = 0; col < GameRules.PLAYFIELD_WIDTH; col++)
            {
                game.Playfield.SetCell(row, col, 1);
            }
        }

        // Tick until piece locks and lines clear
        for (int i = 0; i < 10000; i++)
        {
            game.Tick(softDropHeld: true);
            if (game.GameOver) break;
        }

        Assert.AreEqual(1, game.CurrentLevel, "After 10 lines cleared, level should be 1");
    }

    [Test]
    public void SimulatedGame_StateChanges_FireCorrectly()
    {
        var game = new SimulatedGame(42);
        game.StartGame();

        // After StartGame, state should be Spawning (1 state change from Idle -> Spawning)
        Assert.AreEqual(1, game.StateChangeCount, "StartGame should trigger 1 state change (Idle -> Spawning)");

        // Tick once to spawn -> Playing
        game.Tick();
        Assert.AreEqual(2, game.StateChangeCount, "Spawning -> Playing should be 2nd state change");
    }

    [Test]
    public void SimulatedGame_GameOver_FiresWithFinalScore()
    {
        var game = new SimulatedGame(42);
        game.StartGame();

        // Fill the entire playfield to force game over on next spawn
        for (int row = 0; row < GameRules.PLAYFIELD_TOTAL_HEIGHT; row++)
        {
            for (int col = 0; col < GameRules.PLAYFIELD_WIDTH; col++)
            {
                game.Playfield.SetCell(row, col, 1);
            }
        }

        // Tick -- the first spawn attempt should fail
        game.Tick();

        Assert.IsTrue(game.GameOver, "Game should be over when spawn is blocked");
        Assert.AreEqual(0, game.GameOverScore, "GameOver score should be 0");
    }

    [Test]
    public void SimulatedGame_ExtendedPlay_NoExceptions()
    {
        var game = new SimulatedGame(42);
        game.StartGame();

        // Simulate 60 seconds of play at 60fps = 3600 frames
        // Use soft drop to speed things up
        int maxTicks = 10000;
        for (int i = 0; i < maxTicks; i++)
        {
            game.Tick(softDropHeld: true);
            if (game.GameOver) break;
        }

        // Should not have thrown any exceptions
        Assert.IsTrue(true, "Game ran without exceptions");
    }

    [Test]
    public void SimulatedGame_MultiplePieces_PlayContinues()
    {
        var game = new SimulatedGame(42);
        game.StartGame();

        // Let several pieces fall and lock with soft drop
        for (int i = 0; i < 10000; i++)
        {
            game.Tick(softDropHeld: true);
            if (game.GameOver) break;
        }

        // At least a few pieces should have been processed
        Assert.Greater(game.StateChangeCount, 2, "Multiple state changes should occur during play");
    }
}
