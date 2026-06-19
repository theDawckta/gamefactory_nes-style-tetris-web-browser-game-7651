using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

[TestFixture]
public class StartScreenLeaderboardWidgetTests
{
    private GameObject _go;
    private UIDocument _doc;
    private StartScreenLeaderboardWidget _widget;

    [SetUp]
    public void SetUp()
    {
        // Create a GameObject with UIDocument and StartScreenLeaderboardWidget
        _go = new GameObject("TestLeaderboardScene");
        _doc = _go.AddComponent<UIDocument>();
        _widget = _go.AddComponent<StartScreenLeaderboardWidget>();

        // Without a UXML sourceAsset, Unity provides an empty rootVisualElement.
        // We add the #leaderboard-region element that the widget expects.
        var region = new VisualElement();
        region.name = "leaderboard-region";
        _doc.rootVisualElement.Add(region);

        // Trigger Awake by waiting a frame (Unity lifecycle)
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
    public IEnumerator Refresh_WithFiveEntries_DisplaysAllRows()
    {
        var entries = new LeaderboardEntry[]
        {
            new LeaderboardEntry("AAA", 1000),
            new LeaderboardEntry("BBB", 900),
            new LeaderboardEntry("CCC", 800),
            new LeaderboardEntry("DDD", 700),
            new LeaderboardEntry("EEE", 600)
        };

        yield return null; // wait for Awake to run

        _widget.Refresh(entries);
        yield return null; // allow UI updates

        // Verify header exists
        var region = _doc.rootVisualElement.Q<VisualElement>("leaderboard-region");
        Assert.IsNotNull(region, "leaderboard-region should exist");

        // First child should be the header label
        var children = region.Children().ToArray();
        Assert.GreaterOrEqual(children.Length, 6, "Should have header + 5 rows");

        // Check header text
        var firstChild = children[0];
        Assert.AreEqual("TOP SCORES", ((Label)firstChild).text);
    }

    [UnityTest]
    public IEnumerator Refresh_WithTwoEntries_DisplaysTwoPlusThreePlaceholders()
    {
        var entries = new LeaderboardEntry[]
        {
            new LeaderboardEntry("AAA", 500),
            new LeaderboardEntry("BBB", 400)
        };

        yield return null; // wait for Awake to run
        _widget.Refresh(entries);
        yield return null;

        var region = _doc.rootVisualElement.Q<VisualElement>("leaderboard-region");
        Assert.IsNotNull(region);

        // First child is header, then 5 row containers
        var children = region.Children().ToArray();
        Assert.AreEqual(6, children.Length, "Should have 1 header + 5 rows");

        // Row 0: populated entry
        var row0 = children[1] as VisualElement;
        var labels0 = row0.Children().ToArray();
        Assert.AreEqual("1.", ((Label)labels0[0]).text);
        Assert.AreEqual("AAA   ", ((Label)labels0[1]).text); // padded to 3+ spaces
        Assert.AreEqual("0000500", ((Label)labels0[2]).text);

        // Row 1: populated entry
        var row1 = children[2] as VisualElement;
        var labels1 = row1.Children().ToArray();
        Assert.AreEqual("2.", ((Label)labels1[0]).text);
        Assert.AreEqual("BBB   ", ((Label)labels1[1]).text);
        Assert.AreEqual("0000400", ((Label)labels1[2]).text);

        // Row 2: placeholder
        var row2 = children[3] as VisualElement;
        var labels2 = row2.Children().ToArray();
        Assert.AreEqual("---", ((Label)labels2[1]).text);
        Assert.AreEqual("0000000", ((Label)labels2[2]).text);

        // Row 4: also placeholder
        var row4 = children[5] as VisualElement;
        var labels4 = row4.Children().ToArray();
        Assert.AreEqual("---", ((Label)labels4[1]).text);
        Assert.AreEqual("0000000", ((Label)labels4[2]).text);
    }

    [UnityTest]
    public IEnumerator Refresh_WithNullArray_DisplaysAllPlaceholders()
    {
        yield return null; // wait for Awake to run
        _widget.Refresh(null);
        yield return null;

        var region = _doc.rootVisualElement.Q<VisualElement>("leaderboard-region");
        Assert.IsNotNull(region);

        var children = region.Children().ToArray();
        Assert.AreEqual(6, children.Length, "Should have 1 header + 5 rows even with null input");

        // All 5 rows should be placeholders
        for (int i = 0; i < 5; i++)
        {
            var row = children[i + 1] as VisualElement;
            var labels = row.Children().ToArray();
            Assert.AreEqual("---", ((Label)labels[1]).text, $"Row {i} name should be ---");
            Assert.AreEqual("0000000", ((Label)labels[2]).text, $"Row {i} score should be 0000000");
        }
    }

    [UnityTest]
    public IEnumerator Refresh_WithEmptyArray_DisplaysAllPlaceholders()
    {
        yield return null; // wait for Awake to run
        _widget.Refresh(new LeaderboardEntry[0]);
        yield return null;

        var region = _doc.rootVisualElement.Q<VisualElement>("leaderboard-region");
        Assert.IsNotNull(region);

        var children = region.Children().ToArray();
        Assert.AreEqual(6, children.Length, "Should have 1 header + 5 rows even with empty input");
    }

    [UnityTest]
    public IEnumerator Refresh_Scores_ZeroPaddedToSevenDigits()
    {
        var entries = new LeaderboardEntry[]
        {
            new LeaderboardEntry("AAA", 0),
            new LeaderboardEntry("BBB", 1),
            new LeaderboardEntry("CCC", 9999999)
        };

        yield return null;
        _widget.Refresh(entries);
        yield return null;

        var region = _doc.rootVisualElement.Q<VisualElement>("leaderboard-region");
        var children = region.Children().ToArray();

        // Row 0: score 0 -> "0000000"
        var row0Labels = (children[1] as VisualElement).Children().ToArray();
        Assert.AreEqual("0000000", ((Label)row0Labels[2]).text);

        // Row 1: score 1 -> "0000001"
        var row1Labels = (children[2] as VisualElement).Children().ToArray();
        Assert.AreEqual("0000001", ((Label)row1Labels[2]).text);

        // Row 2: score 9999999 -> "9999999"
        var row2Labels = (children[3] as VisualElement).Children().ToArray();
        Assert.AreEqual("9999999", ((Label)row2Labels[2]).text);
    }

    [UnityTest]
    public IEnumerator Refresh_MixedNullEntries_FillsPlaceholders()
    {
        var entries = new LeaderboardEntry[]
        {
            new LeaderboardEntry("AAA", 100),
            null,
            new LeaderboardEntry("CCC", 300)
        };

        yield return null;
        _widget.Refresh(entries);
        yield return null;

        var region = _doc.rootVisualElement.Q<VisualElement>("leaderboard-region");
        var children = region.Children().ToArray();

        // Row 0: populated
        var row0Labels = (children[1] as VisualElement).Children().ToArray();
        Assert.AreEqual("AAA   ", ((Label)row0Labels[1]).text);

        // Row 1: null entry -> placeholder
        var row1Labels = (children[2] as VisualElement).Children().ToArray();
        Assert.AreEqual("---", ((Label)row1Labels[1]).text);
        Assert.AreEqual("0000000", ((Label)row1Labels[2]).text);

        // Row 2: populated
        var row2Labels = (children[3] as VisualElement).Children().ToArray();
        Assert.AreEqual("CCC   ", ((Label)row2Labels[1]).text);
        Assert.AreEqual("0000300", ((Label)row2Labels[2]).text);

        // Row 3: out of bounds -> placeholder
        var row3Labels = (children[4] as VisualElement).Children().ToArray();
        Assert.AreEqual("---", ((Label)row3Labels[1]).text);
    }

    [UnityTest]
    public IEnumerator Refresh_Ranks_DisplayCorrectly()
    {
        var entries = new LeaderboardEntry[]
        {
            new LeaderboardEntry("A", 1),
            new LeaderboardEntry("B", 2),
            new LeaderboardEntry("C", 3),
            new LeaderboardEntry("D", 4),
            new LeaderboardEntry("E", 5)
        };

        yield return null;
        _widget.Refresh(entries);
        yield return null;

        var region = _doc.rootVisualElement.Q<VisualElement>("leaderboard-region");
        var children = region.Children().ToArray();

        for (int i = 0; i < 5; i++)
        {
            var row = children[i + 1] as VisualElement;
            var rankLabel = ((Label)row.Children().ToArray()[0]);
            Assert.AreEqual($"{i + 1}.", rankLabel.text, $"Rank at row {i} should be '{i + 1}.'");
        }
    }

    [UnityTest]
    public IEnumerator Refresh_ClearsPreviousContent()
    {
        var entries1 = new LeaderboardEntry[]
        {
            new LeaderboardEntry("AAA", 100)
        };

        yield return null;
        _widget.Refresh(entries1);
        yield return null;

        // Now refresh with different data
        var entries2 = new LeaderboardEntry[]
        {
            new LeaderboardEntry("XXX", 999),
            new LeaderboardEntry("YYY", 888)
        };

        _widget.Refresh(entries2);
        yield return null;

        var region = _doc.rootVisualElement.Q<VisualElement>("leaderboard-region");
        var children = region.Children().ToArray();
        Assert.AreEqual(6, children.Length, "Should still be exactly 1 header + 5 rows after re-refresh");

        // First row should now show new data
        var row0Labels = (children[1] as VisualElement).Children().ToArray();
        Assert.AreEqual("XXX   ", ((Label)row0Labels[1]).text);
    }
}
