using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

[TestFixture]
public class GameScreenScoreWidgetTests
{
    private GameObject _go;
    private UIDocument _doc;
    private GameScreenScoreWidget _widget;

    [SetUp]
    public void SetUp()
    {
        _go = new GameObject("TestScoreWidget");
        _doc = _go.AddComponent<UIDocument>();

        // Build the score-region VisualElement with label children
        var scoreRegion = new VisualElement();
        scoreRegion.name = "score-region";
        scoreRegion.AddToClassList("score-region");

        var headerLabel = new Label("SCORE");
        headerLabel.AddToClassList("region-label");
        scoreRegion.Add(headerLabel);

        var valueLabel = new Label("0");
        valueLabel.AddToClassList("region-value");
        scoreRegion.Add(valueLabel);

        _doc.rootVisualElement.Add(scoreRegion);

        // Add the widget after the visual tree is set up
        _widget = _go.AddComponent<GameScreenScoreWidget>();
    }

    [TearDown]
    public void TearDown()
    {
        if (_go != null)
        {
            Object.Destroy(_go);
            _go = null;
        }
    }

    [UnityTest]
    public IEnumerator UpdateScore_Zero_Displays0000000()
    {
        yield return null; // wait for Awake to run

        _widget.UpdateScore(0);
        yield return null;

        var valueLabel = _doc.rootVisualElement.Q<VisualElement>("score-region").Children().OfType<Label>().FirstOrDefault(l => l.ClassListContains("region-value"));
        Assert.IsNotNull(valueLabel, "region-value label should exist");
        Assert.AreEqual("0000000", valueLabel.text, "Score 0 should display as 0000000");
    }

    [UnityTest]
    public IEnumerator UpdateScore_1200_Displays0001200()
    {
        yield return null; // wait for Awake to run

        _widget.UpdateScore(1200);
        yield return null;

        var valueLabel = _doc.rootVisualElement.Q<VisualElement>("score-region").Children().OfType<Label>().FirstOrDefault(l => l.ClassListContains("region-value"));
        Assert.IsNotNull(valueLabel, "region-value label should exist");
        Assert.AreEqual("0001200", valueLabel.text, "Score 1200 should display as 0001200");
    }

    [UnityTest]
    public IEnumerator UpdateScore_9999999_Displays9999999()
    {
        yield return null; // wait for Awake to run

        _widget.UpdateScore(9999999);
        yield return null;

        var valueLabel = _doc.rootVisualElement.Q<VisualElement>("score-region").Children().OfType<Label>().FirstOrDefault(l => l.ClassListContains("region-value"));
        Assert.IsNotNull(valueLabel, "region-value label should exist");
        Assert.AreEqual("9999999", valueLabel.text, "Score 9999999 should display as 9999999");
    }

    [UnityTest]
    public IEnumerator UpdateScore_MultipleCalls_UpdatesCorrectly()
    {
        yield return null; // wait for Awake to run

        _widget.UpdateScore(100);
        yield return null;

        var valueLabel = _doc.rootVisualElement.Q<VisualElement>("score-region").Children().OfType<Label>().FirstOrDefault(l => l.ClassListContains("region-value"));
        Assert.AreEqual("0000100", valueLabel.text, "Score 100 should display as 0000100");

        _widget.UpdateScore(50000);
        yield return null;

        Assert.AreEqual("0050000", valueLabel.text, "Score 50000 should display as 0050000");

        _widget.UpdateScore(0);
        yield return null;

        Assert.AreEqual("0000000", valueLabel.text, "Score 0 should display as 0000000");
    }

    [UnityTest]
    public IEnumerator HeaderLabel_RemainsUnchanged()
    {
        yield return null; // wait for Awake to run

        var headerLabel = _doc.rootVisualElement.Q<VisualElement>("score-region").Children().OfType<Label>().FirstOrDefault(l => l.ClassListContains("region-label"));
        Assert.IsNotNull(headerLabel, "region-label should exist");
        Assert.AreEqual("SCORE", headerLabel.text, "Header should remain SCORE");

        _widget.UpdateScore(42);
        yield return null;

        Assert.AreEqual("SCORE", headerLabel.text, "Header should still be SCORE after UpdateScore");
    }

    [UnityTest]
    public IEnumerator UpdateScore_InitialValue_ShowsZeroPadded()
    {
        yield return null; // wait for Awake to run

        // The initial value in the UXML is "0" - verify UpdateScore changes it
        var valueLabel = _doc.rootVisualElement.Q<VisualElement>("score-region").Children().OfType<Label>().FirstOrDefault(l => l.ClassListContains("region-value"));
        Assert.AreEqual("0", valueLabel.text, "Initial value should be '0' before UpdateScore is called");

        _widget.UpdateScore(1);
        yield return null;

        Assert.AreEqual("0000001", valueLabel.text, "Score 1 should display as 0000001");
    }
}
