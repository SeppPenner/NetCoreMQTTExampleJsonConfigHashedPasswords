// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PasswordHasher.cs" company=".NET Foundation.">
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See ApacheLicense.txt in the project root for license information.
// </copyright>
// <summary>
//   Implements the standard password hashing.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Hashing
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;

    /// <summary>
    ///     Implements the standard password hashing.
    /// </summary>
    public class PasswordHasher
    {
        /* =======================
         * HASHED PASSWORD FORMATS
         * =======================
         * 
         * Version 2:
         * PBKDF2 with HMAC-SHA1, 128-bit salt, 256-bit subkey, 1000 iterations.
         * (See also: SDL crypto guidelines v5.1, Part III)
         * Format: { 0x00, salt, subkey }
         *
         * Version 3:
         * PBKDF2 with HMAC-SHA256, 128-bit salt, 256-bit subkey, 10000 iterations.
         * Format: { 0x01, prf (UInt32), iter count (UInt32), salt length (UInt32), salt, subkey }
         * (All UInt32s are stored big-endian.)
         */

        /// <summary>
        ///     The password hasher compatibility mode.
        /// </summary>
        private readonly PasswordHasherCompatibilityMode compatibilityMode;

        /// <summary>
        ///     The iteration count.
        /// </summary>
        private readonly int iterCount;

        /// <summary>
        ///     The random number generator.
        /// </summary>
        private readonly RandomNumberGenerator rng;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PasswordHasher" /> class.
        /// </summary>
        public PasswordHasher()
        {
            var options = new PasswordHasherOptions();

            this.compatibilityMode = options.CompatibilityMode;
            switch (this.compatibilityMode)
            {
                case PasswordHasherCompatibilityMode.IdentityV2:
                    // nothing else to do
                    break;

                case PasswordHasherCompatibilityMode.IdentityV3:
                    this.iterCount = options.IterationCount;
                    if (this.iterCount < 1)
                    {
                        throw new InvalidOperationException("The iteration count must be a positive integer.");
                    }

                    break;

                default:
                    throw new InvalidOperationException("The provided PasswordHasherCompatibilityMode is invalid.");
            }

            this.rng = options.Rng;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PasswordHasher" /> class.
        /// </summary>
        /// <param name="optionsValue">The options.</param>
        // ReSharper disable once UnusedMember.Global
        public PasswordHasher(PasswordHasherOptions optionsValue)
        {
            var options = optionsValue ?? new PasswordHasherOptions();

            this.compatibilityMode = options.CompatibilityMode;
            switch (this.compatibilityMode)
            {
                case PasswordHasherCompatibilityMode.IdentityV2:
                    // nothing else to do
                    break;

                case PasswordHasherCompatibilityMode.IdentityV3:
                    this.iterCount = options.IterationCount;
                    if (this.iterCount < 1)
                    {
                        throw new InvalidOperationException("The iteration count must be a positive integer.");
                    }

                    break;

                default:
                    throw new InvalidOperationException("The provided PasswordHasherCompatibilityMode is invalid.");
            }

            this.rng = options.Rng;
        }

        /// <summary>
        ///     Returns a hashed representation of the supplied <paramref name="password" />.
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <returns>A hashed representation of the supplied <paramref name="password" />.</returns>
        public virtual string HashPassword(string password)
        {
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }

            return Convert.ToBase64String(
                this.compatibilityMode == PasswordHasherCompatibilityMode.IdentityV2
                    ? HashPasswordV2(password, this.rng)
                    : this.HashPasswordV3(password, this.rng));
        }

        /// <summary>
        ///     Returns a <see cref="PasswordVerificationResult" /> indicating the result of a password hash comparison.
        /// </summary>
        /// <param name="hashedPassword">The hash value for a user's stored password.</param>
        /// <param name="providedPassword">The password supplied for comparison.</param>
        /// <returns>A <see cref="PasswordVerificationResult" /> indicating the result of a password hash comparison.</returns>
        /// <remarks>Implementations of this method should be time consistent.</remarks>
        public virtual PasswordVerificationResult VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            if (hashedPassword == null)
            {
                throw new ArgumentNullException(nameof(hashedPassword));
            }

            if (providedPassword == null)
            {
                throw new ArgumentNullException(nameof(providedPassword));
            }

            var decodedHashedPassword = Convert.FromBase64String(hashedPassword);

            // read the format marker from the hashed password
            if (decodedHashedPassword.Length == 0)
            {
                return PasswordVerificationResult.Failed;
            }

            switch (decodedHashedPassword[0])
            {
                case 0x00:
                    if (VerifyHashedPasswordV2(decodedHashedPassword, providedPassword))
                    {
                        // This is an old password hash format - the caller needs to rehash if we're not running in an older compat mode.
                        return this.compatibilityMode == PasswordHasherCompatibilityMode.IdentityV3
                                   ? PasswordVerificationResult.SuccessRehashNeeded
                                   : PasswordVerificationResult.Success;
                    }
                    else
                    {
                        return PasswordVerificationResult.Failed;
                    }

                case 0x01:
                    if (VerifyHashedPasswordV3(decodedHashedPassword, providedPassword, out var embeddedIterCount))
                    {
                        // If this hasher was configured with a higher iteration count, change the entry now.
                        return embeddedIterCount < this.iterCount
                                   ? PasswordVerificationResult.SuccessRehashNeeded
                                   : PasswordVerificationResult.Success;
                    }
                    else
                    {
                        return PasswordVerificationResult.Failed;
                    }

                default:
                    return PasswordVerificationResult.Failed; // unknown format marker
            }
        }

        /// <summary>
        ///     Hashes the password with version 2.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="rng">The random number generator.</param>
        /// <returns>A <see cref="T:byte[]" /> of the hashed password.</returns>
        private static byte[] HashPasswordV2(string password, RandomNumberGenerator rng)
        {
            const KeyDerivationPrf Pbkdf2Prf = KeyDerivationPrf.HMACSHA1; // default for Rfc2898DeriveBytes
            const int Pbkdf2IterCount = 1000; // default for Rfc2898DeriveBytes
            const int Pbkdf2SubkeyLength = 256 / 8; // 256 bits
            const int SaltSize = 128 / 8; // 128 bits

            // Produce a version 2 (see comment above) text hash.
            var salt = new byte[SaltSize];
            rng.GetBytes(salt);
            var subkey = NetCorePbkdf2Provider.DeriveKey(
                password,
                salt,
                Pbkdf2Prf,
                Pbkdf2IterCount,
                Pbkdf2SubkeyLength);

            var outputBytes = new byte[1 + SaltSize + Pbkdf2SubkeyLength];
            outputBytes[0] = 0x00; // format marker
            Buffer.BlockCopy(salt, 0, outputBytes, 1, SaltSize);
            Buffer.BlockCopy(subkey, 0, outputBytes, 1 + SaltSize, Pbkdf2SubkeyLength);
            return outputBytes;
        }

        /// <summary>
        ///     Hashes the password with version 2.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="rng">The random number generator.</param>
        /// <param name="prf">The PRF.</param>
        /// <param name="iterCount">The iteration count.</param>
        /// <param name="saltSize">The salt size.</param>
        /// <param name="numBytesRequested">The number of requested bytes.</param>
        /// <returns>A <see cref="T:byte[]" /> of the hashed password.</returns>
        private static byte[] HashPasswordV3(
            string password,
            RandomNumberGenerator rng,
            KeyDerivationPrf prf,
            int iterCount,
            int saltSize,
            int numBytesRequested)
        {
            // Produce a version 3 (see comment above) text hash.
            var salt = new byte[saltSize];
            rng.GetBytes(salt);
            var subkey = NetCorePbkdf2Provider.DeriveKey(password, salt, prf, iterCount, numBytesRequested);

            var outputBytes = new byte[13 + salt.Length + subkey.Length];
            outputBytes[0] = 0x01; // format marker
            WriteNetworkByteOrder(outputBytes, 1, (uint)prf);
            WriteNetworkByteOrder(outputBytes, 5, (uint)iterCount);
            WriteNetworkByteOrder(outputBytes, 9, (uint)saltSize);
            Buffer.BlockCopy(salt, 0, outputBytes, 13, salt.Length);
            Buffer.BlockCopy(subkey, 0, outputBytes, 13 + saltSize, subkey.Length);
            return outputBytes;
        }

        /// <summary>
        /// Reads the network byte order.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <returns>A <see cref="uint"/> representing the network byte order.</returns>
        private static uint ReadNetworkByteOrder(IReadOnlyList<byte> buffer, int offset)
        {
            return ((uint)buffer[offset + 0] << 24) | ((uint)buffer[offset + 1] << 16) | ((uint)buffer[offset + 2] << 8)
                   | buffer[offset + 3];
        }

        /// <summary>
        /// Verifies a hashed password in version 2.
        /// </summary>
        /// <param name="hashedPassword">The hashed password.</param>
        /// <param name="password">The password.</param>
        /// <returns><c>true</c> if the password matches, <c>false</c> else.</returns>
        private static bool VerifyHashedPasswordV2(byte[] hashedPassword, string password)
        {
            const KeyDerivationPrf Pbkdf2Prf = KeyDerivationPrf.HMACSHA1; // default for Rfc2898DeriveBytes
            const int Pbkdf2IterCount = 1000; // default for Rfc2898DeriveBytes
            const int Pbkdf2SubkeyLength = 256 / 8; // 256 bits
            const int SaltSize = 128 / 8; // 128 bits

            // We know ahead of time the exact length of a valid hashed password payload.
            if (hashedPassword.Length != 1 + SaltSize + Pbkdf2SubkeyLength)
            {
                return false; // bad size
            }

            var salt = new byte[SaltSize];
            Buffer.BlockCopy(hashedPassword, 1, salt, 0, salt.Length);

            var expectedSubkey = new byte[Pbkdf2SubkeyLength];
            Buffer.BlockCopy(hashedPassword, 1 + salt.Length, expectedSubkey, 0, expectedSubkey.Length);

            // Hash the incoming password and verify it
            var actualSubkey = NetCorePbkdf2Provider.DeriveKey(
                password,
                salt,
                Pbkdf2Prf,
                Pbkdf2IterCount,
                Pbkdf2SubkeyLength);
            return CryptographicOperations.FixedTimeEquals(actualSubkey, expectedSubkey);
        }

        /// <summary>
        /// Verifies a hashed password in version 3.
        /// </summary>
        /// <param name="hashedPassword">The hashed password.</param>
        /// <param name="password">The password.</param>
        /// <param name="iterCount">The iteration count.</param>
        /// <returns><c>true</c> if the password matches, <c>false</c> else.</returns>
        private static bool VerifyHashedPasswordV3(byte[] hashedPassword, string password, out int iterCount)
        {
            iterCount = default;

            try
            {
                // Read header information
                var prf = (KeyDerivationPrf)ReadNetworkByteOrder(hashedPassword, 1);
                iterCount = (int)ReadNetworkByteOrder(hashedPassword, 5);
                var saltLength = (int)ReadNetworkByteOrder(hashedPassword, 9);

                // Read the salt: must be >= 128 bits
                if (saltLength < 128 / 8)
                {
                    return false;
                }

                var salt = new byte[saltLength];
                Buffer.BlockCopy(hashedPassword, 13, salt, 0, salt.Length);

                // Read the subkey (the rest of the payload): must be >= 128 bits
                var subkeyLength = hashedPassword.Length - 13 - salt.Length;
                if (subkeyLength < 128 / 8)
                {
                    return false;
                }

                var expectedSubkey = new byte[subkeyLength];
                Buffer.BlockCopy(hashedPassword, 13 + salt.Length, expectedSubkey, 0, expectedSubkey.Length);

                // Hash the incoming password and verify it
                var actualSubkey = NetCorePbkdf2Provider.DeriveKey(password, salt, prf, iterCount, subkeyLength);
                return CryptographicOperations.FixedTimeEquals(actualSubkey, expectedSubkey);
            }
            catch
            {
                // This should never occur except in the case of a malformed payload, where
                // we might go off the end of the array. Regardless, a malformed payload
                // implies verification failed.
                return false;
            }
        }

        /// <summary>
        /// Writes the network byte order.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="value">The network order value.</param>
        private static void WriteNetworkByteOrder(IList<byte> buffer, int offset, uint value)
        {
            buffer[offset + 0] = (byte)(value >> 24);
            buffer[offset + 1] = (byte)(value >> 16);
            buffer[offset + 2] = (byte)(value >> 8);
            buffer[offset + 3] = (byte)(value >> 0);
        }

        /// <summary>
        ///     Hashes the password with version 3.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="randomNumberGenerator">The random number generator.</param>
        /// <returns>A <see cref="T:byte[]" /> of the hashed password.</returns>
        private byte[] HashPasswordV3(string password, RandomNumberGenerator randomNumberGenerator)
        {
            return HashPasswordV3(password, randomNumberGenerator, KeyDerivationPrf.HMACSHA256, this.iterCount, 128 / 8, 256 / 8);
        }
    }
}