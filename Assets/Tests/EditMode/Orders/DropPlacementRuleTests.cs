using System;
using NUnit.Framework;

namespace XRPick.Orders.Tests
{
    // #168: where a package let go over a container comes to rest.
    public class DropPlacementRuleTests
    {
        private const double HalfX = 0.089;
        private const double HalfZ = 0.0605;
        private const double Height = 0.091;
        private const double Tolerance = 0.0005;

        private static DropSpace Box(double width = 0.4, double rim = 0.2) => new(0, width, 0, 0.4, 0, rim);

        private static DropBody Carton(double x, double z = 0.2, double bottom = 0, double yaw = 0) =>
            new(x, z, HalfX, HalfZ, yaw, bottom, bottom + Height);

        private static DropBody Ring(double x, double z = 0.2, double bottom = 0) => new(x, z, 0.03, 0.03, 0, bottom, bottom + 0.003);

        [Test]
        public void EmptyBox_LiesOnFloorWhereLetGo()
        {
            DropPlan plan = DropPlacementRule.Plan(Carton(0.15, 0.25, 0.3), Box(), new DropBody[0]);

            Assert.AreEqual(DropOutcome.Floor, plan.Outcome);
            Assert.AreEqual(0.15, plan.X, Tolerance);
            Assert.AreEqual(0.25, plan.Z, Tolerance);
            Assert.AreEqual(0, plan.Bottom, Tolerance);
        }

        [Test]
        public void OverlapFortyPercent_RoomBeside_PushedOutByTheOverlap_OnFloor()
        {
            DropBody below = Carton(0.1);
            double overlap = 0.4 * 2 * HalfX;
            DropBody dropped = Carton(0.1 + 2 * HalfX - overlap, bottom: 0.3);

            DropPlan plan = DropPlacementRule.Plan(dropped, Box(), new[] { below });

            Assert.AreEqual(DropOutcome.PushedAside, plan.Outcome);
            Assert.AreEqual(0.1 + 2 * HalfX + DropPlacementRule.Gap, plan.X, Tolerance);
            Assert.AreEqual(0.2, plan.Z, Tolerance);
            Assert.AreEqual(0, plan.Bottom, Tolerance);
        }

        [Test]
        public void OverlapSixtyPercent_LiesOnTop_Unmoved()
        {
            DropBody below = Carton(0.1);
            DropBody dropped = Carton(0.1 + 0.4 * 2 * HalfX, bottom: 0.3);

            DropPlan plan = DropPlacementRule.Plan(dropped, Box(), new[] { below });

            Assert.AreEqual(DropOutcome.OnTop, plan.Outcome);
            Assert.AreEqual(dropped.X, plan.X, Tolerance);
            Assert.AreEqual(Height, plan.Bottom, Tolerance);
        }

        [Test]
        public void ThinRingBelow_LiesOnIt_NotPushed()
        {
            DropBody ring = Ring(0.3);
            DropBody dropped = Carton(0.2, bottom: 0.3);

            DropPlan plan = DropPlacementRule.Plan(dropped, Box(), new[] { ring });

            Assert.AreEqual(DropOutcome.OnTop, plan.Outcome);
            Assert.AreEqual(0.2, plan.X, Tolerance);
            Assert.AreEqual(0.003, plan.Bottom, Tolerance);
        }

        [Test]
        public void SlotWiderThanCarton_LiesInTheSlot_NotLifted()
        {
            // Slot 0.178..0.422 (24.4 cm) between two cartons at the walls; the carton is let go low inside it.
            var others = new[] { Carton(HalfX), Carton(0.6 - HalfX) };
            DropBody dropped = Carton(0.3, bottom: 0.01);

            DropPlan plan = DropPlacementRule.Plan(dropped, Box(0.6), others);

            Assert.AreEqual(DropOutcome.Floor, plan.Outcome);
            Assert.AreEqual(0.3, plan.X, Tolerance);
            Assert.AreEqual(0, plan.Bottom, Tolerance);
        }

        [Test]
        public void LetGoHalfOverTheWall_PulledInside()
        {
            DropPlan plan = DropPlacementRule.Plan(Carton(0.39, bottom: 0.3), Box(), new DropBody[0]);

            Assert.AreEqual(0.4 - HalfX, plan.X, Tolerance);
        }

        [Test]
        public void WallStopsThePush_LiesOnTop()
        {
            DropSpace narrow = Box(0.3);
            DropBody below = Carton(0.3 - HalfX);
            DropBody dropped = Carton(below.X - 0.7 * 2 * HalfX, bottom: 0.3);

            DropPlan plan = DropPlacementRule.Plan(dropped, narrow, new[] { below });

            Assert.AreEqual(DropOutcome.Blocked, plan.Outcome);
            Assert.AreEqual(HalfX, plan.X, Tolerance);
            Assert.AreEqual(Height, plan.Bottom, Tolerance);
        }

        [Test]
        public void WallStopsThePush_OnTopWouldStickOut_LiesLower()
        {
            DropSpace narrow = new(0, 0.3, 0, 0.4, 0, 0.15);
            DropBody below = Carton(0.3 - HalfX);
            DropBody dropped = Carton(below.X - 0.7 * 2 * HalfX, bottom: 0.3);

            DropPlan plan = DropPlacementRule.Plan(dropped, narrow, new[] { below });

            Assert.AreEqual(DropOutcome.Blocked, plan.Outcome);
            Assert.AreEqual(0.15 - DropPlacementRule.RimClearance - Height, plan.Bottom, Tolerance);
        }

        [Test]
        public void FewMillimetresOverAStack_AtTheWall_LiesOnFloor()
        {
            // A slightly tilted carton let go against the front wall reaches 2 mm over a stack behind it.
            var others = new[] { Carton(0.2, 0.2), Carton(0.2, 0.2, Height) };
            DropBody dropped = Carton(0.2, 0.2 - 2 * HalfZ + 0.002, 0.3);
            DropSpace box = new(0, 0.4, 0.2 - 3 * HalfZ + 0.002, 0.4, 0, 0.2);

            DropPlan plan = DropPlacementRule.Plan(dropped, box, others);

            Assert.AreEqual(DropOutcome.Floor, plan.Outcome);
            Assert.AreEqual(0, plan.Bottom, Tolerance);
        }

        [Test]
        public void TallBagHalfOverAnother_OnTopWouldStickOut_PushedBeside()
        {
            // Bags stand 18.6 cm high in a 20 cm container: on top of each other they never fit.
            DropBody Bag(double z, double bottom) => new(0.2, z, 0.078, 0.086, 0, bottom, bottom + 0.186);
            DropBody below = Bag(0.1, 0);
            DropBody dropped = Bag(0.1 + 0.086, 0.3);

            DropPlan plan = DropPlacementRule.Plan(dropped, Box(), new[] { below });

            Assert.AreEqual(DropOutcome.PushedAside, plan.Outcome);
            Assert.AreEqual(0, plan.Bottom, Tolerance);
            Assert.AreEqual(0.1 + 2 * 0.086 + DropPlacementRule.Gap, plan.Z, Tolerance);
        }

        [Test]
        public void FullBox_TopNeverAboveRim()
        {
            var others = new[] { Carton(0.2), Carton(0.2, bottom: Height) };
            DropPlan plan = DropPlacementRule.Plan(Carton(0.2, bottom: 0.4), Box(), others);

            Assert.AreEqual(DropOutcome.Blocked, plan.Outcome);
            Assert.LessOrEqual(plan.Bottom + Height, 0.2 - DropPlacementRule.RimClearance + 1e-9);
        }

        [Test]
        public void PushedOffUpperCarton_LiesOnTheOneBelow()
        {
            // Two layers: a wide base of two cartons, one carton on top of the left one.
            var others = new[] { Carton(0.1), Carton(0.1 + 2 * HalfX + 0.002), Carton(0.1, bottom: Height) };
            DropBody dropped = Carton(0.1 + 2 * HalfX * 0.7, bottom: 0.4);

            DropPlan plan = DropPlacementRule.Plan(dropped, Box(), others);

            Assert.AreEqual(DropOutcome.PushedAside, plan.Outcome);
            Assert.AreEqual(Height, plan.Bottom, Tolerance);
            Assert.GreaterOrEqual(plan.X, 0.1 + 2 * HalfX);
        }

        [Test]
        public void TurnedFootprint_UsesItsRealOutline()
        {
            // Turned 90°, the carton is 12.1 cm wide in x and fits between cartons 12.5 cm apart.
            var others = new[] { Carton(0.1), Carton(0.1 + 2 * HalfX + 0.125) };
            double slotCentre = 0.1 + HalfX + 0.0625;
            DropPlan plan = DropPlacementRule.Plan(Carton(slotCentre, bottom: 0.3, yaw: Math.PI / 2), Box(0.5), others);

            Assert.AreEqual(DropOutcome.Floor, plan.Outcome);
            Assert.AreEqual(slotCentre, plan.X, Tolerance);
        }
    }
}
