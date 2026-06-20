using System;
using UnityEngine;
using UnityEngine.InputSystem;
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
        var doc = GetComponent<UIDocument>();
        if (doc != null && doc.rootVisualElement != null)
        {
            _scoreLabel = doc.rootVisualElement.Q<Label>("score-label");
        }
    }

    /// <summary>
    /// Called by BaseScreen.Show() after the screen is made visible.
    /// Provides a lazy fallback for when Awake could not initialize.
    /// </summary>
    protected override void OnShow()
    {
        if (_scoreLabel != null) return;

        var doc = GetComponent<UIDocument>();
        if (doc != null && doc.rootVisualElement != null)
        {
            _scoreLabel = doc.rootVisualElement.Q<Label>("score-label");
        }
    }

    /// <summary>
    /// Shows the Game Over screen and populates the score display.
    /// </summary>
    /// <param name="finalScore">The final score to display (zero-padded to 7 digits).</param>
    public void ShowWithScore(int finalScore)
    {
        Show();

        // Lazy fallback if Awake couldn't initialize
        if (_scoreLabel == null)
        {
            var doc = GetComponent<UIDocument>();
            if (doc != null && doc.rootVisualElement != null)
            {
                _scoreLabel = doc.rootVisualElement.Q<Label>("score-label");
            }
        }

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

        if ((Keyboard.current?.enterKey?.wasPressedThisFrame ?? false) || (Keyboard.current?.spaceKey?.wasPressedThisFrame ?? false))
        {
            OnContinueRequested?.Invoke();
        }
    }
}
