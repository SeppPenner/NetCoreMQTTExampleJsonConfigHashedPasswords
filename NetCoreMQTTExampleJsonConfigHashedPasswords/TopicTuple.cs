using System.Collections.Generic;

namespace NetCoreMQTTExampleJsonConfigHashedPasswords
{
    /// <summary>
    /// The <see cref="TopicTuple" /> read from the config.json file.
    /// </summary>
    public class TopicTuple
    {
        /// <summary>
        /// Gets or sets the whitelist topics.
        /// </summary>
        public List<string> WhitelistTopics { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the blacklist topics.
        /// </summary>
        public List<string> BlacklistTopics { get; set; } = new List<string>();
    }
}