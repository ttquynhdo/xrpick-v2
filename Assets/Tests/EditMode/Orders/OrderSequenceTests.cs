using System.Collections.Generic;
using NUnit.Framework;

namespace XRPick.Orders.Tests
{
    // #21, docs/decisions/012: MixedPickingScene picks one position at a time.
    public class OrderSequenceTests
    {
        private static List<OrderLine> Mixed() => new()
        {
            new OrderLine("connector_a", "KABELVERBINDER A", 100, 50),
            new OrderLine("screw_b", "SCHRAUBE B", 40, 20)
        };

        private static ContainerContents Full(params (string key, int count)[] items)
        {
            var contents = new ContainerContents();
            foreach (var (key, count) in items)
            {
                for (int i = 0; i < count; i++)
                    contents.AddFull(key);
            }
            return contents;
        }

        [Test]
        public void Starts_AtFirstPosition()
        {
            var sequence = new OrderSequence(Mixed());

            Assert.AreEqual(1, sequence.Position);
            Assert.AreEqual(2, sequence.Count);
            Assert.AreEqual("connector_a", sequence.CurrentLine.materialKey);
            Assert.IsFalse(sequence.IsLast);
        }

        [Test]
        public void Check_RatesOnlyTheCurrentLine()
        {
            var sequence = new OrderSequence(Mixed());

            OrderResult result = sequence.CheckCurrent(Full(("connector_a", 2)), false);

            Assert.IsTrue(result.Correct);
            Assert.AreEqual(1, result.Lines.Count);
        }

        [Test]
        public void Check_MaterialOfALaterPositionIsForeign()
        {
            var sequence = new OrderSequence(Mixed());

            OrderResult result = sequence.CheckCurrent(Full(("connector_a", 2), ("screw_b", 2)), false);

            Assert.IsFalse(result.Correct);
            Assert.AreEqual(OrderErrorCode.WrongMaterial, result.Errors[0].Code);
            Assert.AreEqual("screw_b", result.Errors[0].Item);
        }

        [Test]
        public void Advance_MovesToTheNextLine()
        {
            var sequence = new OrderSequence(Mixed());

            Assert.IsTrue(sequence.TryAdvance());

            Assert.AreEqual(2, sequence.Position);
            Assert.AreEqual("screw_b", sequence.CurrentLine.materialKey);
            Assert.IsTrue(sequence.IsLast);
            Assert.IsTrue(sequence.CheckCurrent(Full(("screw_b", 2)), false).Correct);
        }

        [Test]
        public void Advance_AtTheLastPosition_StaysThere()
        {
            var sequence = new OrderSequence(Mixed());
            sequence.TryAdvance();

            Assert.IsFalse(sequence.TryAdvance());
            Assert.AreEqual(2, sequence.Position);
        }

        [Test]
        public void EmptyOrder_HasNoCurrentLine()
        {
            var sequence = new OrderSequence(new List<OrderLine>());

            Assert.IsNull(sequence.CurrentLine);
            Assert.AreEqual(0, sequence.Count);
            Assert.IsFalse(sequence.TryAdvance());
        }
    }
}
