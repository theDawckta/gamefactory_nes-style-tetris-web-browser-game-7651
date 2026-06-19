using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// MonoBehaviour widget that populates the #leaderboard-region of a Start Screen UIDocument.
/// Renders a "TOP SCORES" header label and up to 5 leaderboard rows (rank, name, score).
/// All text uses labels styled as pure pixel-text per NES convention.
/// </summary>
public class StartScreenLeaderboardWidget : MonoBehaviour
{
    private const int MaxRows = 5;

    // The UIDocument root element for accessing #leaderboard-region
    private VisualElement _region;

    private void Awake()
    {
        // Cache reference to leaderboard region once the visual tree is available
        var doc = GetComponent<UIDocument>();
        if (doc != null)
        {
            _region = doc.rootVisualElement.Q<VisualElement>("leaderboard-region");
        }
    }

    /// <summary>
    /// Clears all existing rows and repopulates the leaderboard from the provided entries.
    /// Fewer than 5 entries leaves remaining slots showing placeholder values ("---" / "0000000").
    /// null or empty arrays display 5 placeholder rows without errors.
    /// </summary>
    public void Refresh(LeaderboardEntry[] entries)
    {
        if (_region == null)
        {
            Debug.LogError("[StartScreenLeaderboardWidget] Could not find #leaderboard-region element.");
            return;
        }

        // Clear existing content
        _region.Clear();

        // Add header label: "TOP SCORES", uppercase, centered
        var header = new Label("TOP SCORES");
        header.AddToClassList("leaderboard-header");
        _region.Add(header);

        // Build up to 5 rows
        for (int i = 0; i < MaxRows; i++)
        {
            LeaderboardEntry entry = null;
            if (entries != null && i < entries.Length)
            {
                entry = entries[i];
            }

            var row = new VisualElement();
            row.AddToClassList("leaderboard-row");

            // Rank label: left-aligned, e.g. "1." through "5."
            var rankLabel = new Label($"{i + 1}.");
            rankLabel.AddToClassList("leaderboard-rank");
            row.Add(rankLabel);

            // Name label: center-aligned, up to 3 chars; empty slots show "---"
            string nameText;
            if (entry != null && !string.IsNullOrEmpty(entry.Name))
            {
                // Pad to exactly 3 characters if shorter
                nameText = entry.Name.PadRight(3, ' ');
            }
            else
            {
                nameText = "---";
            }
            var nameLabel = new Label(nameText);
            nameLabel.AddToClassList("leaderboard-name");
            row.Add(nameLabel);

            // Score label: right-aligned, zero-padded to 7 digits (NES convention)
            string scoreText;
            if (entry != null)
            {
                scoreText = entry.Score.ToString("D7");
            }
            else
            {
                scoreText = "0000000";
            }
            var scoreLabel = new Label(scoreText);
            scoreLabel.AddToClassList("leaderboard-score");
            row.Add(scoreLabel);

            _region.Add(row);
        }
    }
}
