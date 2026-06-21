using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class GameOverScreen : BaseScreen
{
    public event Action OnContinueRequested;

    private Label _scoreLabel;

    protected override void Awake()
    {
        base.Awake();
        PopulateRegions(Root);
    }

    public void PopulateRegions(VisualElement root)
    {
        root.Q("gameover-label-region")?.Add(new Label("GAME OVER"));

        var scoreRegion = root.Q("score-region");
        if (scoreRegion != null)
        {
            _scoreLabel = new Label("SCORE: 0000000");
            scoreRegion.Add(_scoreLabel);
        }

        root.Q("prompt-region")?.Add(new Label("PRESS ENTER TO CONTINUE"));
    }

    public void ShowWithScore(int finalScore)
    {
        Show();
        if (_scoreLabel != null)
            _scoreLabel.text = "SCORE: " + finalScore.ToString("D7");
    }

    private void Update()
    {
        if (!IsVisible) return;
        var kb = Keyboard.current;
        if (kb == null) return;
        if (kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame)
            OnContinueRequested?.Invoke();
    }
}
