using UnityEngine;

namespace Game.Gameplay
{
    public static class GameRules
    {
        public const int SCORE_SINGLE = 40;
        public const int SCORE_DOUBLE = 100;
        public const int SCORE_TRIPLE = 300;
        public const int SCORE_TETRIS = 1200;

        public const int LINES_PER_LEVEL = 10;

        public const int AUTO_REPEAT_INITIAL_DELAY_FRAMES = 16;
        public const int AUTO_REPEAT_REPEAT_RATE_FRAMES = 6;

        public const int PLAYFIELD_WIDTH = 10;
        public const int PLAYFIELD_VISIBLE_HEIGHT = 20;
        public const int PLAYFIELD_BUFFER_ROWS = 2;
        public const int PLAYFIELD_TOTAL_HEIGHT = 22;

        private static readonly int[] _basePoints = { 0, SCORE_SINGLE, SCORE_DOUBLE, SCORE_TRIPLE, SCORE_TETRIS };

        public static readonly int[] FramesPerRow =
        {
            48, 43, 38, 33, 28, 23, 18, 13, 8, 6,
            5, 5, 5, 4, 4, 4, 3, 3, 3, 2,
            2, 2, 2, 2, 2, 2, 2, 2, 2, 1
        };

        public static int CalculateScore(int linesCleared, int level)
        {
            if (linesCleared < 1 || linesCleared > 4)
                return 0;
            return _basePoints[linesCleared] * (level + 1);
        }

        public static int GetFramesPerRow(int level)
        {
            return FramesPerRow[Mathf.Clamp(level, 0, 29)];
        }

        public static int GetLevel(int totalLinesCleared)
        {
            return totalLinesCleared / LINES_PER_LEVEL;
        }
    }
}
