using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Game.Gameplay;
using Game.Systems;

public class AudioControllerTests
{
    private GameObject _controllerGO;
    private GameObject _audioGO;
    private GameplayController _gameplayController;
    private AudioController _audioController;

    [SetUp]
    public void SetUp()
    {
        _controllerGO = new GameObject();
        _gameplayController = _controllerGO.AddComponent<GameplayController>();

        _audioGO = new GameObject();
        _audioGO.AddComponent<AudioListener>();
        _audioGO.AddComponent<AudioSource>();
        _audioGO.AddComponent<AudioSource>();
        _audioController = _audioGO.AddComponent<AudioController>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(_controllerGO);
        Object.Destroy(_audioGO);
    }

    [UnityTest]
    public IEnumerator AudioController_CanBeInstantiated_WithoutErrors()
    {
        yield return null;
        Assert.IsNotNull(_audioController);
    }

    [UnityTest]
    public IEnumerator AudioController_WithNullGameplayController_DoesNotThrow()
    {
        // gameplayController serialized field is unassigned -- Start() must return early without error
        yield return null;
        Assert.IsNotNull(_audioController,
            "AudioController should initialize without error when GameplayController is not assigned");
    }

    [UnityTest]
    public IEnumerator GameplayController_OnPieceMoved_CanBeSubscribedTo()
    {
        yield return null;
        bool fired = false;
        _gameplayController.OnPieceMoved += () => fired = true;
        Assert.IsFalse(fired, "OnPieceMoved should not have fired before any moves");
        _gameplayController.OnPieceMoved -= () => fired = true;
    }

    [UnityTest]
    public IEnumerator GameplayController_OnPieceLocked_CanBeSubscribedTo()
    {
        yield return null;
        bool fired = false;
        _gameplayController.OnPieceLocked += _ => fired = true;
        Assert.IsFalse(fired, "OnPieceLocked should not have fired before game start");
        _gameplayController.OnPieceLocked -= _ => fired = true;
    }

    [UnityTest]
    public IEnumerator GameplayController_OnLinesCleared_CanBeSubscribedTo()
    {
        yield return null;
        bool fired = false;
        _gameplayController.OnLinesCleared += _ => fired = true;
        Assert.IsFalse(fired, "OnLinesCleared should not have fired before game start");
        _gameplayController.OnLinesCleared -= _ => fired = true;
    }

    [UnityTest]
    public IEnumerator GameplayController_OnPieceLocked_FiresDuringGameplay()
    {
        yield return null;
        _gameplayController.StartGame();

        int lockCount = 0;
        _gameplayController.OnPieceLocked += _ => lockCount++;

        // Block row 3 so the piece lands at row 2 and locks within ~96 ticks.
        for (int c = 1; c <= 9; c++)
            _gameplayController.Playfield.SetCell(3, c, 1);

        for (int i = 0; i < 200; i++)
            _gameplayController.Tick();

        Assert.Greater(lockCount, 0,
            "OnPieceLocked should fire at least once after pieces fall and lock during gameplay");
    }

    [UnityTest]
    public IEnumerator GameplayController_OnLinesCleared_FiresWhenLineCompleted()
    {
        yield return null;
        _gameplayController.StartGame();

        int clearedCount = 0;
        _gameplayController.OnLinesCleared += count => clearedCount += count;

        // Pre-fill row 21 (bottom) completely. When the first piece falls and locks at
        // row 20 (blocked by row 21), ClearLines fires OnLinesCleared(1).
        for (int c = 0; c < GameRules.PLAYFIELD_WIDTH; c++)
            _gameplayController.Playfield.SetCell(GameRules.PLAYFIELD_TOTAL_HEIGHT - 1, c, 1);

        for (int i = 0; i < 2000; i++)
            _gameplayController.Tick();

        Assert.Greater(clearedCount, 0,
            "OnLinesCleared should fire with a positive count when a pre-filled line is cleared");
    }

    [UnityTest]
    public IEnumerator GameplayController_CurrentState_IsIdle_BeforeStart()
    {
        yield return null;
        Assert.AreEqual("Idle", _gameplayController.CurrentState,
            "CurrentState should be Idle before StartGame is called");
    }

    [UnityTest]
    public IEnumerator GameplayController_CurrentState_IsNotIdle_AfterStartGame()
    {
        yield return null;
        _gameplayController.StartGame();
        Assert.AreNotEqual("Idle", _gameplayController.CurrentState,
            "CurrentState should not be Idle after StartGame is called");
    }

    [UnityTest]
    public IEnumerator GameplayController_CurrentState_IsIdle_AfterStopGame()
    {
        yield return null;
        _gameplayController.StartGame();
        _gameplayController.StopGame();
        Assert.AreEqual("Idle", _gameplayController.CurrentState,
            "CurrentState should return to Idle after StopGame is called");
    }
}
