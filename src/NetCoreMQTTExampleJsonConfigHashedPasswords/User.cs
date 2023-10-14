// --------------------------------------------------------------------------------------------------------------------
// <copyright file="User.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The <see cref="User" /> read from the config.json file.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleJsonConfigHashedPasswords;

/// <summary>
///     The <see cref="User" /> read from the config.json file.
/// </summary>
public class User
{
    /// <summary>
    ///     Gets or sets the user name.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the client identifier.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the password.
    /// </summary>
    [JsonIgnore]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the subscription topic lists.
    /// </summary>
    public TopicTuple SubscriptionTopicLists { get; set; } = new();

    /// <summary>
    ///     Gets or sets the publish topic lists.
    /// </summary>
    public TopicTuple PublishTopicLists { get; set; } = new();

    /// <summary>
    ///     Gets or sets the client identifier prefix (This can be used to allow several client identifiers with the same
    ///     prefix for one username / password combination).
    /// </summary>
    public string ClientIdPrefix { get; set; } = string.Empty;

    /// <summary> 
    /// Gets or sets a value indicating whether the client id is validated or not. 
    /// </summary> 
    public bool ValidateClientId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user is throttled after a certain limit or not.
    /// </summary>
    public bool ThrottleUser { get; set; }

    /// <summary>
    /// Gets or sets a user's monthly limit in byte.
    /// </summary>
    public long? MonthlyByteLimit { get; set; }
}
