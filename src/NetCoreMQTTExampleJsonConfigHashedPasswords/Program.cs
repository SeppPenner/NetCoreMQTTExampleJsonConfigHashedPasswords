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

        var config = ReadConfiguration(currentPath);

        var optionsBuilder = new MqttServerOptionsBuilder()
#if DEBUG
                .WithDefaultEndpoint().WithDefaultEndpointPort(1883)
#else
                .WithoutDefaultEndpoint()
#endif
                .WithEncryptedEndpoint().WithEncryptedEndpointPort(config.Port)
            .WithEncryptionCertificate(certificate.Export(X509ContentType.Pfx))
            .WithEncryptionSslProtocol(SslProtocols.Tls12).WithConnectionValidator(
                c =>
                {
                    var currentUser = config.Users.FirstOrDefault(u => u.UserName == c.Username);

                    if (currentUser is null)
                    {
                        c.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                        LogMessage(c, true);
                        return;
                    }

                    if (c.Username != currentUser.UserName)
                    {
                        c.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                        LogMessage(c, true);
                        return;
                    }

                    var hashingResult = Hasher.VerifyHashedPassword(currentUser, currentUser.Password, c.Password);

                    if (hashingResult == PasswordVerificationResult.Failed)
                    {
                        c.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                        LogMessage(c, true);
                        return;
                    }

                    if (!currentUser.ValidateClientId)
                    {
                        c.ReasonCode = MqttConnectReasonCode.Success;
                        c.SessionItems.Add(c.ClientId, currentUser);
                        LogMessage(c, false);
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(currentUser.ClientIdPrefix))
                    {
                        if (c.ClientId != currentUser.ClientId)
                        {
                            c.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                            LogMessage(c, true);
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
                    LogMessage(c, false);
                }).WithSubscriptionInterceptor(
                c =>
                {
                    var clientIdPrefix = GetClientIdPrefix(c.ClientId);
                    User? currentUser;
                    bool userFound;

                    if (clientIdPrefix is null)
                    {
                        userFound = c.SessionItems.TryGetValue(c.ClientId, out var currentUserObject);
                        currentUser = currentUserObject as User;
                    }
                    else
                    {
                        userFound = c.SessionItems.TryGetValue(clientIdPrefix, out var currentUserObject);
                        currentUser = currentUserObject as User;
                    }

                    if (!userFound || currentUser is null)
                    {
                        c.AcceptSubscription = false;
                        LogMessage(c, false);
                        return;
                    }

                    var topic = c.TopicFilter.Topic;

                    if (currentUser.SubscriptionTopicLists.BlacklistTopics.Contains(topic))
                    {
                        c.AcceptSubscription = false;
                        LogMessage(c, false);
                        return;
                    }

                    if (currentUser.SubscriptionTopicLists.WhitelistTopics.Contains(topic))
                    {
                        c.AcceptSubscription = true;
                        LogMessage(c, true);
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
                        LogMessage(c, false);
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
                        LogMessage(c, true);
                        return;
                    }

                    c.AcceptSubscription = false;
                    LogMessage(c, false);
                }).WithApplicationMessageInterceptor(
                c =>
                {
                    var clientIdPrefix = GetClientIdPrefix(c.ClientId);
                    User? currentUser;
                    bool userFound;

                    if (clientIdPrefix is null)
                    {
                        userFound = c.SessionItems.TryGetValue(c.ClientId, out var currentUserObject);
                        currentUser = currentUserObject as User;
                    }
                    else
                    {
                        userFound = c.SessionItems.TryGetValue(clientIdPrefix, out var currentUserObject);
                        currentUser = currentUserObject as User;
                    }

                    if (!userFound || currentUser is null)
                    {
                        c.AcceptPublish = false;
                        return;
                    }

                    var topic = c.ApplicationMessage.Topic;

                    if (currentUser.ThrottleUser)
                    {
                        var payload = c.ApplicationMessage?.Payload;

                        if (payload != null)
                        {
                            if (currentUser.MonthlyByteLimit != null)
                            {
                                if (IsUserThrottled(c.ClientId, payload.Length, currentUser.MonthlyByteLimit.Value))
                                {
                                    c.AcceptPublish = false;
                                    return;
                                }
                            }
                        }
                    }

                    if (currentUser.PublishTopicLists.BlacklistTopics.Contains(topic))
                    {
                        c.AcceptPublish = false;
                        return;
                    }

                    if (currentUser.PublishTopicLists.WhitelistTopics.Contains(topic))
                    {
                        c.AcceptPublish = true;
                        LogMessage(c);
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
                        LogMessage(c);
                        return;
                    }

                    c.AcceptPublish = false;
                });

        var mqttServer = new MqttFactory().CreateMqttServer();
        mqttServer.StartAsync(optionsBuilder.Build());
        Console.ReadLine();
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
    /// <param name="context">The MQTT subscription interceptor context.</param> 
    /// <param name="successful">A <see cref="bool"/> value indicating whether the subscription was successful or not.</param> 
    private static void LogMessage(MqttSubscriptionInterceptorContext context, bool successful)
    {
        if (context is null)
        {
            return;
        }

        Logger.Information(
            successful
                ? "New subscription: ClientId = {@ClientId}, TopicFilter = {@TopicFilter}"
                : "Subscription failed for clientId = {@ClientId}, TopicFilter = {@TopicFilter}",
            context.ClientId,
            context.TopicFilter);
    }

    /// <summary>
    ///     Logs the message from the MQTT message interceptor context.
    /// </summary>
    /// <param name="context">The MQTT message interceptor context.</param>
    private static void LogMessage(MqttApplicationMessageInterceptorContext context)
    {
        if (context is null)
        {
            return;
        }

        var payload = context.ApplicationMessage?.Payload is null ? null : Encoding.UTF8.GetString(context.ApplicationMessage.Payload);

        Logger.Information(
            "Message: ClientId = {@ClientId}, Topic = {@Topic}, Payload = {@Payload}, QoS = {@Qos}, Retain-Flag = {@RetainFlag}",
            context.ClientId,
            context.ApplicationMessage?.Topic,
            payload,
            context.ApplicationMessage?.QualityOfServiceLevel,
            context.ApplicationMessage?.Retain);
    }

    /// <summary> 
    ///     Logs the message from the MQTT connection validation context. 
    /// </summary> 
    /// <param name="context">The MQTT connection validation context.</param> 
    /// <param name="showPassword">A <see cref="bool"/> value indicating whether the password is written to the log or not.</param> 
    private static void LogMessage(MqttConnectionValidatorContext context, bool showPassword)
    {
        if (context is null)
        {
            return;
        }

        if (showPassword)
        {
            Logger.Information(
                "New connection: ClientId = {@ClientId}, Endpoint = {@Endpoint}, Username = {@UserName}, Password = {@Password}, CleanSession = {@CleanSession}",
                context.ClientId,
                context.Endpoint,
                context.Username,
                context.Password,
                context.CleanSession);
        }
        else
        {
            Logger.Information(
                "New connection: ClientId = {@ClientId}, Endpoint = {@Endpoint}, Username = {@UserName}, CleanSession = {@CleanSession}",
                context.ClientId,
                context.Endpoint,
                context.Username,
                context.CleanSession);
        }
    }
}
