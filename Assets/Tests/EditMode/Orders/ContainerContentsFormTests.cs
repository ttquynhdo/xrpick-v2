using System.Collections.Generic;
using NUnit.Framework;

namespace XRPick.Orders.Tests
{
    // #19, docs/decisions/016: the opened carton in the storage box is never a full carton.
    public class ContainerContentsFormTests
    {
        private static List<OrderLine> RingD() => new() { new OrderLine("ring_d", "RING D", 55, 25) };

        private static ContainerContents Contents(int full, int loose, int opened)
        {
            var contents = new ContainerContents();
            for (int i = 0; i < full; i++)
                contents.Add("ring_d", PackageForm.FullCarton);
            for (int i = 0; i < loose; i++)
                contents.Add("ring_d", PackageForm.LoosePiece);
            for (int i = 0; i < opened; i++)
                contents.Add("ring_d", PackageForm.OpenedCarton);
            return contents;
        }

        [Test]
        public void OpenedCarton_IsNeverAFullCarton()
        {
            ContainerContents contents = Contents(0, 0, 1);

            Assert.AreEqual(0, contents.FullOf("ring_d"));
            Assert.AreEqual(0, contents.LooseOf("ring_d"));
            Assert.IsTrue(contents.IsEmpty);
        }

        [Test]
        public void TwoCartonsFiveLoose_IsCorrect()
        {
            Assert.IsTrue(OrderRule.Check(RingD(), Contents(2, 5, 0), true).Correct);
        }

        [Test]
        public void ThreeCartons_IsTooMany()
        {
            OrderResult result = OrderRule.Check(RingD(), Contents(3, 0, 0), true);

            Assert.AreEqual(OrderErrorCode.TooMany, result.Errors[0].Code);
        }

        [Test]
        public void OpenedCartonInBox_DoesNotMakeTooMany()
        {
            Assert.IsTrue(OrderRule.Check(RingD(), Contents(2, 5, 1), true).Correct);
        }
    }
}
