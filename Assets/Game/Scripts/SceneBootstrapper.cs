using System;
using UnityEngine;

/// <summary>
/// MonoBehaviour that lives on the GameManager GameObject in Main.unity.
/// Wires together all gameplay systems and UI screens into the full game flow.
/// Manages screen transitions, leaderboard fetching, and game-over qualification logic.
/// </summary>
public class SceneBootstrapper : MonoBehaviour
{
    // -----------------------------------------------------------------------
    // Serialized references (assigned in Inspector)
    // -----------------------------------------------------------------------

    [SerializeField, Tooltip("Reference to the GameplayController component.")]
    private GameplayController gameplayController;

    [SerializeField, Tooltip("Reference to the StartScreen component.")]
    private StartScreen startScreen;

    [SerializeField, Tooltip("Reference to the GameScreen component.")]
    private GameScreen gameScreen;

    [SerializeField, Tooltip("Reference to the GameOverScreen component.")]
    private GameOverScreen gameOverScreen;

    [SerializeField, Tooltip("Reference to the InitialsEntryOverlay component.")]
    private InitialsEntryOverlay initialsEntryOverlay;

    [SerializeField, Tooltip("Reference to the GameScreenScoreWidget component.")]
    private GameScreenScoreWidget scoreWidget;

    [SerializeField, Tooltip("Reference to the GameScreenLevelWidget component.")]
    private GameScreenLevelWidget levelWidget;

    [SerializeField, Tooltip("Reference to the GameScreenLinesWidget component.")]
    private GameScreenLinesWidget linesWidget;

    [SerializeField, Tooltip("Reference to the GameScreenNextWidget component.")]
    private GameScreenNextWidget nextWidget;

    [SerializeField, Tooltip("Reference to the StartScreenLeaderboardWidget component.")]
    private StartScreenLeaderboardWidget leaderboardWidget;

    // -----------------------------------------------------------------------
    // Internal state
    // -----------------------------------------------------------------------

    /// <summary>
    /// The score from the most recent game over, used during initials entry flow.
    /// </summary>
    private int _pendingScore;

    /// <summary>
    /// Whether the current game over score qualifies for the leaderboard.
    /// If false, the initials overlay is not shown and the player waits for continue.
    /// </summary>
    private bool _scoreQualifies;

    private bool _initialized;

    // -----------------------------------------------------------------------
    // Unity lifecycle
    // -----------------------------------------------------------------------

    private void Start()
    {
        if (_initialized) return;
        _initialized = true;

        // Validate required references to prevent NullReferenceException
        ValidateReferences();

        // Hide all screens except start screen
        if (gameScreen != null) gameScreen.Hide();
        if (gameOverScreen != null) gameOverScreen.Hide();
        if (initialsEntryOverlay != null) initialsEntryOverlay.Hide();

        // Show start screen
        if (startScreen != null) startScreen.Show();

        // Fetch leaderboard on startup
        LeaderboardService.GetScores(OnScoresFetched, OnScoresError);

        // Wire up all event handlers
        WireEvents();
    }

    /// <summary>
    /// Validates that all serialized references are assigned.
    /// Logs errors for any null references instead of throwing NullReferenceException.
    /// </summary>
    private void ValidateReferences()
    {
        if (gameplayController == null) Debug.LogWarning("[SceneBootstrapper] gameplayController is not assigned!");
        if (startScreen == null) Debug.LogWarning("[SceneBootstrapper] startScreen is not assigned!");
        if (gameScreen == null) Debug.LogWarning("[SceneBootstrapper] gameScreen is not assigned!");
        if (gameOverScreen == null) Debug.LogWarning("[SceneBootstrapper] gameOverScreen is not assigned!");
        if (initialsEntryOverlay == null) Debug.LogWarning("[SceneBootstrapper] initialsEntryOverlay is not assigned!");
        if (scoreWidget == null) Debug.LogWarning("[SceneBootstrapper] scoreWidget is not assigned!");
        if (levelWidget == null) Debug.LogWarning("[SceneBootstrapper] levelWidget is not assigned!");
        if (linesWidget == null) Debug.LogWarning("[SceneBootstrapper] linesWidget is not assigned!");
        if (nextWidget == null) Debug.LogWarning("[SceneBootstrapper] nextWidget is not assigned!");
        if (leaderboardWidget == null) Debug.LogWarning("[SceneBootstrapper] leaderboardWidget is not assigned!");
    }

    // -----------------------------------------------------------------------
    // Event wiring
    // -----------------------------------------------------------------------

    private void WireEvents()
    {
        if (startScreen != null) startScreen.OnStartRequested += OnStartRequested;
        if (gameplayController != null)
        {
            gameplayController.OnStateChanged += OnGameStateChanged;
            gameplayController.OnGameOver += OnGameOver;
        }
        if (gameOverScreen != null) gameOverScreen.OnContinueRequested += OnContinueRequested;
        if (initialsEntryOverlay != null) initialsEntryOverlay.OnInitialsSubmitted += OnInitialsSubmitted;
    }

    // -----------------------------------------------------------------------
    // Flow methods
    // -----------------------------------------------------------------------

    /// <summary>
    /// Called when the player presses Enter/Space on the Start Screen.
    /// Hides the start screen, shows the game screen, and starts gameplay.
    /// </summary>
    private void OnStartRequested()
    {
        if (startScreen != null) startScreen.Hide();
        if (gameScreen != null) gameScreen.Show();
        if (gameplayController != null) gameplayController.StartGame();
    }

    /// <summary>
    /// Called whenever the GameplayController's state changes.
    /// Updates all HUD widgets with current game state.
    /// </summary>
    private void OnGameStateChanged()
    {
        if (gameplayController == null) return;
        if (scoreWidget != null) scoreWidget.UpdateScore(gameplayController.CurrentScore);
        if (levelWidget != null) levelWidget.UpdateLevel(gameplayController.CurrentLevel);
        if (linesWidget != null) linesWidget.UpdateLines(gameplayController.TotalLinesCleared);
        if (nextWidget != null) nextWidget.UpdateNextPiece(gameplayController.NextPiece);
    }

    /// <summary>
    /// Called when the game ends.
    /// Hides the game screen, shows the game over screen with the final score,
    /// and fetches the leaderboard to check if the score qualifies.
    /// </summary>
    private void OnGameOver(int score)
    {
        _pendingScore = score;
        _scoreQualifies = false;

        if (gameScreen != null) gameScreen.Hide();
        if (gameOverScreen != null) gameOverScreen.ShowWithScore(score);

        // Fetch leaderboard to check qualification
        LeaderboardService.GetScores(
            entries => CheckQualification(score, entries),
            err =>
            {
                // On error, show game over screen without initials flow
                Debug.LogWarning("[SceneBootstrapper] Could not fetch leaderboard for qualification check: " + err);
                _scoreQualifies = false;
            });
    }

    /// <summary>
    /// Determines whether the given score qualifies for the leaderboard.
    /// If it qualifies, shows the initials entry overlay.
    /// If not, the player simply waits for continue on the game over screen.
    /// </summary>
    private void CheckQualification(int score, LeaderboardEntry[] entries)
    {
        bool qualifies = entries.Length < 5 || score > entries[entries.Length - 1].Score;

        if (qualifies)
        {
            _scoreQualifies = true;
            if (initialsEntryOverlay != null) initialsEntryOverlay.ShowForScore(score);
        }
        else
        {
            _scoreQualifies = false;
        }
    }

    /// <summary>
    /// Called when the player presses Enter/Space on the Game Over Screen.
    /// Hides the game over screen, shows the start screen, and refreshes the leaderboard.
    /// </summary>
    private void OnContinueRequested()
    {
        if (gameOverScreen != null) gameOverScreen.Hide();
        if (startScreen != null) startScreen.Show();
        LeaderboardService.GetScores(OnScoresFetched, OnScoresError);
    }

    /// <summary>
    /// Called when the player submits initials via the InitialsEntryOverlay.
    /// Posts the score to the server, then hides overlays and returns to start screen.
    /// </summary>
    private void OnInitialsSubmitted(string name, int score)
    {
        LeaderboardService.PostScore(name, score, OnScoresPosted, OnScoresError);
        if (gameOverScreen != null) gameOverScreen.Hide();
        if (initialsEntryOverlay != null) initialsEntryOverlay.Hide();
    }

    /// <summary>
    /// Called after a score is successfully posted to the server.
    /// Refreshes the leaderboard display and shows the start screen.
    /// </summary>
    private void OnScoresPosted(LeaderboardEntry[] entries)
    {
        OnScoresFetched(entries);
        if (startScreen != null) startScreen.Show();
    }

    /// <summary>
    /// Called when leaderboard scores are successfully fetched.
    /// Refreshes the leaderboard widget with the new entries.
    /// </summary>
    private void OnScoresFetched(LeaderboardEntry[] entries)
    {
        if (leaderboardWidget != null) leaderboardWidget.Refresh(entries);
    }

    /// <summary>
    /// Called when a leaderboard fetch or post fails.
    /// Degrades gracefully by clearing the leaderboard display.
    /// </summary>
    private void OnScoresError(string err)
    {
        Debug.LogWarning("[SceneBootstrapper] Leaderboard error: " + err);
        if (leaderboardWidget != null) leaderboardWidget.Refresh(new LeaderboardEntry[0]);
    }

    // -----------------------------------------------------------------------
    // QA Navigation Contract -- public methods called by reflection
    // -----------------------------------------------------------------------

    /// <summary>
    /// QA entry point: begins gameplay.
    /// Hides the start screen, shows the game screen, and starts the game controller.
    /// </summary>
    public void StartGame()
    {
        if (startScreen != null) startScreen.Hide();
        if (gameScreen != null) gameScreen.Show();
        if (gameplayController != null) gameplayController.StartGame();
    }

    /// <summary>
    /// QA entry point: triggers game over state.
    /// Uses GameplayController.TriggerGameOver to fire the game over flow with score 0.
    /// </summary>
    public void GoToGameOver()
    {
        if (gameplayController != null) gameplayController.TriggerGameOver(0);
    }

    /// <summary>
    /// QA entry point: returns to the start screen.
    /// Hides the game screen and game over screen, then shows the start screen.
    /// </summary>
    public void GoToStart()
    {
        if (gameScreen != null) gameScreen.Hide();
        if (gameOverScreen != null) gameOverScreen.Hide();
        if (initialsEntryOverlay != null) initialsEntryOverlay.Hide();
        if (startScreen != null) startScreen.Show();
    }
}
