using NUnit.Framework;

namespace XRPick.Orders.Tests
{
    // #100, docs/decisions/030: hints on wrong or extra packages in the two ConceptScene containers.
    public class ConceptBoxHintRuleTests
    {
        private const string Sample = "stecker_c";

        private static ConceptBoxHintRule NewRule() => new(Sample, 120);

        private static LyingPackage Carton(int id, ConceptBox box, string key = Sample, int vpe = 40) =>
            new(id, box, key, PackageForm.FullCarton, vpe);

        [Test]
        public void Empty_NoHint_NoPieces()
        {
            ConceptBoxHintRule rule = NewRule();
            rule.Update(0, new LyingPackage[0], 0);

            Assert.IsNull(rule.Current);
            Assert.AreEqual(0, rule.PlacedPieces);
            Assert.IsTrue(rule.NoWrongMaterial);
        }

        [Test]
        public void SampleCartonInOrderBox_CountsAfterSettling_NoHint()
        {
            ConceptBoxHintRule rule = NewRule();
            var lying = new[] { Carton(1, ConceptBox.Order) };

            rule.Update(0, lying, 0);
            Assert.AreEqual(0, rule.PlacedPieces);

            rule.Update(0.5, lying, 0);
            Assert.AreEqual(40, rule.PlacedPieces);
            Assert.IsNull(rule.Current);
        }

        [Test]
        public void SampleCartonOnTopOfShownCartons_TooMany()
        {
            ConceptBoxHintRule rule = NewRule();
            var lying = new[] { Carton(1, ConceptBox.Order) };

            rule.Update(0, lying, 120);
            rule.Update(1, lying, 120);

            Assert.AreEqual(40, rule.PlacedPieces);
            Assert.AreEqual(ConceptHintReason.TooMany, rule.Current.Value.Reason);
            Assert.AreEqual(1, rule.Current.Value.PackageId);
            Assert.AreEqual(ConceptBox.Order, rule.Current.Value.Box);
        }

        [Test]
        public void TooMany_TakenOut_HintGone()
        {
            ConceptBoxHintRule rule = NewRule();
            rule.Update(0, new[] { Carton(1, ConceptBox.Order) }, 120);
            rule.Update(1, new[] { Carton(1, ConceptBox.Order) }, 120);

            rule.Update(2, new LyingPackage[0], 120);

            Assert.IsNull(rule.Current);
            Assert.AreEqual(0, rule.PlacedPieces);
        }

        [Test]
        public void OtherMaterialInOrderBox_WrongMaterial_IstUnchanged()
        {
            ConceptBoxHintRule rule = NewRule();
            var lying = new[] { Carton(2, ConceptBox.Order, "connector_a", 50) };

            rule.Update(0, lying, 0);
            Assert.IsNull(rule.Current, "not settled yet");
            Assert.IsFalse(rule.NoWrongMaterial);

            rule.Update(0.6, lying, 0);
            Assert.AreEqual(ConceptHintReason.WrongMaterial, rule.Current.Value.Reason);
            Assert.AreEqual(0, rule.PlacedPieces);
        }

        [Test]
        public void OpenedCartonAndLoosePieceOfOtherMaterial_WrongMaterial()
        {
            foreach (PackageForm form in new[] { PackageForm.OpenedCarton, PackageForm.LoosePiece })
            {
                ConceptBoxHintRule rule = NewRule();
                var lying = new[] { new LyingPackage(3, ConceptBox.Order, "ring_d", form, 25) };
                rule.Update(0, lying, 0);
                rule.Update(1, lying, 0);

                Assert.AreEqual(ConceptHintReason.WrongMaterial, rule.Current.Value.Reason, form.ToString());
            }
        }

        [Test]
        public void OtherMaterialInStorageBox_WrongMaterial()
        {
            ConceptBoxHintRule rule = NewRule();
            var lying = new[] { Carton(4, ConceptBox.Storage, "screw_b", 20) };
            rule.Update(0, lying, 0);
            rule.Update(1, lying, 0);

            Assert.AreEqual(ConceptHintReason.WrongMaterial, rule.Current.Value.Reason);
            Assert.AreEqual(ConceptBox.Storage, rule.Current.Value.Box);
        }

        [Test]
        public void SampleCartonInStorageBox_NoHint_NotCounted()
        {
            ConceptBoxHintRule rule = NewRule();
            var lying = new[] { Carton(1, ConceptBox.Storage) };
            rule.Update(0, lying, 120);
            rule.Update(1, lying, 120);

            Assert.IsNull(rule.Current);
            Assert.AreEqual(0, rule.PlacedPieces);
            CollectionAssert.AreEqual(new[] { 1 }, rule.SettledSampleIn(ConceptBox.Storage));
        }

        [Test]
        public void TwoWrongPackages_FirstLaidDownShown_ThenTheOther()
        {
            ConceptBoxHintRule rule = NewRule();
            var ring = new LyingPackage(9, ConceptBox.Order, "ring_d", PackageForm.LoosePiece, 25);
            LyingPackage bag = Carton(5, ConceptBox.Storage, "screw_b", 20);

            rule.Update(0, new[] { ring }, 0);
            rule.Update(1, new[] { ring, bag }, 0);
            rule.Update(2, new[] { ring, bag }, 0);
            Assert.AreEqual(9, rule.Current.Value.PackageId);

            rule.Update(3, new[] { bag }, 0);
            Assert.AreEqual(5, rule.Current.Value.PackageId);
        }

        [Test]
        public void HeldPackage_LeavesTheList_LaidDownAgainGoesLast()
        {
            ConceptBoxHintRule rule = NewRule();
            LyingPackage a = Carton(1, ConceptBox.Order, "screw_b", 20);
            LyingPackage b = Carton(2, ConceptBox.Order, "connector_a", 50);
            rule.Update(0, new[] { a }, 0);
            rule.Update(1, new[] { a, b }, 0);
            rule.Update(2, new[] { a, b }, 0);
            Assert.AreEqual(1, rule.Current.Value.PackageId);

            // a picked up (held) and put back.
            rule.Update(3, new[] { b }, 0);
            rule.Update(4, new[] { a, b }, 0);
            rule.Update(5, new[] { a, b }, 0);
            Assert.AreEqual(2, rule.Current.Value.PackageId);
        }

        [Test]
        public void MovedToTheOtherBox_SettlesAgain()
        {
            ConceptBoxHintRule rule = NewRule();
            rule.Update(0, new[] { Carton(1, ConceptBox.Storage, "screw_b", 20) }, 0);
            rule.Update(1, new[] { Carton(1, ConceptBox.Storage, "screw_b", 20) }, 0);
            Assert.IsNotNull(rule.Current);

            rule.Update(1.1, new[] { Carton(1, ConceptBox.Order, "screw_b", 20) }, 0);
            Assert.IsNull(rule.Current);
            rule.Update(1.7, new[] { Carton(1, ConceptBox.Order, "screw_b", 20) }, 0);
            Assert.AreEqual(ConceptBox.Order, rule.Current.Value.Box);
        }

        [Test]
        public void SampleCartonBeforeDemo_TooManyOnlyOnceDemoFillsSoll()
        {
            ConceptBoxHintRule rule = NewRule();
            var lying = new[] { Carton(1, ConceptBox.Order) };
            rule.Update(0, lying, 0);
            rule.Update(1, lying, 0);
            Assert.IsNull(rule.Current);

            rule.Update(2, lying, 80);
            Assert.IsNull(rule.Current, "80 + 40 = Soll");

            rule.Update(3, lying, 120);
            Assert.AreEqual(ConceptHintReason.TooMany, rule.Current.Value.Reason);
        }
    }
}
