namespace NetCoreMQTTExampleJsonConfigHashedPasswords
{
    /// <summary>
    ///     The <see cref="User" /> read from the config.json file.
    /// </summary>
    public class User
    {
        /// <summary>
        ///     Gets or sets the user name.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        ///     Gets or sets the client identifier.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        ///     Gets or sets the password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        ///     Gets or sets the subscription topic lists.
        /// </summary>
        public TopicTuple SubscriptionTopicLists { get; set; } = new TopicTuple();

        /// <summary>
        ///     Gets or sets the publish topic lists.
        /// </summary>
        public TopicTuple PublishTopicLists { get; set; } = new TopicTuple();

        /// <summary>
        ///     Gets or sets the client identifier prefix (This can be used to allow several client identifiers with the same
        ///     prefix for one username / password combination).
        /// </summary>
        public string ClientIdPrefix { get; set; }
    }
}