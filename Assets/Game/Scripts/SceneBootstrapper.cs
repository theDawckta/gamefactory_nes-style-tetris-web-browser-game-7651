using Game.Gameplay;
using UnityEngine;

public class SceneBootstrapper : MonoBehaviour
{
    [SerializeField] private GameplayController gameplayController;
    [SerializeField] private LeaderboardService leaderboardService;
    [SerializeField] private StartScreen startScreen;
    [SerializeField] private GameScreen gameScreen;
    [SerializeField] private GameOverScreen gameOverScreen;
    [SerializeField] private InitialsEntryOverlay initialsEntryOverlay;
    [SerializeField] private GameScreenScoreWidget scoreWidget;
    [SerializeField] private GameScreenLevelWidget levelWidget;
    [SerializeField] private GameScreenLinesWidget linesWidget;
    [SerializeField] private GameScreenNextWidget nextWidget;
    [SerializeField] private StartScreenLeaderboardWidget leaderboardWidget;

    private void Awake()
    {
        gameScreen?.Hide();
        gameOverScreen?.Hide();
        initialsEntryOverlay?.Hide();
        startScreen?.Show();
        leaderboardService?.GetScores(OnScoresFetched, OnScoresError);

        if (startScreen != null) startScreen.OnStartRequested += OnStartRequested;
        if (gameplayController != null)
        {
            gameplayController.OnStateChanged += OnGameStateChanged;
            gameplayController.OnGameOver += OnGameOver;
        }
        if (gameOverScreen != null) gameOverScreen.OnContinueRequested += OnContinueRequested;
        if (initialsEntryOverlay != null) initialsEntryOverlay.OnInitialsSubmitted += OnInitialsSubmitted;
    }

    // QA Navigation Contract
    public void StartGame()
    {
        startScreen?.Hide();
        gameScreen?.Show();
        gameplayController?.StartGame();
    }

    public void GoToGameOver()
    {
        gameplayController?.StopGame();
        OnGameOver(0);
    }

    public void GoToStart()
    {
        gameplayController?.StopGame();
        OnContinueRequested();
    }

    private void OnStartRequested()
    {
        startScreen?.Hide();
        gameScreen?.Show();
        gameplayController?.StartGame();
    }

    private void OnGameStateChanged()
    {
        if (gameplayController == null) return;
        scoreWidget?.UpdateScore(gameplayController.CurrentScore);
        levelWidget?.UpdateLevel(gameplayController.CurrentLevel);
        linesWidget?.UpdateLines(gameplayController.TotalLinesCleared);
        nextWidget?.UpdateNextPiece(gameplayController.NextPiece);
    }

    private void OnGameOver(int score)
    {
        gameScreen?.Hide();
        gameOverScreen?.ShowWithScore(score);
        leaderboardService?.GetScores(
            entries => CheckQualification(score, entries),
            _ => { }
        );
    }

    private void CheckQualification(int score, LeaderboardEntry[] entries)
    {
        if (entries == null || entries.Length < 5 || score > entries[entries.Length - 1].score)
            initialsEntryOverlay?.ShowForScore(score);
    }

    private void OnContinueRequested()
    {
        initialsEntryOverlay?.Hide();
        gameOverScreen?.Hide();
        startScreen?.Show();
        leaderboardService?.GetScores(OnScoresFetched, OnScoresError);
    }

    private void OnInitialsSubmitted(string name, int score)
    {
        leaderboardService?.PostScore(name, score, OnScoresPosted, OnScoresError);
        gameOverScreen?.Hide();
        initialsEntryOverlay?.Hide();
    }

    private void OnScoresPosted(LeaderboardEntry[] entries)
    {
        OnScoresFetched(entries);
        startScreen?.Show();
    }

    private void OnScoresFetched(LeaderboardEntry[] entries)
    {
        leaderboardWidget?.Refresh(entries);
    }

    private void OnScoresError(string err)
    {
        leaderboardWidget?.Refresh(new LeaderboardEntry[0]);
    }
}
