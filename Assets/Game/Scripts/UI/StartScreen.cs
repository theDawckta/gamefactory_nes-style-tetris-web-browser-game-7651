using System;
using Game.Gameplay;
using UnityEngine;

public class StartScreen : BaseScreen
{
    [SerializeField] private LeaderboardService _leaderboardService;
    [SerializeField] private StartScreenLeaderboardWidget _leaderboardWidget;

    public event Action OnStartRequested;

    public override void Show()
    {
        base.Show();
        if (_leaderboardService != null)
        {
            _leaderboardService.GetScores(
                entries => RefreshLeaderboard(entries),
                _ => RefreshLeaderboard(null)
            );
        }
    }

    public void RefreshLeaderboard(LeaderboardEntry[] entries)
    {
        _leaderboardWidget?.Refresh(entries);
    }

    private void Update()
    {
        if (!IsVisible) return;
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            OnStartRequested?.Invoke();
    }
}
