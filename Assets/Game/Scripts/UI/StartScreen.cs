using System;
using Game.Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;

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
        var kb = Keyboard.current;
        if (kb == null) return;
        if (kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame)
            OnStartRequested?.Invoke();
    }
}
