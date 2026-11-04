using NUnit.Framework;

namespace XRPick.Orders.Tests
{
    // #79, docs/decisions/024 (6, 7): ConceptScene task cards follow actions only; done marks stay.
    public class ConceptCardTrackerTests
    {
        private static ConceptCardTracker NewTracker() => new ConceptCardTracker(4);

        [Test]
        public void Starts_AtLabel_NothingDone()
        {
            var tracker = NewTracker();

            Assert.AreEqual(ConceptCard.Label, tracker.Current);
            Assert.IsFalse(tracker.LabelDone);
            Assert.IsFalse(tracker.WmsDone);
            Assert.IsFalse(tracker.QuizDone);
        }

        [Test]
        public void ThreeOfFourMaterials_LabelNotDone()
        {
            var tracker = NewTracker();

            tracker.PackageGrabbed("connector_a");
            tracker.PackageGrabbed("screw_b");
            tracker.PackageGrabbed("ring_d");

            Assert.IsFalse(tracker.LabelDone);
            Assert.AreEqual(ConceptCard.Label, tracker.Current);
        }

        [Test]
        public void FourthMaterial_LabelDone_MovesToWms()
        {
            var tracker = NewTracker();
            tracker.PackageGrabbed("connector_a");
            tracker.PackageGrabbed("screw_b");
            tracker.PackageGrabbed("ring_d");

            Assert.IsTrue(tracker.PackageGrabbed("stecker_c"));
            Assert.IsTrue(tracker.LabelDone);
            Assert.AreEqual(ConceptCard.Wms, tracker.Current);
        }

        [Test]
        public void SameMaterialTwice_CountsOnce()
        {
            var tracker = NewTracker();
            tracker.PackageGrabbed("ring_d");
            tracker.PackageGrabbed("ring_d");
            tracker.PackageGrabbed("ring_d");

            Assert.IsFalse(tracker.PackageGrabbed("ring_d"));
            Assert.IsFalse(tracker.LabelDone);
        }

        [Test]
        public void EmptyMaterialKey_NotCounted()
        {
            var tracker = new ConceptCardTracker(1);

            Assert.IsFalse(tracker.PackageGrabbed(""));
            Assert.IsFalse(tracker.PackageGrabbed(null));
            Assert.IsFalse(tracker.LabelDone);
        }

        [Test]
        public void FirstWeiter_MovesToWms_WithoutLabelDone()
        {
            var tracker = NewTracker();

            Assert.IsTrue(tracker.TourAdvanced());
            Assert.AreEqual(ConceptCard.Wms, tracker.Current);
            Assert.IsFalse(tracker.LabelDone);
            Assert.IsFalse(tracker.TourAdvanced());
        }

        [Test]
        public void QuizOpened_WmsDone_MovesToQuiz()
        {
            var tracker = NewTracker();
            tracker.TourAdvanced();

            Assert.IsTrue(tracker.QuizOpened());
            Assert.IsTrue(tracker.WmsDone);
            Assert.AreEqual(ConceptCard.Quiz, tracker.Current);
            Assert.IsFalse(tracker.QuizOpened());
        }

        [Test]
        public void LabelDoneAfterQuizOpened_KeepsQuizCurrent()
        {
            var tracker = new ConceptCardTracker(1);
            tracker.TourAdvanced();
            tracker.QuizOpened();

            Assert.IsTrue(tracker.PackageGrabbed("stecker_c"));
            Assert.IsTrue(tracker.LabelDone);
            Assert.AreEqual(ConceptCard.Quiz, tracker.Current);
        }

        [Test]
        public void QuizCompleted_AllCurrentOff_MarksStay()
        {
            var tracker = new ConceptCardTracker(1);
            tracker.PackageGrabbed("stecker_c");
            tracker.QuizOpened();

            Assert.IsTrue(tracker.QuizCompleted());
            Assert.AreEqual(ConceptCard.None, tracker.Current);
            Assert.IsTrue(tracker.LabelDone && tracker.WmsDone && tracker.QuizDone);
            Assert.IsFalse(tracker.QuizCompleted());
            Assert.IsFalse(tracker.TourAdvanced());
            Assert.AreEqual(ConceptCard.None, tracker.Current);
        }
    }
}
