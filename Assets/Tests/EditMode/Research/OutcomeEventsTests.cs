using NUnit.Framework;

namespace XRPick.Research.Tests
{
    public class OutcomeEventsTests
    {
        private static readonly OrderLine Connector = new OrderLine("connector_a", "KABELVERBINDER A", 150, 50) { empfKz = "003" };
        private static readonly OrderLine Ring = new OrderLine("ring_d", "RING D", 55, 25);

        private static ContainerContents Full(string key, int n)
        {
            var c = new ContainerContents();
            for (int i = 0; i < n; i++)
            {
                c.AddFull(key);
            }
            return c;
        }

        [Test]
        public void ConfirmPressed_MatchesSpecExample()
        {
            OrderResult result = OrderRule.Check(new[] { Connector }, Full("connector_a", 2), false);
            string json = OutcomeEvents.ConfirmPressed(1, 1, 2, 1, result,
                OutcomeEvents.ConfirmErrors(result, false), 41.2).ToJson();
            Assert.AreEqual(
                "{\"attempt_no\":1,\"position\":1,\"position_count\":2,\"attempt_in_position\":1," +
                "\"line\":{\"item\":\"connector_a\",\"soll\":150,\"vpe\":50,\"empf_kz\":\"003\",\"required_full\":3,\"required_loose\":null," +
                "\"placed_full\":2,\"placed_loose\":null}," +
                "\"foreign\":[]," +
                "\"correct\":false," +
                "\"error_codes\":[{\"code\":\"TOO_FEW\",\"item\":\"connector_a\"}]," +
                "\"feedback_key\":null," +
                "\"time_since_scene_enter_s\":41.2}", json);
        }

        [Test]
        public void ConfirmPressed_ComplexLooseAndForeign()
        {
            ContainerContents c = Full("ring_d", 2);
            c.AddLoose("ring_d");
            c.AddLoose("ring_d");
            c.AddLoose("ring_d");
            c.AddLoose("ring_d");
            c.AddLoose("ring_d");
            c.AddLoose("ring_d");
            c.AddFull("screw_b");
            c.AddLoose("screw_b");
            OrderResult result = OrderRule.Check(new[] { Ring }, c, true);
            string json = OutcomeEvents.ConfirmPressed(2, 1, 2, 2, result,
                OutcomeEvents.ConfirmErrors(result, false), 10).ToJson();
            StringAssert.Contains("\"required_loose\":5,\"placed_full\":2,\"placed_loose\":6", json);
            StringAssert.Contains("\"foreign\":[{\"item\":\"screw_b\",\"count\":2}]", json);
            StringAssert.Contains(
                "\"error_codes\":[{\"code\":\"WRONG_MATERIAL\",\"item\":\"screw_b\"},{\"code\":\"EXTRA_LOOSE\",\"item\":\"ring_d\"}]",
                json);
        }

        [Test]
        public void ConfirmErrors_EkzPlacementMakesCorrectQuantityWrong()
        {
            var basic = new OrderLine("training_b", "SCHLAUCHSCHELLE B", 300, 50);
            OrderResult result = OrderRule.Check(new[] { basic }, Full("training_b", 6), false);
            Assert.IsTrue(result.Correct);
            string json = OutcomeEvents.ConfirmPressed(1, 1, 1, 1, result,
                OutcomeEvents.ConfirmErrors(result, true), 5).ToJson();
            StringAssert.Contains("\"correct\":false,\"error_codes\":[{\"code\":\"EKZ_PLACEMENT\",\"item\":null}]", json);
        }

        [Test]
        public void ConfirmPressed_PlacementBetweenErrorCodesAndTime()
        {
            OrderResult result = OrderRule.Check(new[] { Connector }, Full("connector_a", 3), false);
            var placement = new PlacementInfo(3,
                new[] { "Interactables/GG_Package (4)", "Interactables/GG_Package (5)" },
                new[] { "Interactables/GG_Package (1)" },
                new[] { "Interactables/GG_Package (3)" }, false);
            string json = OutcomeEvents.ConfirmPressed(2, 1, 2, 2, result,
                OutcomeEvents.ConfirmErrors(result, true), 65.5, placement, "order.feedback.ekz3_loose").ToJson();
            StringAssert.Contains(
                "\"error_codes\":[{\"code\":\"EKZ_PLACEMENT\",\"item\":null}]," +
                "\"placement\":{\"places\":3," +
                "\"placed\":[\"Interactables/GG_Package (4)\",\"Interactables/GG_Package (5)\"]," +
                "\"loose\":[\"Interactables/GG_Package (1)\"]," +
                "\"places_held_by_other\":[\"Interactables/GG_Package (3)\"]," +
                "\"places_shown\":false}," +
                "\"feedback_key\":\"order.feedback.ekz3_loose\"," +
                "\"time_since_scene_enter_s\":65.5}", json);
        }

        [Test]
        public void ConfirmPressed_FeedbackKeyWithoutPlacement()
        {
            ContainerContents c = Full("ring_d", 2);
            for (int i = 0; i < 5; i++)
            {
                c.AddLoose("ring_d");
            }
            OrderResult result = OrderRule.Check(new[] { Ring }, c, true);
            Assert.IsTrue(result.Correct);
            string json = OutcomeEvents.ConfirmPressed(3, 2, 2, 1, result,
                OutcomeEvents.ConfirmErrors(result, false), 7, null, "complex.feedback.correct").ToJson();
            StringAssert.Contains(
                "\"error_codes\":[],\"feedback_key\":\"complex.feedback.correct\",\"time_since_scene_enter_s\":7}", json);
        }

        [Test]
        public void ConfirmPressed_NoPlacementMeansNoField()
        {
            OrderResult result = OrderRule.Check(new[] { Ring }, Full("ring_d", 2), true);
            string json = OutcomeEvents.ConfirmPressed(1, 1, 2, 1, result,
                OutcomeEvents.ConfirmErrors(result, false), 3).ToJson();
            StringAssert.DoesNotContain("placement", json);
        }

        [Test]
        public void Placement_NullListsBecomeEmptyArrays()
        {
            string json = new PlacementInfo(6, null, null, null, true).ToJson();
            Assert.AreEqual("{\"places\":6,\"placed\":[],\"loose\":[],\"places_held_by_other\":[],\"places_shown\":true}",
                json);
        }

        [Test]
        public void ConfirmPressed_EmptyIsAloneAndCorrectFollowsCodes()
        {
            OrderResult empty = OrderRule.Check(new[] { Connector }, new ContainerContents(), false);
            string json = OutcomeEvents.ConfirmPressed(1, 1, 2, 1, empty,
                OutcomeEvents.ConfirmErrors(empty, false), 1).ToJson();
            StringAssert.Contains("\"correct\":false,\"error_codes\":[{\"code\":\"EMPTY\",\"item\":null}]", json);

            OrderResult right = OrderRule.Check(new[] { Connector }, Full("connector_a", 3), false);
            json = OutcomeEvents.ConfirmPressed(2, 1, 2, 2, right, OutcomeEvents.ConfirmErrors(right, false), 2).ToJson();
            StringAssert.Contains("\"correct\":true,\"error_codes\":[]", json);
        }

        [Test]
        public void QuizAnswer_WrongCarriesWrongAnswerCode()
        {
            Assert.AreEqual(
                "{\"attempt_no\":1,\"quiz\":\"complex_remainder\",\"question_id\":\"cartons\",\"selected\":\"1\"," +
                "\"correct\":false,\"attempt_in_question\":1," +
                "\"error_codes\":[{\"code\":\"WRONG_ANSWER\",\"item\":null}],\"time_since_scene_enter_s\":22.8}",
                OutcomeEvents.QuizAnswer(1, OutcomeEvents.QuizComplexRemainder, "cartons", "1", false, 1, 22.8).ToJson());
        }

        [Test]
        public void SmallEvents_HaveSpecKeys()
        {
            Assert.AreEqual("{\"reason\":\"quiz_first\",\"position\":1,\"time_since_scene_enter_s\":3.5}",
                OutcomeEvents.ConfirmIgnored(OutcomeEvents.IgnoredQuizFirst, 1, 3.5).ToJson());
            Assert.AreEqual("{\"position\":2,\"attempts_in_position\":3}",
                OutcomeEvents.PositionCompleted(2, 3).ToJson());
            Assert.AreEqual("{\"attempts_to_correct\":3,\"time_to_first_confirm_s\":38,\"time_to_correct_s\":95.5}",
                OutcomeEvents.TaskCompleted(3, 38.0, 95.5).ToJson());
            Assert.AreEqual("{\"step_id\":\"material_flow\",\"step_index\":11}",
                OutcomeEvents.TourStep("material_flow", 11).ToJson());
            Assert.AreEqual("{\"what\":\"storage_out\",\"position\":1}",
                OutcomeEvents.StationMove(OutcomeEvents.MoveStorageOut, 1).ToJson());
            Assert.AreEqual("{\"action\":\"grab\"}", OutcomeEvents.WelcomePracticeDone(OutcomeEvents.PracticeGrab).ToJson());
            Assert.AreEqual("{\"time_since_scene_enter_s\":12.346}", OutcomeEvents.WelcomeTasksOpened(12.3456).ToJson());
        }
    }
}
