// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Hämmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   This program can be used to generate hashes from a certain password to use in the config.json file.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CreateHashes
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using Microsoft.AspNetCore.Identity;

    using NetCoreMQTTExampleJsonConfigHashedPasswords;

    /// <summary>
    ///     This program can be used to generate hashes from a certain password to use in the config.json file.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public class Program
    {
        /// <summary>
        ///     Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args)
        {
            Console.WriteLine("Please type in the password you want to hash:");
            var password = Console.ReadLine();
            var hasher = new PasswordHasher<User>();
            var hashedPassword = hasher.HashPassword(new User(), password);
            Console.WriteLine($"Your password hash is {hashedPassword}. Please copy this from the console.");
            Console.WriteLine("Press any key to close this window.");
            Console.ReadKey();
        }
    }
}