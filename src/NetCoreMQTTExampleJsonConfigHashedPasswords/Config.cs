// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Config.cs" company="Hämmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   The <see cref="Config" /> read from the config.json file.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleJsonConfigHashedPasswords;

/// <summary>
///     The <see cref="Config" /> read from the config.json file.
/// </summary>
public class Config
{
    /// <summary>
    ///     Gets or sets the port.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    ///     Gets or sets the list of valid users.
    /// </summary>
    public List<User> Users { get; set; } = new();
}
