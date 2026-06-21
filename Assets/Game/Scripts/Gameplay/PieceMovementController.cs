using System;

/// <summary>
/// Manages the currently active Tetromino piece: position, rotation,
/// gravity tick, auto-repeat input handling, and NES lock behavior.
/// Plain C# class -- not a MonoBehaviour.
/// </summary>
public class PieceMovementController
{
    // -----------------------------------------------------------------------
    // Dependencies
    // -----------------------------------------------------------------------
    private readonly PlayfieldModel _model;
    private readonly GameStateMachine _stateMachine;

    // -----------------------------------------------------------------------
    // Active piece state
    // -----------------------------------------------------------------------
    private PieceState _currentPiece;
    private int _gravityTimer;
    private int _leftHoldFrames;
    private int _rightHoldFrames;
    private bool _landed;
    private bool _hasActivePiece;

    // -----------------------------------------------------------------------
    // Events
    // -----------------------------------------------------------------------
    public event System.Action<PieceState> OnPieceLocked;
    public event System.Action OnSpawnFailed;

    // -----------------------------------------------------------------------
    // Public API
    // -----------------------------------------------------------------------
    public PieceState CurrentPiece
    {
        get { return _currentPiece; }
    }

    public PieceMovementController(PlayfieldModel model, GameStateMachine stateMachine)
    {
        _model = model;
        _stateMachine = stateMachine;
        _gravityTimer = 0;
        _leftHoldFrames = 0;
        _rightHoldFrames = 0;
        _landed = false;
        _hasActivePiece = false;
    }

    /// <summary>
    /// Spawns a new piece at the standard NES spawn position
    /// (row 1, center column, rotation 0). Row 1 keeps all piece offsets within
    /// the valid grid range; pieces with -1 row offsets (S, Z, J, L) need row >= 1.
    /// Raises OnSpawnFailed if the spawn position is blocked.
    /// </summary>
    public void SpawnPiece(PieceType type)
    {
        _currentPiece.Type = type;
        _currentPiece.Row = 1;
        _currentPiece.Col = GameRules.PLAYFIELD_WIDTH / 2 - 1;
        _currentPiece.Rotation = 0;

        if (!CanPlace(_currentPiece))
        {
            OnSpawnFailed?.Invoke();
            _hasActivePiece = false;
            return;
        }

        _hasActivePiece = true;
        _landed = false;
        _gravityTimer = 0;
        _leftHoldFrames = 0;
        _rightHoldFrames = 0;
    }

    /// <summary>
    /// Main game loop tick. Processes input then gravity.
    /// </summary>
    public void Tick(int currentLevel, bool leftHeld, bool rightHeld, bool rotatePressed, bool softDropHeld)
    {
        if (!_hasActivePiece)
            return;

        // 1. Process input (move, rotate)
        HandleInput(leftHeld, rightHeld, rotatePressed, softDropHeld);

        // 2. Process gravity
        int framesPerRow = softDropHeld ? 1 : GameRules.GetFramesPerRow(currentLevel);
        _gravityTimer++;

        if (_gravityTimer >= framesPerRow)
        {
            _gravityTimer = 0;

            if (_landed)
            {
                // NES lock: lock immediately on the very next gravity tick after landing -- no added delay
                LockPiece();
                return;
            }

            // Attempt to move piece down one row
            var candidate = new PieceState
            {
                Type = _currentPiece.Type,
                Row = _currentPiece.Row + 1,
                Col = _currentPiece.Col,
                Rotation = _currentPiece.Rotation
            };

            if (CanPlace(candidate))
            {
                _currentPiece = candidate;
            }
            else
            {
                // Cannot move down -- piece has landed
                _landed = true;
            }
        }
    }

    /// <summary>
    /// Handles player input: left/right movement with auto-repeat,
    /// rotation (no wall kick), and soft drop (handled via gravity override in Tick).
    /// </summary>
    public void HandleInput(bool leftHeld, bool rightHeld, bool rotatePressed, bool softDropHeld)
    {
        if (!_hasActivePiece)
            return;

        // --- Left movement with auto-repeat ---
        if (leftHeld)
        {
            if (ShouldAutoRepeat(_leftHoldFrames))
            {
                var candidate = new PieceState
                {
                    Type = _currentPiece.Type,
                    Row = _currentPiece.Row,
                    Col = _currentPiece.Col - 1,
                    Rotation = _currentPiece.Rotation
                };
                if (CanPlace(candidate))
                {
                    _currentPiece = candidate;
                }
            }
            _leftHoldFrames++;
        }
        else
        {
            _leftHoldFrames = 0;
        }

        // --- Right movement with auto-repeat ---
        if (rightHeld)
        {
            if (ShouldAutoRepeat(_rightHoldFrames))
            {
                var candidate = new PieceState
                {
                    Type = _currentPiece.Type,
                    Row = _currentPiece.Row,
                    Col = _currentPiece.Col + 1,
                    Rotation = _currentPiece.Rotation
                };
                if (CanPlace(candidate))
                {
                    _currentPiece = candidate;
                }
            }
            _rightHoldFrames++;
        }
        else
        {
            _rightHoldFrames = 0;
        }

        // --- Rotation (single frame, no auto-repeat, no wall kick) ---
        if (rotatePressed)
        {
            var candidate = new PieceState
            {
                Type = _currentPiece.Type,
                Row = _currentPiece.Row,
                Col = _currentPiece.Col,
                Rotation = (_currentPiece.Rotation + 1) % 4
            };
            if (CanPlace(candidate))
            {
                _currentPiece = candidate;
            }
            // If blocked, piece stays in place -- NES has no wall kick
        }

        // --- Soft drop is handled in Tick via gravity timer override ---
    }

    // -----------------------------------------------------------------------
    // Auto-repeat helper
    // -----------------------------------------------------------------------

    /// <summary>
    /// Returns true when the piece should move due to auto-repeat timing.
    /// Frame 0: immediate move on first hold.
    /// Frames 1..15: no repeat (initial delay).
    /// Frame 16+: repeat every 6 frames.
    /// </summary>
    private bool ShouldAutoRepeat(int holdFrames)
    {
        // First frame of hold -- move immediately
        if (holdFrames == 0)
            return true;

        // After initial delay, repeat at fixed rate
        if (holdFrames >= GameRules.AUTO_REPEAT_INITIAL_DELAY_FRAMES)
        {
            int elapsedAfterDelay = holdFrames - GameRules.AUTO_REPEAT_INITIAL_DELAY_FRAMES;
            if (elapsedAfterDelay % GameRules.AUTO_REPEAT_REPEAT_RATE_FRAMES == 0)
                return true;
        }

        return false;
    }

    // -----------------------------------------------------------------------
    // Collision detection
    // -----------------------------------------------------------------------

    /// <summary>
    /// Checks whether all cells of the candidate piece state fit within
    /// the playfield bounds and do not overlap any occupied cells.
    /// </summary>
    private bool CanPlace(PieceState candidate)
    {
        var offsets = PlayfieldModel.GetCellOffsets(candidate.Type, candidate.Rotation);
        int cellCount = offsets.Length / 2;

        for (int i = 0; i < cellCount; i++)
        {
            int row = candidate.Row + offsets[i * 2];
            int col = candidate.Col + offsets[i * 2 + 1];

            if (!_model.IsInBounds(row, col))
                return false;
            if (!_model.IsEmpty(row, col))
                return false;
        }

        return true;
    }

    // -----------------------------------------------------------------------
    // Lock
    // -----------------------------------------------------------------------

    /// <summary>
    /// Locks the current piece into the playfield grid and raises OnPieceLocked.
    /// </summary>
    private void LockPiece()
    {
        _model.LockPiece(_currentPiece);
        OnPieceLocked?.Invoke(_currentPiece);
        _hasActivePiece = false;
    }
}
