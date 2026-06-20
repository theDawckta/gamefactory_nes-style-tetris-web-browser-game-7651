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

    /// <summary>
    /// Whether UI element references have been initialized.
    /// </summary>
    private bool _initialized;

    /// <summary>
    /// Initializes UI element references lazily. Called from OnShow() and ShowWithScore()
    /// to ensure rootVisualElement is available.
    /// </summary>
    private void EnsureInitialized()
    {
        if (_initialized) return;
        _initialized = true;

        var doc = GetComponent<UIDocument>();
        if (doc == null || doc.rootVisualElement == null) return;

        var root = doc.rootVisualElement;
        _scoreLabel = root.Q<Label>("score-label");
    }

    /// <summary>
    /// Called by BaseScreen.Show() after the screen is made visible.
    /// </summary>
    protected override void OnShow()
    {
        EnsureInitialized();
    }

    /// <summary>
    /// Shows the Game Over screen and populates the score display.
    /// </summary>
    /// <param name="finalScore">The final score to display (zero-padded to 7 digits).</param>
    public void ShowWithScore(int finalScore)
    {
        Show();
        EnsureInitialized();

        if (_scoreLabel != null)
        {
            _scoreLabel.text = "SCORE: " + finalScore.ToString("D7");
        }
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
