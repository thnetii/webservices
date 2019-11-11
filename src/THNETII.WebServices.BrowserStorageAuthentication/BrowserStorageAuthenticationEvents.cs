using System;
using System.Threading.Tasks;

namespace THNETII.WebServices.BrowserStorageAuthentication
{
    public class BrowserStorageAuthenticationEvents
    {
        public Func<BrowserStorageSigningInContext, Task> OnSigningIn
        { get; set; } = context => Task.CompletedTask;

        public virtual Task SigningIn(BrowserStorageSigningInContext context) =>
            OnSigningIn is Func<BrowserStorageSigningInContext, Task> f
            ? f(context) : Task.CompletedTask;
    }
}
