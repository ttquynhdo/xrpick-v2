using NUnit.Framework;

namespace XRPick.Research.Tests
{
    public class ErrorFolderTests
    {
        [Test]
        public void RepeatedError_IsFoldedWithinWindow()
        {
            var f = new ErrorFolder(5, 200);
            Assert.IsTrue(f.Decide("boom", 0).Write);
            for (int i = 1; i <= 60; i++)
            {
                Assert.IsFalse(f.Decide("boom", i * 0.016).Write);
            }

            var next = f.Decide("boom", 5.5);
            Assert.IsTrue(next.Write);
            Assert.AreEqual(60, next.FoldedCount);
        }

        [Test]
        public void Cap_IsReportedOnce()
        {
            var f = new ErrorFolder(5, 2);
            f.Decide("a", 0);
            f.Decide("b", 0);
            var third = f.Decide("c", 0);
            Assert.IsFalse(third.Write);
            Assert.IsTrue(third.CapReachedNow);
            Assert.IsFalse(f.Decide("d", 0).CapReachedNow);
        }

        [TestCase(@"C:\Users\Anna Maria\proj\Foo.cs:12", "Foo.cs:12")]
        [TestCase(@"C:\Program Files\Unity\Editor\x.dll", "x.dll")]
        [TestCase("/Users/Anna Maria/x/Foo.cs:3", "Foo.cs:3")]
        public void StripPaths_HandlesSpacesInSegments(string input, string expected)
        {
            Assert.AreEqual(expected, ErrorFolder.StripPaths(input));
        }

        [Test]
        public void StripPaths_LeavesPlainTextAndUrlsUnchanged()
        {
            Assert.AreEqual("use and/or Soll/Ist 3/5", ErrorFolder.StripPaths("use and/or Soll/Ist 3/5"));
            Assert.AreEqual("see https://h/x now", ErrorFolder.StripPaths("see https://h/x now"));
        }

        [Test]
        public void DrainPending_ReturnsFoldedCountsOnce()
        {
            var f = new ErrorFolder(5, 200);
            f.Decide("boom", 0);
            f.Decide("boom", 1);
            f.Decide("boom", 2);
            f.Decide("boom", 3);
            var first = f.DrainPending();
            Assert.AreEqual(1, first.Count);
            Assert.AreEqual("boom", first[0].Key);
            Assert.AreEqual(3, first[0].Value);
            Assert.AreEqual(0, f.DrainPending().Count);
        }

        [Test]
        public void DrainPending_WorksAfterCapReached()
        {
            var f = new ErrorFolder(5, 2);
            f.Decide("a", 0);
            f.Decide("a", 1);
            f.Decide("b", 1);
            Assert.IsTrue(f.Decide("c", 1).CapReachedNow);
            var pending = f.DrainPending();
            Assert.AreEqual(1, pending.Count);
            Assert.AreEqual("a", pending[0].Key);
        }

        [Test]
        public void StripPaths_KeepsOnlyFileNames()
        {
            Assert.AreEqual("at Foo.cs:12 and Bar.cs:3",
                ErrorFolder.StripPaths(@"at C:\Users\anna\proj\Foo.cs:12 and /home/x/Bar.cs:3"));
        }
    }
}
