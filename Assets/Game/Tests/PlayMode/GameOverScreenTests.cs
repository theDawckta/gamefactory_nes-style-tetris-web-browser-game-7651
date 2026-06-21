using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.TestTools;

public class GameOverScreenTests
{
    private GameObject _go;
    private GameOverScreen _screen;
    private PanelSettings _panelSettings;

    [SetUp]
    public void SetUp()
    {
        _go = new GameObject();
        _go.SetActive(false);
        _panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
        var doc = _go.AddComponent<UIDocument>();
        doc.panelSettings = _panelSettings;
        _screen = _go.AddComponent<GameOverScreen>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(_go);
        Object.Destroy(_panelSettings);
    }

    private VisualElement BuildRegionRoot()
    {
        var root = new VisualElement();
        root.Add(new VisualElement { name = "gameover-label-region" });
        root.Add(new VisualElement { name = "score-region" });
        root.Add(new VisualElement { name = "prompt-region" });
        return root;
    }

    [UnityTest]
    public IEnumerator Awake_StartsHidden()
    {
        _go.SetActive(true);
        yield return null;
        Assert.IsFalse(_screen.IsVisible);
    }

    [UnityTest]
    public IEnumerator Awake_WithNoRegions_DoesNotThrow()
    {
        _go.SetActive(true);
        yield return null;
        Assert.IsNotNull(_screen);
    }

    [UnityTest]
    public IEnumerator Show_SetsIsVisible_True()
    {
        _go.SetActive(true);
        yield return null;
        _screen.Show();
        Assert.IsTrue(_screen.IsVisible);
    }

    [UnityTest]
    public IEnumerator Hide_SetsIsVisible_False()
    {
        _go.SetActive(true);
        yield return null;
        _screen.Show();
        _screen.Hide();
        Assert.IsFalse(_screen.IsVisible);
    }

    [UnityTest]
    public IEnumerator ShowWithScore_SetsIsVisible_True()
    {
        _go.SetActive(true);
        yield return null;
        _screen.ShowWithScore(5000);
        Assert.IsTrue(_screen.IsVisible);
    }

    [UnityTest]
    public IEnumerator PopulateRegions_GameoverRegion_ContainsGameOverLabel()
    {
        _go.SetActive(true);
        yield return null;
        var root = BuildRegionRoot();
        _screen.PopulateRegions(root);
        var region = root.Q("gameover-label-region");
        bool found = false;
        foreach (var child in region.Children())
        {
            if (child is Label lbl && lbl.text == "GAME OVER")
            {
                found = true;
                break;
            }
        }
        Assert.IsTrue(found, "gameover-label-region must contain a 'GAME OVER' label");
    }

    [UnityTest]
    public IEnumerator PopulateRegions_ScoreRegion_ContainsInitialScoreLabel()
    {
        _go.SetActive(true);
        yield return null;
        var root = BuildRegionRoot();
        _screen.PopulateRegions(root);
        var region = root.Q("score-region");
        bool found = false;
        foreach (var child in region.Children())
        {
            if (child is Label lbl && lbl.text == "SCORE: 0000000")
            {
                found = true;
                break;
            }
        }
        Assert.IsTrue(found, "score-region must contain initial 'SCORE: 0000000' label");
    }

    [UnityTest]
    public IEnumerator PopulateRegions_PromptRegion_ContainsPressEnterLabel()
    {
        _go.SetActive(true);
        yield return null;
        var root = BuildRegionRoot();
        _screen.PopulateRegions(root);
        var region = root.Q("prompt-region");
        bool found = false;
        foreach (var child in region.Children())
        {
            if (child is Label lbl && lbl.text == "PRESS ENTER TO CONTINUE")
            {
                found = true;
                break;
            }
        }
        Assert.IsTrue(found, "prompt-region must contain 'PRESS ENTER TO CONTINUE' label");
    }

    [UnityTest]
    public IEnumerator ShowWithScore_5000_DisplaysScore0005000()
    {
        _go.SetActive(true);
        yield return null;
        var root = BuildRegionRoot();
        _screen.PopulateRegions(root);
        _screen.ShowWithScore(5000);
        var scoreRegion = root.Q("score-region");
        bool found = false;
        foreach (var child in scoreRegion.Children())
        {
            if (child is Label lbl && lbl.text == "SCORE: 0005000")
            {
                found = true;
                break;
            }
        }
        Assert.IsTrue(found, "Expected 'SCORE: 0005000' in score-region");
    }

    [UnityTest]
    public IEnumerator ShowWithScore_Zero_DisplaysZeroPadded()
    {
        _go.SetActive(true);
        yield return null;
        var root = BuildRegionRoot();
        _screen.PopulateRegions(root);
        _screen.ShowWithScore(0);
        var scoreRegion = root.Q("score-region");
        bool found = false;
        foreach (var child in scoreRegion.Children())
        {
            if (child is Label lbl && lbl.text == "SCORE: 0000000")
            {
                found = true;
                break;
            }
        }
        Assert.IsTrue(found, "Expected 'SCORE: 0000000' after ShowWithScore(0)");
    }

    [UnityTest]
    public IEnumerator ShowWithScore_9999999_DisplaysFullValue()
    {
        _go.SetActive(true);
        yield return null;
        var root = BuildRegionRoot();
        _screen.PopulateRegions(root);
        _screen.ShowWithScore(9999999);
        var scoreRegion = root.Q("score-region");
        bool found = false;
        foreach (var child in scoreRegion.Children())
        {
            if (child is Label lbl && lbl.text == "SCORE: 9999999")
            {
                found = true;
                break;
            }
        }
        Assert.IsTrue(found, "Expected 'SCORE: 9999999' after ShowWithScore(9999999)");
    }

    [UnityTest]
    public IEnumerator ShowWithScore_MultipleCalls_UpdatesScore()
    {
        _go.SetActive(true);
        yield return null;
        var root = BuildRegionRoot();
        _screen.PopulateRegions(root);
        _screen.ShowWithScore(1000);
        _screen.ShowWithScore(2500);
        var scoreRegion = root.Q("score-region");
        bool found = false;
        foreach (var child in scoreRegion.Children())
        {
            if (child is Label lbl && lbl.text == "SCORE: 0002500")
            {
                found = true;
                break;
            }
        }
        Assert.IsTrue(found, "Expected 'SCORE: 0002500' after second ShowWithScore call");
    }

    [UnityTest]
    public IEnumerator OnContinueRequested_CanSubscribeAndUnsubscribe()
    {
        _go.SetActive(true);
        yield return null;
        bool fired = false;
        System.Action handler = () => fired = true;
        _screen.OnContinueRequested += handler;
        _screen.OnContinueRequested -= handler;
        Assert.IsFalse(fired);
    }

    [UnityTest]
    public IEnumerator PopulateRegions_WithNullRegions_DoesNotThrow()
    {
        _go.SetActive(true);
        yield return null;
        var root = new VisualElement();
        Assert.DoesNotThrow(() => _screen.PopulateRegions(root));
    }
}
