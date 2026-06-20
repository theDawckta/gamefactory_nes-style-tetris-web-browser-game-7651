using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// MonoBehaviour widget that populates the #level-region of a Game Screen UIDocument.
/// Displays a "LEVEL" header label and the current level as a zero-padded 2-digit integer.
/// Font is sourced from CoreAssets/Runtime/Fonts/default.asset via USS.
/// </summary>
public class GameScreenLevelWidget : MonoBehaviour
{
    // The VisualElement for the level region
    private VisualElement _levelRegion;

    // The value label inside the level region that shows the numeric level
    private Label _valueLabel;

    /// <summary>
    /// Whether UI element references have been initialized.
    /// </summary>
    private bool _initialized;

    /// <summary>
    /// Lazily initializes UI element references on first use.
    /// This defers the query until the visual tree is fully constructed.
    /// </summary>
    private void EnsureInitialized()
    {
        if (_initialized) return;
        _initialized = true;

        var doc = GetComponent<UIDocument>();
        if (doc == null || doc.rootVisualElement == null)
        {
            Debug.LogError("[GameScreenLevelWidget] No UIDocument or rootVisualElement found on this GameObject.");
            return;
        }

        _levelRegion = doc.rootVisualElement.Q<VisualElement>("level-region");
        if (_levelRegion == null)
        {
            Debug.LogError("[GameScreenLevelWidget] Could not find #level-region element.");
            return;
        }

        // Find the value label by class "region-value" among children
        _valueLabel = _levelRegion.Children().OfType<Label>().FirstOrDefault(l => l.ClassListContains("region-value"));
        if (_valueLabel == null)
        {
            Debug.LogError("[GameScreenLevelWidget] Could not find .region-value label in #level-region.");
        }
    }

    /// <summary>
    /// Updates the level display value, zero-padded to 2 digits.
    /// Examples: UpdateLevel(0) -> "00", UpdateLevel(9) -> "09", UpdateLevel(15) -> "15"
    /// </summary>
    public void UpdateLevel(int level)
    {
        EnsureInitialized();

        if (_valueLabel != null)
        {
            _valueLabel.text = level.ToString("D2");
        }
    }
}
