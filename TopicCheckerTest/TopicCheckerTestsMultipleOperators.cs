using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetCoreMQTTExampleJsonConfigHashedPasswords;

namespace TopicCheckerTest
{
    /// <summary>
    ///     A test class to test the <see cref="TopicCheckerTest" /> with multiple operators.
    /// </summary>
    [TestClass]
    public class TopicCheckerTestsMultipleOperators
    {
        /// <summary>
        ///     Checks the tester with a valid # topic with multiple operators and a single +.
        /// </summary>
        [TestMethod]
        public void CheckMultipleValueCrossMatchSinglePlus()
        {
            var result = TopicChecker.Regex("a/#", "a/+/a");
            Assert.IsTrue(result);
        }

        /// <summary>
        ///     Checks the tester with a valid # topic with multiple operators and multiple +.
        /// </summary>
        [TestMethod]
        public void CheckMultipleValueCrossMatchMultiplePlus()
        {
            var result = TopicChecker.Regex("a/#", "a/+/+/a");
            Assert.IsTrue(result);
        }

        /// <summary>
        ///     Checks the tester with a valid # topic with multiple operators.
        /// </summary>
        [TestMethod]
        public void CheckMultipleValueCrossMatchMixed()
        {
            var result = TopicChecker.Regex("a/#", "a/+/a/#");
            Assert.IsTrue(result);
        }

        /// <summary>
        ///     Checks the tester with an invalid # topic with multiple operators.
        /// </summary>
        [TestMethod]
        public void CheckMultipleValueCrossDontMatchMixed()
        {
            var result = TopicChecker.Regex("a/#", "a/+/+/#/a");
            Assert.IsFalse(result);
        }

        /// <summary>
        ///     Checks the tester with multiple + topics without operators.
        /// </summary>
        [TestMethod]
        public void CheckMultipleValuePlusMatchNoOperators()
        {
            var result = TopicChecker.Regex("a/+/+/a", "a/b/b/a");
            Assert.IsTrue(result);
        }
    }
}