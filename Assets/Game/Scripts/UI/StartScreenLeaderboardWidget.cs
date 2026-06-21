using Game.Gameplay;
using UnityEngine;
using UnityEngine.UIElements;

public class StartScreenLeaderboardWidget : MonoBehaviour
{
    private VisualElement _leaderboardRegion;

    private void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        var region = root.Q("leaderboard-region");
        if (region != null)
            PopulateRegion(region);
    }

    public void PopulateRegion(VisualElement leaderboardRegion)
    {
        _leaderboardRegion = leaderboardRegion;
        Refresh(null);
    }

    public void Refresh(LeaderboardEntry[] entries)
    {
        if (_leaderboardRegion == null)
            return;

        _leaderboardRegion.Clear();
        _leaderboardRegion.style.alignItems = Align.Center;

        var header = new Label("TOP SCORES");
        header.style.unityTextAlign = TextAnchor.MiddleCenter;
        _leaderboardRegion.Add(header);

        for (int i = 0; i < 5; i++)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.width = Length.Percent(100);

            string nameText = "---";
            string scoreText = "0000000";

            if (entries != null && i < entries.Length && entries[i] != null)
            {
                nameText = entries[i].name;
                scoreText = entries[i].score.ToString("D7");
            }

            var rankLabel = new Label((i + 1) + ".");
            rankLabel.style.unityTextAlign = TextAnchor.MiddleLeft;

            var nameLabel = new Label(nameText);
            nameLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            nameLabel.style.flexGrow = 1;

            var scoreLabel = new Label(scoreText);
            scoreLabel.style.unityTextAlign = TextAnchor.MiddleRight;

            row.Add(rankLabel);
            row.Add(nameLabel);
            row.Add(scoreLabel);

            _leaderboardRegion.Add(row);
        }
    }
}
