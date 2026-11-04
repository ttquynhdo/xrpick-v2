using NUnit.Framework;

namespace XRPick.Research.Tests
{
    public class VisitOutcomeTests
    {
        private static readonly OrderLine Pos1 = new OrderLine("connector_a", "KABELVERBINDER A", 150, 50);
        private static readonly OrderLine Pos2 = new OrderLine("screw_b", "SCHRAUBE B", 60, 20);

        private static OrderResult Check(OrderLine line, int cartons)
        {
            var c = new ContainerContents();
            for (int i = 0; i < cartons; i++)
            {
                c.AddFull(line.materialKey);
            }
            return OrderRule.Check(new[] { line }, c, false);
        }

        private static VisitSummary Summary(VisitOutcome outcome)
        {
            var row = new VisitSummary();
            outcome.FillSummary(row);
            return row;
        }

        [Test]
        public void Mixed_NumbersAttemptsPerVisitAndPosition()
        {
            var o = new VisitOutcome("MixedPickingScene");
            StringAssert.Contains("\"attempt_no\":1,\"position\":1,\"position_count\":2,\"attempt_in_position\":1",
                o.Confirm(Check(Pos1, 2), 1, 2, false, 10).ToJson());
            StringAssert.Contains("\"attempt_no\":2,\"position\":1,\"position_count\":2,\"attempt_in_position\":2",
                o.Confirm(Check(Pos1, 3), 1, 2, false, 20).ToJson());
            Assert.AreEqual("{\"position\":1,\"attempts_in_position\":2}", o.PositionCompleted(1).ToJson());
            o.Ignored(OutcomeEvents.IgnoredStationMoving, 2, 21);
            StringAssert.Contains("\"attempt_no\":3,\"position\":2,\"position_count\":2,\"attempt_in_position\":1",
                o.Confirm(Check(Pos2, 0), 2, 2, false, 30).ToJson());
            o.Confirm(Check(Pos2, 3), 2, 2, false, 40);
            Assert.AreEqual("{\"position\":2,\"attempts_in_position\":2}", o.PositionCompleted(2).ToJson());
            Assert.AreEqual("{\"attempts_to_correct\":4,\"time_to_first_confirm_s\":10,\"time_to_correct_s\":40}",
                o.Complete(40).ToJson());

            VisitSummary row = Summary(o);
            Assert.AreEqual(4, row.ConfirmCount);
            Assert.AreEqual(1, row.ConfirmIgnored);
            Assert.AreEqual(false, row.FirstAttemptCorrect);
            Assert.AreEqual(4, row.AttemptsToCorrect);
            Assert.AreEqual(10, row.TimeToFirstConfirmS);
            Assert.AreEqual(40, row.TimeToCorrectS);
            Assert.AreEqual(true, row.Completed);
            Assert.AreEqual("TOO_FEW:1;EMPTY:1", row.ErrorCodesAll);
        }

        [Test]
        public void TaskCompleted_OnlyOncePerVisit()
        {
            var o = new VisitOutcome("BasicPickingScene");
            Assert.IsNotNull(o.Complete(5));
            Assert.IsNull(o.Complete(6));
        }

        [Test]
        public void Complex_RemainderQuizIsNotACountedAttempt()
        {
            var o = new VisitOutcome("ComplexScene");
            StringAssert.Contains("\"attempt_no\":1,\"quiz\":\"complex_remainder\",\"question_id\":\"cartons\"",
                o.QuizAnswer(OutcomeEvents.QuizComplexRemainder, "cartons", "1", false, 5).ToJson());
            StringAssert.Contains("\"attempt_in_question\":2",
                o.QuizAnswer(OutcomeEvents.QuizComplexRemainder, "cartons", "2", true, 6).ToJson());
            o.Ignored(OutcomeEvents.IgnoredQuizFirst, 1, 7);
            // The first red-button press gets attempt_no 1: a separate counter from quiz_answer.
            StringAssert.Contains("\"attempt_no\":1,",
                o.Confirm(Check(new OrderLine("ring_d", "RING D", 50, 25), 2), 1, 2, false, 20).ToJson());

            VisitSummary row = Summary(o);
            Assert.AreEqual(1, row.ConfirmCount);
            Assert.AreEqual(1, row.ConfirmIgnored);
            Assert.AreEqual(true, row.FirstAttemptCorrect);
            Assert.AreEqual(20, row.TimeToFirstConfirmS);
            Assert.IsNull(row.ErrorCodesAll);
            Assert.AreEqual(false, row.Completed);
            Assert.IsNull(row.AttemptsToCorrect);
        }

        [Test]
        public void Concept_QuizTouchesAreTheCountedAttempts()
        {
            var o = new VisitOutcome(VisitOutcome.ConceptScene);
            o.QuizAnswer(OutcomeEvents.QuizConcept, "cartons_from_soll", "2", false, 50);
            o.QuizAnswer(OutcomeEvents.QuizConcept, "cartons_from_soll", "4", true, 55);
            o.QuizAnswer(OutcomeEvents.QuizConcept, "material_by_matnr", "screw_b", true, 60);
            o.QuizAnswer(OutcomeEvents.QuizConcept, "ekz_003", "ordered", true, 65);
            o.QuizAnswer(OutcomeEvents.QuizConcept, "ekz_000", "poured", true, 70);
            StringAssert.Contains("\"attempts_to_correct\":5,", o.Complete(70).ToJson());

            VisitSummary row = Summary(o);
            Assert.AreEqual(5, row.ConfirmCount);
            Assert.IsNull(row.ConfirmIgnored);
            Assert.AreEqual(false, row.FirstAttemptCorrect);
            Assert.AreEqual(50, row.TimeToFirstConfirmS);
            Assert.AreEqual("WRONG_ANSWER:1", row.ErrorCodesAll);
            Assert.AreEqual(true, row.Completed);
        }

        [Test]
        public void Welcome_LeavesOutcomeColumnsEmpty()
        {
            VisitSummary row = Summary(new VisitOutcome(VisitOutcome.WelcomeScene));
            Assert.IsNull(row.ConfirmCount);
            Assert.IsNull(row.ConfirmIgnored);
            Assert.IsNull(row.Completed);
            Assert.IsNull(row.FirstAttemptCorrect);
        }

        [Test]
        public void PickingVisitWithoutAttempts_HasZeroCountsAndNotCompleted()
        {
            VisitSummary row = Summary(new VisitOutcome("BasicPickingScene"));
            Assert.AreEqual(0, row.ConfirmCount);
            Assert.AreEqual(0, row.ConfirmIgnored);
            Assert.IsNull(row.FirstAttemptCorrect);
            Assert.IsNull(row.TimeToFirstConfirmS);
            Assert.AreEqual(false, row.Completed);
        }
    }
}
