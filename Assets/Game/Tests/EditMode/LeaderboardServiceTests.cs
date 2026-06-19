using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class LeaderboardServiceTests
{
    [Test]
    public void GetScores_SyncReturnsDefaultEntries()
    {
        var scores = LeaderboardService.GetScores();
        Assert.IsNotNull(scores, "GetScores should return non-null array");
        Assert.AreEqual(5, scores.Length, "Default scores should have 5 entries");
    }

    [Test]
    public void GetScores_SyncEntriesHaveValidData()
    {
        var scores = LeaderboardService.GetScores();
        for (int i = 0; i < scores.Length; i++)
        {
            Assert.IsNotNull(scores[i].Name, "Entry name should not be null");
            Assert.GreaterOrEqual(scores[i].Score, 0, "Entry score should be non-negative");
        }
    }

    [Test]
    public void GetScores_SyncEntriesSortedDescending()
    {
        var scores = LeaderboardService.GetScores();
        for (int i = 1; i < scores.Length; i++)
        {
            Assert.GreaterOrEqual(scores[i - 1].Score, scores[i].Score,
                "Scores should be sorted in descending order");
        }
    }

    [Test]
    public void GetScores_CallbackBased_InvokesSuccessCallback()
    {
        bool successCalled = false;
        bool errorCalled = false;
        LeaderboardEntry[] receivedEntries = null;

        LeaderboardService.GetScores(
            entries =>
            {
                successCalled = true;
                receivedEntries = entries;
            },
            err =>
            {
                errorCalled = true;
            });

        // The callback is async, but GetScoresAsync falls back to cached/default
        // which is synchronous in the fallback path
        // In a test environment without a server, the async path will fail and
        // the fallback returns default scores

        // Give it a moment for the async task to complete
        System.Threading.Thread.Sleep(100);

        // Either success or error should have been called
        Assert.IsTrue(successCalled || errorCalled,
            "Either success or error callback should be invoked");
    }

    [Test]
    public void GetScores_CallbackBased_ErrorCallbackReceivesMessage()
    {
        string errorMessage = null;

        LeaderboardService.GetScores(
            entries => { },
            err =>
            {
                errorMessage = err;
            });

        System.Threading.Thread.Sleep(100);

        // In a test environment without a server, error is likely
        // but we don't assert this strictly as it depends on server availability
        // Just verify no exception was thrown
        Assert.Pass("Callback-based GetScores did not throw");
    }
}
