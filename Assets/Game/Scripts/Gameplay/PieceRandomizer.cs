using System;

namespace Game.Gameplay
{
    public class PieceRandomizer
    {
        private readonly Random _random;
        private int _lastPieceIndex;
        private PieceType _nextPiece;

        public PieceRandomizer(int seed)
        {
            _random = new Random(seed);
            _lastPieceIndex = -1;
            _nextPiece = GenerateNext();
        }

        public PieceRandomizer()
        {
            _random = new Random(Environment.TickCount);
            _lastPieceIndex = -1;
            _nextPiece = GenerateNext();
        }

        public PieceType Next()
        {
            PieceType result = _nextPiece;
            _lastPieceIndex = (int)result;
            _nextPiece = GenerateNext();
            return result;
        }

        public PieceType Peek()
        {
            return _nextPiece;
        }

        private PieceType GenerateNext()
        {
            int roll = _random.Next(0, 8);
            if (roll == 7 || roll == _lastPieceIndex)
                roll = _random.Next(0, 7);
            return (PieceType)roll;
        }
    }
}
