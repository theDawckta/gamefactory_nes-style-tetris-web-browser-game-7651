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

    // -----------------------------------------------------------------------
    // Unity lifecycle
    // -----------------------------------------------------------------------

    private void Awake()
    {
        // Hide all screens except start screen
        gameScreen.Hide();
        gameOverScreen.Hide();
        initialsEntryOverlay.Hide();

        // Show start screen
        startScreen.Show();

        // Fetch leaderboard on startup
        LeaderboardService.GetScores(OnScoresFetched, OnScoresError);

        // Wire up all event handlers
        WireEvents();
    }

    // -----------------------------------------------------------------------
    // Event wiring
    // -----------------------------------------------------------------------

    private void WireEvents()
    {
        startScreen.OnStartRequested += OnStartRequested;
        gameplayController.OnStateChanged += OnGameStateChanged;
        gameplayController.OnGameOver += OnGameOver;
        gameOverScreen.OnContinueRequested += OnContinueRequested;
        initialsEntryOverlay.OnInitialsSubmitted += OnInitialsSubmitted;
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
        startScreen.Hide();
        gameScreen.Show();
        gameplayController.StartGame();
    }

    /// <summary>
    /// Called whenever the GameplayController's state changes.
    /// Updates all HUD widgets with current game state.
    /// </summary>
    private void OnGameStateChanged()
    {
        scoreWidget.UpdateScore(gameplayController.CurrentScore);
        levelWidget.UpdateLevel(gameplayController.CurrentLevel);
        linesWidget.UpdateLines(gameplayController.TotalLinesCleared);
        nextWidget.UpdateNextPiece(gameplayController.NextPiece);
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

        gameScreen.Hide();
        gameOverScreen.ShowWithScore(score);

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
            initialsEntryOverlay.ShowForScore(score);
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
        gameOverScreen.Hide();
        startScreen.Show();
        LeaderboardService.GetScores(OnScoresFetched, OnScoresError);
    }

    /// <summary>
    /// Called when the player submits initials via the InitialsEntryOverlay.
    /// Posts the score to the server, then hides overlays and returns to start screen.
    /// </summary>
    private void OnInitialsSubmitted(string name, int score)
    {
        LeaderboardService.PostScore(name, score, OnScoresPosted, OnScoresError);
        gameOverScreen.Hide();
        initialsEntryOverlay.Hide();
    }

    /// <summary>
    /// Called after a score is successfully posted to the server.
    /// Refreshes the leaderboard display and shows the start screen.
    /// </summary>
    private void OnScoresPosted(LeaderboardEntry[] entries)
    {
        OnScoresFetched(entries);
        startScreen.Show();
    }

    /// <summary>
    /// Called when leaderboard scores are successfully fetched.
    /// Refreshes the leaderboard widget with the new entries.
    /// </summary>
    private void OnScoresFetched(LeaderboardEntry[] entries)
    {
        leaderboardWidget.Refresh(entries);
    }

    /// <summary>
    /// Called when a leaderboard fetch or post fails.
    /// Degrades gracefully by clearing the leaderboard display.
    /// </summary>
    private void OnScoresError(string err)
    {
        Debug.LogWarning("[SceneBootstrapper] Leaderboard error: " + err);
        leaderboardWidget.Refresh(new LeaderboardEntry[0]);
    }
}
