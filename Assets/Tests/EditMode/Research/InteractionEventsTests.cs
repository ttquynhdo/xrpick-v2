using NUnit.Framework;

namespace XRPick.Research.Tests
{
    public class InteractionEventsTests
    {
        [Test]
        public void GrabStart_HandAndInputNullUntil74()
        {
            Assert.AreEqual(
                "{\"object_id\":\"Interactables/Carton (2)\",\"material_key\":\"connector_a\",\"hand\":null,\"input\":null}",
                InteractionEvents.GrabStart("Interactables/Carton (2)", "connector_a").ToJson());
        }

        [Test]
        public void GrabEnd_RoundsDuration()
        {
            Assert.AreEqual(
                "{\"object_id\":\"a\",\"material_key\":\"ring_d\",\"hand\":null,\"input\":null,\"duration_s\":1.235}",
                InteractionEvents.GrabEnd("a", "ring_d", 1.23456).ToJson());
        }

        [Test]
        public void GrabEnd_WithoutStartHasNullDuration()
        {
            StringAssert.EndsWith("\"duration_s\":null}", InteractionEvents.GrabEnd("a", "ring_d", null).ToJson());
        }

        [Test]
        public void Container_MatchesSpec()
        {
            Assert.AreEqual(
                "{\"container\":\"order\",\"object_id\":\"a\",\"material_key\":\"screw_b\",\"count_after\":3}",
                InteractionEvents.Container(InteractionEvents.ContainerOrder, "a", "screw_b", 3).ToJson());
        }

        [Test]
        public void ConceptHint_HasContainerReasonObjectMaterialAndStep()
        {
            Assert.AreEqual(
                "{\"container\":\"storage\",\"reason\":\"WRONG_MATERIAL\",\"object_id\":\"Interactables/GG_Package\"," +
                "\"material_key\":\"connector_a\",\"tour_step_id\":\"cartons\"}",
                InteractionEvents.ConceptHint(InteractionEvents.ContainerStorage, InteractionEvents.HintWrongMaterial,
                    "Interactables/GG_Package", "connector_a", "cartons").ToJson());
            StringAssert.Contains("\"tour_step_id\":null",
                InteractionEvents.ConceptHint(InteractionEvents.ContainerOrder, InteractionEvents.HintTooMany, "a",
                    "stecker_c", null).ToJson());
        }

        [Test]
        public void MenuSelect_HasTargetSceneAndBlockedNoItemLabel()
        {
            string json = InteractionEvents.MenuSelect("BasicPickingScene", true).ToJson();
            Assert.AreEqual("{\"target_scene\":\"BasicPickingScene\",\"blocked\":true}", json);
            StringAssert.DoesNotContain("item_label", json);
        }
    }

    public class VisitInteractionTests
    {
        [Test]
        public void GrabEnded_ReturnsSecondsSinceStart()
        {
            var v = new VisitInteraction("MixedPickingScene");
            v.GrabStarted("a", 10.0);
            Assert.AreEqual(2.5, v.GrabEnded("a", 12.5).Value, 1e-9);
        }

        [Test]
        public void GrabEnded_WithoutStartIsNull()
        {
            var v = new VisitInteraction("MixedPickingScene");
            Assert.IsNull(v.GrabEnded("a", 1.0));
            v.GrabStarted("a", 1.0);
            v.GrabEnded("a", 2.0);
            Assert.IsNull(v.GrabEnded("a", 3.0));
        }

        [Test]
        public void FillSummary_WritesCounts()
        {
            var v = new VisitInteraction("MixedPickingScene");
            v.GrabStarted("a", 0);
            v.GrabStarted("b", 1);
            v.ContainerEntered();
            v.ContainerEntered();
            v.ContainerExited();
            var row = new VisitSummary();
            v.FillSummary(row);
            Assert.AreEqual(2, row.Grabs);
            Assert.AreEqual(2, row.ContainerEnters);
            Assert.AreEqual(1, row.ContainerExits);
        }

        [Test]
        public void FillSummary_NoInteractionIsZeroNotEmpty()
        {
            var row = new VisitSummary();
            new VisitInteraction("MixedPickingScene").FillSummary(row);
            Assert.AreEqual(0, row.Grabs);
            Assert.AreEqual(0, row.ContainerEnters);
        }

        [Test]
        public void FillSummary_WelcomeLeavesAllEmpty()
        {
            var row = new VisitSummary();
            new VisitInteraction("WelcomeScene").FillSummary(row);
            Assert.IsNull(row.Grabs);
            Assert.IsNull(row.ContainerEnters);
            Assert.IsNull(row.ContainerExits);
        }

        [Test]
        public void FillSummary_ConceptHasGrabsButNoContainer()
        {
            var v = new VisitInteraction("ConceptScene");
            v.GrabStarted("a", 0);
            var row = new VisitSummary();
            v.FillSummary(row);
            Assert.AreEqual(1, row.Grabs);
            Assert.IsNull(row.ContainerEnters);
            Assert.IsNull(row.ContainerExits);
        }
    }
}
