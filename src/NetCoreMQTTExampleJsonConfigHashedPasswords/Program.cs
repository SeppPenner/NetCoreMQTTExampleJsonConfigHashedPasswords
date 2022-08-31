// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Hämmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   The main program.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleJsonConfigHashedPasswords;

/// <summary>
///     The main program.
/// </summary>
public class Program
{
    /// <summary>
    ///     The <see cref="PasswordHasher{TUser}"></see>.
    /// </summary>
    private static readonly PasswordHasher<User> Hasher = new();

    /// <summary>
    ///     The client identifier prefixes that are currently used.
    /// </summary>
    private static readonly List<string> ClientIdPrefixesUsed = new();

    /// <summary>
    /// Gets or sets the data limit cache for throttling for monthly data.
    /// </summary>
    private static readonly MemoryCache DataLimitCacheMonth = MemoryCache.Default;

    /// <summary>
    /// The logger.
    /// </summary>
    private static readonly ILogger Logger = Log.ForContext<Program>();

    /// <summary>
    /// The configuration.
    /// </summary>
    private static Config config = new();

    /// <summary>
    ///     The main method that starts the service.
    /// </summary>
    /// <param name="args">Some arguments. Currently unused.</param>
    public static void Main(string[] args)
    {
        var currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
        var certificate = new X509Certificate2(
            Path.Combine(currentPath, "certificate.pfx"),
            "test",
            X509KeyStorageFlags.Exportable);

        Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.File(
            Path.Combine(currentPath, @"log\NetCoreMQTTExampleJsonConfigHashedPasswords_.txt"),
            rollingInterval: RollingInterval.Day).CreateLogger();

        config = ReadConfiguration(currentPath);

        var optionsBuilder = new MqttServerOptionsBuilder()
#if DEBUG
                .WithDefaultEndpoint().WithDefaultEndpointPort(1883)
#else
                .WithoutDefaultEndpoint()
#endif
                .WithEncryptedEndpoint().WithEncryptedEndpointPort(config.Port)
            .WithEncryptionCertificate(certificate.Export(X509ContentType.Pfx))
            .WithEncryptionSslProtocol(SslProtocols.Tls12);

        var mqttServer = new MqttFactory().CreateMqttServer(optionsBuilder.Build());
        mqttServer.ValidatingConnectionAsync += ValidateConnectionAsync;
        mqttServer.InterceptingSubscriptionAsync += InterceptSubscriptionAsync;
        mqttServer.InterceptingPublishAsync += InterceptApplicationMessagePublishAsync;
        mqttServer.StartAsync();
        Console.ReadLine();
    }

    /// <summary>
    /// Validates the MQTT connection.
    /// </summary>
    /// <param name="args">The arguments.</param>
    private static Task ValidateConnectionAsync(ValidatingConnectionEventArgs args)
    {
        try
        {
            var currentUser = config.Users.FirstOrDefault(u => u.UserName == args.UserName);

            if (currentUser is null)
            {
                args.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                LogMessage(args, true);
                return Task.CompletedTask;
            }

            if (args.UserName != currentUser.UserName)
            {
                args.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                LogMessage(args, true);
                return Task.CompletedTask;
            }

            var hashingResult = Hasher.VerifyHashedPassword(currentUser, currentUser.Password, args.Password);

            if (hashingResult == PasswordVerificationResult.Failed)
            {
                args.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                LogMessage(args, true);
                return Task.CompletedTask;
            }

            if (!currentUser.ValidateClientId)
            {
                args.ReasonCode = MqttConnectReasonCode.Success;
                args.SessionItems.Add(args.ClientId, currentUser);
                LogMessage(args, false);
                return Task.CompletedTask;
            }

            if (string.IsNullOrWhiteSpace(currentUser.ClientIdPrefix))
            {
                if (args.ClientId != currentUser.ClientId)
                {
                    args.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                    LogMessage(args, true);
                    return Task.CompletedTask;
                }

                args.SessionItems.Add(currentUser.ClientId, currentUser);
            }
            else
            {
                if (!ClientIdPrefixesUsed.Contains(currentUser.ClientIdPrefix))
                {
                    ClientIdPrefixesUsed.Add(currentUser.ClientIdPrefix);
                }

                args.SessionItems.Add(currentUser.ClientIdPrefix, currentUser);
            }

            args.ReasonCode = MqttConnectReasonCode.Success;
            LogMessage(args, false);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Logger.Error("An error occurred: {Exception}.", ex);
            return Task.FromException(ex);
        }
    }

    /// <summary>
    /// Validates the MQTT subscriptions.
    /// </summary>
    /// <param name="args">The arguments.</param>
    private static Task InterceptSubscriptionAsync(InterceptingSubscriptionEventArgs args)
    {
        try
        {
            var clientIdPrefix = GetClientIdPrefix(args.ClientId);
            User? currentUser = null;

            if (string.IsNullOrWhiteSpace(clientIdPrefix))
            {
                if (args.SessionItems.Contains(args.ClientId))
                {
                    currentUser = args.SessionItems[args.ClientId] as User;
                }
            }
            else
            {
                if (args.SessionItems.Contains(clientIdPrefix))
                {
                    currentUser = args.SessionItems[clientIdPrefix] as User;
                }
            }

            if (currentUser is null)
            {
                args.ProcessSubscription = false;
                LogMessage(args, false);
                return Task.CompletedTask;
            }

            var topic = args.TopicFilter.Topic;

            if (currentUser.SubscriptionTopicLists.BlacklistTopics.Contains(topic))
            {
                args.ProcessSubscription = false;
                LogMessage(args, false);
                return Task.CompletedTask;
            }

            if (currentUser.SubscriptionTopicLists.WhitelistTopics.Contains(topic))
            {
                args.ProcessSubscription = true;
                LogMessage(args, true);
                return Task.CompletedTask;
            }

            foreach (var forbiddenTopic in currentUser.SubscriptionTopicLists.BlacklistTopics)
            {
                var doesTopicMatch = TopicChecker.Regex(forbiddenTopic, topic);
                if (!doesTopicMatch)
                {
                    continue;
                }

                args.ProcessSubscription = false;
                LogMessage(args, false);
                return Task.CompletedTask;
            }

            foreach (var allowedTopic in currentUser.SubscriptionTopicLists.WhitelistTopics)
            {
                var doesTopicMatch = TopicChecker.Regex(allowedTopic, topic);
                if (!doesTopicMatch)
                {
                    continue;
                }

                args.ProcessSubscription = true;
                LogMessage(args, true);
                return Task.CompletedTask;
            }

            args.ProcessSubscription = false;
            LogMessage(args, false);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Logger.Error("An error occurred: {Exception}.", ex);
            return Task.FromException(ex);
        }
    }

    /// <summary>
    /// Validates the MQTT application messages.
    /// </summary>
    /// <param name="args">The arguments.</param>
    private static Task InterceptApplicationMessagePublishAsync(InterceptingPublishEventArgs args)
    {
        try
        {
            var clientIdPrefix = GetClientIdPrefix(args.ClientId);
            User? currentUser = null;

            if (string.IsNullOrWhiteSpace(clientIdPrefix))
            {
                if (args.SessionItems.Contains(args.ClientId))
                {
                    currentUser = args.SessionItems[args.ClientId] as User;
                }
            }
            else
            {
                if (args.SessionItems.Contains(clientIdPrefix))
                {
                    currentUser = args.SessionItems[clientIdPrefix] as User;
                }
            }

            if (currentUser is null)
            {
                args.ProcessPublish = false;
                return Task.CompletedTask;
            }

            var topic = args.ApplicationMessage.Topic;

            if (currentUser.ThrottleUser)
            {
                var payload = args.ApplicationMessage?.Payload;

                if (payload != null)
                {
                    if (currentUser.MonthlyByteLimit != null)
                    {
                        if (IsUserThrottled(args.ClientId, payload.Length, currentUser.MonthlyByteLimit.Value))
                        {
                            args.ProcessPublish = false;
                            return Task.CompletedTask;
                        }
                    }
                }
            }

            if (currentUser.PublishTopicLists.BlacklistTopics.Contains(topic))
            {
                args.ProcessPublish = false;
                return Task.CompletedTask;
            }

            if (currentUser.PublishTopicLists.WhitelistTopics.Contains(topic))
            {
                args.ProcessPublish = true;
                LogMessage(args);
                return Task.CompletedTask;
            }

            foreach (var forbiddenTopic in currentUser.PublishTopicLists.BlacklistTopics)
            {
                var doesTopicMatch = TopicChecker.Regex(forbiddenTopic, topic);
                if (!doesTopicMatch)
                {
                    continue;
                }

                args.ProcessPublish = false;
                return Task.CompletedTask;
            }

            foreach (var allowedTopic in currentUser.PublishTopicLists.WhitelistTopics)
            {
                var doesTopicMatch = TopicChecker.Regex(allowedTopic, topic);
                if (!doesTopicMatch)
                {
                    continue;
                }

                args.ProcessPublish = true;
                LogMessage(args);
                return Task.CompletedTask;
            }

            args.ProcessPublish = false;
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Logger.Error("An error occurred: {Exception}.", ex);
            return Task.FromException(ex);
        }
    }

    /// <summary>
    ///     Gets the client id prefix for a client id if there is one or <c>null</c> else.
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

        return string.Empty;
    }

    /// <summary>
    /// Checks whether a user has used the maximum of its publishing limit for the month or not.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="sizeInBytes">The message size in bytes.</param>
    /// <param name="monthlyByteLimit">The monthly byte limit.</param>
    /// <returns>A value indicating whether the user will be throttled or not.</returns>
    private static bool IsUserThrottled(string clientId, long sizeInBytes, long monthlyByteLimit)
    {
        var foundUserInCache = DataLimitCacheMonth.GetCacheItem(clientId);

        if (foundUserInCache is null)
        {
            DataLimitCacheMonth.Add(clientId, sizeInBytes, DateTimeOffset.Now.EndOfCurrentMonth());

            if (sizeInBytes < monthlyByteLimit)
            {
                return false;
            }

            Logger.Information("The client with client id {@ClientId} is now locked until the end of this month because it already used its data limit.", clientId);
            return true;
        }

        try
        {
            var currentValue = Convert.ToInt64(foundUserInCache.Value);
            currentValue = checked(currentValue + sizeInBytes);
            DataLimitCacheMonth[clientId] = currentValue;

            if (currentValue >= monthlyByteLimit)
            {
                Logger.Information("The client with client id {@ClientId} is now locked until the end of this month because it already used its data limit.", clientId);
                return true;
            }
        }
        catch (OverflowException)
        {
            Logger.Information("OverflowException thrown.");
            Logger.Information("The client with client id {@ClientId} is now locked until the end of this month because it already used its data limit.", clientId);
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Reads the configuration.
    /// </summary>
    /// <param name="currentPath">The current path.</param>
    /// <returns>A <see cref="Config" /> object.</returns>
    private static Config ReadConfiguration(string currentPath)
    {
        var config = new Config();

        var filePath = $"{currentPath}\\config.json";

        if (!File.Exists(filePath))
        {
            return config;
        }

        using var r = new StreamReader(filePath);
        var json = r.ReadToEnd();
        config = JsonConvert.DeserializeObject<Config>(json) ?? new();

        return config;
    }

    /// <summary> 
    ///     Logs the message from the MQTT subscription interceptor context. 
    /// </summary> 
    /// <param name="args">The arguments.</param>
    /// <param name="successful">A <see cref="bool"/> value indicating whether the subscription was successful or not.</param> 
    private static void LogMessage(InterceptingSubscriptionEventArgs args, bool successful)
    {
        if (args is null)
        {
            return;
        }

        Logger.Information(
            successful
                ? "New subscription: ClientId = {@ClientId}, TopicFilter = {@TopicFilter}"
                : "Subscription failed for clientId = {@ClientId}, TopicFilter = {@TopicFilter}",
            args.ClientId,
            args.TopicFilter);
    }

    /// <summary>
    ///     Logs the message from the MQTT message interceptor context.
    /// </summary>
    /// <param name="args">The arguments.</param>
    private static void LogMessage(InterceptingPublishEventArgs args)
    {
        if (args is null)
        {
            return;
        }

        var payload = args.ApplicationMessage?.Payload is null ? null : Encoding.UTF8.GetString(args.ApplicationMessage.Payload);

        Logger.Information(
            "Message: ClientId = {@ClientId}, Topic = {@Topic}, Payload = {@Payload}, QoS = {@Qos}, Retain-Flag = {@RetainFlag}",
            args.ClientId,
            args.ApplicationMessage?.Topic,
            payload,
            args.ApplicationMessage?.QualityOfServiceLevel,
            args.ApplicationMessage?.Retain);
    }

    /// <summary> 
    ///     Logs the message from the MQTT connection validation context. 
    /// </summary> 
    /// <param name="args">The arguments.</param>
    /// <param name="showPassword">A <see cref="bool"/> value indicating whether the password is written to the log or not.</param> 
    private static void LogMessage(ValidatingConnectionEventArgs args, bool showPassword)
    {
        if (args is null)
        {
            return;
        }

        if (showPassword)
        {
            Logger.Information(
                "New connection: ClientId = {@ClientId}, Endpoint = {@Endpoint}, Username = {@UserName}, Password = {@Password}, CleanSession = {@CleanSession}",
                args.ClientId,
                args.Endpoint,
                args.UserName,
                args.Password,
                args.CleanSession);
        }
        else
        {
            Logger.Information(
                "New connection: ClientId = {@ClientId}, Endpoint = {@Endpoint}, Username = {@UserName}, CleanSession = {@CleanSession}",
                args.ClientId,
                args.Endpoint,
                args.UserName,
                args.CleanSession);
        }
    }
}
