using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.TestTools;

public class GameScreenScoreWidgetTests
{
    private GameObject _go;
    private GameScreenScoreWidget _widget;
    private PanelSettings _panelSettings;

    [SetUp]
    public void SetUp()
    {
        _go = new GameObject();
        _go.SetActive(false);
        _panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
        var doc = _go.AddComponent<UIDocument>();
        doc.panelSettings = _panelSettings;
        _widget = _go.AddComponent<GameScreenScoreWidget>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(_go);
        Object.Destroy(_panelSettings);
    }

    [UnityTest]
    public IEnumerator Awake_WithNoScoreRegion_DoesNotThrow()
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
        var scoreRegion = new VisualElement { name = "score-region" };
        _widget.PopulateRegion(scoreRegion);
        Assert.AreEqual(2, scoreRegion.childCount);
    }

    [UnityTest]
    public IEnumerator PopulateRegion_HeaderLabel_IsScore()
    {
        _go.SetActive(true);
        yield return null;
        var scoreRegion = new VisualElement { name = "score-region" };
        _widget.PopulateRegion(scoreRegion);
        var header = (Label)scoreRegion[0];
        Assert.AreEqual("SCORE", header.text);
    }

    [UnityTest]
    public IEnumerator PopulateRegion_ValueLabel_InitiallyZeroPadded()
    {
        _go.SetActive(true);
        yield return null;
        var scoreRegion = new VisualElement { name = "score-region" };
        _widget.PopulateRegion(scoreRegion);
        var value = (Label)scoreRegion[1];
        Assert.AreEqual("0000000", value.text);
    }

    [UnityTest]
    public IEnumerator UpdateScore_Zero_Displays0000000()
    {
        _go.SetActive(true);
        yield return null;
        var scoreRegion = new VisualElement { name = "score-region" };
        _widget.PopulateRegion(scoreRegion);
        _widget.UpdateScore(0);
        var value = (Label)scoreRegion[1];
        Assert.AreEqual("0000000", value.text);
    }

    [UnityTest]
    public IEnumerator UpdateScore_1200_Displays0001200()
    {
        _go.SetActive(true);
        yield return null;
        var scoreRegion = new VisualElement { name = "score-region" };
        _widget.PopulateRegion(scoreRegion);
        _widget.UpdateScore(1200);
        var value = (Label)scoreRegion[1];
        Assert.AreEqual("0001200", value.text);
    }

    [UnityTest]
    public IEnumerator UpdateScore_9999999_Displays9999999()
    {
        _go.SetActive(true);
        yield return null;
        var scoreRegion = new VisualElement { name = "score-region" };
        _widget.PopulateRegion(scoreRegion);
        _widget.UpdateScore(9999999);
        var value = (Label)scoreRegion[1];
        Assert.AreEqual("9999999", value.text);
    }

    [UnityTest]
    public IEnumerator UpdateScore_MultipleCalls_UpdatesCorrectly()
    {
        _go.SetActive(true);
        yield return null;
        var scoreRegion = new VisualElement { name = "score-region" };
        _widget.PopulateRegion(scoreRegion);
        _widget.UpdateScore(100);
        _widget.UpdateScore(5000);
        var value = (Label)scoreRegion[1];
        Assert.AreEqual("0005000", value.text);
    }

    [UnityTest]
    public IEnumerator PopulateRegion_CalledTwice_ClearsAndRepopulates()
    {
        _go.SetActive(true);
        yield return null;
        var scoreRegion = new VisualElement { name = "score-region" };
        _widget.PopulateRegion(scoreRegion);
        _widget.PopulateRegion(scoreRegion);
        Assert.AreEqual(2, scoreRegion.childCount);
    }
}
