using System;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

[TestFixture]
public class GameplayControllerPlayModeTests
{
    private GameObject _gameObject;
    private GameplayController _controller;

    [SetUp]
    public void Setup()
    {
        _gameObject = new GameObject("GameplayControllerTest");
        _controller = _gameObject.AddComponent<GameplayController>();
    }

    [TearDown]
    public void Teardown()
    {
        if (_gameObject != null)
        {
            UnityEngine.Object.Destroy(_gameObject);
        }
    }

    [UnityTest]
    public IEnumerator GameplayController_Awake_InitializesInIdleState()
    {
        yield return null; // Wait for Awake to run

        Assert.IsNotNull(_controller, "GameplayController should be created");
        Assert.IsNotNull(_controller.Playfield, "Playfield should not be null");
        Assert.AreEqual(0, _controller.CurrentScore, "Score should be 0 initially");
        Assert.AreEqual(0, _controller.TotalLinesCleared, "Lines should be 0 initially");
        Assert.AreEqual(0, _controller.CurrentLevel, "Level should be 0 initially");
    }

    [UnityTest]
    public IEnumerator GameplayController_StartGame_ResetsState()
    {
        yield return null; // Wait for Awake

        // Start the game
        _controller.StartGame();
        yield return null; // Let Tick process the Spawning state

        Assert.AreEqual(0, _controller.CurrentScore, "Score should be 0 after StartGame");
        Assert.AreEqual(0, _controller.TotalLinesCleared, "Lines should be 0 after StartGame");
        Assert.AreEqual(0, _controller.CurrentLevel, "Level should be 0 after StartGame");
    }

    [UnityTest]
    public IEnumerator GameplayController_StartGame_SpawnsFirstPiece()
    {
        yield return null; // Wait for Awake

        _controller.StartGame();
        yield return null; // Tick processes Spawning -> Playing

        var activePiece = _controller.ActivePiece;
        Assert.IsTrue(Enum.IsDefined(typeof(PieceType), activePiece.Type), "Active piece should have valid type");
    }

    [UnityTest]
    public IEnumerator GameplayController_NextPiece_ReturnsValidType()
    {
        yield return null; // Wait for Awake

        var nextPiece = _controller.NextPiece;
        Assert.IsTrue(Enum.IsDefined(typeof(PieceType), nextPiece), "NextPiece should be valid PieceType");
    }

    [UnityTest]
    public IEnumerator GameplayController_OnStateChanged_FiresOnStartGame()
    {
        yield return null; // Wait for Awake

        int stateChangeCount = 0;
        _controller.OnStateChanged += () => { stateChangeCount++; };

        _controller.StartGame();
        yield return null; // Tick processes Spawning -> Playing

        Assert.Greater(stateChangeCount, 0, "OnStateChanged should fire at least once during StartGame");
    }

    [UnityTest]
    public IEnumerator GameplayController_Playfield_ClearsOnStartGame()
    {
        yield return null; // Wait for Awake

        // Fill the playfield manually
        for (int row = 0; row < GameRules.PLAYFIELD_TOTAL_HEIGHT; row++)
        {
            for (int col = 0; col < GameRules.PLAYFIELD_WIDTH; col++)
            {
                _controller.Playfield.SetCell(row, col, 1);
            }
        }

        // Verify it's filled
        Assert.AreEqual(1, _controller.Playfield.GetCell(10, 5), "Playfield should be filled");

        // Start game should clear it
        _controller.StartGame();
        yield return null;

        Assert.AreEqual(0, _controller.Playfield.GetCell(10, 5), "Playfield should be cleared after StartGame");
    }

    [UnityTest]
    public IEnumerator GameplayController_Tick_DoesNotThrowWithoutStartGame()
    {
        yield return null; // Wait for Awake

        // Tick without starting game (Idle state) should not throw
        bool threw = false;
        try
        {
            for (int i = 0; i < 100; i++)
            {
                _controller.Tick();
            }
        }
        catch
        {
            threw = true;
        }

        Assert.IsFalse(threw, "Tick in Idle state should not throw");
    }

    [UnityTest]
    public IEnumerator GameplayController_ExtendedPlay_NoExceptions()
    {
        yield return null; // Wait for Awake

        _controller.StartGame();

        // Simulate many ticks with soft drop (fast fall)
        for (int i = 0; i < 500; i++)
        {
            _controller.Tick();
            yield return null; // Process each frame
        }

        Assert.IsTrue(true, "Game ran 500 ticks without exceptions");
    }

    [UnityTest]
    public IEnumerator GameplayController_LevelDerivedFromLinesCleared()
    {
        yield return null; // Wait for Awake

        // Level is derived from TotalLinesCleared via GameRules.GetLevel()
        // At 0 lines, level should be 0
        Assert.AreEqual(0, _controller.CurrentLevel, "Level should be 0 with 0 lines cleared");

        // Verify the level calculation is correct by checking GameRules
        Assert.AreEqual(0, GameRules.GetLevel(0), "0 lines -> level 0");
        Assert.AreEqual(0, GameRules.GetLevel(9), "9 lines -> level 0");
        Assert.AreEqual(1, GameRules.GetLevel(10), "10 lines -> level 1");
        Assert.AreEqual(2, GameRules.GetLevel(20), "20 lines -> level 2");
        Assert.AreEqual(5, GameRules.GetLevel(50), "50 lines -> level 5");
    }

    [UnityTest]
    public IEnumerator GameplayController_OnGameOver_FiresWhenSpawnBlocked()
    {
        yield return null; // Wait for Awake

        _controller.StartGame();
        yield return null; // Spawn first piece

        // Fill the entire playfield to block spawning
        for (int row = 0; row < GameRules.PLAYFIELD_TOTAL_HEIGHT; row++)
        {
            for (int col = 0; col < GameRules.PLAYFIELD_WIDTH; col++)
            {
                _controller.Playfield.SetCell(row, col, 1);
            }
        }

        int gameOverScore = -1;
        _controller.OnGameOver += (score) => { gameOverScore = score; };

        // Tick until game over
        for (int i = 0; i < 10000; i++)
        {
            _controller.Tick();
            yield return null;
            if (gameOverScore >= 0) break;
        }

        Assert.GreaterOrEqual(gameOverScore, 0, "OnGameOver should fire with a score");
    }

    [UnityTest]
    public IEnumerator GameplayController_MultipleStartGames_ResetProperly()
    {
        yield return null; // Wait for Awake

        // First game
        _controller.StartGame();
        yield return null;

        // Tick a bit
        for (int i = 0; i < 100; i++)
        {
            _controller.Tick();
            yield return null;
        }

        int scoreAfterFirst = _controller.CurrentScore;

        // Start a new game
        _controller.StartGame();
        yield return null;

        Assert.AreEqual(0, _controller.CurrentScore, "Score should reset to 0 on new game");
        Assert.AreEqual(0, _controller.TotalLinesCleared, "Lines should reset to 0 on new game");
        Assert.AreEqual(0, _controller.CurrentLevel, "Level should reset to 0 on new game");
    }
}
