using System;
using UnityEngine;

/// <summary>
/// Central coordinator for a single Tetris game session.
/// Manages the game loop, scoring, level progression, and state transitions.
/// </summary>
public class GameplayController : MonoBehaviour
{
    // -----------------------------------------------------------------------
    // Internal components
    // -----------------------------------------------------------------------
    private GameStateMachine _stateMachine;
    private PlayfieldModel _playfield;
    private PieceMovementController _pieceController;
    private PieceRandomizer _randomizer;

    // -----------------------------------------------------------------------
    // Game state
    // -----------------------------------------------------------------------
    private int _currentScore;
    private int _totalLinesCleared;

    // -----------------------------------------------------------------------
    // Events
    // -----------------------------------------------------------------------

    /// <summary>
    /// Fires on any state transition so UI can refresh.
    /// </summary>
    public event System.Action OnStateChanged;

    /// <summary>
    /// Fires when the game ends with the final score.
    /// </summary>
    public event System.Action<int> OnGameOver;

    // -----------------------------------------------------------------------
    // Public properties
    // -----------------------------------------------------------------------

    /// <summary>
    /// The current accumulated score.
    /// </summary>
    public int CurrentScore
    {
        get { return _currentScore; }
    }

    /// <summary>
    /// The current game level, derived from total lines cleared.
    /// </summary>
    public int CurrentLevel
    {
        get { return GameRules.GetLevel(_totalLinesCleared); }
    }

    /// <summary>
    /// The total number of lines cleared in this game session.
    /// </summary>
    public int TotalLinesCleared
    {
        get { return _totalLinesCleared; }
    }

    /// <summary>
    /// The next piece type that will be spawned (from the randomizer's peek).
    /// </summary>
    public PieceType NextPiece
    {
        get { return _randomizer.Peek(); }
    }

    /// <summary>
    /// The currently active falling piece, or default if no piece is active.
    /// </summary>
    public PieceState ActivePiece
    {
        get { return _pieceController.CurrentPiece; }
    }

    /// <summary>
    /// The playfield model for direct grid access.
    /// </summary>
    public PlayfieldModel Playfield
    {
        get { return _playfield; }
    }

    // -----------------------------------------------------------------------
    // Unity lifecycle
    // -----------------------------------------------------------------------

    private void Awake()
    {
        _playfield = new PlayfieldModel();
        _stateMachine = new GameStateMachine();
        _pieceController = new PieceMovementController(_playfield, _stateMachine);
        _randomizer = new PieceRandomizer(Environment.TickCount);

        // Wire up piece controller events
        _pieceController.OnPieceLocked += OnPieceLockedHandler;
        _pieceController.OnSpawnFailed += OnSpawnFailedHandler;

        // Wire up state machine events
        _stateMachine.OnStateChanged += OnStateMachineStateChanged;

        // Start in Idle state
        _stateMachine.CurrentState = GameStateMachine.GameState.Idle;
    }

    /// <summary>
    /// Called each frame to drive the game loop.
    /// </summary>
    private void Update()
    {
        Tick();
    }

    // -----------------------------------------------------------------------
    // Public API
    // -----------------------------------------------------------------------

    /// <summary>
    /// Resets all game state and starts a new game.
    /// Transitions to "Spawning" state.
    /// </summary>
    public void StartGame()
    {
        _currentScore = 0;
        _totalLinesCleared = 0;
        _playfield.Clear();

        // Transition to Spawning to begin the game
        _stateMachine.ChangeState(GameStateMachine.GameState.Spawning);
    }

    /// <summary>
    /// Main tick called from Update(). Processes the current state and input.
    /// </summary>
    public void Tick()
    {
        var state = _stateMachine.CurrentState;

        switch (state)
        {
            case GameStateMachine.GameState.Idle:
                // Do nothing -- waiting for StartGame()
                break;

            case GameStateMachine.GameState.Spawning:
                SpawnNextPiece();
                break;

            case GameStateMachine.GameState.Playing:
                ProcessPlayingState();
                break;

            case GameStateMachine.GameState.LineClear:
                ProcessLineClearState();
                break;

            case GameStateMachine.GameState.GameOver:
                // Game is over -- stop all processing
                break;

            // Legacy states -- no-op
            case GameStateMachine.GameState.WaitingForPiece:
            case GameStateMachine.GameState.Dropping:
                break;
        }
    }

    // -----------------------------------------------------------------------
    // State handlers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Spawns the next piece from the randomizer.
    /// If spawn succeeds, transitions to "Playing".
    /// If spawn fails, transitions to "GameOver".
    /// </summary>
    private void SpawnNextPiece()
    {
        PieceType nextType = _randomizer.Next();
        _pieceController.SpawnPiece(nextType);

        // OnSpawnFailed is raised synchronously by SpawnPiece if blocked
        // If we reach here, spawn succeeded -- transition to Playing
        _stateMachine.ChangeState(GameStateMachine.GameState.Playing);
    }

    /// <summary>
    /// Processes the Playing state: reads input and ticks the piece controller.
    /// </summary>
    private void ProcessPlayingState()
    {
        // Read input using Unity Legacy Input Manager
        bool leftHeld = Input.GetKey(KeyCode.LeftArrow);
        bool rightHeld = Input.GetKey(KeyCode.RightArrow);
        bool rotatePressed = Input.GetKeyDown(KeyCode.UpArrow);
        bool softDropHeld = Input.GetKey(KeyCode.DownArrow);

        int level = CurrentLevel;
        _pieceController.Tick(level, leftHeld, rightHeld, rotatePressed, softDropHeld);
    }

    /// <summary>
    /// Processes the LineClear state: clears lines, computes score, updates level,
    /// then transitions to "Spawning" for the next piece.
    /// </summary>
    private void ProcessLineClearState()
    {
        int levelAtClear = CurrentLevel;
        int linesCleared = _playfield.ClearLines();

        if (linesCleared > 0)
        {
            // Compute score using the level at the time of line clear (before progression)
            int scoreAdded = GameRules.CalculateScore(linesCleared, levelAtClear);
            _currentScore += scoreAdded;
            _totalLinesCleared += linesCleared;
        }

        // Transition to Spawning for the next piece
        _stateMachine.ChangeState(GameStateMachine.GameState.Spawning);
    }

    // -----------------------------------------------------------------------
    // Event handlers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Called when a piece is locked into the playfield.
    /// Transitions to "LineClear" state to process line clearing.
    /// </summary>
    private void OnPieceLockedHandler(PieceState lockedPiece)
    {
        _stateMachine.ChangeState(GameStateMachine.GameState.LineClear);
    }

    /// <summary>
    /// Called when a piece cannot be spawned (spawn position blocked).
    /// Transitions to "GameOver" state.
    /// </summary>
    private void OnSpawnFailedHandler()
    {
        _stateMachine.ChangeState(GameStateMachine.GameState.GameOver);
    }

    /// <summary>
    /// Called whenever the state machine changes state.
    /// Raises OnStateChanged for UI listeners.
    /// Also handles GameOver event.
    /// </summary>
    private void OnStateMachineStateChanged(GameStateMachine.GameState newState)
    {
        // Raise the public OnStateChanged event
        OnStateChanged?.Invoke();

        // If transitioning to GameOver, raise OnGameOver with final score
        if (newState == GameStateMachine.GameState.GameOver)
        {
            OnGameOver?.Invoke(_currentScore);
        }
    }
}
