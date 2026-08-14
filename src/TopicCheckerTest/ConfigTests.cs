// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigTests.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A test class to test the <see cref="Config" /> read from the config.json file.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TopicCheckerTest;

/// <summary>
///     A test class to test the <see cref="Config" /> read from the config.json file.
/// </summary>
[TestClass]
public class ConfigTests
{
    /// <summary>
    ///     The password that both users of the shipped config.json file are hashed from.
    /// </summary>
    private const string Password = "Test";

    /// <summary>
    ///     The password hasher used to verify the hashes from the config.json file.
    /// </summary>
    private readonly PasswordHasher<User> hasher = new();

    /// <summary>
    ///     Checks whether the port and the users are read from the shipped config.json file.
    /// </summary>
    [TestMethod]
    public void DeserializeReadsThePortAndTheUsers()
    {
        var config = ReadConfiguration();

        Assert.AreEqual(8883, config.Port);
        Assert.AreEqual(2, config.Users.Count);
        Assert.AreEqual("Hans", config.Users[0].UserName);
        Assert.AreEqual("Hans", config.Users[0].ClientId);
        Assert.AreEqual("Hans2", config.Users[1].UserName);
        Assert.AreEqual("Test_", config.Users[1].ClientIdPrefix);
    }

    /// <summary>
    ///     Checks whether the topic lists are read from the shipped config.json file.
    /// </summary>
    [TestMethod]
    public void DeserializeReadsTheTopicLists()
    {
        var config = ReadConfiguration();
        var user = config.Users[0];

        CollectionAssert.AreEqual(new List<string> { "a", "b/+", "c/#" }, user.SubscriptionTopicLists.BlacklistTopics);
        CollectionAssert.AreEqual(new List<string> { "d", "e/+", "f/#" }, user.SubscriptionTopicLists.WhitelistTopics);
        CollectionAssert.AreEqual(new List<string> { "a", "b/+", "c/#" }, user.PublishTopicLists.BlacklistTopics);
        CollectionAssert.AreEqual(new List<string> { "d", "e/+", "f/#" }, user.PublishTopicLists.WhitelistTopics);
    }

    /// <summary>
    ///     Checks whether the password hash of every user survives the deserialization. Without it no user can ever
    ///     log in, because the hash is the only thing the connection validation compares against.
    /// </summary>
    [TestMethod]
    public void DeserializeReadsThePasswordHash()
    {
        var config = ReadConfiguration();

        foreach (var user in config.Users)
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(user.Password), $"The password hash of {user.UserName} was dropped.");
        }
    }

    /// <summary>
    ///     Checks whether the hashes of the shipped config.json file verify the password documented in the readme.
    ///     This is the complete authentication path of the example, from the file to the hash comparison.
    ///     The result is <see cref="PasswordVerificationResult.SuccessRehashNeeded" /> and not
    ///     <see cref="PasswordVerificationResult.Success" />, because the shipped hashes were created with the 10000
    ///     iterations of the original hasher, while the current default is higher. The connection validation only
    ///     rejects <see cref="PasswordVerificationResult.Failed" />, so both results let the user in.
    /// </summary>
    [TestMethod]
    public void TheShippedHashesVerifyTheDocumentedPassword()
    {
        var config = ReadConfiguration();

        foreach (var user in config.Users)
        {
            var result = this.hasher.VerifyHashedPassword(user, user.Password, Password);
            Assert.AreEqual(PasswordVerificationResult.SuccessRehashNeeded, result, $"The user {user.UserName} cannot log in.");
        }
    }

    /// <summary>
    ///     Checks whether a wrong password is rejected by the hashes of the shipped config.json file.
    /// </summary>
    [TestMethod]
    public void TheShippedHashesRejectAWrongPassword()
    {
        var config = ReadConfiguration();

        foreach (var user in config.Users)
        {
            var result = this.hasher.VerifyHashedPassword(user, user.Password, "WrongPassword");
            Assert.AreEqual(PasswordVerificationResult.Failed, result, $"The user {user.UserName} accepts a wrong password.");
        }
    }

    /// <summary>
    ///     Checks whether the client id validation is switched on for a user that does not mention the flag. The flag
    ///     exists as a way out for a user that may use every client id, so a config.json file that names a client id or
    ///     a client id prefix must get that value checked.
    /// </summary>
    [TestMethod]
    public void ValidateClientIdIsOnUnlessTheConfigurationSwitchesItOff()
    {
        var config = ReadConfiguration();

        Assert.IsTrue(new User().ValidateClientId);
        Assert.IsTrue(config.Users[0].ValidateClientId);
        Assert.IsTrue(config.Users[1].ValidateClientId);
    }

    /// <summary>
    ///     Checks whether the flag can be switched off the way the readme writes it, as the string false and not as
    ///     the JSON literal false.
    /// </summary>
    [TestMethod]
    public void ValidateClientIdCanBeSwitchedOffAsAString()
    {
        const string Json = """
            {
              "Port": 8883,
              "Users": [
                {
                  "UserName": "Hans2",
                  "Password": "AQAAAAEAACcQAAAAECcnkwU+LImyVorjCCNzpTGgYOjVxFd+i/PW3MyU2sws80uPkPrppb+AXnvaxVI/0Q==",
                  "ValidateClientId": "false"
                }
              ]
            }
            """;

        var config = JsonConvert.DeserializeObject<Config>(Json);

        Assert.IsNotNull(config);
        Assert.IsFalse(config.Users[0].ValidateClientId);
    }

    /// <summary>
    ///     Reads the config.json file that the main project ships, copied next to the test assembly.
    /// </summary>
    /// <returns>The <see cref="Config" /> read from the config.json file.</returns>
    private static Config ReadConfiguration()
    {
        var json = File.ReadAllText("config.json");
        var config = JsonConvert.DeserializeObject<Config>(json);
        Assert.IsNotNull(config);
        return config;
    }
}
