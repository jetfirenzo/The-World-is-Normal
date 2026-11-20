using NUnit.Framework;
using TWIN.Core;

namespace TWIN.Tests
{
    public class EndingResolverTests
    {
        /// <summary>Builds a state by answering the four forks (n = normal, s = strange, '-' = fork 4 skipped: no door).</summary>
        static GameState Play(string answers)
        {
            var s = new GameState();
            for (int i = 0; i < answers.Length; i++)
            {
                if (answers[i] == '-') continue;
                s.ApplyFork(i, answers[i] == 's');
            }
            return s;
        }

        [Test]
        public void FourStrange_IsSystemError()
        {
            var s = Play("ssss");
            Assert.AreEqual(4, s.strangeCount);
            Assert.AreEqual(Ending.SystemError, EndingResolver.Resolve(s));
        }

        [Test]
        public void SystemError_OverridesEverything()
        {
            var s = new GameState { strangeCount = 4, route = Route.A, notePocketed = false };
            Assert.AreEqual(Ending.SystemError, EndingResolver.Resolve(s));
        }

        [TestCase("nsss")]
        [TestCase("snss")]
        [TestCase("ssns")]
        public void DoorOpened_WithThreeStrange_IsWakeUp(string answers)
        {
            var s = Play(answers);
            Assert.AreEqual(3, s.strangeCount);
            Assert.IsTrue(s.DoorOpened);
            Assert.AreEqual(Ending.WakeUp, EndingResolver.Resolve(s));
        }

        [TestCase("snns")] // note pocketed made the door appear with 1 strange; opening it makes 2
        [TestCase("nsns")]
        [TestCase("nnss")]
        public void DoorOpened_WithTwoStrange_IsTheFuneral(string answers)
        {
            var s = Play(answers);
            Assert.AreEqual(2, s.strangeCount);
            Assert.IsTrue(s.DoorOpened);
            Assert.AreEqual(Ending.TheFuneral, EndingResolver.Resolve(s));
        }

        [TestCase("ssnn")]
        [TestCase("sssn")]
        [TestCase("nssn")]
        public void BackToBed_WithTwoOrThreeStrange_IsTheNote(string answers)
        {
            var s = Play(answers);
            Assert.IsFalse(s.DoorOpened);
            Assert.That(s.strangeCount, Is.InRange(2, 3));
            Assert.AreEqual(Ending.TheNote, EndingResolver.Resolve(s));
        }

        [TestCase("nnn-")] // no door, no vote
        [TestCase("nsn-")] // one strange, note binned: no door
        [TestCase("snnn")] // note pocketed: door appeared, Sam went back to bed
        public void BackToBed_WithAtMostOneStrange_IsGoodMorningSam(string answers)
        {
            var s = Play(answers);
            Assert.IsFalse(s.DoorOpened);
            Assert.That(s.strangeCount, Is.LessThanOrEqualTo(1));
            Assert.AreEqual(Ending.GoodMorningSam, EndingResolver.Resolve(s));
        }

        [Test]
        public void NullState_IsGoodMorningSam()
        {
            Assert.AreEqual(Ending.GoodMorningSam, EndingResolver.Resolve(null));
        }

        [Test]
        public void EveryEnding_HasADisplayName()
        {
            foreach (Ending e in System.Enum.GetValues(typeof(Ending)))
                Assert.IsFalse(string.IsNullOrEmpty(EndingResolver.DisplayName(e)), e.ToString());
        }
    }
}
