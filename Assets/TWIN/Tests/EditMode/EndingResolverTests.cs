using NUnit.Framework;
using TWIN.Core;

namespace TWIN.Tests
{
    public class EndingResolverTests
    {
        [Test]
        public void FourStrange_IsSystemError()
        {
            var s = new GameState();
            for (int i = 0; i < 4; i++) s.ApplyFork(i, true);
            Assert.AreEqual(4, s.strangeCount);
            Assert.AreEqual(EndingResolver.SystemError, EndingResolver.Resolve(s));
        }

        [Test]
        public void SystemError_OverridesRouteAndNote()
        {
            var s = new GameState { strangeCount = 4, route = Route.A, notePocketed = false };
            Assert.AreEqual(EndingResolver.SystemError, EndingResolver.Resolve(s));
            s.route = Route.B; s.notePocketed = true;
            Assert.AreEqual(EndingResolver.SystemError, EndingResolver.Resolve(s));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void LessThanFourStrange_IsStillUnknown(int strange)
        {
            // TODO(story): replace with the real ending names once the rules are supplied.
            var s = new GameState { strangeCount = strange };
            Assert.AreEqual(EndingResolver.Unknown, EndingResolver.Resolve(s));
        }

        [Test]
        public void NullState_IsUnknown()
        {
            Assert.AreEqual(EndingResolver.Unknown, EndingResolver.Resolve(null));
        }
    }
}
