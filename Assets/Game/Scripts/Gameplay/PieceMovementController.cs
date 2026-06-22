using System;
using OneTimeGames.CoreSystems;

namespace Game.Gameplay
{
    public class PieceMovementController
    {
        public event Action<PieceState> OnPieceLocked;
        public event Action OnSpawnFailed;
        public event Action OnPieceMoved;

        private readonly PlayfieldModel _model;
        private readonly GameStateMachine _stateMachine;

        private PieceState _currentPiece;
        private int _gravityTimer;
        private int _leftHoldFrames;
        private int _rightHoldFrames;
        private bool _landed;
        private bool _hasPiece;

        public PieceState CurrentPiece => _currentPiece;

        public PieceMovementController(PlayfieldModel model, GameStateMachine stateMachine)
        {
            _model = model;
            _stateMachine = stateMachine;
        }

        public void SpawnPiece(PieceType type)
        {
            var candidate = new PieceState { Type = type, Row = GameRules.PLAYFIELD_BUFFER_ROWS, Col = 3, Rotation = 0 };

            if (!CanPlace(candidate))
            {
                OnSpawnFailed?.Invoke();
                return;
            }

            _currentPiece = candidate;
            _gravityTimer = 0;
            _leftHoldFrames = 0;
            _rightHoldFrames = 0;
            _landed = false;
            _hasPiece = true;
        }

        public void Tick(int currentLevel, bool leftHeld, bool rightHeld, bool rotatePressed, bool softDropHeld)
        {
            if (!_hasPiece)
                return;

            HandleInput(leftHeld, rightHeld, rotatePressed);
            HandleGravity(currentLevel, softDropHeld);
        }

        private void HandleInput(bool leftHeld, bool rightHeld, bool rotatePressed)
        {
            if (!leftHeld)
            {
                _leftHoldFrames = 0;
            }
            else
            {
                if (_leftHoldFrames == 0)
                {
                    TryMove(0, -1);
                }
                else if (_leftHoldFrames >= GameRules.AUTO_REPEAT_INITIAL_DELAY_FRAMES)
                {
                    int repeatFrames = _leftHoldFrames - GameRules.AUTO_REPEAT_INITIAL_DELAY_FRAMES;
                    if (repeatFrames % GameRules.AUTO_REPEAT_REPEAT_RATE_FRAMES == 0)
                        TryMove(0, -1);
                }
                _leftHoldFrames++;
            }

            if (!rightHeld)
            {
                _rightHoldFrames = 0;
            }
            else
            {
                if (_rightHoldFrames == 0)
                {
                    TryMove(0, 1);
                }
                else if (_rightHoldFrames >= GameRules.AUTO_REPEAT_INITIAL_DELAY_FRAMES)
                {
                    int repeatFrames = _rightHoldFrames - GameRules.AUTO_REPEAT_INITIAL_DELAY_FRAMES;
                    if (repeatFrames % GameRules.AUTO_REPEAT_REPEAT_RATE_FRAMES == 0)
                        TryMove(0, 1);
                }
                _rightHoldFrames++;
            }

            if (rotatePressed)
                TryRotate();
        }

        private void HandleGravity(int currentLevel, bool softDropHeld)
        {
            int framesPerRow = GameRules.GetFramesPerRow(currentLevel);

            if (softDropHeld)
                _gravityTimer = framesPerRow;
            else
                _gravityTimer++;

            if (_gravityTimer >= framesPerRow)
            {
                _gravityTimer = 0;

                if (_landed)
                {
                    LockPiece();
                }
                else
                {
                    if (!TryMove(1, 0))
                        _landed = true;
                }
            }
        }

        private bool TryMove(int rowDelta, int colDelta)
        {
            var candidate = new PieceState
            {
                Type = _currentPiece.Type,
                Row = _currentPiece.Row + rowDelta,
                Col = _currentPiece.Col + colDelta,
                Rotation = _currentPiece.Rotation
            };

            if (!CanPlace(candidate))
                return false;

            _currentPiece = candidate;
            if (_landed && CanFall())
                _landed = false;
            if (colDelta != 0)
                OnPieceMoved?.Invoke();
            return true;
        }

        private void TryRotate()
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
                if (_landed && CanFall())
                    _landed = false;
                OnPieceMoved?.Invoke();
            }
        }

        private bool CanFall()
        {
            var candidate = new PieceState
            {
                Type = _currentPiece.Type,
                Row = _currentPiece.Row + 1,
                Col = _currentPiece.Col,
                Rotation = _currentPiece.Rotation
            };
            return CanPlace(candidate);
        }

        private void LockPiece()
        {
            var locked = _currentPiece;
            _hasPiece = false;
            _model.LockPiece(locked);
            OnPieceLocked?.Invoke(locked);
        }

        private bool CanPlace(PieceState candidate)
        {
            int typeIndex = (int)candidate.Type;
            int rot = candidate.Rotation % 4;

            for (int i = 0; i < 4; i++)
            {
                int r = candidate.Row + PlayfieldModel.CellOffsets[typeIndex, rot, i * 2];
                int c = candidate.Col + PlayfieldModel.CellOffsets[typeIndex, rot, i * 2 + 1];

                if (!_model.IsInBounds(r, c) || !_model.IsEmpty(r, c))
                    return false;
            }
            return true;
        }
    }
}
