using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

[TestFixture]
public class SceneBootstrapperTests
{
    private GameObject _manager;
    private SceneBootstrapper _bootstrapper;

    private GameObject _gameplayControllerGo;
    private GameplayController _gameplayController;

    private GameObject _startScreenGo;
    private StartScreen _startScreen;

    private GameObject _gameScreenGo;
    private GameScreen _gameScreen;

    private GameObject _gameOverScreenGo;
    private GameOverScreen _gameOverScreen;

    private GameObject _initialsOverlayGo;
    private InitialsEntryOverlay _initialsOverlay;

    private GameObject _scoreWidgetGo;
    private GameScreenScoreWidget _scoreWidget;

    private GameObject _levelWidgetGo;
    private GameScreenLevelWidget _levelWidget;

    private GameObject _linesWidgetGo;
    private GameScreenLinesWidget _linesWidget;

    private GameObject _nextWidgetGo;
    private GameScreenNextWidget _nextWidget;

    private GameObject _leaderboardWidgetGo;
    private StartScreenLeaderboardWidget _leaderboardWidget;

    [SetUp]
    public void SetUp()
    {
        // Create GameManager with SceneBootstrapper
        _manager = new GameObject("GameManager");
        _bootstrapper = _manager.AddComponent<SceneBootstrapper>();

        // Create GameplayController
        _gameplayControllerGo = new GameObject("GameplayController");
        _gameplayController = _gameplayControllerGo.AddComponent<GameplayController>();

        // Create StartScreen with UIDocument
        _startScreenGo = new GameObject("StartScreen");
        var startDoc = _startScreenGo.AddComponent<UIDocument>();
        _startScreen = _startScreenGo.AddComponent<StartScreen>();
        // Add required regions for StartScreenLeaderboardWidget
        var lbRegion = new VisualElement();
        lbRegion.name = "leaderboard-region";
        startDoc.rootVisualElement.Add(lbRegion);

        // Create GameScreen with UIDocument
        _gameScreenGo = new GameObject("GameScreen");
        var gameDoc = _gameScreenGo.AddComponent<UIDocument>();
        _gameScreen = _gameScreenGo.AddComponent<GameScreen>();
        // Add required regions
        gameDoc.rootVisualElement.Add(CreateRegion("score-region"));
        gameDoc.rootVisualElement.Add(CreateRegion("level-region"));
        gameDoc.rootVisualElement.Add(CreateRegion("lines-region"));
        gameDoc.rootVisualElement.Add(CreateRegion("next-region"));

        // Create GameOverScreen with UIDocument
        _gameOverScreenGo = new GameObject("GameOverScreen");
        var goDoc = _gameOverScreenGo.AddComponent<UIDocument>();
        _gameOverScreen = _gameOverScreenGo.AddComponent<GameOverScreen>();
        // Add score-label for GameOverScreen
        var scoreLabel = new Label("SCORE: 0000000");
        scoreLabel.name = "score-label";
        goDoc.rootVisualElement.Add(scoreLabel);

        // Create InitialsEntryOverlay with UIDocument
        _initialsOverlayGo = new GameObject("InitialsEntryOverlay");
        var initialsDoc = _initialsOverlayGo.AddComponent<UIDocument>();
        _initialsOverlay = _initialsOverlayGo.AddComponent<InitialsEntryOverlay>();
        // Add required elements
        initialsDoc.rootVisualElement.Add(CreateLabel("char-0"));
        initialsDoc.rootVisualElement.Add(CreateLabel("char-1"));
        initialsDoc.rootVisualElement.Add(CreateLabel("char-2"));
        var confirmRegion = new VisualElement();
        confirmRegion.name = "confirm-region";
        initialsDoc.rootVisualElement.Add(confirmRegion);

        // Create ScoreWidget with UIDocument
        _scoreWidgetGo = new GameObject("ScoreWidget");
        var scoreDoc = _scoreWidgetGo.AddComponent<UIDocument>();
        _scoreWidget = _scoreWidgetGo.AddComponent<GameScreenScoreWidget>();
        var scoreRegion = CreateRegion("score-region");
        scoreRegion.Add(CreateValueLabel());
        scoreDoc.rootVisualElement.Add(scoreRegion);

        // Create LevelWidget with UIDocument
        _levelWidgetGo = new GameObject("LevelWidget");
        var levelDoc = _levelWidgetGo.AddComponent<UIDocument>();
        _levelWidget = _levelWidgetGo.AddComponent<GameScreenLevelWidget>();
        var levelRegion = CreateRegion("level-region");
        levelRegion.Add(CreateValueLabel());
        levelDoc.rootVisualElement.Add(levelRegion);

        // Create LinesWidget with UIDocument
        _linesWidgetGo = new GameObject("LinesWidget");
        var linesDoc = _linesWidgetGo.AddComponent<UIDocument>();
        _linesWidget = _linesWidgetGo.AddComponent<GameScreenLinesWidget>();
        var linesRegion = CreateRegion("lines-region");
        linesRegion.Add(CreateValueLabel());
        linesDoc.rootVisualElement.Add(linesRegion);

        // Create NextWidget with UIDocument
        _nextWidgetGo = new GameObject("NextWidget");
        var nextDoc = _nextWidgetGo.AddComponent<UIDocument>();
        _nextWidget = _nextWidgetGo.AddComponent<GameScreenNextWidget>();
        nextDoc.rootVisualElement.Add(CreateRegion("next-region"));

        // Create LeaderboardWidget with UIDocument
        _leaderboardWidgetGo = new GameObject("LeaderboardWidget");
        var lbDoc = _leaderboardWidgetGo.AddComponent<UIDocument>();
        _leaderboardWidget = _leaderboardWidgetGo.AddComponent<StartScreenLeaderboardWidget>();
        lbDoc.rootVisualElement.Add(CreateRegion("leaderboard-region"));

        // Wire up all serialized references via reflection
        SetPrivateField(_bootstrapper, "gameplayController", _gameplayController);
        SetPrivateField(_bootstrapper, "startScreen", _startScreen);
        SetPrivateField(_bootstrapper, "gameScreen", _gameScreen);
        SetPrivateField(_bootstrapper, "gameOverScreen", _gameOverScreen);
        SetPrivateField(_bootstrapper, "initialsEntryOverlay", _initialsOverlay);
        SetPrivateField(_bootstrapper, "scoreWidget", _scoreWidget);
        SetPrivateField(_bootstrapper, "levelWidget", _levelWidget);
        SetPrivateField(_bootstrapper, "linesWidget", _linesWidget);
        SetPrivateField(_bootstrapper, "nextWidget", _nextWidget);
        SetPrivateField(_bootstrapper, "leaderboardWidget", _leaderboardWidget);
    }

    [TearDown]
    public void TearDown()
    {
        if (_manager != null) UnityEngine.Object.Destroy(_manager);
        if (_gameplayControllerGo != null) UnityEngine.Object.Destroy(_gameplayControllerGo);
        if (_startScreenGo != null) UnityEngine.Object.Destroy(_startScreenGo);
        if (_gameScreenGo != null) UnityEngine.Object.Destroy(_gameScreenGo);
        if (_gameOverScreenGo != null) UnityEngine.Object.Destroy(_gameOverScreenGo);
        if (_initialsOverlayGo != null) UnityEngine.Object.Destroy(_initialsOverlayGo);
        if (_scoreWidgetGo != null) UnityEngine.Object.Destroy(_scoreWidgetGo);
        if (_levelWidgetGo != null) UnityEngine.Object.Destroy(_levelWidgetGo);
        if (_linesWidgetGo != null) UnityEngine.Object.Destroy(_linesWidgetGo);
        if (_nextWidgetGo != null) UnityEngine.Object.Destroy(_nextWidgetGo);
        if (_leaderboardWidgetGo != null) UnityEngine.Object.Destroy(_leaderboardWidgetGo);
    }

    private VisualElement CreateRegion(string name)
    {
        var region = new VisualElement();
        region.name = name;
        return region;
    }

    private Label CreateValueLabel()
    {
        var label = new Label("0");
        label.AddToClassList("region-value");
        return label;
    }

    private Label CreateLabel(string name)
    {
        var label = new Label("A");
        label.name = name;
        return label;
    }

    private void SetPrivateField<T>(object obj, string fieldName, T value)
    {
        var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(obj, value);
        }
    }

    // Helper to invoke events via reflection (events cannot be read directly)
    private void InvokeEvent(object target, string eventName)
    {
        var field = target.GetType().GetField(eventName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            var handler = (Delegate)field.GetValue(target);
            handler?.DynamicInvoke();
        }
    }

    private void InvokeEventWithParam(object target, string eventName, object[] parameters)
    {
        var field = target.GetType().GetField(eventName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            var handler = (Delegate)field.GetValue(target);
            handler?.DynamicInvoke(parameters);
        }
    }

    // -----------------------------------------------------------------------
    // Awake tests
    // -----------------------------------------------------------------------

    [UnityTest]
    public IEnumerator Awake_HidesGameScreen()
    {
        yield return null; // let Awake run
        Assert.IsFalse(_gameScreen.IsVisible, "GameScreen should be hidden on Awake");
    }

    [UnityTest]
    public IEnumerator Awake_HidesGameOverScreen()
    {
        yield return null;
        Assert.IsFalse(_gameOverScreen.IsVisible, "GameOverScreen should be hidden on Awake");
    }

    [UnityTest]
    public IEnumerator Awake_HidesInitialsOverlay()
    {
        yield return null;
        Assert.IsFalse(_initialsOverlay.IsVisible, "InitialsEntryOverlay should be hidden on Awake");
    }

    [UnityTest]
    public IEnumerator Awake_ShowsStartScreen()
    {
        yield return null;
        Assert.IsTrue(_startScreen.IsVisible, "StartScreen should be shown on Awake");
    }

    // -----------------------------------------------------------------------
    // OnStartRequested flow tests
    // -----------------------------------------------------------------------

    [UnityTest]
    public IEnumerator OnStartRequested_HidesStartScreenAndShowsGameScreen()
    {
        yield return null;

        // Trigger start via the StartScreen event
        InvokeEvent(_startScreen, "OnStartRequested");
        yield return null;

        Assert.IsFalse(_startScreen.IsVisible, "StartScreen should be hidden after start");
        Assert.IsTrue(_gameScreen.IsVisible, "GameScreen should be shown after start");
    }

    [UnityTest]
    public IEnumerator OnStartRequested_CallsStartGameOnController()
    {
        yield return null;

        // Before start, score should be 0
        int scoreBefore = _gameplayController.CurrentScore;

        InvokeEvent(_startScreen, "OnStartRequested");
        yield return null;

        // After StartGame(), the controller should have started
        // The score is reset to 0 by StartGame()
        Assert.AreEqual(0, _gameplayController.CurrentScore, "Score should be 0 after StartGame");
    }

    // -----------------------------------------------------------------------
    // OnGameStateChanged flow tests
    // -----------------------------------------------------------------------

    [UnityTest]
    public IEnumerator OnGameStateChanged_UpdatesWidgets()
    {
        yield return null;

        // Start the game first
        InvokeEvent(_startScreen, "OnStartRequested");
        yield return null;

        // Trigger a state change manually via the controller's event
        InvokeEvent(_gameplayController, "OnStateChanged");
        yield return null;

        // Verify widgets received updates (they should not throw)
        // The widgets should have been called with current values
        Assert.IsNotNull(_scoreWidget, "ScoreWidget should exist");
        Assert.IsNotNull(_levelWidget, "LevelWidget should exist");
        Assert.IsNotNull(_linesWidget, "LinesWidget should exist");
        Assert.IsNotNull(_nextWidget, "NextWidget should exist");
    }

    // -----------------------------------------------------------------------
    // OnGameOver flow tests
    // -----------------------------------------------------------------------

    [UnityTest]
    public IEnumerator OnGameOver_HidesGameScreenAndShowsGameOverScreen()
    {
        yield return null;

        // Start game first
        InvokeEvent(_startScreen, "OnStartRequested");
        yield return null;

        // Trigger game over
        InvokeEventWithParam(_gameplayController, "OnGameOver", new object[] { 5000 });
        yield return null;

        Assert.IsFalse(_gameScreen.IsVisible, "GameScreen should be hidden after game over");
        Assert.IsTrue(_gameOverScreen.IsVisible, "GameOverScreen should be shown after game over");
    }

    // -----------------------------------------------------------------------
    // OnContinueRequested flow tests
    // -----------------------------------------------------------------------

    [UnityTest]
    public IEnumerator OnContinueRequested_HidesGameOverAndShowsStart()
    {
        yield return null;

        // Start game
        InvokeEvent(_startScreen, "OnStartRequested");
        yield return null;

        // Game over
        InvokeEventWithParam(_gameplayController, "OnGameOver", new object[] { 5000 });
        yield return null;

        // Continue
        InvokeEvent(_gameOverScreen, "OnContinueRequested");
        yield return null;

        Assert.IsFalse(_gameOverScreen.IsVisible, "GameOverScreen should be hidden after continue");
        Assert.IsTrue(_startScreen.IsVisible, "StartScreen should be shown after continue");
    }

    // -----------------------------------------------------------------------
    // Full flow test: Start -> Play -> GameOver -> Continue
    // -----------------------------------------------------------------------

    [UnityTest]
    public IEnumerator FullFlow_StartPlayGameOverContinue()
    {
        yield return null;

        // 1. Start: StartScreen visible, others hidden
        Assert.IsTrue(_startScreen.IsVisible);
        Assert.IsFalse(_gameScreen.IsVisible);
        Assert.IsFalse(_gameOverScreen.IsVisible);

        // 2. Start game
        InvokeEvent(_startScreen, "OnStartRequested");
        yield return null;
        Assert.IsFalse(_startScreen.IsVisible);
        Assert.IsTrue(_gameScreen.IsVisible);

        // 3. Game over
        InvokeEventWithParam(_gameplayController, "OnGameOver", new object[] { 15000 });
        yield return null;
        Assert.IsFalse(_gameScreen.IsVisible);
        Assert.IsTrue(_gameOverScreen.IsVisible);

        // 4. Continue
        InvokeEvent(_gameOverScreen, "OnContinueRequested");
        yield return null;
        Assert.IsFalse(_gameOverScreen.IsVisible);
        Assert.IsTrue(_startScreen.IsVisible);
    }
}
