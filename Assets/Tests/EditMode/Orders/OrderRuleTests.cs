using System.Collections.Generic;
using NUnit.Framework;

namespace XRPick.Orders.Tests
{
    public class OrderRuleTests
    {
        // MixedPickingScene order (A5): 100 KABELVERBINDER A (VPE 50) + 40 SCHRAUBE B (VPE 20).
        private static readonly List<OrderLine> Mixed = new()
        {
            new OrderLine("connector_a", "KABELVERBINDER A", 100, 50),
            new OrderLine("screw_b", "SCHRAUBE B", 40, 20)
        };

        // ComplexScene order (A5): 55 RING D, VPE 25 = 2 cartons + 5 loose pieces.
        private static readonly List<OrderLine> Complex = new()
        {
            new OrderLine("ring_d", "RING D", 55, 25)
        };

        private static ContainerContents Contents(params (string key, int full, int loose)[] items)
        {
            var contents = new ContainerContents();
            foreach (var (key, full, loose) in items)
            {
                for (int i = 0; i < full; i++)
                    contents.AddFull(key);
                for (int i = 0; i < loose; i++)
                    contents.AddLoose(key);
            }
            return contents;
        }

        private static string Codes(OrderResult result)
        {
            var parts = new List<string>();
            foreach (OrderError e in result.Errors)
                parts.Add(OrderErrorCodes.ToLogName(e.Code) + ":" + (e.Item ?? "-"));
            return string.Join(",", parts);
        }

        [Test]
        public void OrderLine_DerivesCartonsAndLoosePieces()
        {
            Assert.AreEqual(2, Complex[0].RequiredFull);
            Assert.AreEqual(5, Complex[0].RequiredLoose);
            Assert.AreEqual(2, Mixed[0].RequiredFull);
            Assert.AreEqual(0, Mixed[0].RequiredLoose);
        }

        [Test]
        public void Empty_IsLoggedAlone()
        {
            OrderResult result = OrderRule.Check(Complex, new ContainerContents(), true);
            Assert.AreEqual("EMPTY:-", Codes(result));
            Assert.IsFalse(result.Correct);
        }

        [Test]
        public void Mixed_CorrectOrder()
        {
            OrderResult result = OrderRule.Check(Mixed, Contents(("connector_a", 2, 0), ("screw_b", 2, 0)), false);
            Assert.IsTrue(result.Correct);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [Test]
        public void Mixed_TooFewAndTooMany()
        {
            OrderResult result = OrderRule.Check(Mixed, Contents(("connector_a", 3, 0), ("screw_b", 1, 0)), false);
            Assert.AreEqual("TOO_MANY:connector_a,TOO_FEW:screw_b", Codes(result));
        }

        [Test]
        public void Mixed_ForeignMaterialComesFirst()
        {
            OrderResult result = OrderRule.Check(Mixed, Contents(("connector_a", 2, 0), ("screw_b", 2, 0), ("ring_d", 1, 0)), false);
            Assert.AreEqual("WRONG_MATERIAL:ring_d", Codes(result));
            Assert.AreEqual(1, result.Foreign.Count);
            Assert.AreEqual(1, result.Foreign[0].Full);
            Assert.AreEqual(0, result.Foreign[0].Loose);
        }

        [Test]
        public void ForeignMaterials_SortedByKey_NotByPlacementOrder()
        {
            OrderResult a = OrderRule.Check(Complex, Contents(("screw_b", 1, 0), ("connector_a", 1, 0)), true);
            OrderResult b = OrderRule.Check(Complex, Contents(("connector_a", 1, 0), ("screw_b", 1, 0)), true);
            Assert.AreEqual(Codes(a), Codes(b));
            Assert.AreEqual("WRONG_MATERIAL:connector_a,WRONG_MATERIAL:screw_b,TOO_FEW:ring_d,MISSING_LOOSE:ring_d", Codes(a));
        }

        [Test]
        public void LineWithoutKey_DoesNotThrow()
        {
            var lines = new List<OrderLine> { new OrderLine(null, "X", 50, 50) };
            Assert.DoesNotThrow(() => OrderRule.Check(lines, Contents(("ring_d", 1, 0)), false));
        }

        [Test]
        public void Mixed_PlacedLooseIsNullWhenSceneDoesNotCountLoose()
        {
            OrderResult result = OrderRule.Check(Mixed, Contents(("connector_a", 2, 0), ("screw_b", 2, 0)), false);
            Assert.IsNull(result.Lines[0].PlacedLoose);
        }

        [Test]
        public void Complex_CorrectComposition()
        {
            OrderResult result = OrderRule.Check(Complex, Contents(("ring_d", 2, 5)), true);
            Assert.IsTrue(result.Correct);
            Assert.AreEqual(5, result.Lines[0].PlacedLoose);
        }

        // Same total (55) but another split is wrong until A7 says otherwise.
        [Test]
        public void Complex_OtherSplitOfSameTotal_IsWrong()
        {
            OrderResult result = OrderRule.Check(Complex, Contents(("ring_d", 1, 30)), true);
            Assert.AreEqual("TOO_FEW:ring_d,EXTRA_LOOSE:ring_d", Codes(result));
        }

        [Test]
        public void Complex_MissingLoose()
        {
            OrderResult result = OrderRule.Check(Complex, Contents(("ring_d", 2, 4)), true);
            Assert.AreEqual("MISSING_LOOSE:ring_d", Codes(result));
        }

        // Wrong material checked together with every other skill, in priority order.
        [Test]
        public void Complex_WrongMaterialInsteadOfOrder_ReportsAllInOrder()
        {
            OrderResult result = OrderRule.Check(Complex, Contents(("connector_a", 2, 0), ("ring_d", 0, 5)), true);
            Assert.AreEqual("WRONG_MATERIAL:connector_a,TOO_FEW:ring_d", Codes(result));
        }

        [Test]
        public void PackageWithoutKey_IsForeign()
        {
            var contents = new ContainerContents();
            contents.AddFull(null);
            contents.AddFull("ring_d");
            contents.AddFull("ring_d");
            contents.AddLoose("ring_d");
            contents.AddLoose("ring_d");
            contents.AddLoose("ring_d");
            contents.AddLoose("ring_d");
            contents.AddLoose("ring_d");
            OrderResult result = OrderRule.Check(Complex, contents, true);
            Assert.AreEqual("WRONG_MATERIAL:other", Codes(result));
        }

        [Test]
        public void Correct_IffNoErrors()
        {
            OrderResult wrong = OrderRule.Check(Mixed, Contents(("connector_a", 1, 0)), false);
            Assert.AreEqual(wrong.Errors.Count == 0, wrong.Correct);
            OrderResult right = OrderRule.Check(Complex, Contents(("ring_d", 2, 5)), true);
            Assert.AreEqual(right.Errors.Count == 0, right.Correct);
        }

        [Test]
        public void LogNames_MatchSpec()
        {
            Assert.AreEqual("EMPTY", OrderErrorCodes.ToLogName(OrderErrorCode.Empty));
            Assert.AreEqual("WRONG_TYPE", OrderErrorCodes.ToLogName(OrderErrorCode.WrongType));
            Assert.AreEqual("WRONG_MATERIAL", OrderErrorCodes.ToLogName(OrderErrorCode.WrongMaterial));
            Assert.AreEqual("TOO_FEW", OrderErrorCodes.ToLogName(OrderErrorCode.TooFew));
            Assert.AreEqual("TOO_MANY", OrderErrorCodes.ToLogName(OrderErrorCode.TooMany));
            Assert.AreEqual("MISSING_LOOSE", OrderErrorCodes.ToLogName(OrderErrorCode.MissingLoose));
            Assert.AreEqual("EXTRA_LOOSE", OrderErrorCodes.ToLogName(OrderErrorCode.ExtraLoose));
            Assert.AreEqual("NO_ANSWER", OrderErrorCodes.ToLogName(OrderErrorCode.NoAnswer));
            Assert.AreEqual("WRONG_ANSWER", OrderErrorCodes.ToLogName(OrderErrorCode.WrongAnswer));
        }

        [Test]
        public void LineProblems_NoneForSceneOrders()
        {
            Assert.IsEmpty(OrderRule.FindLineProblems(Mixed, false));
            Assert.IsEmpty(OrderRule.FindLineProblems(Complex, true));
        }

        [Test]
        public void LineProblems_ReportsBadInspectorData()
        {
            var lines = new List<OrderLine>
            {
                new OrderLine("connector_a", "A", 110, 50),
                new OrderLine("connector_a", "A", 40, 0),
                new OrderLine("", "B", 0, 20)
            };

            // Remainder without loose counting, duplicate key, vpe 0, empty key, soll 0.
            Assert.AreEqual(5, OrderRule.FindLineProblems(lines, false).Count);
            // With loose counting the remainder of line 0 is fine.
            Assert.AreEqual(4, OrderRule.FindLineProblems(lines, true).Count);
        }

        // Every code has a row in analysis/skill_map.csv, otherwise analyse.R warns "unknown error code".
        [Test]
        public void LogNames_AllInSkillMap()
        {
            string path = System.IO.Path.Combine(UnityEngine.Application.dataPath, "..", "analysis", "skill_map.csv");
            var known = new HashSet<string>();
            foreach (string row in System.IO.File.ReadAllLines(path))
                known.Add(row.Split(',')[0]);

            foreach (OrderErrorCode code in System.Enum.GetValues(typeof(OrderErrorCode)))
                Assert.IsTrue(known.Contains(OrderErrorCodes.ToLogName(code)), OrderErrorCodes.ToLogName(code));
        }
    }
}
