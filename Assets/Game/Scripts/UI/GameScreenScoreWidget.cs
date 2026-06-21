using UnityEngine;
using UnityEngine.UIElements;

public class GameScreenScoreWidget : MonoBehaviour
{
    private Label _valueLabel;

    private void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        var scoreRegion = root.Q("score-region");
        if (scoreRegion != null)
            PopulateRegion(scoreRegion);
    }

    public void PopulateRegion(VisualElement scoreRegion)
    {
        scoreRegion.Clear();
        scoreRegion.style.alignItems = Align.Center;
        scoreRegion.style.justifyContent = Justify.Center;

        var header = new Label("SCORE");
        scoreRegion.Add(header);

        _valueLabel = new Label("0000000");
        scoreRegion.Add(_valueLabel);
    }

    public void UpdateScore(int score)
    {
        if (_valueLabel != null)
            _valueLabel.text = score.ToString("D7");
    }
}
