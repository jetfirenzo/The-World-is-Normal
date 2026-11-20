using System;
using NUnit.Framework;
using TWIN.Core;

namespace TWIN.Tests
{
    public class GameStateTests
    {
        [Test]
        public void Fresh_State_IsNeutral()
        {
            var s = new GameState();
            Assert.AreEqual(0, s.strangeCount);
            Assert.AreEqual(Route.None, s.route);
            Assert.IsFalse(s.notePocketed);
            Assert.IsFalse(s.DoorAppearsInLastMorning);
        }

        [Test]
        public void Fork1_Strange_PocketsTheNote()
        {
            var s = new GameState();
            s.ApplyFork(0, true);
            Assert.IsTrue(s.notePocketed);
            Assert.AreEqual(1, s.strangeCount);
            Assert.IsTrue(s.DoorAppearsInLastMorning, "pocketed note alone makes the door appear");
        }

        [Test]
        public void Fork1_Normal_BinsTheNote()
        {
            var s = new GameState();
            s.ApplyFork(0, false);
            Assert.IsFalse(s.notePocketed);
            Assert.AreEqual(0, s.strangeCount);
        }

        [Test]
        public void Fork2_SetsRoute()
        {
            var a = new GameState(); a.ApplyFork(1, false);
            var b = new GameState(); b.ApplyFork(1, true);
            Assert.AreEqual(Route.A, a.route);
            Assert.AreEqual(Route.B, b.route);
        }

        [Test]
        public void TwoStrange_WithoutNote_MakesDoorAppear()
        {
            var s = new GameState();
            s.ApplyFork(0, false); // note binned
            s.ApplyFork(1, true);
            s.ApplyFork(2, true);
            Assert.AreEqual(2, s.strangeCount);
            Assert.IsFalse(s.notePocketed);
            Assert.IsTrue(s.DoorAppearsInLastMorning);
        }

        [Test]
        public void OneStrange_WithoutNote_NoDoor()
        {
            var s = new GameState();
            s.ApplyFork(0, false);
            s.ApplyFork(1, true);
            s.ApplyFork(2, false);
            Assert.IsFalse(s.DoorAppearsInLastMorning);
        }

        [Test]
        public void StrangeCount_NeverExceedsFour()
        {
            var s = new GameState();
            for (int i = 0; i < 4; i++) s.ApplyFork(i, true);
            Assert.AreEqual(4, s.strangeCount);
        }

        [Test]
        public void Fork_CannotBeAnsweredTwice()
        {
            var s = new GameState();
            s.ApplyFork(0, true);
            Assert.Throws<InvalidOperationException>(() => s.ApplyFork(0, false));
        }

        [Test]
        public void Reset_ClearsEverything()
        {
            var s = new GameState();
            s.ApplyFork(0, true); s.ApplyFork(1, true);
            s.Reset();
            Assert.AreEqual(0, s.strangeCount);
            Assert.AreEqual(Route.None, s.route);
            Assert.IsFalse(s.notePocketed);
            Assert.IsFalse(s.forkAnswered[0]);
        }
    }
}
