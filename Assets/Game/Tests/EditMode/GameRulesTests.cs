using NUnit.Framework;
using Game.Gameplay;

namespace GameTests.EditMode
{
    public class GameRulesTests
    {
        [Test]
        public void CalculateScore_SingleLine_Level0_Returns40()
        {
            Assert.AreEqual(40, GameRules.CalculateScore(1, 0));
        }

        [Test]
        public void CalculateScore_Tetris_Level0_Returns1200()
        {
            Assert.AreEqual(1200, GameRules.CalculateScore(4, 0));
        }

        [Test]
        public void CalculateScore_Tetris_Level2_Returns3600()
        {
            Assert.AreEqual(3600, GameRules.CalculateScore(4, 2));
        }

        [Test]
        public void CalculateScore_Double_Level0_Returns100()
        {
            Assert.AreEqual(100, GameRules.CalculateScore(2, 0));
        }

        [Test]
        public void CalculateScore_Triple_Level0_Returns300()
        {
            Assert.AreEqual(300, GameRules.CalculateScore(3, 0));
        }

        [Test]
        public void CalculateScore_ZeroLines_ReturnsZero()
        {
            Assert.AreEqual(0, GameRules.CalculateScore(0, 0));
        }

        [Test]
        public void CalculateScore_InvalidLines_ReturnsZero()
        {
            Assert.AreEqual(0, GameRules.CalculateScore(5, 0));
            Assert.AreEqual(0, GameRules.CalculateScore(-1, 0));
        }

        [Test]
        public void GetFramesPerRow_Level0_Returns48()
        {
            Assert.AreEqual(48, GameRules.GetFramesPerRow(0));
        }

        [Test]
        public void GetFramesPerRow_Level9_Returns6()
        {
            Assert.AreEqual(6, GameRules.GetFramesPerRow(9));
        }

        [Test]
        public void GetFramesPerRow_Level29_Returns1()
        {
            Assert.AreEqual(1, GameRules.GetFramesPerRow(29));
        }

        [Test]
        public void GetFramesPerRow_Level30_ClampsTo1()
        {
            Assert.AreEqual(1, GameRules.GetFramesPerRow(30));
        }

        [Test]
        public void GetFramesPerRow_NegativeLevel_ClampsTo48()
        {
            Assert.AreEqual(48, GameRules.GetFramesPerRow(-1));
        }

        [Test]
        public void GetLevel_0Lines_Returns0()
        {
            Assert.AreEqual(0, GameRules.GetLevel(0));
        }

        [Test]
        public void GetLevel_9Lines_Returns0()
        {
            Assert.AreEqual(0, GameRules.GetLevel(9));
        }

        [Test]
        public void GetLevel_10Lines_Returns1()
        {
            Assert.AreEqual(1, GameRules.GetLevel(10));
        }

        [Test]
        public void GetLevel_19Lines_Returns1()
        {
            Assert.AreEqual(1, GameRules.GetLevel(19));
        }

        [Test]
        public void GetLevel_20Lines_Returns2()
        {
            Assert.AreEqual(2, GameRules.GetLevel(20));
        }

        [Test]
        public void Constants_AutoRepeatInitialDelay_Is16()
        {
            Assert.AreEqual(16, GameRules.AUTO_REPEAT_INITIAL_DELAY_FRAMES);
        }

        [Test]
        public void Constants_AutoRepeatRepeatRate_Is6()
        {
            Assert.AreEqual(6, GameRules.AUTO_REPEAT_REPEAT_RATE_FRAMES);
        }

        [Test]
        public void Constants_PlayfieldTotalHeight_Is22()
        {
            Assert.AreEqual(22, GameRules.PLAYFIELD_TOTAL_HEIGHT);
        }

        [Test]
        public void Constants_PlayfieldWidth_Is10()
        {
            Assert.AreEqual(10, GameRules.PLAYFIELD_WIDTH);
        }

        [Test]
        public void Constants_PlayfieldVisibleHeight_Is20()
        {
            Assert.AreEqual(20, GameRules.PLAYFIELD_VISIBLE_HEIGHT);
        }

        [Test]
        public void FramesPerRowTable_HasCorrectLength()
        {
            Assert.AreEqual(30, GameRules.FramesPerRow.Length);
        }
    }
}
