// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TopicTuple.cs" company="Hämmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   The <see cref="TopicTuple" /> read from the config.json file.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleJsonConfigHashedPasswords;

/// <summary>
///     The <see cref="TopicTuple" /> read from the config.json file.
/// </summary>
public class TopicTuple
{
    /// <summary>
    ///     Gets or sets the whitelist topics.
    /// </summary>
    public List<string> WhitelistTopics { get; set; } = new();

    /// <summary>
    ///     Gets or sets the blacklist topics.
    /// </summary>
    public List<string> BlacklistTopics { get; set; } = new();
}
