using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// MonoBehaviour widget that populates the #score-region of a Game Screen UIDocument.
/// Displays a "SCORE" header label and the current score as a zero-padded 7-digit integer.
/// Font is sourced from CoreAssets/Runtime/Fonts/default.asset via USS.
/// </summary>
public class GameScreenScoreWidget : MonoBehaviour
{
    // The VisualElement for the score region
    private VisualElement _scoreRegion;

    // The value label inside the score region that shows the numeric score
    private Label _valueLabel;

    private void Awake()
    {
        var doc = GetComponent<UIDocument>();
        if (doc == null || doc.rootVisualElement == null)
            return;

        _scoreRegion = doc.rootVisualElement.Q<VisualElement>("score-region");
        if (_scoreRegion == null)
            return;

        // Find the value label by class "region-value" among children
        _valueLabel = _scoreRegion.Children().OfType<Label>().FirstOrDefault(l => l.ClassListContains("region-value"));
    }

    /// <summary>
    /// Updates the score display value, zero-padded to 7 digits.
    /// Examples: UpdateScore(0) -> "0000000", UpdateScore(1200) -> "0001200", UpdateScore(9999999) -> "9999999"
    /// </summary>
    public void UpdateScore(int score)
    {
        // Lazy fallback: if Awake could not find the region, try again now
        if (_valueLabel == null)
        {
            var doc = GetComponent<UIDocument>();
            if (doc != null && doc.rootVisualElement != null)
            {
                _scoreRegion = doc.rootVisualElement.Q<VisualElement>("score-region");
                if (_scoreRegion != null)
                {
                    _valueLabel = _scoreRegion.Children().OfType<Label>().FirstOrDefault(l => l.ClassListContains("region-value"));
                }
            }
        }

        if (_valueLabel != null)
        {
            _valueLabel.text = score.ToString("D7");
        }
    }
}
