/// <summary>
/// Represents a single leaderboard entry with a name and score.
/// Matches the server API schema: { "name": string, "score": integer }.
/// </summary>
public class LeaderboardEntry
{
    public string Name;
    public int Score;

    public LeaderboardEntry(string name, int score)
    {
        Name = name;
        Score = score;
    }
}
