using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.TestTools;

public class GameScreenLevelWidgetTests
{
    private GameObject _go;
    private GameScreenLevelWidget _widget;
    private PanelSettings _panelSettings;

    [SetUp]
    public void SetUp()
    {
        _go = new GameObject();
        _go.SetActive(false);
        _panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
        var doc = _go.AddComponent<UIDocument>();
        doc.panelSettings = _panelSettings;
        _widget = _go.AddComponent<GameScreenLevelWidget>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(_go);
        Object.Destroy(_panelSettings);
    }

    [UnityTest]
    public IEnumerator Awake_WithNoLevelRegion_DoesNotThrow()
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
        var levelRegion = new VisualElement { name = "level-region" };
        _widget.PopulateRegion(levelRegion);
        Assert.AreEqual(2, levelRegion.childCount);
    }

    [UnityTest]
    public IEnumerator PopulateRegion_HeaderLabel_IsLevel()
    {
        _go.SetActive(true);
        yield return null;
        var levelRegion = new VisualElement { name = "level-region" };
        _widget.PopulateRegion(levelRegion);
        var header = (Label)levelRegion[0];
        Assert.AreEqual("LEVEL", header.text);
    }

    [UnityTest]
    public IEnumerator PopulateRegion_ValueLabel_InitializedToZeroPadded()
    {
        _go.SetActive(true);
        yield return null;
        var levelRegion = new VisualElement { name = "level-region" };
        _widget.PopulateRegion(levelRegion);
        var value = (Label)levelRegion[1];
        Assert.AreEqual("00", value.text);
    }

    [UnityTest]
    public IEnumerator UpdateLevel_Zero_Displays00()
    {
        _go.SetActive(true);
        yield return null;
        var levelRegion = new VisualElement { name = "level-region" };
        _widget.PopulateRegion(levelRegion);
        _widget.UpdateLevel(0);
        var value = (Label)levelRegion[1];
        Assert.AreEqual("00", value.text);
    }

    [UnityTest]
    public IEnumerator UpdateLevel_Nine_Displays09()
    {
        _go.SetActive(true);
        yield return null;
        var levelRegion = new VisualElement { name = "level-region" };
        _widget.PopulateRegion(levelRegion);
        _widget.UpdateLevel(9);
        var value = (Label)levelRegion[1];
        Assert.AreEqual("09", value.text);
    }

    [UnityTest]
    public IEnumerator UpdateLevel_Fifteen_Displays15()
    {
        _go.SetActive(true);
        yield return null;
        var levelRegion = new VisualElement { name = "level-region" };
        _widget.PopulateRegion(levelRegion);
        _widget.UpdateLevel(15);
        var value = (Label)levelRegion[1];
        Assert.AreEqual("15", value.text);
    }

    [UnityTest]
    public IEnumerator UpdateLevel_MultipleCalls_UpdatesCorrectly()
    {
        _go.SetActive(true);
        yield return null;
        var levelRegion = new VisualElement { name = "level-region" };
        _widget.PopulateRegion(levelRegion);
        _widget.UpdateLevel(5);
        _widget.UpdateLevel(15);
        var value = (Label)levelRegion[1];
        Assert.AreEqual("15", value.text);
    }

    [UnityTest]
    public IEnumerator PopulateRegion_CalledTwice_ClearsAndRepopulates()
    {
        _go.SetActive(true);
        yield return null;
        var levelRegion = new VisualElement { name = "level-region" };
        _widget.PopulateRegion(levelRegion);
        _widget.PopulateRegion(levelRegion);
        Assert.AreEqual(2, levelRegion.childCount);
    }
}
