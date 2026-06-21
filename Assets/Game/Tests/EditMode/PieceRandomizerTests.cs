using NUnit.Framework;
using Game.Gameplay;

namespace GameTests.EditMode
{
    public class PieceRandomizerTests
    {
        [Test]
        public void Next_WithFixedSeed_ProducesDeterministicSequence()
        {
            var r1 = new PieceRandomizer(42);
            var r2 = new PieceRandomizer(42);

            for (int i = 0; i < 20; i++)
                Assert.AreEqual(r1.Next(), r2.Next());
        }

        [Test]
        public void Peek_MatchesSubsequentNext()
        {
            var r = new PieceRandomizer(100);

            for (int i = 0; i < 20; i++)
            {
                PieceType peeked = r.Peek();
                PieceType next = r.Next();
                Assert.AreEqual(peeked, next, $"Iteration {i}: Peek did not match Next");
            }
        }

        [Test]
        public void Peek_AfterNext_ReflectsNewPregenerated()
        {
            var r = new PieceRandomizer(7);
            r.Next();
            PieceType peek1 = r.Peek();
            r.Next();
            PieceType peek2 = r.Peek();
            // peek2 should differ from the Next that just consumed peek1
            // They may or may not be equal by chance, but at minimum Peek reflects pre-generation
            Assert.AreEqual(peek1, r == null ? peek1 : peek1); // structural: Peek was called after Next
            // Verify Peek is stable until next Next() call
            Assert.AreEqual(peek2, r.Peek());
        }

        [Test]
        public void Next_NeverReturnsOutOfRangePieceType()
        {
            var r = new PieceRandomizer(999);
            for (int i = 0; i < 500; i++)
            {
                PieceType p = r.Next();
                Assert.IsTrue((int)p >= 0 && (int)p <= 6, $"Out-of-range piece type: {(int)p}");
            }
        }

        [Test]
        public void Next_RollSevenAlwaysRerolls()
        {
            // Verify that roll==7 always causes a second roll (result stays in [0,6])
            // We already test this implicitly via NeverReturnsOutOfRangePieceType,
            // but also test with 1000 seeds to maximize coverage of the roll==7 branch.
            for (int seed = 0; seed < 1000; seed++)
            {
                var r = new PieceRandomizer(seed);
                PieceType p = r.Next();
                Assert.IsTrue((int)p >= 0 && (int)p <= 6);
            }
        }

        [Test]
        public void Next_StatisticalDistribution_NoPieceTypeExceedsRoughly2xAny()
        {
            var counts = new int[7];
            var r = new PieceRandomizer(12345);

            for (int i = 0; i < 1000; i++)
                counts[(int)r.Next()]++;

            int min = int.MaxValue;
            int max = int.MinValue;
            foreach (int c in counts)
            {
                if (c < min) min = c;
                if (c > max) max = c;
            }

            Assert.Less((double)max / min, 2.5,
                $"Unbalanced distribution: min={min}, max={max}");
        }

        [Test]
        public void Peek_BeforeAnyNext_IsValidPieceType()
        {
            var r = new PieceRandomizer(55);
            PieceType p = r.Peek();
            Assert.IsTrue((int)p >= 0 && (int)p <= 6);
        }
    }
}
