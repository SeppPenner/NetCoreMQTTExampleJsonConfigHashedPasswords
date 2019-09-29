﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PasswordVerificationResult.cs" company=".NET Foundation.">
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See ApacheLicense.txt in the project root for license information.
// </copyright>
// <summary>
//   Specifies the results for password verification.
// </summary>
// --------------------------------------------------------------------------------------------------------------------


namespace Hashing
{
    /// <summary>
    ///     Specifies the results for password verification.
    /// </summary>
    public enum PasswordVerificationResult
    {
        /// <summary>
        ///     Indicates password verification failed.
        /// </summary>
        Failed = 0,

        /// <summary>
        ///     Indicates password verification was successful.
        /// </summary>
        Success = 1,

        /// <summary>
        ///     Indicates password verification was successful however the password was encoded using a deprecated algorithm
        ///     and should be rehashed and updated.
        /// </summary>
        SuccessRehashNeeded = 2
    }
}