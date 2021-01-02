// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PasswordHasherCompatibilityMode.cs" company=".NET Foundation.">
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See ApacheLicense.txt in the project root for license information.
// </copyright>
// <summary>
//   Specifies the format used for hashing passwords.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Hashing
{
    /// <summary>
    ///     Specifies the format used for hashing passwords.
    /// </summary>
    public enum PasswordHasherCompatibilityMode
    {
        /// <summary>
        ///     Indicates hashing passwords in a way that is compatible with ASP.NET Identity versions 1 and 2.
        /// </summary>
        IdentityV2,

        /// <summary>
        ///     Indicates hashing passwords in a way that is compatible with ASP.NET Identity version 3.
        /// </summary>
        IdentityV3
    }
}