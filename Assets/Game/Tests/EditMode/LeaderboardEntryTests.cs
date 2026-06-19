using System;
using NUnit.Framework;

[TestFixture]
public class LeaderboardEntryTests
{
    [Test]
    public void Constructor_SetsNameAndScore()
    {
        var entry = new LeaderboardEntry("ABC", 12345);
        Assert.AreEqual("ABC", entry.Name);
        Assert.AreEqual(12345, entry.Score);
    }

    [Test]
    public void Constructor_AcceptsShortName()
    {
        var entry = new LeaderboardEntry("X", 0);
        Assert.AreEqual("X", entry.Name);
        Assert.AreEqual(0, entry.Score);
    }

    [Test]
    public void Score_ZeroPaddedToSevenDigits()
    {
        var entry = new LeaderboardEntry("ABC", 42);
        string padded = entry.Score.ToString("D7");
        Assert.AreEqual("0000042", padded);
    }

    [Test]
    public void Score_AllSevenDigits()
    {
        var entry = new LeaderboardEntry("ABC", 9999999);
        string padded = entry.Score.ToString("D7");
        Assert.AreEqual("9999999", padded);
    }

    [Test]
    public void Score_ZeroDisplaysAsSevenZeros()
    {
        var entry = new LeaderboardEntry("---", 0);
        string padded = entry.Score.ToString("D7");
        Assert.AreEqual("0000000", padded);
    }

    [Test]
    public void NullName_ProducesPlaceholderText()
    {
        var entries = new LeaderboardEntry[] { null };
        Assert.IsNull(entries[0]);
    }
}
