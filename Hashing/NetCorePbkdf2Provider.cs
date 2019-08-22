// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NetCorePbkdf2Provider.cs" company=".NET Foundation.">
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See ApacheLicense.txt in the project root for license information.
// </copyright>
// <summary>
//   Implements Pbkdf2 using <see cref="Rfc2898DeriveBytes" />.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Hashing
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Implements Pbkdf2 using <see cref="Rfc2898DeriveBytes"/>.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public static class NetCorePbkdf2Provider
    {
        /// <summary>
        /// The fallback provider.
        /// </summary>
        private static readonly ManagedPbkdf2Provider FallbackProvider = new ManagedPbkdf2Provider();

        /// <summary>
        /// Gets the derived key.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="salt">The salt.</param>
        /// <param name="prf">The PRF.</param>
        /// <param name="iterationCount">The iteration count.</param>
        /// <param name="numBytesRequested">The number of requested bytes.</param>
        /// <returns>A <see cref="T:byte[]"/> of the derived key data.</returns>
        public static byte[] DeriveKey(string password, byte[] salt, KeyDerivationPrf prf, int iterationCount, int numBytesRequested)
        {
            Debug.Assert(password != null, "Password != null");
            Debug.Assert(salt != null, "Salt != null");
            Debug.Assert(iterationCount > 0, "iterationCount > 0");
            Debug.Assert(numBytesRequested > 0, "numBytesRequested > 0");

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (salt.Length < 8)
            {
                // Rfc2898DeriveBytes enforces the 8 byte recommendation.
                // To maintain compatibility, we call into ManagedPbkdf2Provider for salts shorter than 8 bytes
                // because we can't use Rfc2898DeriveBytes with this salt.
                return FallbackProvider.DeriveKey(password, salt, prf, iterationCount, numBytesRequested);
            }

            return DeriveKeyImpl(password, salt, prf, iterationCount, numBytesRequested);
        }

        /// <summary>
        /// Gets the derived key.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="salt">The salt.</param>
        /// <param name="prf">The PRF.</param>
        /// <param name="iterationCount">The iteration count.</param>
        /// <param name="numBytesRequested">The number of requested bytes.</param>
        /// <returns>A <see cref="T:byte[]"/> of the derived key data.</returns>
        private static byte[] DeriveKeyImpl(string password, byte[] salt, KeyDerivationPrf prf, int iterationCount, int numBytesRequested)
        {
            HashAlgorithmName algorithmName;
            switch (prf)
            {
                case KeyDerivationPrf.HMACSHA1:
                    algorithmName = HashAlgorithmName.SHA1;
                    break;
                case KeyDerivationPrf.HMACSHA256:
                    algorithmName = HashAlgorithmName.SHA256;
                    break;
                case KeyDerivationPrf.HMACSHA512:
                    algorithmName = HashAlgorithmName.SHA512;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var passwordBytes = Encoding.UTF8.GetBytes(password);
            using (var rfc = new Rfc2898DeriveBytes(passwordBytes, salt, iterationCount, algorithmName))
            {
                return rfc.GetBytes(numBytesRequested);
            }
        }
    }
}
