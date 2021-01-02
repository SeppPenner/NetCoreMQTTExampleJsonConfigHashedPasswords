// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PasswordHasherOptions.cs" company=".NET Foundation.">
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See ApacheLicense.txt in the project root for license information.
// </copyright>
// <summary>
//   Specifies options for password hashing.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Hashing
{
    using System.Security.Cryptography;

    /// <summary>
    ///     Specifies options for password hashing.
    /// </summary>
    public class PasswordHasherOptions
    {
        /// <summary>
        ///     The default random number generator.
        /// </summary>
        private static readonly RandomNumberGenerator DefaultRng = RandomNumberGenerator.Create();

        /// <summary>
        ///     Gets or sets the compatibility mode used when hashing passwords. Defaults to 'ASP.NET Identity version 3'.
        /// </summary>
        /// <value>
        ///     The compatibility mode used when hashing passwords.
        /// </value>
        public PasswordHasherCompatibilityMode CompatibilityMode { get; set; } =
            PasswordHasherCompatibilityMode.IdentityV3;

        /// <summary>
        ///     Gets or sets the number of iterations used when hashing passwords using PBKDF2. Default is 10,000.
        /// </summary>
        /// <value>
        ///     The number of iterations used when hashing passwords using PBKDF2.
        /// </value>
        /// <remarks>
        ///     This value is only used when the compatibility mode is set to 'V3'.
        ///     The value must be a positive integer.
        /// </remarks>
        public int IterationCount { get; set; } = 10000;

        /// <summary>
        ///     Gets or sets the internal random number generator. For unit testing.
        /// </summary>
        internal RandomNumberGenerator Rng { get; set; } = DefaultRng;
    }
}