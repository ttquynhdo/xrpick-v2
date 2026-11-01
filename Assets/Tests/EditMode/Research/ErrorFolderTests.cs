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

        [Test]
        public void StripPaths_KeepsOnlyFileNames()
        {
            Assert.AreEqual("at Foo.cs:12 and Bar.cs:3",
                ErrorFolder.StripPaths(@"at C:\Users\anna\proj\Foo.cs:12 and /home/x/Bar.cs:3"));
        }
    }
}
