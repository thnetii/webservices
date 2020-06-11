using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.AspNetCore.Authentication;

namespace THNETII.WebServices.Authentication
{
    public static class AuthenticationHandlerTypeHelpers
    {
        /// <summary>
        /// Checks whether an authentication scheme handler type implements
        /// the authentication handler interface.
        /// </summary>
        /// <param name="handlerType">The authentication handler type, e.g. the value of <see cref="AuthenticationScheme.HandlerType"/>.</param>
        /// <returns><see langword="true"/> if <see cref="IAuthenticationHandler"/> is assignable from <paramref name="handlerType"/>.</returns>
        public static bool IsHandler(Type handlerType)
            => typeof(IAuthenticationHandler).IsAssignableFrom(handlerType);

        /// <summary>
        /// Checks whether an authentication scheme handler type implements
        /// the authentication handler interface for sign-in.
        /// </summary>
        /// <param name="handlerType">The authentication handler type, e.g. the value of <see cref="AuthenticationScheme.HandlerType"/>.</param>
        /// <returns><see langword="true"/> if <see cref="IAuthenticationSignInHandler"/> is assignable from <paramref name="handlerType"/>.</returns>
        public static bool IsSignInHandler(Type handlerType)
            => typeof(IAuthenticationSignInHandler).IsAssignableFrom(handlerType);

        /// <summary>
        /// Checks whether an authentication scheme handler type implements
        /// the authentication handler interface for sign-out.
        /// </summary>
        /// <param name="handlerType">The authentication handler type, e.g. the value of <see cref="AuthenticationScheme.HandlerType"/>.</param>
        /// <returns><see langword="true"/> if <see cref="IAuthenticationSignOutHandler"/> is assignable from <paramref name="handlerType"/>.</returns>
        public static bool IsSignOutHandler(Type handlerType)
            => typeof(IAuthenticationSignOutHandler).IsAssignableFrom(handlerType);

        /// <summary>
        /// Checks whether an authentication scheme handler type implements
        /// the authentication handler interface for incoming requests.
        /// </summary>
        /// <param name="handlerType">The authentication handler type, e.g. the value of <see cref="AuthenticationScheme.HandlerType"/>.</param>
        /// <returns><see langword="true"/> if <see cref="IAuthenticationRequestHandler"/> is assignable from <paramref name="handlerType"/>.</returns>
        public static bool IsRequestHandler(Type handlerType)
            => typeof(IAuthenticationRequestHandler).IsAssignableFrom(handlerType);

        /// <summary>
        /// Gets the <see cref="AuthenticationSchemeOptions"/> configured for an
        /// authentication handler, if the handler is a descendent of the
        /// <see cref="AuthenticationHandler{TOptions}"/> class.
        /// </summary>
        /// <param name="handler">The authentication handler to get options from.</param>
        /// <param name="handlerType">Optional. The authentication handler type, e.g. the value of <see cref="AuthenticationScheme.HandlerType"/>. If <see langword="null"/> or omitted, the result of calling <see cref="object.GetType"/> on <paramref name="handler"/> is used.</param>
        /// <returns>
        /// The value of the <see cref="AuthenticationHandler{TOptions}.Options"/> property when <paramref name="handler"/> is casted to
        /// a specific instance of <see cref="AuthenticationHandler{TOptions}"/>.
        /// If <paramref name="handler"/> does not derive from <see cref="AuthenticationHandler{TOptions}"/> or if <paramref name="handler"/> is <see langword="null"/>,
        /// <see langword="null"/> is returned.
        /// </returns>
        /// <remarks>
        /// The method determines the implementation of the <see cref="AuthenticationHandler{TOptions}"/>
        /// class and queries assembly metadata for the specific generic parameter used in the <paramref name="handlerType"/>.
        /// Then reflection is used to construct a specific delegate to access the <see cref="AuthenticationHandler{TOptions}.Options"/>
        /// property of the authentication handler. The constructed delegate is cached for subsequent calls using the same type.
        /// </remarks>
        public static AuthenticationSchemeOptions GetHandlerOptions(IAuthenticationHandler handler, Type handlerType = null)
        {
            if (handler is null)
                return null;
            else if (handlerType is null)
                handlerType = handler.GetType();
            bool found = false;
            HandlerGetOptionsFunc getOptionsFunc;
            lock (HandlerGetOptionsFuncs)
            {
                found = HandlerGetOptionsFuncs.TryGetValue(handlerType, out getOptionsFunc);
            }
            if (!found)
            {
                getOptionsFunc = CreateHandlerGetOptionsFunc(handlerType);
                lock (HandlerGetOptionsFuncs)
                {
                    HandlerGetOptionsFuncs[handlerType] = getOptionsFunc;
                }
            }
            return getOptionsFunc?.Invoke(handler);
        }

        private delegate AuthenticationSchemeOptions HandlerGetOptionsFunc(IAuthenticationHandler handler);

        private static HandlerGetOptionsFunc CreateHandlerGetOptionsFunc(Type handlerType)
        {
            var authHandlerType = EnumerateTypeHierarchy(handlerType)
                .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(AuthenticationHandler<>));
            if (authHandlerType is null)
                return null;
            var authOptionsType = authHandlerType.GetGenericArguments()[0];

            return (HandlerGetOptionsFunc)
                GetHandlerOptionsGeneric
                .MakeGenericMethod(authOptionsType)
                .CreateDelegate(typeof(HandlerGetOptionsFunc));
        }

        private static readonly MethodInfo GetHandlerOptionsGeneric =
            ((HandlerGetOptionsFunc)GetHandlerOptionsImpl<AuthenticationSchemeOptions>)
            .Method.GetGenericMethodDefinition();

        private static readonly Dictionary<Type, HandlerGetOptionsFunc> HandlerGetOptionsFuncs =
            new Dictionary<Type, HandlerGetOptionsFunc>();

        private static AuthenticationSchemeOptions GetHandlerOptionsImpl<TOptions>(IAuthenticationHandler handler)
            where TOptions : AuthenticationSchemeOptions, new()
        {
            if (handler is AuthenticationHandler<TOptions> handlerInstance)
                return handlerInstance.Options;
            return null;
        }

        /// <summary>
        /// Enumerates the type hierarchy of a specific type in reverse bottom-up
        /// tree order, starting with the specifie type.
        /// </summary>
        /// <param name="type">The type to start the enumeration with.</param>
        /// <returns>
        /// An enumerable sequence of <see cref="Type"/> in ordered from the most
        /// specific implementation of <paramref name="type"/> to the least specific (i.e. <see cref="object"/>).
        /// Returns an empty sequence if <paramref name="type"/> is <see langword="null"/>.
        /// </returns>
        private static IEnumerable<Type> EnumerateTypeHierarchy(Type type)
        {
            for (var t = type; t is Type; t = t.BaseType)
                yield return t;
        }
    }
}
