using UnityEngine;
using UnityEngine.UIElements;

public class GameScreen : BaseScreen
{
    public VisualElement ScoreRegion { get; private set; }
    public VisualElement LevelRegion { get; private set; }
    public VisualElement LinesRegion { get; private set; }
    public VisualElement NextRegion { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        ScoreRegion = Root?.Q("score-region");
        LevelRegion = Root?.Q("level-region");
        LinesRegion = Root?.Q("lines-region");
        NextRegion = Root?.Q("next-region");
    }
}
