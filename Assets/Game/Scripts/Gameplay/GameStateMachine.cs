using System;

/// <summary>
/// Minimal state machine for game flow control.
/// Tracks the current game state (e.g. Idle, Spawning, Playing, LineClear, GameOver).
/// </summary>
public class GameStateMachine
{
    public enum GameState
    {
        // Legacy states (kept for backward compatibility)
        WaitingForPiece,
        Dropping,
        GameOver,
        // Marathon game loop states
        Idle,
        Spawning,
        Playing,
        LineClear
    }

    private GameState _state;

    public GameState CurrentState
    {
        get { return _state; }
        set { ChangeState(value); }
    }

    public event System.Action<GameState> OnStateChanged;

    public GameStateMachine()
    {
        _state = GameState.Idle;
    }

    public void ChangeState(GameState newState)
    {
        if (_state == newState)
            return;

        _state = newState;
        OnStateChanged?.Invoke(newState);
    }
}
