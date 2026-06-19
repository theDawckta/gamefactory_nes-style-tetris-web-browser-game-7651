using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// MonoBehaviour widget that populates the #lines-region of a Game Screen UIDocument.
/// Displays a "LINES" header label and the total lines cleared as a zero-padded 3-digit integer.
/// Font is sourced from CoreAssets/Runtime/Fonts/default.asset via USS.
/// </summary>
public class GameScreenLinesWidget : MonoBehaviour
{
    // The VisualElement for the lines region
    private VisualElement _linesRegion;

    // The value label inside the lines region that shows the numeric lines count
    private Label _valueLabel;

    private void Awake()
    {
        var doc = GetComponent<UIDocument>();
        if (doc == null)
        {
            Debug.LogError("[GameScreenLinesWidget] No UIDocument component found on this GameObject.");
            return;
        }

        _linesRegion = doc.rootVisualElement.Q<VisualElement>("lines-region");
        if (_linesRegion == null)
        {
            Debug.LogError("[GameScreenLinesWidget] Could not find #lines-region element.");
            return;
        }

        // Find the value label by class "region-value" among children
        _valueLabel = _linesRegion.Children().OfType<Label>().FirstOrDefault(l => l.ClassListContains("region-value"));
        if (_valueLabel == null)
        {
            Debug.LogError("[GameScreenLinesWidget] Could not find .region-value label in #lines-region.");
        }
    }

    /// <summary>
    /// Updates the lines display value, zero-padded to 3 digits.
    /// Examples: UpdateLines(0) -> "000", UpdateLines(40) -> "040", UpdateLines(130) -> "130"
    /// </summary>
    public void UpdateLines(int lines)
    {
        if (_valueLabel != null)
        {
            _valueLabel.text = lines.ToString("D3");
        }
    }
}
