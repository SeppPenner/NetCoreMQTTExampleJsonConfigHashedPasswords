
namespace NetCoreMQTTExampleJsonConfigHashedPasswords
{
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// A class to test the topics validity.
    /// </summary>
    public static class TopicChecker
    {
        /// <summary>
        /// Does a regex check on the topics.
        /// </summary>
        /// <param name="allowedTopic">The allowed topic.</param>
        /// <param name="topic">The topic.</param>
        /// <returns><c>true</c> if the topic is valid, <c>false</c> if not.</returns>
        public static bool Regex(string allowedTopic, string topic)
        {
            var realTopicRegex = allowedTopic.Replace(@"/", @"\/").Replace("+", "§").Replace("#", @"[a-zA-Z0-9 \/_#+.-]*").Replace("§", @"[a-zA-Z0-9 _.-]*");
            var regex = new Regex(realTopicRegex);
            var matches = regex.Matches(topic);

            foreach (var match in matches.ToList())
            {
                if (match.Value == topic)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Does a match check on the topics.
        /// This function is the translation of https://github.com/eclipse/mosquitto/blob/master/lib/util_topic.c#L138 from the Eclipse Mosquitto project to C#.
        /// This function is dual licensed under the Eclipse Public License 1.0 and the Eclipse Distribution License 1.0. Check the epl-v10 and edl-v10 files
        /// </summary>
        /// <param name="allowedTopic">The allowed topic.</param>
        /// <param name="topic">The topic.</param>
        /// <returns><c>true</c> if the topic is valid, <c>false</c> if not.</returns>
        public static bool TopicMatch(string allowedTopic, string topic)
        {
            if (string.IsNullOrWhiteSpace(allowedTopic) || string.IsNullOrWhiteSpace(topic))
            {
                return false;
            }

            if (allowedTopic == topic)
            {
                return true;
            }

            var topicLength = topic.Length;
            var allowedTopicLength = allowedTopic.Length;
            var position = 0;
            var allowedTopicIndex = 0;
            var topicIndex = 0;

            if ((allowedTopic[allowedTopicIndex] == '$' && topic[topicIndex] != '$') || (topic[topicIndex] == '$' && allowedTopic[allowedTopicIndex] != '$'))
            {
                return true;
            }

            while (allowedTopicIndex < allowedTopicLength)
            {
                if (topic[topicIndex] == '+' || topic[topicIndex] == '#')
                {
                    return false;
                }

                if (allowedTopic[allowedTopicIndex] != topic[topicIndex] || topicIndex >= topicLength)
                {
                    // Check for wildcard matches
                    if (allowedTopic[allowedTopicIndex] == '+')
                    {
                        // Check for bad "+foo" or "a/+foo" subscription
                        if (position > 0 && allowedTopic[allowedTopicIndex - 1] != '/')
                        {
                            return false;
                        }

                        // Check for bad "foo+" or "foo+/a" subscription
                        if (allowedTopicIndex + 1 < allowedTopicLength && allowedTopic[allowedTopicIndex + 1] != '/')
                        {
                            return false;
                        }

                        position++;
                        allowedTopicIndex++;
                        while (topicIndex < topicLength && topic[topicIndex] != '/')
                        {
                            topicIndex++;
                        }

                        if (topicIndex >= topicLength && allowedTopicIndex >= allowedTopicLength)
                        {
                            return true;
                        }
                    }
                    else if (allowedTopic[allowedTopicIndex] == '#')
                    {
                        // Check for bad "foo#" subscription
                        if (position > 0 && allowedTopic[allowedTopicIndex - 1] != '/')
                        {
                            return false;
                        }

                        // Check for # not the final character of the sub, e.g. "#foo"
                        if (allowedTopicIndex + 1 < allowedTopicLength)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        // Check for e.g. foo/bar matching foo/+/#
                        if (topicIndex >= topicLength && position > 0 && allowedTopic[allowedTopicIndex - 1] == '+' && allowedTopic[allowedTopicIndex] == '/' && allowedTopic[allowedTopicIndex + 1] == '#')
                        {
                            return true;
                        }

                        // There is no match at this point, but is the sub invalid?
                        while (allowedTopicIndex < allowedTopicLength)
                        {
                            if (allowedTopic[allowedTopicIndex] == '#' && allowedTopicIndex + 1 < allowedTopicLength)
                            {
                                return false;
                            }

                            position++;
                            allowedTopicIndex++;
                        }

                        // Valid input, but no match
                        return false;
                    }
                }
                else
                {
                    // sub[spos] == topic[tpos]
                    if (topicIndex + 1 >= topicLength)
                    {
                        // Check for e.g. foo matching foo/#
                        if (allowedTopic[allowedTopicIndex + 1] == '/' && allowedTopic[allowedTopicIndex + 2] == '#' && allowedTopicIndex + 3 >= allowedTopicLength)
                        {
                            return true;
                        }
                    }

                    position++;
                    allowedTopicIndex++;
                    topicIndex++;

                    if (allowedTopicIndex >= allowedTopicLength && topicIndex >= topicLength)
                    {
                        return true;
                    }
                    else if (topicIndex >= topicLength && allowedTopic[allowedTopicIndex] == '+' && allowedTopicIndex + 1 >= allowedTopicLength)
                    {
                        if (position > 0 && allowedTopic[allowedTopicIndex - 1] != '/')
                        {
                            return false;
                        }

                        position++;
                        allowedTopicIndex++;

                        return true;
                    }
                }
            }

            if (topicIndex < topicLength || allowedTopicIndex < allowedTopicLength)
            {
                return false;
            }

            return true;
        }
    }
}