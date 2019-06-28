namespace CreateHashes
{
    using Hashing;
    using System;

    /// <summary>
    /// This program can be used to generate hashes from a certain password to use in the config.json file.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args)
        {
            Console.WriteLine("Please type in the password you want to hash:");
            var password = Console.ReadLine();
            var hasher = new PasswordHasher();
            var hashedPassword = hasher.HashPassword(password);
            Console.WriteLine($"Your password hash is {hashedPassword}. Please copy this from the console.");
            Console.WriteLine("Press any key to close this window.");
            Console.ReadKey();
        }
    }
}
