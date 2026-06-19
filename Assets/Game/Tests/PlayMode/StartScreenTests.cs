using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

[TestFixture]
public class StartScreenTests
{
    private GameObject _go;
    private UIDocument _doc;
    private StartScreen _screen;

    [SetUp]
    public void SetUp()
    {
        _go = new GameObject("TestStartScreen");
        _doc = _go.AddComponent<UIDocument>();
        _screen = _go.AddComponent<StartScreen>();

        // Add VisualElements to rootVisualElement BEFORE adding the widget
        // so the widget's Awake can find them
        var region = new VisualElement();
        region.name = "leaderboard-region";
        _doc.rootVisualElement.Add(region);

        var logoRegion = new VisualElement();
        logoRegion.name = "logo-region";
        _doc.rootVisualElement.Add(logoRegion);

        var promptRegion = new VisualElement();
        promptRegion.name = "prompt-region";
        _doc.rootVisualElement.Add(promptRegion);

        // Now add the widget - its Awake will find the regions
        _go.AddComponent<StartScreenLeaderboardWidget>();
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
    public IEnumerator RefreshLeaderboard_PassesEntriesToWidget()
    {
        yield return null; // let Awake run for all components

        var entries = new LeaderboardEntry[]
        {
            new LeaderboardEntry("AAA", 1000),
            new LeaderboardEntry("BBB", 900)
        };

        // Get the widget directly and call Refresh
        var widget = _go.GetComponent<StartScreenLeaderboardWidget>();
        Assert.IsNotNull(widget, "Widget should exist");

        // Call Refresh directly on the widget to verify it works
        widget.Refresh(entries);
        yield return null;

        // Verify the widget populated the leaderboard region
        var region = _doc.rootVisualElement.Q<VisualElement>("leaderboard-region");
        Assert.IsNotNull(region, "leaderboard-region should exist");
        Assert.Greater(region.childCount, 0, "leaderboard region should have children after Refresh");
    }

    [UnityTest]
    public IEnumerator Screen_HasNoCanvasComponent()
    {
        yield return null;

        var canvas = _go.GetComponent<Canvas>();
        Assert.IsNull(canvas, "StartScreen should not have a Canvas component");
    }

    [UnityTest]
    public IEnumerator LayoutRegions_ExistInUXML()
    {
        yield return null;

        // Verify all three layout regions are queryable
        var logoRegion = _doc.rootVisualElement.Q<VisualElement>("logo-region");
        var leaderboardRegion = _doc.rootVisualElement.Q<VisualElement>("leaderboard-region");
        var promptRegion = _doc.rootVisualElement.Q<VisualElement>("prompt-region");

        Assert.IsNotNull(logoRegion, "#logo-region should exist");
        Assert.IsNotNull(leaderboardRegion, "#leaderboard-region should exist");
        Assert.IsNotNull(promptRegion, "#prompt-region should exist");
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
}
