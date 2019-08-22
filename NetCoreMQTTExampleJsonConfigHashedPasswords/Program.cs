namespace NetCoreMQTTExampleJsonConfigHashedPasswords
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security.Authentication;
    using System.Security.Cryptography.X509Certificates;

    using Hashing;

    using MQTTnet;
    using MQTTnet.Protocol;
    using MQTTnet.Server;

    using Newtonsoft.Json;

    /// <summary>
    ///     The main program.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The <see cref="PasswordHasher"></see>.
        /// </summary>
        private static readonly PasswordHasher Hasher = new PasswordHasher();

        /// <summary>
        /// The client identifier prefixes that are currently used.
        /// </summary>
        private static readonly List<string> ClientIdPrefixesUsed = new List<string>();

        /// <summary>
        ///     The main method that starts the service.
        /// </summary>
        /// <param name="args">Some arguments. Currently unused.</param>
        [SuppressMessage(
            "StyleCop.CSharp.DocumentationRules",
            "SA1650:ElementDocumentationMustBeSpelledCorrectly",
            Justification = "Reviewed. Suppression is OK here.")]
        public static void Main(string[] args)
        {
            var currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var certificate = new X509Certificate2(
                Path.Combine(currentPath, "certificate.pfx"),
                "test",
                X509KeyStorageFlags.Exportable);

            var config = ReadConfiguration(currentPath);

            var optionsBuilder = new MqttServerOptionsBuilder()
                //// .WithDefaultEndpoint().WithDefaultEndpointPort(1883) // For testing purposes only
                .WithoutDefaultEndpoint()
                .WithEncryptedEndpoint().WithEncryptedEndpointPort(config.Port)
                .WithEncryptionCertificate(certificate.Export(X509ContentType.Pfx))
                .WithEncryptionSslProtocol(SslProtocols.Tls12).WithConnectionValidator(
                    c =>
                        {
                            var currentUser = config.Users.FirstOrDefault(u => u.UserName == c.Username);

                            if (currentUser == null)
                            {
                                c.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                                return;
                            }

                            if (c.Username != currentUser.UserName)
                            {
                                c.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                                return;
                            }

                            var hashingResult = Hasher.VerifyHashedPassword(currentUser.Password, c.Password);

                            if (hashingResult == PasswordVerificationResult.Failed)
                            {
                                c.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                                return;
                            }

                            if (string.IsNullOrWhiteSpace(currentUser.ClientIdPrefix))
                            {
                                if (c.ClientId != currentUser.ClientId)
                                {
                                    c.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                                    return;
                                }

                                c.SessionItems.Add(currentUser.ClientId, currentUser);
                            }
                            else
                            {
                                if (!ClientIdPrefixesUsed.Contains(currentUser.ClientIdPrefix))
                                {
                                    ClientIdPrefixesUsed.Add(currentUser.ClientIdPrefix);
                                }

                                c.SessionItems.Add(currentUser.ClientIdPrefix, currentUser);
                            }

                            c.ReasonCode = MqttConnectReasonCode.Success;
                        }).WithSubscriptionInterceptor(
                    c =>
                        {
                            var clientIdPrefix = GetClientIdPrefix(c.ClientId);
                            User currentUser;
                            bool userFound;

                            if (clientIdPrefix == null)
                            {
                                userFound = c.SessionItems.TryGetValue(c.ClientId, out var currentUserObject);
                                currentUser = currentUserObject as User;
                            }
                            else
                            {
                                userFound = c.SessionItems.TryGetValue(clientIdPrefix, out var currentUserObject);
                                currentUser = currentUserObject as User;
                            }

                            if (!userFound || currentUser == null)
                            {
                                c.AcceptSubscription = false;
                                return;
                            }

                            var topic = c.TopicFilter.Topic;

                            if (currentUser.SubscriptionTopicLists.BlacklistTopics.Contains(topic))
                            {
                                c.AcceptSubscription = false;
                                return;
                            }

                            if (currentUser.SubscriptionTopicLists.WhitelistTopics.Contains(topic))
                            {
                                c.AcceptSubscription = true;
                                return;
                            }

                            foreach (var forbiddenTopic in currentUser.SubscriptionTopicLists.BlacklistTopics)
                            {
                                var doesTopicMatch = TopicChecker.Regex(forbiddenTopic, topic);
                                if (!doesTopicMatch)
                                {
                                    continue;
                                }

                                c.AcceptSubscription = false;
                                return;
                            }

                            foreach (var allowedTopic in currentUser.SubscriptionTopicLists.WhitelistTopics)
                            {
                                var doesTopicMatch = TopicChecker.Regex(allowedTopic, topic);
                                if (!doesTopicMatch)
                                {
                                    continue;
                                }

                                c.AcceptSubscription = true;
                                return;
                            }

                            c.AcceptSubscription = false;
                        }).WithApplicationMessageInterceptor(
                    c =>
                        {
                            var clientIdPrefix = GetClientIdPrefix(c.ClientId);
                            User currentUser;
                            bool userFound;

                            if (clientIdPrefix == null)
                            {
                                userFound = c.SessionItems.TryGetValue(c.ClientId, out var currentUserObject);
                                currentUser = currentUserObject as User;
                            }
                            else
                            {
                                userFound = c.SessionItems.TryGetValue(clientIdPrefix, out var currentUserObject);
                                currentUser = currentUserObject as User;
                            }

                            if (!userFound || currentUser == null)
                            {
                                c.AcceptPublish = false;
                                return;
                            }

                            var topic = c.ApplicationMessage.Topic;

                            if (currentUser.PublishTopicLists.BlacklistTopics.Contains(topic))
                            {
                                c.AcceptPublish = false;
                                return;
                            }

                            if (currentUser.PublishTopicLists.WhitelistTopics.Contains(topic))
                            {
                                c.AcceptPublish = true;
                                return;
                            }

                            foreach (var forbiddenTopic in currentUser.PublishTopicLists.BlacklistTopics)
                            {
                                var doesTopicMatch = TopicChecker.Regex(forbiddenTopic, topic);
                                if (!doesTopicMatch)
                                {
                                    continue;
                                }

                                c.AcceptPublish = false;
                                return;
                            }

                            foreach (var allowedTopic in currentUser.PublishTopicLists.WhitelistTopics)
                            {
                                var doesTopicMatch = TopicChecker.Regex(allowedTopic, topic);
                                if (!doesTopicMatch)
                                {
                                    continue;
                                }

                                c.AcceptPublish = true;
                                return;
                            }

                            c.AcceptPublish = false;
                        });

            var mqttServer = new MqttFactory().CreateMqttServer();
            mqttServer.StartAsync(optionsBuilder.Build());
            Console.ReadLine();
        }

        /// <summary>
        /// Gets the client id prefix for a client id if there is one or <c>null</c> else.
        /// </summary>
        /// <param name="clientId">The client id.</param>
        /// <returns>The client id prefix for a client id if there is one or <c>null</c> else.</returns>
        private static string GetClientIdPrefix(string clientId)
        {
            foreach (var clientIdPrefix in ClientIdPrefixesUsed)
            {
                if (clientId.StartsWith(clientIdPrefix))
                {
                    return clientIdPrefix;
                }
            }

            return null;
        }

        /// <summary>
        /// Reads the configuration.
        /// </summary>
        /// <param name="currentPath">The current path.</param>
        /// <returns>A <see cref="Config"/> object.</returns>
        private static Config ReadConfiguration(string currentPath)
        {
            var config = new Config();

            var filePath = $"{currentPath}\\config.json";

            if (!File.Exists(filePath))
            {
                return config;
            }

            using (var r = new StreamReader(filePath))
            {
                var json = r.ReadToEnd();
                config = JsonConvert.DeserializeObject<Config>(json);
            }

            return config;
        }
    }
}