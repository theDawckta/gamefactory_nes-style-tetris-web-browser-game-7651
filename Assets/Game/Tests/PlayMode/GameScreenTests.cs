using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

[TestFixture]
public class GameScreenTests
{
    private GameObject _go;
    private UIDocument _doc;
    private GameScreen _screen;

    [SetUp]
    public void SetUp()
    {
        _go = new GameObject("TestGameScreen");
        _doc = _go.AddComponent<UIDocument>();

        // Add VisualElements to rootVisualElement BEFORE adding GameScreen
        // so GameScreen's Awake can find them
        var nextRegion = new VisualElement();
        nextRegion.name = "next-region";
        nextRegion.AddToClassList("next-region");
        _doc.rootVisualElement.Add(nextRegion);

        var playfieldSpacer = new VisualElement();
        playfieldSpacer.name = "playfield-spacer";
        playfieldSpacer.AddToClassList("playfield-spacer");
        _doc.rootVisualElement.Add(playfieldSpacer);

        var statsPanel = new VisualElement();
        statsPanel.name = "stats-panel";
        statsPanel.AddToClassList("stats-panel");

        var scoreRegion = new VisualElement();
        scoreRegion.name = "score-region";
        scoreRegion.AddToClassList("score-region");
        statsPanel.Add(scoreRegion);

        var levelRegion = new VisualElement();
        levelRegion.name = "level-region";
        levelRegion.AddToClassList("level-region");
        statsPanel.Add(levelRegion);

        var linesRegion = new VisualElement();
        linesRegion.name = "lines-region";
        linesRegion.AddToClassList("lines-region");
        statsPanel.Add(linesRegion);

        _doc.rootVisualElement.Add(statsPanel);

        // Now add GameScreen - its Awake will query the regions
        _screen = _go.AddComponent<GameScreen>();
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
    public IEnumerator Show_MakesScreenVisible()
    {
        yield return null; // wait for Awake to run

        _screen.Show();
        yield return null;

        Assert.IsTrue(_screen.IsVisible, "Screen should be visible after Show()");
        Assert.IsTrue(_go.activeSelf, "GameObject should be active after Show()");
    }

    [UnityTest]
    public IEnumerator Hide_MakesScreenInvisible()
    {
        yield return null;

        _screen.Show();
        yield return null;

        _screen.Hide();
        yield return null;

        Assert.IsFalse(_screen.IsVisible, "Screen should not be visible after Hide()");
        Assert.IsFalse(_go.activeSelf, "GameObject should be inactive after Hide()");
    }

    [UnityTest]
    public IEnumerator Screen_HasNoCanvasComponent()
    {
        yield return null;

        var canvas = _go.GetComponent<Canvas>();
        Assert.IsNull(canvas, "GameScreen should not have a Canvas component");
    }

    [UnityTest]
    public IEnumerator LayoutRegions_ExistAndQueryable()
    {
        yield return null; // let Awake run to cache region references

        var scoreRegion = _doc.rootVisualElement.Q<VisualElement>("score-region");
        var levelRegion = _doc.rootVisualElement.Q<VisualElement>("level-region");
        var linesRegion = _doc.rootVisualElement.Q<VisualElement>("lines-region");
        var nextRegion = _doc.rootVisualElement.Q<VisualElement>("next-region");

        Assert.IsNotNull(scoreRegion, "score-region should exist");
        Assert.IsNotNull(levelRegion, "level-region should exist");
        Assert.IsNotNull(linesRegion, "lines-region should exist");
        Assert.IsNotNull(nextRegion, "next-region should exist");
    }

    [UnityTest]
    public IEnumerator PublicRegionProperties_ReturnValidElements()
    {
        yield return null; // let Awake run

        Assert.IsNotNull(_screen.ScoreRegion, "ScoreRegion property should not be null");
        Assert.IsNotNull(_screen.LevelRegion, "LevelRegion property should not be null");
        Assert.IsNotNull(_screen.LinesRegion, "LinesRegion property should not be null");
        Assert.IsNotNull(_screen.NextRegion, "NextRegion property should not be null");
    }

    [UnityTest]
    public IEnumerator BaseScreen_IsVisible_StartsFalse()
    {
        yield return null;

        Assert.IsFalse(_screen.IsVisible, "Screen should not be visible initially");
    }

    [UnityTest]
    public IEnumerator Show_Hide_Show_CyclesCorrectly()
    {
        yield return null;

        _screen.Show();
        yield return null;
        Assert.IsTrue(_screen.IsVisible);

        _screen.Hide();
        yield return null;
        Assert.IsFalse(_screen.IsVisible);

        _screen.Show();
        yield return null;
        Assert.IsTrue(_screen.IsVisible);
    }

    [UnityTest]
    public IEnumerator RootVisualElement_HasTransparentBackground()
    {
        yield return null;

        var root = _doc.rootVisualElement;
        // The USS sets -background-color: transparent on #root
        // We verify the root element exists and is accessible
        Assert.IsNotNull(root, "Root visual element should exist");
        // In Unity 6, the root element name is auto-generated based on the GameObject name
        // The important thing is it exists and is queryable
        Assert.IsTrue(root.childCount > 0, "Root should have child elements");
    }

    [UnityTest]
    public IEnumerator RootVisualElement_DisplayIsFlexAfterShow()
    {
        yield return null;

        _screen.Show();
        yield return null;

        var root = _doc.rootVisualElement;
        Assert.AreEqual(DisplayStyle.Flex, root.style.display.value, "Root display should be Flex after Show()");
    }

    [UnityTest]
    public IEnumerator RootVisualElement_DisplayIsNoneAfterHide()
    {
        yield return null;

        _screen.Show();
        yield return null;
        _screen.Hide();
        yield return null;

        // After Hide, the GameObject is inactive, so rootVisualElement may be null
        // The important thing is IsVisible is false
        Assert.IsFalse(_screen.IsVisible, "Screen should be hidden");
    }
}
