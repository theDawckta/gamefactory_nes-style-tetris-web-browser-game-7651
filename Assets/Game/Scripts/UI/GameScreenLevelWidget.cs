using UnityEngine;
using UnityEngine.UIElements;

public class GameScreenLevelWidget : MonoBehaviour
{
    private Label _valueLabel;

    private void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        var levelRegion = root.Q("level-region");
        if (levelRegion != null)
            PopulateRegion(levelRegion);
    }

    public void PopulateRegion(VisualElement levelRegion)
    {
        levelRegion.Clear();
        levelRegion.style.alignItems = Align.Center;
        levelRegion.style.justifyContent = Justify.Center;

        var header = new Label("LEVEL");
        levelRegion.Add(header);

        _valueLabel = new Label("00");
        levelRegion.Add(_valueLabel);
    }

    public void UpdateLevel(int level)
    {
        if (_valueLabel != null)
            _valueLabel.text = level.ToString("D2");
    }
}
