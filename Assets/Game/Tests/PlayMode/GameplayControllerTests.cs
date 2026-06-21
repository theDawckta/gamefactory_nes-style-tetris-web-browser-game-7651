using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Game.Gameplay;

public class GameplayControllerTests
{
    private GameObject _go;
    private GameplayController _controller;

    [SetUp]
    public void SetUp()
    {
        _go = new GameObject();
        _controller = _go.AddComponent<GameplayController>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(_go);
    }

    [UnityTest]
    public IEnumerator StartGame_ResetsScore_ToZero()
    {
        yield return null;
        _controller.StartGame();
        Assert.AreEqual(0, _controller.CurrentScore);
    }

    [UnityTest]
    public IEnumerator StartGame_ResetsTotalLinesCleared_ToZero()
    {
        yield return null;
        _controller.StartGame();
        Assert.AreEqual(0, _controller.TotalLinesCleared);
    }

    [UnityTest]
    public IEnumerator StartGame_ResetsCurrentLevel_ToZero()
    {
        yield return null;
        _controller.StartGame();
        Assert.AreEqual(0, _controller.CurrentLevel);
    }

    [UnityTest]
    public IEnumerator StartGame_ClearsPlayfield()
    {
        yield return null;
        // Pre-fill some cells then start game
        _controller.StartGame();
        for (int r = 0; r < 22; r++)
            for (int c = 0; c < 10; c++)
                _controller.Playfield.SetCell(r, c, 1);

        _controller.StartGame();

        // Active piece is not in grid; all grid cells must be zero
        for (int r = 0; r < 22; r++)
            for (int c = 0; c < 10; c++)
                Assert.AreEqual(0, _controller.Playfield.GetCell(r, c),
                    $"Cell ({r},{c}) should be 0 after StartGame clears the playfield");
    }

    [UnityTest]
    public IEnumerator StartGame_ExposesPlayfield_NotNull()
    {
        yield return null;
        _controller.StartGame();
        Assert.IsNotNull(_controller.Playfield);
    }

    [UnityTest]
    public IEnumerator StartGame_NextPiece_IsValidPieceType()
    {
        yield return null;
        _controller.StartGame();
        PieceType next = _controller.NextPiece;
        Assert.IsTrue(System.Enum.IsDefined(typeof(PieceType), next));
    }

    [UnityTest]
    public IEnumerator OnStateChanged_FiresOnStartGame()
    {
        yield return null;
        int fireCount = 0;
        _controller.OnStateChanged += () => fireCount++;
        _controller.StartGame();
        Assert.Greater(fireCount, 0, "OnStateChanged should fire at least once when StartGame() is called");
    }

    [UnityTest]
    public IEnumerator OnGameOver_FiresWithFinalScore_WhenSpawnBlocked()
    {
        // Awake() runs after AddComponent; wait one frame to ensure it completes
        yield return null;

        int gameOverScore = -1;
        _controller.OnGameOver += score => gameOverScore = score;

        _controller.StartGame();

        // Fill row 1, cols 1-9 (not col 0 so it is never a full row and never cleared).
        // This blocks all piece types from moving into row 1 on the first gravity tick.
        // The active piece is already spawned (not in the grid yet), so this prefill
        // does not interfere with the initial spawn check.
        for (int c = 1; c <= 9; c++)
            _controller.Playfield.SetCell(1, c, 1);

        // Wait enough frames for gravity to land the piece (48 frames at level 0)
        // and then lock it (48 more frames). 200 frames is well above the 96 needed.
        for (int i = 0; i < 200; i++)
            yield return null;

        Assert.AreEqual(0, gameOverScore,
            "OnGameOver should fire with the final score (0 since no lines were cleared)");
    }

    [UnityTest]
    public IEnumerator GameLoop_NoExceptionAfterManyTicks()
    {
        yield return null;
        _controller.StartGame();

        // Simulate 60 seconds at 60 FPS = 3600 ticks. No exception must be thrown.
        Assert.DoesNotThrow(() =>
        {
            for (int i = 0; i < 3600; i++)
                _controller.Tick();
        });
    }

    [UnityTest]
    public IEnumerator StartGame_CalledTwice_ResetsPreviousState()
    {
        yield return null;
        _controller.StartGame();

        // Let a piece fall partially
        for (int i = 0; i < 50; i++)
            _controller.Tick();

        // Second StartGame must reset everything
        _controller.StartGame();
        Assert.AreEqual(0, _controller.CurrentScore);
        Assert.AreEqual(0, _controller.TotalLinesCleared);
        Assert.AreEqual(0, _controller.CurrentLevel);
    }

    [UnityTest]
    public IEnumerator StopGame_PreventsOnGameOverFromFiring()
    {
        yield return null;
        _controller.StartGame();

        int gameOverCount = 0;
        _controller.OnGameOver += _ => gameOverCount++;

        _controller.StopGame();

        for (int i = 0; i < 200; i++)
            _controller.Tick();

        Assert.AreEqual(0, gameOverCount, "StopGame should halt the game loop so OnGameOver does not fire");
    }
}
