using System;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Game Over Screen: displays GAME OVER text, final score, and a continue prompt.
/// Derives from BaseScreen for show/hide lifecycle.
/// Fires OnContinueRequested when the player presses Enter or Space.
/// Uses a semi-transparent dark overlay so the frozen playfield is dimly visible.
/// </summary>
public class GameOverScreen : BaseScreen
{
    /// <summary>
    /// Fired when the player presses Enter or Space to continue.
    /// </summary>
    public event System.Action OnContinueRequested;

    private Label _scoreLabel;

    private void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        _scoreLabel = root.Q<Label>("score-label");
    }

    /// <summary>
    /// Shows the Game Over screen and populates the score display.
    /// </summary>
    /// <param name="finalScore">The final score to display (zero-padded to 7 digits).</param>
    public void ShowWithScore(int finalScore)
    {
        if (_scoreLabel != null)
        {
            _scoreLabel.text = "SCORE: " + finalScore.ToString("D7");
        }

        Show();
    }

    private void Update()
    {
        if (!IsVisible)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            OnContinueRequested?.Invoke();
        }
    }
}
