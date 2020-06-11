using System;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;

using Xunit;

namespace THNETII.WebServices.Authentication.Test
{
    public static class AuthenticationHandlerTypeHelpersTest
    {
        [Theory]
        [InlineData(typeof(IAuthenticationHandler))]
        [InlineData(typeof(AuthenticationHandler<>))]
        [InlineData(typeof(RemoteAuthenticationHandler<>))]
        [InlineData(typeof(OAuthHandler<>))]
        [InlineData(typeof(CookieAuthenticationHandler))]
        public static void IsAuthenticationHandler_returns_true_for_AuthenticationHandler_types(Type type)
        {
            Assert.True(AuthenticationHandlerTypeHelpers.IsHandler(type));
        }

        [Theory]
        [InlineData(typeof(object))]
        [InlineData(typeof(int))]
        [InlineData(typeof(string))]
        public static void IsAuthenticationHandler_returns_false_for_non_AuthenticationHandler_types(Type type)
        {
            Assert.False(AuthenticationHandlerTypeHelpers.IsHandler(type));
        }

        [Fact]
        public static void IsAuthenticationHandler_returns_false_for_null_type()
        {
            Assert.False(AuthenticationHandlerTypeHelpers.IsHandler(null));
        }

        [Theory]
        [InlineData(typeof(IAuthenticationSignInHandler))]
        [InlineData(typeof(SignInAuthenticationHandler<>))]
        [InlineData(typeof(CookieAuthenticationHandler))]
        public static void IsAuthenticationSignInHandler_returns_true_for_AuthenticationHandler_types(Type type)
        {
            Assert.True(AuthenticationHandlerTypeHelpers.IsSignInHandler(type));
        }

        [Theory]
        [InlineData(typeof(RemoteAuthenticationHandler<>))]
        [InlineData(typeof(OAuthHandler<>))]
        public static void IsAuthenticationSignInHandler_returns_false_for_AuthenticationHandlers_that_do_not_support_sign_in(Type type)
        {
            Assert.False(AuthenticationHandlerTypeHelpers.IsSignInHandler(type));
        }

        [Theory]
        [InlineData(typeof(object))]
        [InlineData(typeof(int))]
        [InlineData(typeof(string))]
        public static void IsAuthenticationSignInHandler_returns_false_for_non_AuthenticationHandler_types(Type type)
        {
            Assert.False(AuthenticationHandlerTypeHelpers.IsSignInHandler(type));
        }

        [Fact]
        public static void IsAuthenticationSignInHandler_returns_false_for_null_type()
        {
            Assert.False(AuthenticationHandlerTypeHelpers.IsSignInHandler(null));
        }

        [Theory]
        [InlineData(typeof(IAuthenticationSignInHandler))]
        [InlineData(typeof(IAuthenticationSignOutHandler))]
        [InlineData(typeof(SignInAuthenticationHandler<>))]
        [InlineData(typeof(SignOutAuthenticationHandler<>))]
        [InlineData(typeof(CookieAuthenticationHandler))]
        public static void IsAuthenticationSignOutHandler_returns_true_for_AuthenticationHandler_types(Type type)
        {
            Assert.True(AuthenticationHandlerTypeHelpers.IsSignOutHandler(type));
        }

        [Theory]
        [InlineData(typeof(RemoteAuthenticationHandler<>))]
        [InlineData(typeof(OAuthHandler<>))]
        public static void IsAuthenticationSignOutHandler_returns_false_for_AuthenticationHandlers_that_do_not_support_sign_out(Type type)
        {
            Assert.False(AuthenticationHandlerTypeHelpers.IsSignOutHandler(type));
        }

        [Theory]
        [InlineData(typeof(object))]
        [InlineData(typeof(int))]
        [InlineData(typeof(string))]
        public static void IsAuthenticationSignOutHandler_returns_false_for_non_AuthenticationHandler_types(Type type)
        {
            Assert.False(AuthenticationHandlerTypeHelpers.IsSignOutHandler(type));
        }

        [Fact]
        public static void IsAuthenticationSignOutHandler_returns_false_for_null_type()
        {
            Assert.False(AuthenticationHandlerTypeHelpers.IsSignOutHandler(null));
        }

        [Theory]
        [InlineData(typeof(IAuthenticationRequestHandler))]
        [InlineData(typeof(RemoteAuthenticationHandler<>))]
        [InlineData(typeof(OAuthHandler<>))]
        public static void IsAuthenticationRequestHandler_returns_true_for_AuthenticationHandler_types(Type type)
        {
            Assert.True(AuthenticationHandlerTypeHelpers.IsRequestHandler(type));
        }

        [Theory]
        [InlineData(typeof(IAuthenticationHandler))]
        [InlineData(typeof(AuthenticationHandler<>))]
        [InlineData(typeof(CookieAuthenticationHandler))]
        public static void IsAuthenticationRequestHandler_returns_false_for_non_remote_AuthenticationHandler_types(Type type)
        {
            Assert.False(AuthenticationHandlerTypeHelpers.IsRequestHandler(type));
        }

        [Theory]
        [InlineData(typeof(object))]
        [InlineData(typeof(int))]
        [InlineData(typeof(string))]
        public static void IsAuthenticationRequestHandler_returns_false_for_non_AuthenticationHandler_types(Type type)
        {
            Assert.False(AuthenticationHandlerTypeHelpers.IsRequestHandler(type));
        }

        [Fact]
        public static void IsAuthenticationRequestHandler_returns_false_for_null_type()
        {
            Assert.False(AuthenticationHandlerTypeHelpers.IsRequestHandler(null));
        }
    }
}
