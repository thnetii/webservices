using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace THNETII.WebServices.Authentication
{
    public sealed class MemoryCacheTicketStore : ITicketStore, IDisposable
    {
        public static readonly string OptionsName = nameof(MemoryCacheTicketStore);
        private readonly MemoryCache memoryCache;

        public MemoryCacheTicketStore(IOptionsMonitor<MemoryCacheOptions> options = null)
        {
            var cacheOptions = options is null ? new MemoryCacheOptions()
                : options.Get(OptionsName) ?? options.CurrentValue;
            memoryCache = new MemoryCache(cacheOptions);
        }

        public void Dispose() => memoryCache.Dispose();

        public Task RemoveAsync(string key)
        {
            memoryCache.Remove(key);
            return Task.CompletedTask;
        }

        public Task RenewAsync(string key, AuthenticationTicket ticket)
        {
            if (ticket is null)
                return Task.CompletedTask;

            var options = new MemoryCacheEntryOptions();
            var expiresUtc = ticket.Properties.ExpiresUtc;
            if (expiresUtc.HasValue)
                options.SetAbsoluteExpiration(expiresUtc.Value);

            memoryCache.Set(key, ticket, options);

            return Task.CompletedTask;
        }

        public Task<AuthenticationTicket> RetrieveAsync(string key)
            => Task.FromResult(memoryCache.Get<AuthenticationTicket>(key));

        [SuppressMessage("Reliability", "CA2007: Consider calling ConfigureAwait on the awaited task")]
        public async Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            if (ticket is null)
                return null;

            string sessionId = Guid.NewGuid().ToString();

            await RenewAsync(sessionId, ticket);

            return sessionId;
        }
    }
}
