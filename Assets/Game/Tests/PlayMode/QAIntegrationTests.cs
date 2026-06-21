using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Integration tests that verify the complete game flow and component interactions.
/// Replaces the broken version that referenced unavailable InputSystem types.
/// </summary>
[TestFixture]
public class QAIntegrationTests
{
    private GameObject _gameplayControllerGo;
    private GameplayController _gameplayController;

    [SetUp]
    public void SetUp()
    {
        _gameplayControllerGo = new GameObject("GameplayController");
        _gameplayController = _gameplayControllerGo.AddComponent<GameplayController>();
    }

    [TearDown]
    public void TearDown()
    {
        if (_gameplayControllerGo != null)
        {
            UnityEngine.Object.Destroy(_gameplayControllerGo);
        }
    }

    // -----------------------------------------------------------------------
    // QA journey pre-condition: start screen visible on load
    // -----------------------------------------------------------------------

    [UnityTest]
    public IEnumerator StartScreen_IsVisible_OnLoad()
    {
        // Verifies journey step 1 pre-condition: when the scene loads the
        // controller must be in Idle state so SceneBootstrapper can show the
        // start screen unobstructed. OnGameOver must NOT fire before the player
        // explicitly starts a game.
        yield return null; // Let Awake run

        bool gameOverFired = false;
        _gameplayController.OnGameOver += (score) => { gameOverFired = true; };

        // Run several Update frames in Idle state -- no game over should fire
        for (int i = 0; i < 10; i++)
        {
            yield return null;
        }

        Assert.IsFalse(gameOverFired, "OnGameOver must not fire before StartGame() -- controller must initialize to Idle, not GameOver");
        Assert.AreEqual(0, _gameplayController.CurrentScore, "Score must be 0 on initial load");
        Assert.AreEqual(0, _gameplayController.TotalLinesCleared, "Lines cleared must be 0 on initial load");
        Assert.AreEqual(0, _gameplayController.CurrentLevel, "Level must be 0 on initial load");
    }

    // -----------------------------------------------------------------------
    // Integration: Initial state verification
    // -----------------------------------------------------------------------

    [UnityTest]
    public IEnumerator Integration_Controller_InitialStateCorrect()
    {
        yield return null;

        Assert.IsNotNull(_gameplayController, "GameplayController should exist");
        Assert.IsNotNull(_gameplayController.Playfield, "Playfield should not be null");
        Assert.AreEqual(0, _gameplayController.CurrentScore, "Score should be 0 on init");
        Assert.AreEqual(0, _gameplayController.TotalLinesCleared, "Lines should be 0 on init");
        Assert.AreEqual(0, _gameplayController.CurrentLevel, "Level should be 0 on init");
    }

    // -----------------------------------------------------------------------
    // Integration: Start game flow
    // -----------------------------------------------------------------------

    [UnityTest]
    public IEnumerator Integration_StartGame_ResetsState()
    {
        yield return null;

        _gameplayController.StartGame();
        yield return null;

        Assert.AreEqual(0, _gameplayController.CurrentScore, "Score should be 0 after start");
        Assert.AreEqual(0, _gameplayController.TotalLinesCleared, "Lines should be 0 after start");
        Assert.AreEqual(0, _gameplayController.CurrentLevel, "Level should be 0 on start");
    }

    [UnityTest]
    public IEnumerator Integration_StartGame_SpawnsPiece()
    {
        yield return null;

        _gameplayController.StartGame();
        yield return null;

        var activePiece = _gameplayController.ActivePiece;
        Assert.IsTrue(Enum.IsDefined(typeof(PieceType), activePiece.Type), "Active piece type should be valid");
    }

    // -----------------------------------------------------------------------
    // Integration: Game over flow
    // -----------------------------------------------------------------------

    [UnityTest]
    public IEnumerator Integration_GameOver_EventFiresWithScore()
    {
        yield return null;

        _gameplayController.StartGame();
        yield return null;

        int receivedScore = -1;
        _gameplayController.OnGameOver += (score) => { receivedScore = score; };

        // Use TriggerGameOver to reliably trigger game over with the current score
        _gameplayController.TriggerGameOver();

        Assert.GreaterOrEqual(receivedScore, 0, "OnGameOver should fire with a score >= 0");
    }

    // -----------------------------------------------------------------------
    // Integration: Game rules verification
    // -----------------------------------------------------------------------

    [Test]
    public void Integration_Scoring_SingleLineLevel0()
    {
        int score = GameRules.CalculateScore(1, 0);
        Assert.AreEqual(40, score, "Single line at level 0 should be 40 points");
    }

    [Test]
    public void Integration_Scoring_TetrisLevel0()
    {
        int score = GameRules.CalculateScore(4, 0);
        Assert.AreEqual(1200, score, "Tetris at level 0 should be 1200 points");
    }

    [Test]
    public void Integration_Scoring_SingleLineLevel9()
    {
        int score = GameRules.CalculateScore(1, 9);
        Assert.AreEqual(400, score, "Single line at level 9 should be 400 points (40 * 10)");
    }

    [Test]
    public void Integration_Scoring_ZeroLinesReturnsZero()
    {
        int score = GameRules.CalculateScore(0, 5);
        Assert.AreEqual(0, score, "Zero lines cleared should return 0 points");
    }

    [Test]
    public void Integration_LevelProgression_CorrectCalculation()
    {
        Assert.AreEqual(0, GameRules.GetLevel(0), "0 lines -> level 0");
        Assert.AreEqual(0, GameRules.GetLevel(9), "9 lines -> level 0");
        Assert.AreEqual(1, GameRules.GetLevel(10), "10 lines -> level 1");
        Assert.AreEqual(5, GameRules.GetLevel(57), "57 lines -> level 5");
    }

    [Test]
    public void Integration_Gravity_FramesPerRowDecreases()
    {
        int fps0 = GameRules.GetFramesPerRow(0);
        int fps9 = GameRules.GetFramesPerRow(9);
        Assert.Greater(fps0, fps9, "Gravity should increase (fewer frames) at higher levels");
        Assert.AreEqual(48, fps0, "Level 0 should have 48 frames per row");
        Assert.AreEqual(6, fps9, "Level 9 should have 6 frames per row");
    }

    [Test]
    public void Integration_Gravity_ClampsAtLevel29()
    {
        int fps29 = GameRules.GetFramesPerRow(29);
        int fps50 = GameRules.GetFramesPerRow(50);
        Assert.AreEqual(fps29, fps50, "Gravity should clamp at level 29");
        Assert.AreEqual(1, fps29, "Level 29 should have 1 frame per row");
    }

    // -----------------------------------------------------------------------
    // Integration: Playfield dimensions
    // -----------------------------------------------------------------------

    [Test]
    public void Integration_Playfield_DimensionsCorrect()
    {
        Assert.AreEqual(10, GameRules.PLAYFIELD_WIDTH, "Playfield width should be 10");
        Assert.AreEqual(20, GameRules.PLAYFIELD_VISIBLE_HEIGHT, "Visible height should be 20");
        Assert.AreEqual(22, GameRules.PLAYFIELD_TOTAL_HEIGHT, "Total height should be 22 (20 + 2 buffer)");
    }

    // -----------------------------------------------------------------------
    // Integration: GameplayController tick stability
    // -----------------------------------------------------------------------

    [UnityTest]
    public IEnumerator Integration_Controller_TickStability_NoCrash()
    {
        yield return null;

        _gameplayController.StartGame();
        yield return null;

        bool threw = false;
        for (int i = 0; i < 1000; i++)
        {
            try
            {
                _gameplayController.Tick();
            }
            catch
            {
                threw = true;
                break;
            }
            yield return null;
        }

        Assert.IsFalse(threw, "Controller should survive 1000 ticks without exceptions");
    }

    // -----------------------------------------------------------------------
    // Integration: Piece randomization
    // -----------------------------------------------------------------------

    [UnityTest]
    public IEnumerator Integration_Randomizer_ProducesValidPieces()
    {
        yield return null;

        _gameplayController.StartGame();
        yield return null;

        var activePiece = _gameplayController.ActivePiece;
        Assert.IsTrue(Enum.IsDefined(typeof(PieceType), activePiece.Type), "Active piece type should be valid");

        var nextPiece = _gameplayController.NextPiece;
        Assert.IsTrue(Enum.IsDefined(typeof(PieceType), nextPiece), "Next piece type should be valid");
    }

    // -----------------------------------------------------------------------
    // Integration: Multiple game restarts
    // -----------------------------------------------------------------------

    [UnityTest]
    public IEnumerator Integration_MultipleRestarts_ResetProperly()
    {
        yield return null;

        // First game
        _gameplayController.StartGame();
        yield return null;

        for (int i = 0; i < 100; i++)
        {
            _gameplayController.Tick();
            yield return null;
        }

        // Start a new game
        _gameplayController.StartGame();
        yield return null;

        Assert.AreEqual(0, _gameplayController.CurrentScore, "Score should reset to 0 on new game");
        Assert.AreEqual(0, _gameplayController.TotalLinesCleared, "Lines should reset to 0 on new game");
        Assert.AreEqual(0, _gameplayController.CurrentLevel, "Level should reset to 0 on new game");
    }
}
