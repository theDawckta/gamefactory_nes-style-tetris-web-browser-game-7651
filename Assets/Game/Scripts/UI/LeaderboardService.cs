using System;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Service for fetching leaderboard scores from the game server.
/// Provides both async and synchronous access patterns.
/// </summary>
public class LeaderboardService
{
    private static readonly string ServerUrl = "http://localhost/scores";
    private static LeaderboardEntry[] _cachedScores;

    /// <summary>
    /// Fetches the leaderboard scores from the server asynchronously.
    /// Returns cached scores if the server is unavailable.
    /// </summary>
    public static async Task<LeaderboardEntry[]> GetScoresAsync()
    {
        try
        {
            using (var client = new HttpClient())
            {
                var json = await client.GetStringAsync(ServerUrl);
                var data = JsonUtility.FromJson<LeaderboardEntryArray>(json);
                _cachedScores = data.entries;
                return _cachedScores;
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[LeaderboardService] Could not fetch scores from server: " + ex.Message);
            return _cachedScores ?? GetDefaultScores();
        }
    }

    /// <summary>
    /// Synchronously returns the leaderboard scores.
    /// Returns cached scores or defaults if nothing has been fetched yet.
    /// </summary>
    public static LeaderboardEntry[] GetScores()
    {
        if (_cachedScores != null)
        {
            return _cachedScores;
        }
        return GetDefaultScores();
    }

    private static LeaderboardEntry[] GetDefaultScores()
    {
        _cachedScores = new LeaderboardEntry[]
        {
            new LeaderboardEntry("AAA", 10000),
            new LeaderboardEntry("BBB", 8000),
            new LeaderboardEntry("CCC", 6000),
            new LeaderboardEntry("DDD", 4000),
            new LeaderboardEntry("EEE", 2000)
        };
        return _cachedScores;
    }

    /// <summary>
    /// Internal wrapper for JSON deserialization.
    /// </summary>
    [Serializable]
    private class LeaderboardEntryArray
    {
        public LeaderboardEntry[] entries;
    }
}
