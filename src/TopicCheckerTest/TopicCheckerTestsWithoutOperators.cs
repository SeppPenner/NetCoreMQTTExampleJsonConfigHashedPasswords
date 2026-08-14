// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TopicCheckerTestsWithoutOperators.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A test class to test the <see cref="TopicChecker" /> with topics that contain no operator at all.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TopicCheckerTest;

/// <summary>
///     A test class to test the <see cref="TopicChecker" /> with topics that contain no operator at all.
/// </summary>
[TestClass]
public class TopicCheckerTestsWithoutOperators
{
    /// <summary>
    ///     Checks the tester with two identical topics.
    /// </summary>
    [TestMethod]
    public void CheckIdenticalTopicsMatch()
    {
        var result = TopicChecker.Regex("a/b/c", "a/b/c");
        Assert.IsTrue(result);
    }

    /// <summary>
    ///     Checks the tester with two different topics.
    /// </summary>
    [TestMethod]
    public void CheckDifferentTopicsDontMatch()
    {
        var result = TopicChecker.Regex("a/b/c", "a/b/d");
        Assert.IsFalse(result);
    }

    /// <summary>
    ///     Checks the tester with a topic that is only a part of the allowed topic. The regular expression is not
    ///     anchored, the check that the whole topic was matched is what rejects this.
    /// </summary>
    [TestMethod]
    public void CheckShorterTopicDoesntMatch()
    {
        var result = TopicChecker.Regex("a/b/c", "a/b");
        Assert.IsFalse(result);
    }
}
