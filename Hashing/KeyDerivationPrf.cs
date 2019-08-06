// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KeyDerivationPrf.cs" company=".NET Foundation.">
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See ApacheLicense.txt in the project root for license information.
// </copyright>
// <summary>
//   Specifies the PRF which should be used for the key derivation algorithm.
// </summary>
// --------------------------------------------------------------------------------------------------------------------


namespace Hashing
{
    /// <summary>
    /// Specifies the PRF which should be used for the key derivation algorithm.
    /// </summary>
    // ReSharper disable InconsistentNaming
    public enum KeyDerivationPrf
    {
        /// <summary>
        /// The HMAC algorithm (RFC 2104) using the SHA-1 hash function (FIPS 180-4).
        /// </summary>
        HMACSHA1,

        /// <summary>
        /// The HMAC algorithm (RFC 2104) using the SHA-256 hash function (FIPS 180-4).
        /// </summary>
        HMACSHA256,

        /// <summary>
        /// The HMAC algorithm (RFC 2104) using the SHA-512 hash function (FIPS 180-4).
        /// </summary>
        HMACSHA512
    }
}
