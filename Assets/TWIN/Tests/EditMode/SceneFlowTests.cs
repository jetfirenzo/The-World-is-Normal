using NUnit.Framework;
using TWIN.Core;
using UnityEngine;

namespace TWIN.Tests
{
    public class SceneFlowTests
    {
        SceneFlow Flow() => ScriptableObject.CreateInstance<SceneFlow>();

        [Test]
        public void NormalPath_GoesThroughFrontDoor()
        {
            var f = Flow();
            var s = new GameState();
            s.ApplyFork(2, false);
            Assert.AreEqual(f.frontDoor, f.NextAfter(f.shop, s));
            Assert.AreEqual(f.lastMorning, f.NextAfter(f.frontDoor, s));
        }

        [Test]
        public void StrangePath_GoesThroughBackrooms()
        {
            var f = Flow();
            var s = new GameState();
            s.ApplyFork(2, true);
            Assert.AreEqual(f.backrooms, f.NextAfter(f.shop, s));
            Assert.AreEqual(f.lastMorning, f.NextAfter(f.backrooms, s));
        }

        [Test]
        public void FullOrder_FromBootToEnd()
        {
            var f = Flow();
            var s = new GameState();
            string[] expected = { f.bedroom, f.bathroom, f.livingRoom, f.street, f.shop, f.frontDoor, f.lastMorning, f.endOfDemo, f.boot };
            string cur = f.boot;
            foreach (var e in expected)
            {
                cur = f.NextAfter(cur, s);
                Assert.AreEqual(e, cur);
            }
        }

        [Test]
        public void UnknownScene_HasNoNext()
        {
            Assert.IsNull(Flow().NextAfter("SampleScene", new GameState()));
        }
    }
}
