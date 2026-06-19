using UnityEngine;

/// <summary>
/// Static class containing all NES Tetris game constants and lookup tables.
/// Single source of truth for gameplay numeric values such as scoring, gravity,
/// level progression, and playfield dimensions.
/// </summary>
public static class GameRules
{
    // -----------------------------------------------------------------------
    // Scoring table (points before level multiplier, per original NES Tetris)
    // -----------------------------------------------------------------------
    public const int SCORE_SINGLE = 40;
    public const int SCORE_DOUBLE = 100;
    public const int SCORE_TRIPLE = 300;
    public const int SCORE_TETRIS = 1200;

    private static readonly int[] s_BasePoints = { 0, SCORE_SINGLE, SCORE_DOUBLE, SCORE_TRIPLE, SCORE_TETRIS };

    /// <summary>
    /// Calculates the score awarded for clearing lines at a given level.
    /// Formula: basePoints[linesCleared] * (level + 1)
    /// Returns 0 when linesCleared is not in the range [1..4].
    /// </summary>
    public static int CalculateScore(int linesCleared, int level)
    {
        if (linesCleared < 1 || linesCleared > 4)
            return 0;

        int clampedLevel = Mathf.Clamp(level, 0, int.MaxValue - 1);
        return s_BasePoints[linesCleared] * (clampedLevel + 1);
    }

    // -----------------------------------------------------------------------
    // Gravity lookup table – exact NES frames-per-row for each level
    // Level 0 through 29; level >= 29 clamps to the entry at index 29.
    // -----------------------------------------------------------------------
    public static readonly int[] FramesPerRow = new[]
    {
        // Levels 0-9
        48, 43, 38, 33, 28, 23, 18, 13, 8, 6,
        // Levels 10-19
        5, 5, 5,
        4, 4, 4,
        3, 3, 3,
        2,
        // Levels 20-27
        2, 2, 2, 2, 2, 2, 2, 2,
        // Level 28
        2,
        // Level 29 (hard drop / fastest gravity)
        1
    };

    /// <summary>
    /// Returns the number of frames per row for the given level.
    /// Levels outside [0..29] are clamped to that range.
    /// </summary>
    public static int GetFramesPerRow(int level)
    {
        return FramesPerRow[Mathf.Clamp(level, 0, FramesPerRow.Length - 1)];
    }

    // -----------------------------------------------------------------------
    // Level progression
    // -----------------------------------------------------------------------
    public const int LINES_PER_LEVEL = 10;

    /// <summary>
    /// Returns the current game level based on the total lines cleared.
    /// </summary>
    public static int GetLevel(int totalLinesCleared)
    {
        return totalLinesCleared / LINES_PER_LEVEL;
    }

    // -----------------------------------------------------------------------
    // Input auto-repeat constants (frames)
    // -----------------------------------------------------------------------
    public const int AUTO_REPEAT_INITIAL_DELAY_FRAMES = 16;
    public const int AUTO_REPEAT_REPEAT_RATE_FRAMES = 6;

    // -----------------------------------------------------------------------
    // Playfield dimensions
    // -----------------------------------------------------------------------
    public const int PLAYFIELD_WIDTH = 10;
    public const int PLAYFIELD_VISIBLE_HEIGHT = 20;
    public const int PLAYFIELD_BUFFER_ROWS = 2;
    public const int PLAYFIELD_TOTAL_HEIGHT = PLAYFIELD_VISIBLE_HEIGHT + PLAYFIELD_BUFFER_ROWS; // 22
}
