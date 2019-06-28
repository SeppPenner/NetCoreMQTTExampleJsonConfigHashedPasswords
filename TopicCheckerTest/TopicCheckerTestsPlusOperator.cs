using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetCoreMQTTExampleJsonConfigHashedPasswords;

namespace TopicCheckerTest
{
    /// <summary>
    /// A test class to test the <see cref="TopicCheckerTest"/> with the + operator.
    /// </summary>
    [TestClass]
    public class TopicCheckerTestsPlusOperator
    {
        /// <summary>
        /// Checks the tester with a valid topic for the + operator.
        /// </summary>
        [TestMethod]
        public void CheckSingleValuePlusMatch()
        {
            var result = TopicChecker.TopicMatch("a/+", "a/b");
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Checks the tester with an invalid topic for the + operator.
        /// </summary>
        [TestMethod]
        public void CheckSingleValuePlusDontMatch()
        {
            var result = TopicChecker.TopicMatch("a/+", "a/b/c");
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Checks the tester with a valid topic with a + for the + operator.
        /// </summary>
        [TestMethod]
        public void CheckSingleValuePlusMatchWithPlus()
        {
            var result = TopicChecker.TopicMatch("a/+", "a/+");
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Checks the tester with an invalid topic with a # for the + operator.
        /// </summary>
        [TestMethod]
        public void CheckSingleValuePlusDontMatchWithCross()
        {
            var result = TopicChecker.TopicMatch("a/+", "a/#");
            Assert.IsFalse(result);
        }
    }
}
