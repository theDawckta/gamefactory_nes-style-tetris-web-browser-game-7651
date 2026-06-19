using System;

/// <summary>
/// Minimal state machine for game flow control.
/// Tracks the current game state (e.g. WaitingForPiece, Dropping, GameOver).
/// </summary>
public class GameStateMachine
{
    public enum GameState
    {
        WaitingForPiece,
        Dropping,
        GameOver
    }

    private GameState _state;

    public GameState CurrentState
    {
        get { return _state; }
        set { _state = value; }
    }

    public event System.Action<GameState> OnStateChanged;

    public GameStateMachine()
    {
        _state = GameState.WaitingForPiece;
    }

    public void ChangeState(GameState newState)
    {
        _state = newState;
        OnStateChanged?.Invoke(newState);
    }
}
