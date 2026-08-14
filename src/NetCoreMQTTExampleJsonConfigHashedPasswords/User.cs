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
    ///     Gets or sets the hashed password. This is read from the config.json file, so it must not be ignored by the
    ///     serializer, it is the only value the connection validation compares against.
    /// </summary>
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
    /// Gets or sets a value indicating whether the client id is validated or not. This defaults to <c>true</c>,
    /// because the flag exists to switch the validation off for a user that may use every client id. A config.json
    /// file that names a client id or a client id prefix expects that value to be checked.
    /// </summary>
    public bool ValidateClientId { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the user is throttled after a certain limit or not.
    /// </summary>
    public bool ThrottleUser { get; set; }

    /// <summary>
    /// Gets or sets a user's monthly limit in byte.
    /// </summary>
    public long? MonthlyByteLimit { get; set; }
}
