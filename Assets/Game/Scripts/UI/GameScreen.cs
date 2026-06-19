using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Game Screen: HUD overlay shown during active gameplay.
/// Displays Score, Level, Lines cleared, and Next piece preview.
/// Derives from BaseScreen for show/hide lifecycle.
/// Does not handle input itself -- input is handled by GameplayController.
/// Layout: NEXT on left, playfield centered (world space), SCORE/LEVEL/LINES grouped on right.
/// </summary>
public class GameScreen : BaseScreen
{
    /// <summary>
    /// Reference to the score region VisualElement.
    /// </summary>
    private VisualElement _scoreRegion;

    /// <summary>
    /// Reference to the level region VisualElement.
    /// </summary>
    private VisualElement _levelRegion;

    /// <summary>
    /// Reference to the lines region VisualElement.
    /// </summary>
    private VisualElement _linesRegion;

    /// <summary>
    /// Reference to the next piece region VisualElement.
    /// </summary>
    private VisualElement _nextRegion;

    private void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        _scoreRegion = root.Q<VisualElement>("score-region");
        _levelRegion = root.Q<VisualElement>("level-region");
        _linesRegion = root.Q<VisualElement>("lines-region");
        _nextRegion = root.Q<VisualElement>("next-region");
    }

    /// <summary>
    /// Returns the score region for populating score display.
    /// </summary>
    public VisualElement ScoreRegion
    {
        get { return _scoreRegion; }
    }

    /// <summary>
    /// Returns the level region for populating level display.
    /// </summary>
    public VisualElement LevelRegion
    {
        get { return _levelRegion; }
    }

    /// <summary>
    /// Returns the lines region for populating lines cleared display.
    /// </summary>
    public VisualElement LinesRegion
    {
        get { return _linesRegion; }
    }

    /// <summary>
    /// Returns the next piece preview region.
    /// </summary>
    public VisualElement NextRegion
    {
        get { return _nextRegion; }
    }
}
