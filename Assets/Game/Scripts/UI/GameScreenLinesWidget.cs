using UnityEngine;
using UnityEngine.UIElements;

public class GameScreenLinesWidget : MonoBehaviour
{
    private Label _valueLabel;

    private void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        var linesRegion = root.Q("lines-region");
        if (linesRegion != null)
            PopulateRegion(linesRegion);
    }

    public void PopulateRegion(VisualElement linesRegion)
    {
        linesRegion.Clear();
        linesRegion.style.alignItems = Align.Center;
        linesRegion.style.justifyContent = Justify.Center;

        var header = new Label("LINES");
        linesRegion.Add(header);

        _valueLabel = new Label("000");
        linesRegion.Add(_valueLabel);
    }

    public void UpdateLines(int lines)
    {
        if (_valueLabel != null)
            _valueLabel.text = lines.ToString("D3");
    }
}
