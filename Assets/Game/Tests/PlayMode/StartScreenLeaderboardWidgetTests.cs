using System.Collections;
using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.TestTools;

public class StartScreenLeaderboardWidgetTests
{
    private GameObject _go;
    private StartScreenLeaderboardWidget _widget;
    private PanelSettings _panelSettings;

    [SetUp]
    public void SetUp()
    {
        _go = new GameObject();
        _go.SetActive(false);
        _panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
        var doc = _go.AddComponent<UIDocument>();
        doc.panelSettings = _panelSettings;
        _widget = _go.AddComponent<StartScreenLeaderboardWidget>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(_go);
        Object.Destroy(_panelSettings);
    }

    [UnityTest]
    public IEnumerator Awake_WithNoLeaderboardRegion_DoesNotThrow()
    {
        _go.SetActive(true);
        yield return null;
        Assert.IsNotNull(_widget);
    }

    [UnityTest]
    public IEnumerator Refresh_HeaderLabel_IsTopScores()
    {
        _go.SetActive(true);
        yield return null;
        var region = new VisualElement { name = "leaderboard-region" };
        _widget.PopulateRegion(region);
        var header = (Label)region[0];
        Assert.AreEqual("TOP SCORES", header.text);
    }

    [UnityTest]
    public IEnumerator Refresh_WithFiveEntries_DisplaysAllRowsCorrectly()
    {
        _go.SetActive(true);
        yield return null;
        var region = new VisualElement { name = "leaderboard-region" };
        _widget.PopulateRegion(region);
        var entries = new LeaderboardEntry[]
        {
            new LeaderboardEntry { name = "AAA", score = 9999999 },
            new LeaderboardEntry { name = "BBB", score = 8888888 },
            new LeaderboardEntry { name = "CCC", score = 7777777 },
            new LeaderboardEntry { name = "DDD", score = 6666666 },
            new LeaderboardEntry { name = "EEE", score = 5555555 },
        };
        _widget.Refresh(entries);
        // header + 5 rows
        Assert.AreEqual(6, region.childCount);
        for (int i = 0; i < 5; i++)
        {
            var row = region[i + 1];
            var nameLabel = (Label)row[1];
            var scoreLabel = (Label)row[2];
            Assert.AreEqual(entries[i].name, nameLabel.text);
            Assert.AreEqual(entries[i].score.ToString("D7"), scoreLabel.text);
        }
    }

    [UnityTest]
    public IEnumerator Refresh_WithTwoEntries_ShowsTwoRowsAndThreePlaceholders()
    {
        _go.SetActive(true);
        yield return null;
        var region = new VisualElement { name = "leaderboard-region" };
        _widget.PopulateRegion(region);
        var entries = new LeaderboardEntry[]
        {
            new LeaderboardEntry { name = "AAA", score = 1000000 },
            new LeaderboardEntry { name = "BBB", score = 500000 },
        };
        _widget.Refresh(entries);
        Assert.AreEqual(6, region.childCount);
        for (int i = 0; i < 2; i++)
        {
            var row = region[i + 1];
            Assert.AreEqual(entries[i].name, ((Label)row[1]).text);
        }
        for (int i = 2; i < 5; i++)
        {
            var row = region[i + 1];
            Assert.AreEqual("---", ((Label)row[1]).text);
            Assert.AreEqual("0000000", ((Label)row[2]).text);
        }
    }

    [UnityTest]
    public IEnumerator Refresh_WithNullArray_ShowsFivePlaceholders_WithNoErrors()
    {
        _go.SetActive(true);
        yield return null;
        var region = new VisualElement { name = "leaderboard-region" };
        _widget.PopulateRegion(region);
        _widget.Refresh(null);
        Assert.AreEqual(6, region.childCount);
        for (int i = 0; i < 5; i++)
        {
            var row = region[i + 1];
            Assert.AreEqual("---", ((Label)row[1]).text);
            Assert.AreEqual("0000000", ((Label)row[2]).text);
        }
    }

    [UnityTest]
    public IEnumerator Refresh_WithEmptyArray_ShowsFivePlaceholders()
    {
        _go.SetActive(true);
        yield return null;
        var region = new VisualElement { name = "leaderboard-region" };
        _widget.PopulateRegion(region);
        _widget.Refresh(new LeaderboardEntry[0]);
        Assert.AreEqual(6, region.childCount);
        for (int i = 0; i < 5; i++)
        {
            var row = region[i + 1];
            Assert.AreEqual("---", ((Label)row[1]).text);
            Assert.AreEqual("0000000", ((Label)row[2]).text);
        }
    }

    [UnityTest]
    public IEnumerator Refresh_ScoresZeroPaddedToSevenDigits()
    {
        _go.SetActive(true);
        yield return null;
        var region = new VisualElement { name = "leaderboard-region" };
        _widget.PopulateRegion(region);
        var entries = new LeaderboardEntry[]
        {
            new LeaderboardEntry { name = "AAA", score = 42 },
        };
        _widget.Refresh(entries);
        var row = region[1];
        Assert.AreEqual("0000042", ((Label)row[2]).text);
    }

    [UnityTest]
    public IEnumerator Refresh_CalledTwice_ClearsAndRepopulates()
    {
        _go.SetActive(true);
        yield return null;
        var region = new VisualElement { name = "leaderboard-region" };
        _widget.PopulateRegion(region);
        _widget.Refresh(new LeaderboardEntry[] { new LeaderboardEntry { name = "AAA", score = 100 } });
        _widget.Refresh(new LeaderboardEntry[] { new LeaderboardEntry { name = "ZZZ", score = 999 } });
        Assert.AreEqual(6, region.childCount);
        var firstRow = region[1];
        Assert.AreEqual("ZZZ", ((Label)firstRow[1]).text);
    }

    [UnityTest]
    public IEnumerator Refresh_RankLabels_AreCorrect()
    {
        _go.SetActive(true);
        yield return null;
        var region = new VisualElement { name = "leaderboard-region" };
        _widget.PopulateRegion(region);
        _widget.Refresh(null);
        for (int i = 0; i < 5; i++)
        {
            var row = region[i + 1];
            var rankLabel = (Label)row[0];
            Assert.AreEqual((i + 1) + ".", rankLabel.text);
        }
    }
}
