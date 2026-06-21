using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.TestTools;

public class GameScreenLinesWidgetTests
{
    private GameObject _go;
    private GameScreenLinesWidget _widget;
    private PanelSettings _panelSettings;

    [SetUp]
    public void SetUp()
    {
        _go = new GameObject();
        _go.SetActive(false);
        _panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
        var doc = _go.AddComponent<UIDocument>();
        doc.panelSettings = _panelSettings;
        _widget = _go.AddComponent<GameScreenLinesWidget>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(_go);
        Object.Destroy(_panelSettings);
    }

    [UnityTest]
    public IEnumerator Awake_WithNoLinesRegion_DoesNotThrow()
    {
        _go.SetActive(true);
        yield return null;
        Assert.IsNotNull(_widget);
    }

    [UnityTest]
    public IEnumerator PopulateRegion_AddsHeaderAndValueLabels()
    {
        _go.SetActive(true);
        yield return null;
        var linesRegion = new VisualElement { name = "lines-region" };
        _widget.PopulateRegion(linesRegion);
        Assert.AreEqual(2, linesRegion.childCount);
    }

    [UnityTest]
    public IEnumerator PopulateRegion_HeaderLabel_IsLines()
    {
        _go.SetActive(true);
        yield return null;
        var linesRegion = new VisualElement { name = "lines-region" };
        _widget.PopulateRegion(linesRegion);
        var header = (Label)linesRegion[0];
        Assert.AreEqual("LINES", header.text);
    }

    [UnityTest]
    public IEnumerator PopulateRegion_ValueLabel_InitiallyZeroPadded()
    {
        _go.SetActive(true);
        yield return null;
        var linesRegion = new VisualElement { name = "lines-region" };
        _widget.PopulateRegion(linesRegion);
        var value = (Label)linesRegion[1];
        Assert.AreEqual("000", value.text);
    }

    [UnityTest]
    public IEnumerator UpdateLines_Zero_Displays000()
    {
        _go.SetActive(true);
        yield return null;
        var linesRegion = new VisualElement { name = "lines-region" };
        _widget.PopulateRegion(linesRegion);
        _widget.UpdateLines(0);
        var value = (Label)linesRegion[1];
        Assert.AreEqual("000", value.text);
    }

    [UnityTest]
    public IEnumerator UpdateLines_40_Displays040()
    {
        _go.SetActive(true);
        yield return null;
        var linesRegion = new VisualElement { name = "lines-region" };
        _widget.PopulateRegion(linesRegion);
        _widget.UpdateLines(40);
        var value = (Label)linesRegion[1];
        Assert.AreEqual("040", value.text);
    }

    [UnityTest]
    public IEnumerator UpdateLines_130_Displays130()
    {
        _go.SetActive(true);
        yield return null;
        var linesRegion = new VisualElement { name = "lines-region" };
        _widget.PopulateRegion(linesRegion);
        _widget.UpdateLines(130);
        var value = (Label)linesRegion[1];
        Assert.AreEqual("130", value.text);
    }

    [UnityTest]
    public IEnumerator UpdateLines_MultipleCalls_UpdatesCorrectly()
    {
        _go.SetActive(true);
        yield return null;
        var linesRegion = new VisualElement { name = "lines-region" };
        _widget.PopulateRegion(linesRegion);
        _widget.UpdateLines(10);
        _widget.UpdateLines(40);
        var value = (Label)linesRegion[1];
        Assert.AreEqual("040", value.text);
    }

    [UnityTest]
    public IEnumerator PopulateRegion_CalledTwice_ClearsAndRepopulates()
    {
        _go.SetActive(true);
        yield return null;
        var linesRegion = new VisualElement { name = "lines-region" };
        _widget.PopulateRegion(linesRegion);
        _widget.PopulateRegion(linesRegion);
        Assert.AreEqual(2, linesRegion.childCount);
    }
}
