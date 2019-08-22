// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ManagedPbkdf2Provider.cs" company=".NET Foundation.">
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See ApacheLicense.txt in the project root for license information.
// </copyright>
// <summary>
//   A PBKDF2 provider which utilizes the managed hash algorithm classes as PRFs.
//   This isn't the preferred provider since the implementation is slow, but it is provided as a fallback.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Hashing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// A PBKDF2 provider which utilizes the managed hash algorithm classes as PRFs.
    /// This isn't the preferred provider since the implementation is slow, but it is provided as a fallback.
    /// </summary>
    internal sealed class ManagedPbkdf2Provider
    {
        /// <summary>
        /// Gets the derived key.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="salt">The salt.</param>
        /// <param name="prf">The PRF.</param>
        /// <param name="iterationCount">The iteration count.</param>
        /// <param name="numBytesRequested">The number of requested bytes.</param>
        /// <returns>A <see cref="T:byte[]"/> of the derived key data.</returns>
        public byte[] DeriveKey(string password, byte[] salt, KeyDerivationPrf prf, int iterationCount, int numBytesRequested)
        {
            Debug.Assert(password != null, "Password != null");
            Debug.Assert(salt != null, "Salt != null");
            Debug.Assert(iterationCount > 0, "iterationCount > 0");
            Debug.Assert(numBytesRequested > 0, "numBytesRequested > 0");

            /* PBKDF2 is defined in NIST SP800-132, Sec. 5.3.
            http://csrc.nist.gov/publications/nistpubs/800-132/nist-sp800-132.pdf */

            var retVal = new byte[numBytesRequested];
            var numBytesWritten = 0;
            var numBytesRemaining = numBytesRequested;

            // For each block index, U_0 := Salt || block_index
            var saltWithBlockIndex = new byte[checked(salt.Length + sizeof(uint))];
            Buffer.BlockCopy(salt, 0, saltWithBlockIndex, 0, salt.Length);

            using (var hashAlgorithm = PrfToManagedHmacAlgorithm(prf, password))
            {
                for (uint blockIndex = 1; numBytesRemaining > 0; blockIndex++)
                {
                    // write the block index out as big-endian
                    saltWithBlockIndex[saltWithBlockIndex.Length - 4] = (byte)(blockIndex >> 24);
                    saltWithBlockIndex[saltWithBlockIndex.Length - 3] = (byte)(blockIndex >> 16);
                    saltWithBlockIndex[saltWithBlockIndex.Length - 2] = (byte)(blockIndex >> 8);
                    saltWithBlockIndex[saltWithBlockIndex.Length - 1] = (byte)blockIndex;

                    // U_1 = PRF(U_0) = PRF(Salt || block_index)
                    // T_blockIndex = U_1
                    var u1 = hashAlgorithm.ComputeHash(saltWithBlockIndex); // this is U_1
                    var newIndex = u1;

                    for (var iter = 1; iter < iterationCount; iter++)
                    {
                        u1 = hashAlgorithm.ComputeHash(u1);
                        XorBuffers(u1, newIndex);

                        // At this point, the 'u1' variable actually contains U_{iter+1} (due to indexing differences).
                    }

                    // At this point, we're done iterating on this block, so copy the transformed block into retVal.
                    var numBytesToCopy = Math.Min(numBytesRemaining, newIndex.Length);
                    Buffer.BlockCopy(newIndex, 0, retVal, numBytesWritten, numBytesToCopy);
                    numBytesWritten += numBytesToCopy;
                    numBytesRemaining -= numBytesToCopy;
                }
            }

            // retVal := T_1 || T_2 || ... || T_n, where T_n may be truncated to meet the desired output length
            return retVal;
        }

        /// <summary>
        /// Gets the key hash algorithm.
        /// </summary>
        /// <param name="prf">The PRF.</param>
        /// <param name="password">The password.</param>
        /// <returns>The <see cref="KeyedHashAlgorithm"/> to use.</returns>
        private static KeyedHashAlgorithm PrfToManagedHmacAlgorithm(KeyDerivationPrf prf, string password)
        {
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            try
            {
                switch (prf)
                {
                    case KeyDerivationPrf.HMACSHA1:
                        return new HMACSHA1(passwordBytes);
                    case KeyDerivationPrf.HMACSHA256:
                        return new HMACSHA256(passwordBytes);
                    case KeyDerivationPrf.HMACSHA512:
                        return new HMACSHA512(passwordBytes);
                    default:
                        throw new Exception("Unrecognized PRF.");
                }
            }
            finally
            {
                // The HMAC ctor makes a duplicate of this key; we clear original buffer to limit exposure to the GC.
                Array.Clear(passwordBytes, 0, passwordBytes.Length);
            }
        }

        /// <summary>
        /// Buffers the source with XORs.
        /// </summary>
        /// <param name="source">The source <see cref="T:byte[]"/>.</param>
        /// <param name="destination">The destination <see cref="T:byte[]"/>.</param>
        private static void XorBuffers(IReadOnlyList<byte> source, IList<byte> destination)
        {
            // Note: destination buffer is mutated.
            Debug.Assert(source.Count == destination.Count, "source.Length == destination.Length");

            for (var i = 0; i < source.Count; i++)
            {
                destination[i] ^= source[i];
            }
        }
    }
}
