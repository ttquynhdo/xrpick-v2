using System.Collections.Generic;
using NUnit.Framework;

namespace XRPick.Orders.Tests
{
    // #19, docs/decisions/016: the division question before position 1 of ComplexScene.
    public class RemainderQuizTests
    {
        private static readonly OrderLine RingD = new("ring_d", "RING D", 55, 25);
        private static readonly OrderLine Clamp = new("training_b", "SCHLAUCHSCHELLE B", 156, 50);

        [Test]
        public void Answers_ComeFromTheOrderLine()
        {
            Assert.AreEqual(2, RemainderQuiz.FullCartons(RingD));
            Assert.AreEqual(5, RemainderQuiz.LoosePieces(RingD));
            Assert.AreEqual(3, RemainderQuiz.FullCartons(Clamp));
            Assert.AreEqual(6, RemainderQuiz.LoosePieces(Clamp));
        }

        [Test]
        public void CartonOptions_RoundDownRightRoundUp()
        {
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, RemainderQuiz.CartonOptions(RingD));
        }

        [Test]
        public void LooseOptions_RestLeftInCartonAndAllLoose()
        {
            CollectionAssert.AreEqual(new[] { 5, 20, 55 }, RemainderQuiz.LooseOptions(RingD));
        }

        [TestCase(55, 25)]
        [TestCase(156, 50)]
        [TestCase(50, 20)]
        [TestCase(25, 25)]
        [TestCase(7, 25)]
        public void Options_AreThreeDistinctAscendingAndHoldTheAnswer(int soll, int vpe)
        {
            var line = new OrderLine("x", "X", soll, vpe);

            AssertOptions(RemainderQuiz.CartonOptions(line), RemainderQuiz.FullCartons(line));
            AssertOptions(RemainderQuiz.LooseOptions(line), RemainderQuiz.LoosePieces(line));
        }

        private static void AssertOptions(IReadOnlyList<int> options, int answer)
        {
            Assert.AreEqual(3, options.Count);
            CollectionAssert.AllItemsAreUnique(options);
            CollectionAssert.IsOrdered(options);
            CollectionAssert.Contains(options, answer);
            Assert.GreaterOrEqual(options[0], 0);
        }
    }
}
