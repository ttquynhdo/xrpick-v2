using NUnit.Framework;

namespace XRPick.Research.Tests
{
    public class ParticipantCodeBufferTests
    {
        [Test]
        public void Buffer_ThreeDigitsMakeCode()
        {
            var b = new ParticipantCodeBuffer();
            b.AddDigit(0); b.AddDigit(1); b.AddDigit(7);
            Assert.IsTrue(b.IsComplete);
            Assert.AreEqual("P017", b.Code);
            Assert.AreEqual("P 0 1 7", b.Display);
        }

        [Test]
        public void Buffer_FourthDigitIgnored()
        {
            var b = new ParticipantCodeBuffer();
            b.AddDigit(1); b.AddDigit(2); b.AddDigit(3);
            Assert.IsFalse(b.AddDigit(4));
            Assert.AreEqual("P123", b.Code);
        }

        [Test]
        public void Buffer_DeleteOnEmptyIsNoOp()
        {
            var b = new ParticipantCodeBuffer();
            Assert.IsFalse(b.DeleteLast());
            Assert.AreEqual("P _ _ _", b.Display);
            Assert.IsNull(b.Code);
        }

        [Test]
        public void Buffer_RejectsNonDigits()
        {
            Assert.IsFalse(new ParticipantCodeBuffer().AddDigit(10));
            Assert.IsFalse(new ParticipantCodeBuffer().AddDigit(-1));
        }

        [Test]
        public void Buffer_PartialDisplay()
        {
            var b = new ParticipantCodeBuffer();
            b.AddDigit(5);
            Assert.AreEqual("P 5 _ _", b.Display);
            Assert.IsFalse(b.IsComplete);
        }
    }
}
