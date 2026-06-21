using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.TestTools;
using Game.Gameplay;

public class SceneBootstrapperTests
{
    private GameObject _bootGo;
    private SceneBootstrapper _bootstrapper;

    private GameObject _gameplayGo;
    private GameplayController _gameplayController;

    private GameObject _leaderboardGo;
    private LeaderboardService _leaderboardService;

    private GameObject _startScreenGo;
    private StartScreen _startScreen;
    private PanelSettings _startPanel;

    private GameObject _gameScreenGo;
    private GameScreen _gameScreen;
    private PanelSettings _gamePanel;

    private GameObject _gameOverGo;
    private GameOverScreen _gameOverScreen;
    private PanelSettings _gameOverPanel;

    private GameObject _initialsGo;
    private InitialsEntryOverlay _initialsOverlay;
    private PanelSettings _initialsPanel;

    private GameObject _scoreWidgetGo;
    private GameScreenScoreWidget _scoreWidget;
    private PanelSettings _scorePanel;

    private GameObject _levelWidgetGo;
    private GameScreenLevelWidget _levelWidget;
    private PanelSettings _levelPanel;

    private GameObject _linesWidgetGo;
    private GameScreenLinesWidget _linesWidget;
    private PanelSettings _linesPanel;

    private GameObject _nextWidgetGo;
    private GameScreenNextWidget _nextWidget;
    private PanelSettings _nextPanel;

    private GameObject _leaderboardWidgetGo;
    private StartScreenLeaderboardWidget _leaderboardWidget;
    private PanelSettings _leaderboardWidgetPanel;

    private GameObject CreateScreenGo<T>(out T component, out PanelSettings panel) where T : MonoBehaviour
    {
        var go = new GameObject();
        go.SetActive(false);
        panel = ScriptableObject.CreateInstance<PanelSettings>();
        var doc = go.AddComponent<UIDocument>();
        doc.panelSettings = panel;
        component = go.AddComponent<T>();
        return go;
    }

    private void SetField(string name, object value)
    {
        var field = typeof(SceneBootstrapper).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(_bootstrapper, value);
    }

    [SetUp]
    public void SetUp()
    {
        _gameplayGo = new GameObject();
        _gameplayGo.SetActive(false);
        _gameplayController = _gameplayGo.AddComponent<GameplayController>();
        _gameplayGo.SetActive(true);

        _leaderboardGo = new GameObject();
        _leaderboardService = _leaderboardGo.AddComponent<LeaderboardService>();

        _startScreenGo = CreateScreenGo<StartScreen>(out _startScreen, out _startPanel);
        _gameScreenGo = CreateScreenGo<GameScreen>(out _gameScreen, out _gamePanel);
        _gameOverGo = CreateScreenGo<GameOverScreen>(out _gameOverScreen, out _gameOverPanel);
        _initialsGo = CreateScreenGo<InitialsEntryOverlay>(out _initialsOverlay, out _initialsPanel);
        _scoreWidgetGo = CreateScreenGo<GameScreenScoreWidget>(out _scoreWidget, out _scorePanel);
        _levelWidgetGo = CreateScreenGo<GameScreenLevelWidget>(out _levelWidget, out _levelPanel);
        _linesWidgetGo = CreateScreenGo<GameScreenLinesWidget>(out _linesWidget, out _linesPanel);
        _nextWidgetGo = CreateScreenGo<GameScreenNextWidget>(out _nextWidget, out _nextPanel);
        _leaderboardWidgetGo = CreateScreenGo<StartScreenLeaderboardWidget>(out _leaderboardWidget, out _leaderboardWidgetPanel);

        _startScreenGo.SetActive(true);
        _gameScreenGo.SetActive(true);
        _gameOverGo.SetActive(true);
        _initialsGo.SetActive(true);
        _scoreWidgetGo.SetActive(true);
        _levelWidgetGo.SetActive(true);
        _linesWidgetGo.SetActive(true);
        _nextWidgetGo.SetActive(true);
        _leaderboardWidgetGo.SetActive(true);

        _bootGo = new GameObject();
        _bootGo.SetActive(false);
        _bootstrapper = _bootGo.AddComponent<SceneBootstrapper>();

        SetField("gameplayController", _gameplayController);
        SetField("leaderboardService", _leaderboardService);
        SetField("startScreen", _startScreen);
        SetField("gameScreen", _gameScreen);
        SetField("gameOverScreen", _gameOverScreen);
        SetField("initialsEntryOverlay", _initialsOverlay);
        SetField("scoreWidget", _scoreWidget);
        SetField("levelWidget", _levelWidget);
        SetField("linesWidget", _linesWidget);
        SetField("nextWidget", _nextWidget);
        SetField("leaderboardWidget", _leaderboardWidget);
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(_bootGo);
        Object.Destroy(_gameplayGo);
        Object.Destroy(_leaderboardGo);
        Object.Destroy(_startScreenGo);
        Object.Destroy(_gameScreenGo);
        Object.Destroy(_gameOverGo);
        Object.Destroy(_initialsGo);
        Object.Destroy(_scoreWidgetGo);
        Object.Destroy(_levelWidgetGo);
        Object.Destroy(_linesWidgetGo);
        Object.Destroy(_nextWidgetGo);
        Object.Destroy(_leaderboardWidgetGo);
        Object.Destroy(_startPanel);
        Object.Destroy(_gamePanel);
        Object.Destroy(_gameOverPanel);
        Object.Destroy(_initialsPanel);
        Object.Destroy(_scorePanel);
        Object.Destroy(_levelPanel);
        Object.Destroy(_linesPanel);
        Object.Destroy(_nextPanel);
        Object.Destroy(_leaderboardWidgetPanel);
    }

    [UnityTest]
    public IEnumerator Awake_GameScreen_StartsHidden()
    {
        _bootGo.SetActive(true);
        yield return null;
        Assert.IsFalse(_gameScreen.IsVisible, "GameScreen should be hidden after Awake");
    }

    [UnityTest]
    public IEnumerator Awake_GameOverScreen_StartsHidden()
    {
        _bootGo.SetActive(true);
        yield return null;
        Assert.IsFalse(_gameOverScreen.IsVisible, "GameOverScreen should be hidden after Awake");
    }

    [UnityTest]
    public IEnumerator Awake_InitialsOverlay_StartsHidden()
    {
        _bootGo.SetActive(true);
        yield return null;
        Assert.IsFalse(_initialsOverlay.IsVisible, "InitialsEntryOverlay should be hidden after Awake");
    }

    [UnityTest]
    public IEnumerator Awake_StartScreen_IsVisible()
    {
        _bootGo.SetActive(true);
        yield return null;
        Assert.IsTrue(_startScreen.IsVisible, "StartScreen should be visible after Awake");
    }

    [UnityTest]
    public IEnumerator StartGame_HidesStartScreen()
    {
        _bootGo.SetActive(true);
        yield return null;
        _bootstrapper.StartGame();
        Assert.IsFalse(_startScreen.IsVisible, "StartGame should hide the StartScreen");
    }

    [UnityTest]
    public IEnumerator StartGame_ShowsGameScreen()
    {
        _bootGo.SetActive(true);
        yield return null;
        _bootstrapper.StartGame();
        Assert.IsTrue(_gameScreen.IsVisible, "StartGame should show the GameScreen");
    }

    [UnityTest]
    public IEnumerator GoToGameOver_HidesGameScreen()
    {
        _bootGo.SetActive(true);
        yield return null;
        _bootstrapper.StartGame();
        _bootstrapper.GoToGameOver();
        Assert.IsFalse(_gameScreen.IsVisible, "GoToGameOver should hide the GameScreen");
    }

    [UnityTest]
    public IEnumerator GoToGameOver_ShowsGameOverScreen()
    {
        _bootGo.SetActive(true);
        yield return null;
        _bootstrapper.GoToGameOver();
        Assert.IsTrue(_gameOverScreen.IsVisible, "GoToGameOver should show the GameOverScreen");
    }

    [UnityTest]
    public IEnumerator GoToStart_ShowsStartScreen()
    {
        _bootGo.SetActive(true);
        yield return null;
        _bootstrapper.GoToGameOver();
        _bootstrapper.GoToStart();
        Assert.IsTrue(_startScreen.IsVisible, "GoToStart should show the StartScreen");
    }

    [UnityTest]
    public IEnumerator GoToStart_HidesGameOverScreen()
    {
        _bootGo.SetActive(true);
        yield return null;
        _bootstrapper.GoToGameOver();
        _bootstrapper.GoToStart();
        Assert.IsFalse(_gameOverScreen.IsVisible, "GoToStart should hide the GameOverScreen");
    }

    [UnityTest]
    public IEnumerator Awake_WithNullDependencies_DoesNotThrow()
    {
        var nullBootGo = new GameObject();
        nullBootGo.SetActive(false);
        var nullBootstrapper = nullBootGo.AddComponent<SceneBootstrapper>();
        Assert.DoesNotThrow(() => nullBootGo.SetActive(true));
        yield return null;
        Object.Destroy(nullBootGo);
    }

    [UnityTest]
    public IEnumerator StartGame_WithNullGameplayController_DoesNotThrow()
    {
        SetField("gameplayController", null);
        _bootGo.SetActive(true);
        yield return null;
        Assert.DoesNotThrow(() => _bootstrapper.StartGame());
    }

    [UnityTest]
    public IEnumerator Continue_FromGameOver_ShowsStartScreen()
    {
        _bootGo.SetActive(true);
        yield return null;
        _bootstrapper.StartGame();
        _bootstrapper.GoToGameOver();
        _bootstrapper.GoToStart();
        for (int i = 0; i < 10; i++)
            yield return null;
        Assert.IsTrue(_startScreen.IsVisible, "StartScreen should be visible after GoToStart");
        Assert.IsFalse(_gameOverScreen.IsVisible, "GameOverScreen should be hidden after GoToStart");
    }

    [UnityTest]
    public IEnumerator GameOver_NonQualifying_InitialsOverlayNotShown()
    {
        _bootGo.SetActive(true);
        yield return null;
        _bootstrapper.GoToGameOver();
        // Simulate overlay being shown (as happens when score qualifies for the leaderboard)
        _initialsOverlay.ShowForScore(0);
        _bootstrapper.GoToStart();
        Assert.IsFalse(_initialsOverlay.IsVisible, "InitialsEntryOverlay should be hidden after GoToStart");
    }

    [UnityTest]
    public IEnumerator InitialsEntry_ConfirmSubmit_HidesOverlay()
    {
        _bootGo.SetActive(true);
        yield return null;

        _bootstrapper.StartGame();

        // Fill row 1 cols 1-9 so the running controller will game-over at ~96 frames if StopGame is not called
        for (int c = 1; c <= 9; c++)
            _gameplayController.Playfield.SetCell(1, c, 1);

        _initialsOverlay.ShowForScore(100);

        // Submit initials (four AdvanceSlot calls simulate SOFT_DROP on CONFIRM)
        _initialsOverlay.AdvanceSlot();
        _initialsOverlay.AdvanceSlot();
        _initialsOverlay.AdvanceSlot();
        _initialsOverlay.AdvanceSlot();

        // Track any OnGameOver events that fire after submit -- there must be none
        int postSubmitGameOverCount = 0;
        _gameplayController.OnGameOver += _ => postSubmitGameOverCount++;

        // Wait enough frames for a still-running controller to game-over (~96 frames at level 0)
        for (int i = 0; i < 200; i++)
            yield return null;

        Assert.AreEqual(0, postSubmitGameOverCount,
            "GameplayController must not fire OnGameOver after submit -- StopGame was not called in OnInitialsSubmitted");
        Assert.IsFalse(_initialsOverlay.IsVisible,
            "InitialsEntryOverlay should remain hidden after submit");
    }
}
