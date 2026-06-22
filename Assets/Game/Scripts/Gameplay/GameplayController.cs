using System;
using OneTimeGames.CoreSystems;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Gameplay
{
    public class GameplayController : MonoBehaviour
    {
        private GameStateMachine _stateMachine;
        private PlayfieldModel _playfield;
        private PieceMovementController _pieceController;
        private PieceRandomizer _randomizer;

        private int _currentScore;
        private int _totalLinesCleared;
        private bool _spawnFailed;

        private bool _leftHeld;
        private bool _rightHeld;
        private bool _rotatePressed;
        private bool _softDropHeld;

        public int CurrentScore => _currentScore;
        public int CurrentLevel => GameRules.GetLevel(_totalLinesCleared);
        public int TotalLinesCleared => _totalLinesCleared;
        public PieceType NextPiece => _randomizer.Peek();
        public PieceState ActivePiece => _pieceController.CurrentPiece;
        public PlayfieldModel Playfield => _playfield;

        public event Action OnStateChanged;
        public event Action<int> OnGameOver;

        private void Awake()
        {
            _playfield = new PlayfieldModel();
            _stateMachine = new GameStateMachine();
            _pieceController = new PieceMovementController(_playfield, _stateMachine);
            _randomizer = new PieceRandomizer();

            _pieceController.OnPieceLocked += HandlePieceLocked;
            _pieceController.OnSpawnFailed += HandleSpawnFailed;

            _stateMachine.RegisterState("Idle", null, null, null);
            _stateMachine.RegisterState("Spawning", EnterSpawning, null, null);
            _stateMachine.RegisterState("Playing", null, TickPlaying, null);
            _stateMachine.RegisterState("LineClear", EnterLineClear, null, null);
            _stateMachine.RegisterState("GameOver", EnterGameOver, null, null);

            _stateMachine.TransitionTo("Idle");
        }

        public void StartGame()
        {
            _currentScore = 0;
            _totalLinesCleared = 0;
            _playfield.Clear();
            _randomizer = new PieceRandomizer(Environment.TickCount);
            TransitionToState("Spawning");
        }

        public void StopGame()
        {
            _stateMachine.TransitionTo("Idle");
        }

        public void Tick()
        {
            var kb = Keyboard.current;
            if (kb != null)
            {
                _leftHeld = kb.leftArrowKey.isPressed;
                _rightHeld = kb.rightArrowKey.isPressed;
                _rotatePressed = kb.upArrowKey.wasPressedThisFrame;
                _softDropHeld = kb.downArrowKey.isPressed;
            }
            else
            {
                _leftHeld = false;
                _rightHeld = false;
                _rotatePressed = false;
                _softDropHeld = false;
            }
            _stateMachine.Tick();
        }

        private void Update()
        {
            Tick();
        }

        private void TransitionToState(string stateId)
        {
            _stateMachine.TransitionTo(stateId);
            OnStateChanged?.Invoke();
        }

        private void EnterSpawning()
        {
            _spawnFailed = false;
            _pieceController.SpawnPiece(_randomizer.Next());
            if (!_spawnFailed)
                TransitionToState("Playing");
        }

        private void TickPlaying()
        {
            _pieceController.Tick(CurrentLevel, _leftHeld, _rightHeld, _rotatePressed, _softDropHeld);
        }

        private void HandlePieceLocked(PieceState piece)
        {
            TransitionToState("LineClear");
        }

        private void HandleSpawnFailed()
        {
            _spawnFailed = true;
            TransitionToState("GameOver");
        }

        private void EnterLineClear()
        {
            int levelBeforeClear = CurrentLevel;
            int linesCleared = _playfield.ClearLines();
            _totalLinesCleared += linesCleared;
            _currentScore += GameRules.CalculateScore(linesCleared, levelBeforeClear);
            TransitionToState("Spawning");
        }

        private void EnterGameOver()
        {
            OnGameOver?.Invoke(_currentScore);
        }
    }
}
