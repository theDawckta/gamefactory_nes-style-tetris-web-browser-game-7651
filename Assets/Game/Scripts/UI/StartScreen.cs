using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

/// <summary>
/// Start Screen: displays the title logo, leaderboard table, and start prompt.
/// Derives from BaseScreen for show/hide lifecycle.
/// Fires OnStartRequested when the player presses Enter or Space.
/// </summary>
public class StartScreen : BaseScreen
{
    /// <summary>
    /// Fired when the player presses Enter or Space to start the game.
    /// </summary>
    public event System.Action OnStartRequested;

    private StartScreenLeaderboardWidget _leaderboardWidget;

    private void Awake()
    {
        _leaderboardWidget = GetComponent<StartScreenLeaderboardWidget>();
    }

    /// <summary>
    /// Called by BaseScreen.Show() after the screen is made visible.
    /// Fetches leaderboard scores and populates the leaderboard widget.
    /// </summary>
    protected override void OnShow()
    {
        var scores = LeaderboardService.GetScores();
        RefreshLeaderboard(scores);
    }

    /// <summary>
    /// Public method to refresh the leaderboard with new entries.
    /// </summary>
    public void RefreshLeaderboard(LeaderboardEntry[] entries)
    {
        if (_leaderboardWidget != null)
        {
            _leaderboardWidget.Refresh(entries);
        }
    }

    private void Update()
    {
        if (!IsVisible)
        {
            return;
        }

        if ((Keyboard.current?.enterKey?.wasPressedThisFrame ?? false) || (Keyboard.current?.spaceKey?.wasPressedThisFrame ?? false))
        {
            OnStartRequested?.Invoke();
        }
    }
}
