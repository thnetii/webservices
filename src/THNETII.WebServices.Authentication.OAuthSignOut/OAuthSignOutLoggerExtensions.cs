using System;
using Microsoft.Extensions.Logging;

namespace THNETII.WebServices.Authentication.OAuthSignOut
{
    internal static class OAuthSignOutLoggerExtensions
    {
        private static readonly Action<ILogger, string, string, Exception> tokenRevoked =
            LoggerMessage.Define<string, string>(
                LogLevel.Information, new EventId(21, "OAuthTokenRevoked"),
                "OAuthToken '{TokenName}' was revoked for authentication scheme '{OAuthScheme}'");

        private static readonly Action<ILogger, string, string, Exception> tokenRevokeFailure =
            LoggerMessage.Define<string, string>(
                LogLevel.Warning, new EventId(22, "OAuthTokenRevokeFailure"),
                "OAuthToken '{TokenName}' revocation for authentication scheme '{OAuthScheme}' failed.");

        private static readonly Action<ILogger, string, Exception> signedOut =
            LoggerMessage.Define<string>(
                LogLevel.Debug, new EventId(20, "OAuthSignedOut"),
                "AuthenticationScheme: {AuthenticationScheme} was signed out.");

        public static void TokenRevoked(this ILogger logger, string tokenName, string oauthScheme) =>
            tokenRevoked(logger, tokenName, oauthScheme, null);

        public static void TokenRevokeFailure(this ILogger logger, string tokenName, string oauthScheme, Exception exception) =>
            tokenRevokeFailure(logger, tokenName, oauthScheme, exception);

        public static void SignedOut(this ILogger logger, string oauthScheme) =>
            signedOut(logger, oauthScheme, null);
    }
}
