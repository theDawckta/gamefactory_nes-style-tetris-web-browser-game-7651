using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

[TestFixture]
public class GameScreenLevelWidgetTests
{
    private GameObject _go;
    private UIDocument _doc;
    private GameScreenLevelWidget _widget;

    [SetUp]
    public void SetUp()
    {
        _go = new GameObject("TestLevelWidget");
        _doc = _go.AddComponent<UIDocument>();

        // Build the level-region VisualElement with label children
        var levelRegion = new VisualElement();
        levelRegion.name = "level-region";
        levelRegion.AddToClassList("level-region");

        var headerLabel = new Label("LEVEL");
        headerLabel.AddToClassList("region-label");
        levelRegion.Add(headerLabel);

        var valueLabel = new Label("1");
        valueLabel.AddToClassList("region-value");
        levelRegion.Add(valueLabel);

        _doc.rootVisualElement.Add(levelRegion);

        // Add the widget after the visual tree is set up
        _widget = _go.AddComponent<GameScreenLevelWidget>();
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
    public IEnumerator UpdateLevel_Zero_Displays00()
    {
        yield return null; // wait for Awake to run

        _widget.UpdateLevel(0);
        yield return null;

        var valueLabel = _doc.rootVisualElement.Q<VisualElement>("level-region").Children().OfType<Label>().FirstOrDefault(l => l.ClassListContains("region-value"));
        Assert.IsNotNull(valueLabel, "region-value label should exist");
        Assert.AreEqual("00", valueLabel.text, "Level 0 should display as 00");
    }

    [UnityTest]
    public IEnumerator UpdateLevel_9_Displays09()
    {
        yield return null; // wait for Awake to run

        _widget.UpdateLevel(9);
        yield return null;

        var valueLabel = _doc.rootVisualElement.Q<VisualElement>("level-region").Children().OfType<Label>().FirstOrDefault(l => l.ClassListContains("region-value"));
        Assert.IsNotNull(valueLabel, "region-value label should exist");
        Assert.AreEqual("09", valueLabel.text, "Level 9 should display as 09");
    }

    [UnityTest]
    public IEnumerator UpdateLevel_15_Displays15()
    {
        yield return null; // wait for Awake to run

        _widget.UpdateLevel(15);
        yield return null;

        var valueLabel = _doc.rootVisualElement.Q<VisualElement>("level-region").Children().OfType<Label>().FirstOrDefault(l => l.ClassListContains("region-value"));
        Assert.IsNotNull(valueLabel, "region-value label should exist");
        Assert.AreEqual("15", valueLabel.text, "Level 15 should display as 15");
    }

    [UnityTest]
    public IEnumerator UpdateLevel_MultipleCalls_UpdatesCorrectly()
    {
        yield return null; // wait for Awake to run

        _widget.UpdateLevel(1);
        yield return null;

        var valueLabel = _doc.rootVisualElement.Q<VisualElement>("level-region").Children().OfType<Label>().FirstOrDefault(l => l.ClassListContains("region-value"));
        Assert.AreEqual("01", valueLabel.text, "Level 1 should display as 01");

        _widget.UpdateLevel(29);
        yield return null;

        Assert.AreEqual("29", valueLabel.text, "Level 29 should display as 29");

        _widget.UpdateLevel(0);
        yield return null;

        Assert.AreEqual("00", valueLabel.text, "Level 0 should display as 00");
    }

    [UnityTest]
    public IEnumerator HeaderLabel_RemainsUnchanged()
    {
        yield return null; // wait for Awake to run

        var headerLabel = _doc.rootVisualElement.Q<VisualElement>("level-region").Children().OfType<Label>().FirstOrDefault(l => l.ClassListContains("region-label"));
        Assert.IsNotNull(headerLabel, "region-label should exist");
        Assert.AreEqual("LEVEL", headerLabel.text, "Header should remain LEVEL");

        _widget.UpdateLevel(42);
        yield return null;

        Assert.AreEqual("LEVEL", headerLabel.text, "Header should still be LEVEL after UpdateLevel");
    }

    [UnityTest]
    public IEnumerator UpdateLevel_InitialValue_ShowsZeroPadded()
    {
        yield return null; // wait for Awake to run

        // The initial value in the UXML is "1" - verify UpdateLevel changes it
        var valueLabel = _doc.rootVisualElement.Q<VisualElement>("level-region").Children().OfType<Label>().FirstOrDefault(l => l.ClassListContains("region-value"));
        Assert.AreEqual("1", valueLabel.text, "Initial value should be '1' before UpdateLevel is called");

        _widget.UpdateLevel(1);
        yield return null;

        Assert.AreEqual("01", valueLabel.text, "Level 1 should display as 01");
    }
}
