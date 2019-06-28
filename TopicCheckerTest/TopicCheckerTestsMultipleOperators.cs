using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetCoreMQTTExampleJsonConfigHashedPasswords;

namespace TopicCheckerTest
{
    /// <summary>
    /// A test class to test the <see cref="TopicCheckerTest"/> with multiple operators.
    /// </summary>
    [TestClass]
    public class TopicCheckerTestsMultipleOperators
    {
        /// <summary>
        /// Checks the tester with a valid # topic with multiple operators and a single +.
        /// </summary>
        [TestMethod]
        public void CheckMultipleValueCrossMatchSinglePlus()
        {
            var result = TopicChecker.TopicMatch("a/#", "a/+/a");
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Checks the tester with a valid # topic with multiple operators and multiple +.
        /// </summary>
        [TestMethod]
        public void CheckMultipleValueCrossMatchMultiplePlus()
        {
            var result = TopicChecker.TopicMatch("a/#", "a/+/+/a");
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Checks the tester with a valid # topic with multiple operators and a single #.
        /// </summary>
        [TestMethod]
        public void CheckMultipleValueCrossMatchSingleCross()
        {
            var result = TopicChecker.TopicMatch("a/#", "a/#/a");
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Checks the tester with a valid # topic with multiple operators and multiple #.
        /// </summary>
        [TestMethod]
        public void CheckMultipleValueCrossMatchMultipleCrosses()
        {
            var result = TopicChecker.TopicMatch("a/#", "a/#/a/#/b");
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Checks the tester with a valid + topic with multiple operators and a single +.
        /// </summary>
        [TestMethod]
        public void CheckMultipleValuePlusDontMatchSinglePlus()
        {
            var result = TopicChecker.TopicMatch("a/+", "a/+/a");
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Checks the tester with a valid + topic with multiple operators and multiple +.
        /// </summary>
        [TestMethod]
        public void CheckMultipleValuePlusDontMatchMultiplePlus()
        {
            var result = TopicChecker.TopicMatch("a/+", "a/+/+/a");
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Checks the tester with a valid + topic with multiple operators and a single #.
        /// </summary>
        [TestMethod]
        public void CheckMultipleValuePlusDontMatchSingleCross()
        {
            var result = TopicChecker.TopicMatch("a/+", "a/#/a");
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Checks the tester with a valid + topic with multiple operators and multiple #.
        /// </summary>
        [TestMethod]
        public void CheckMultipleValuePlusDontMatchMultipleCrosses()
        {
            var result = TopicChecker.TopicMatch("a/+", "a/#/#/a");
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Checks the tester with a valid # topic with multiple operators completely mixed.
        /// </summary>
        [TestMethod]
        public void CheckMultipleValueCrossMatchMultipleOperatorsMixed()
        {
            var result = TopicChecker.TopicMatch("a/#", "a/#/+/a/+/#");
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Checks the tester with a valid # topic with multiple operators completely mixed (Second variant).
        /// </summary>
        [TestMethod]
        public void CheckMultipleValueCrossMatchMultipleOperatorsMixed2()
        {
            var result = TopicChecker.TopicMatch("a/#", "a/b/+/a/+/#/c/l/1234/#");
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Checks the tester with an invalid # topic with multiple operators completely mixed.
        /// </summary>
        [TestMethod]
        public void CheckMultipleValueCrossDontMatchMultipleOperatorsMixed()
        {
            var result = TopicChecker.TopicMatch("a/#", "a/#/+/a/+/#/?");
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Checks the tester with an invalid + topic with multiple operators completely mixed.
        /// </summary>
        [TestMethod]
        public void CheckMultipleValuePlusDontMatchMultipleOperatorsMixed()
        {
            var result = TopicChecker.TopicMatch("a/+", "a/#/+/a/+/#");
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Checks the tester with an invalid + topic with multiple operators completely mixed (Second variant).
        /// </summary>
        [TestMethod]
        public void CheckMultipleValuePlusMatchMultipleOperatorsMixed2()
        {
            var result = TopicChecker.TopicMatch("a/+", "a/b/+/a/+/#/c/l/1234/#");
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Checks the tester with an invalid mixed topic with multiple mixed operators.
        /// </summary>
        [TestMethod]
        public void CheckMultipleValuesMixedDontMatchMultipleOperatorsMixed()
        {
            var result = TopicChecker.TopicMatch("a/+/#", "a/#/+/a/+/#");
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Checks the tester with a valid mixed topic with multiple mixed operators.
        /// </summary>
        [TestMethod]
        public void CheckMultipleValuesMixedMatchMultipleOperatorsMixed()
        {
            var result = TopicChecker.TopicMatch("a/#", "a/#/+/a/+/#");
            Assert.IsTrue(result);
        }
    }
}
