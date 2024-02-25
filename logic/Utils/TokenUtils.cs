using System;
using System.Security.Cryptography;

namespace Fork.Logic.Utils;

public static class TokenUtils
{
    /// <summary>
    ///     Generates a random token used for communicating with the Discord bot
    /// </summary>
    /// <returns>Secure discord token</returns>
    public static string GenerateDiscordToken()
    {
        using (RandomNumberGenerator rng = new RNGCryptoServiceProvider())
        {
            byte[] tokenData = new byte[32];
            rng.GetBytes(tokenData);

            return Convert.ToBase64String(tokenData);
        }
    }
}