using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

[TestFixture]
public class GameScreenLinesWidgetTests
{
    private GameObject _go;
    private UIDocument _doc;
    private GameScreenLinesWidget _widget;

    [SetUp]
    public void SetUp()
    {
        _go = new GameObject("TestLinesWidget");
        _doc = _go.AddComponent<UIDocument>();

        // Build the lines-region VisualElement with label children
        var linesRegion = new VisualElement();
        linesRegion.name = "lines-region";
        linesRegion.AddToClassList("lines-region");

        var headerLabel = new Label("LINES");
        headerLabel.AddToClassList("region-label");
        linesRegion.Add(headerLabel);

        var valueLabel = new Label("0");
        valueLabel.AddToClassList("region-value");
        linesRegion.Add(valueLabel);

        _doc.rootVisualElement.Add(linesRegion);

        // Add the widget after the visual tree is set up
        _widget = _go.AddComponent<GameScreenLinesWidget>();
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
    public IEnumerator UpdateLines_Zero_Displays000()
    {
        yield return null; // wait for Awake to run

        _widget.UpdateLines(0);
        yield return null;

        var valueLabel = _doc.rootVisualElement.Q<VisualElement>("lines-region").Children().OfType<Label>().FirstOrDefault(l => l.ClassListContains("region-value"));
        Assert.IsNotNull(valueLabel, "region-value label should exist");
        Assert.AreEqual("000", valueLabel.text, "Lines 0 should display as 000");
    }

    [UnityTest]
    public IEnumerator UpdateLines_40_Displays040()
    {
        yield return null; // wait for Awake to run

        _widget.UpdateLines(40);
        yield return null;

        var valueLabel = _doc.rootVisualElement.Q<VisualElement>("lines-region").Children().OfType<Label>().FirstOrDefault(l => l.ClassListContains("region-value"));
        Assert.IsNotNull(valueLabel, "region-value label should exist");
        Assert.AreEqual("040", valueLabel.text, "Lines 40 should display as 040");
    }

    [UnityTest]
    public IEnumerator UpdateLines_130_Displays130()
    {
        yield return null; // wait for Awake to run

        _widget.UpdateLines(130);
        yield return null;

        var valueLabel = _doc.rootVisualElement.Q<VisualElement>("lines-region").Children().OfType<Label>().FirstOrDefault(l => l.ClassListContains("region-value"));
        Assert.IsNotNull(valueLabel, "region-value label should exist");
        Assert.AreEqual("130", valueLabel.text, "Lines 130 should display as 130");
    }

    [UnityTest]
    public IEnumerator UpdateLines_MultipleCalls_UpdatesCorrectly()
    {
        yield return null; // wait for Awake to run

        _widget.UpdateLines(5);
        yield return null;

        var valueLabel = _doc.rootVisualElement.Q<VisualElement>("lines-region").Children().OfType<Label>().FirstOrDefault(l => l.ClassListContains("region-value"));
        Assert.AreEqual("005", valueLabel.text, "Lines 5 should display as 005");

        _widget.UpdateLines(99);
        yield return null;

        Assert.AreEqual("099", valueLabel.text, "Lines 99 should display as 099");

        _widget.UpdateLines(0);
        yield return null;

        Assert.AreEqual("000", valueLabel.text, "Lines 0 should display as 000");
    }

    [UnityTest]
    public IEnumerator HeaderLabel_RemainsUnchanged()
    {
        yield return null; // wait for Awake to run

        var headerLabel = _doc.rootVisualElement.Q<VisualElement>("lines-region").Children().OfType<Label>().FirstOrDefault(l => l.ClassListContains("region-label"));
        Assert.IsNotNull(headerLabel, "region-label should exist");
        Assert.AreEqual("LINES", headerLabel.text, "Header should remain LINES");

        _widget.UpdateLines(42);
        yield return null;

        Assert.AreEqual("LINES", headerLabel.text, "Header should still be LINES after UpdateLines");
    }

    [UnityTest]
    public IEnumerator UpdateLines_InitialValue_ShowsZeroPadded()
    {
        yield return null; // wait for Awake to run

        // The initial value in the UXML is "0" - verify UpdateLines changes it
        var valueLabel = _doc.rootVisualElement.Q<VisualElement>("lines-region").Children().OfType<Label>().FirstOrDefault(l => l.ClassListContains("region-value"));
        Assert.AreEqual("0", valueLabel.text, "Initial value should be '0' before UpdateLines is called");

        _widget.UpdateLines(1);
        yield return null;

        Assert.AreEqual("001", valueLabel.text, "Lines 1 should display as 001");
    }
}
